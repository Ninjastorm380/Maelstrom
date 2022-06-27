
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Threading

Public Partial Class Socket
    Implements IDisposable

    
    Private Sub ReliableRead(Byref Buffer as Byte(), Byval Offset as UInt32, Byval Count as UInt32)
            Dim A as UInt32 = 0    ' Counter/Read Offset
            Dim B as UInt32 = 0    ' Retry Counter
        Dim C as UInt32 = 100  ' Retry Limit
            Dim NetError As UInt32 = 0
            
            Do Until A = Count
                NetError = NetSocket.Receive(Buffer, A + Offset, Count - A, SocketFlags.None)
                If NetError > 0 Then
                    A += NetError
                    B = 0
                Else 
                    B += 1
                    If B >= C Then
                        IsClosed = True
                        Exit Sub
                    End If
                End If
            Loop
    End Sub
    
    Private Sub ReliableWrite(Byref Buffer as Byte(), Byval Offset as UInt32, Byval Count as UInt32)
        Dim A as UInt32 = 0    ' Counter/Read Offset
        Dim B as UInt32 = 0    ' Retry Counter
        Dim C as UInt32 = 100  ' Retry Limit
        Dim NetError As UInt32 = 0

        Do Until A = Count
            NetError = NetSocket.Send(Buffer, A + Offset, Count - A, SocketFlags.None)
            If NetError > 0 Then
                A += NetError
                B = 0
            Else
                B += 1
                If B >= C Then
                    IsClosed = True
                    Exit Sub
                End If
            End If
        Loop
    End Sub
    
    Private Sub AsyncRead
        SyncLock ReadLock
            
            If NetSocket.Available < 32 Then Return
            ReliableRead(AsyncHeader.HeaderRaw, 0, 32)
            If AsyncTransformBuffer.Length < AsyncHeader.Length Then ReDim AsyncTransformBuffer(AsyncHeader.Length - 1)
            ReliableRead(AsyncTransformBuffer, 0, AsyncHeader.Length)
            
            If (AsyncHeader.SubSocketConfig And SubSocketConfigFlag.Encrypted) <> 0 Then
                RemoteTransform.TransformBlock(AsyncTransformBuffer,
                                               0,
                                               AsyncHeader.EncryptedLength,
                                               AsyncTransformBuffer,
                                               0)
            End If   
            If (AsyncHeader.SubSocketConfig And SubSocketConfigFlag.Compressed) <> 0 Then
                RemoteDecompressor.Transform(AsyncTransformBuffer,
                                             AsyncTransformBuffer,
                                             AsyncHeader.CompressedLength)
            End If
            
            SyncLock BufferLock
                SubSocketBuffers(AsyncHeader.SubSocket).Write(AsyncHeader.HeaderRaw, 32)
                SubSocketBuffers(AsyncHeader.SubSocket).Write(AsyncTransformBuffer, AsyncHeader.RawLength)
            End SyncLock
        End SyncLock
    End Sub
    
    Public Function Read(Byval SubSocket as UInt32, Byref Data as Byte()) As UInt32
        SyncLock BufferLock


            SubSocketBuffers(SubSocket).Read(BufferHeader.HeaderRaw, 32, 0)
            If BufferTransformBuffer.Length < BufferHeader.RawLength Then _
                ReDim BufferTransformBuffer(BufferHeader.RawLength - 1)
            SubSocketBuffers(SubSocket).Read(BufferTransformBuffer, BufferHeader.RawLength, 32)
            SubSocketBuffers(SubSocket).Shift(BufferHeader.RawLength + 32)

            
            ReDim Data(BufferHeader.RawLength - 1)
            Buffer.BlockCopy(BufferTransformBuffer, 0, Data, 0, BufferHeader.RawLength)
            Return BufferHeader.RawLength
        End SyncLock
    End Function

    Public Sub Write(Byval SubSocket as UInt32, Data as Byte())
        SyncLock WriteLock
            LocalHeader.RawLength = Data.Length
            LocalHeader.SubSocket = SubSocket
            LocalHeader.SubSocketConfig = SubSocketConfigs(SubSocket)

            LocalHeader.CompressedLength = 0

            If LocalTransformBuffer.Length < LocalHeader.RawLength Then ReDim LocalTransformBuffer(LocalHeader.RawLength - 1)
            Buffer.BlockCopy(Data, 0, LocalTransformBuffer, 0, LocalHeader.RawLength)
            
            If (LocalHeader.SubSocketConfig And SubSocketConfigFlag.Compressed) <> 0 Then
                LocalHeader.CompressedLength = LocalCompressor.Transform(LocalTransformBuffer,
                                                                         LocalTransformBuffer,
                                                                         LocalHeader.RawLength)
            End If

            If LocalTransformBuffer.Length < LocalHeader.EncryptedLength Then ReDim Preserve LocalTransformBuffer(LocalHeader.EncryptedLength - 1)
            If (LocalHeader.SubSocketConfig And SubSocketConfigFlag.Encrypted) <> 0 Then
                If (LocalHeader.SubSocketConfig And SubSocketConfigFlagLarge) <> 0 Then
                    For Offset = 0 To LocalHeader.EncryptedLength - 1 Step LargeBlockSize
                        LocalTransform.TransformBlock(LocalTransformBuffer,
                                                      Offset,
                                                      LocalHeader.EncryptedLength - Offset,
                                                      LocalTransformBuffer,
                                                      Offset)
                    Next
                Else
                    LocalTransform.TransformBlock(LocalTransformBuffer,
                                                  0,
                                                  LocalHeader.EncryptedLength,
                                                  LocalTransformBuffer,
                                                  0)
                End If


            End If
            
            Try
                Try
                    ReliableWrite(LocalHeader.HeaderRaw, 0, 32)

                    If LocalHeader.Length >= LargeBlockSize Then 
                        If NetSocket.Available >= 32 Then
                            SyncLock ReadLock
                                If NetSocket.Available >= 32 Then
                                    Dim AsyncReadThread As new Thread(Addressof AsyncRead)
                                    AsyncReadThread.Start()
                                End If
                            End SyncLock
                        End If
                        
                    End If
                    
                    ReliableWrite(LocalTransformBuffer, 0, LocalHeader.Length)

                Catch IsDisposedEx as ObjectDisposedException
                    Isclosed = True
                End Try
            Catch SocketError as SocketException
                Select Case SocketError.SocketErrorCode
                    Case = System.Net.Sockets.SocketError.Shutdown
                        Isclosed = True
                    Case = System.Net.Sockets.SocketError.Disconnecting
                        Isclosed = True
                    Case = System.Net.Sockets.SocketError.OperationAborted
                        Isclosed = True
                    Case = System.Net.Sockets.SocketError.TimedOut
                        Isclosed = True
                    Case = System.Net.Sockets.SocketError.ConnectionReset
                        Isclosed = True
                    Case Else : Throw
                End Select
            End Try
        End SyncLock
    End Sub

    Private Function IsConnected As Boolean
        If IsDisposed = True Then Return False
        If IsClosed = True Then Return False
        If NetSocket is Nothing Then Return False
        If NetSocket.Connected = False Then Return False
        DataIsAvailable = NetSocket.Poll(0, SelectMode.SelectRead)
        DataIsNotAvailable = (NetSocket.Available = 0)

        'A - DataIsAvailable, B - DataIsNotAvailable
        '
        'A = true,  B = false - connected, data is available for reading. not inverts to true
        'A = false, B = true  - connected, data is not available for reading. not inverts to true
        'A = false, B = false - connected, data is available for reading. not inverts to true
        'A = true,  B = true  - disconnected, data is not available for reading. not inverts to false

        Return Not (DataIsAvailable = True AndAlso DataIsNotAvailable = True)
    End Function

    Private Function BootstrapInit(ServerSpawned as Boolean) As UInt32

        'Initialize for handshake
        Dim UTCTimestamp as DateTime = DateTime.UtcNow()
        LocalRNG = New VariableRNG({UTCTimestamp.Year,
                                    UTCTimestamp.Month,
                                    UTCTimestamp.Day,
                                    UTCTimestamp.Hour,
                                    UTCTimestamp.Minute,
                                    Math.Floor(UTCTimestamp.Second/2),
                                    RemotePort})
        If ServerSpawned = True Then
            LocalKey = LocalRNG.Next(32)
            LocalIV = LocalRNG.Next(16)
            RemoteKey = LocalRNG.Next(32)
            RemoteIV = LocalRNG.Next(16)

            LocalCSP = New AesCryptoServiceProvider With 
                {.Key = LocalKey, .IV = LocalIV, .Padding = PaddingMode.None}
            RemoteCSP = New AesCryptoServiceProvider With 
                {.Key = RemoteKey, .IV = RemoteIV, .Padding = PaddingMode.None}

            LocalTransform = LocalCSP.CreateEncryptor()
            RemoteTransform = RemoteCSP.CreateDecryptor()
        Else
            RemoteKey = LocalRNG.Next(32)
            RemoteIV = LocalRNG.Next(16)
            LocalKey = LocalRNG.Next(32)
            LocalIV = LocalRNG.Next(16)


            LocalCSP = New AesCryptoServiceProvider With 
                {.Key = LocalKey, .IV = LocalIV, .Padding = PaddingMode.None}
            RemoteCSP = New AesCryptoServiceProvider With 
                {.Key = RemoteKey, .IV = RemoteIV, .Padding = PaddingMode.None}

            LocalTransform = LocalCSP.CreateEncryptor()
            RemoteTransform = RemoteCSP.CreateDecryptor()
        End If


        'Prepare handshake bootstrap data
        LocalTimestamp = DateTime.Now
        Buffer.BlockCopy(MaelstromHandshakeHeader, 0, LocalProtocolHandshake, 0, 16)
        Buffer.BlockCopy(MaelstromHandshakeVersion, 0, LocalProtocolHandshake, 16, 16)
        PackTimestamp(LocalProtocolHandshake, 32, LocalTimestamp)

        'Declare transfer variables
        Dim WriteRawBuffer(63) as Byte
        Dim ReadRawBuffer(63) as Byte

        'Transfer handshake bootstrap data
        LocalTransform.TransformBlock(LocalProtocolHandshake, 0, 64, WriteRawBuffer, 0)

        Try
            NetSocket.Send(WriteRawBuffer, 0, 64, SocketFlags.None)
            NetSocket.Receive(ReadRawBuffer, 0, 64, SocketFlags.None)
        Catch err as SocketException
            Select Case err.SocketErrorCode
                Case = System.Net.Sockets.SocketError.Shutdown
                    Buffer.BlockCopy(MaelstromHeaderZeroed, 0, ReadRawBuffer, 0, 32)
                Case = System.Net.Sockets.SocketError.Disconnecting
                    Buffer.BlockCopy(MaelstromHeaderZeroed, 0, ReadRawBuffer, 0, 32)
                Case = System.Net.Sockets.SocketError.OperationAborted
                    Buffer.BlockCopy(MaelstromHeaderZeroed, 0, ReadRawBuffer, 0, 32)
                Case = System.Net.Sockets.SocketError.TimedOut
                    Buffer.BlockCopy(MaelstromHeaderZeroed, 0, ReadRawBuffer, 0, 32)
                Case = System.Net.Sockets.SocketError.ConnectionReset
                    Buffer.BlockCopy(MaelstromHeaderZeroed, 0, ReadRawBuffer, 0, 32)
                Case Else : Throw
            End Select
        End Try
        RemoteTransform.TransformBlock(ReadRawBuffer, 0, 64, RemoteProtocolHandshake, 0)

        'Cleanup after transfer
        LocalCSP.Dispose()
        LocalTransform.Dispose()
        RemoteCSP.Dispose()
        RemoteTransform.Dispose()

        'Perform handshake bootstrap verification
        If BinaryCompare(LocalProtocolHandshake, RemoteProtocolHandshake, 0, 16) = False Then Return 1
        If BinaryCompare(LocalProtocolHandshake, RemoteProtocolHandshake, 16, 16) = False Then Return 2
        Return 0
    End Function

    Friend Function Init(TransportSocket as System.Net.Sockets.Socket, ServerSpawned as Boolean) as SocketError

        'Set up base variables
        NetSocket = TransportSocket
        NetSocket.SendBufferSize = Int32.MaxValue
        NetSocket.ReceiveBufferSize = Int32.MaxValue '2000000
        NetSocket.SendTimeout = 1000
        NetSocket.ReceiveTimeout = 1000
        NetSocket.Blocking = True
        NetSocket.NoDelay = True
        NetStream = New NetworkStream(NetSocket, FileAccess.ReadWrite, False)
        LocalEndpoint = CType(NetSocket.LocalEndPoint, IPEndPoint)
        RemoteEndpoint = CType(NetSocket.RemoteEndPoint, IPEndPoint)

        'Set the non-server port
        If ServerSpawned = True Then
            RemotePort = RemoteEndpoint.Port
        Else
            RemotePort = LocalEndpoint.Port
        End If

        'Attempt to bootstrap init handshake
        BootstrapRetryResult = UInt32.MaxValue
        BootstrapRetryCounter = 0
        Do Until BootstrapRetryResult = 0 Or BootstrapRetryCounter = BootstrapRetryLimit
            BootstrapRetryResult = BootstrapInit(ServerSpawned)
            BootstrapRetryCounter += 1
        Loop

        'Abort and return if we couldn't bootstrap init successfully.
        If BootstrapRetryResult <> 0 Then return BootstrapRetryResult


        'Handshake and initialize with full functionality
        UnpackTimestamp(RemoteProtocolHandshake, 32, RemoteTimestamp)
        LocalRNG = New VariableRNG({LocalTimestamp.Year,
                                    LocalTimestamp.Month,
                                    LocalTimestamp.Day,
                                    LocalTimestamp.Hour,
                                    LocalTimestamp.Minute,
                                    LocalTimestamp.Second,
                                    LocalTimestamp.Millisecond,
                                    RemotePort})
        RemoteRNG = New VariableRNG({RemoteTimestamp.Year,
                                     RemoteTimestamp.Month,
                                     RemoteTimestamp.Day,
                                     RemoteTimestamp.Hour,
                                     RemoteTimestamp.Minute,
                                     RemoteTimestamp.Second,
                                     RemoteTimestamp.Millisecond,
                                     RemotePort})


        LocalKey = LocalRNG.Next(32)
        LocalIV = LocalRNG.Next(16)
        RemoteKey = RemoteRNG.Next(32)
        RemoteIV = RemoteRNG.Next(16)

        LocalCSP = New AesCryptoServiceProvider With 
            {.Key = LocalKey, .IV = LocalIV, .Padding = PaddingMode.None}
        RemoteCSP = New AesCryptoServiceProvider With 
            {.Key = RemoteKey, .IV = RemoteIV, .Padding = PaddingMode.None}

        LocalTransform = LocalCSP.CreateEncryptor()
        RemoteTransform = RemoteCSP.CreateDecryptor()
        

        LocalCompressor = New WriteCompressor()
        RemoteDecompressor = New ReadDecompressor()


        IsClosed = False
        Return 0
    End Function

    Public Sub CreateSubSocket(SubSocket as UInt32)
        SyncLock BufferLock
        SubSocketBuffers.Add(SubSocket, New QueueStream())
        SubSocketConfigs.Add(SubSocket, SubSocketConfigFlag.Nothing)
End SyncLock
    End Sub

    Public Sub RemoveSubSocket(SubSocket as UInt32)
        SyncLock BufferLock
        SubSocketBuffers.Remove(SubSocket)
        SubSocketConfigs.Remove(SubSocket)
End SyncLock
    End Sub

    Public Function SubSocketExists(SubSocket as UInt32) As Boolean
        SyncLock BufferLock
        Return SubSocketBuffers.Contains(SubSocket)
            End SyncLock
    End Function

    Public Function GetFreeSubSocket() As UInt32
        SyncLock BufferLock
        For SubSocket = 0 to UInt32.MaxValue - 1
            If SubSocketBuffers.Contains(SubSocket) = False Then Return SubSocket
        Next
            End SyncLock
        Throw New SubSocketsExhaustedException("No more subsockets available! Free subsockets by calling method 'RemoveSubSocket(SubSocket as UInt32)'!")
    End Function

    Public Sub Close()
        IsClosed = True
        SyncLock WriteLock
            SyncLock ReadLock
                NetSocket.Close()
            End SyncLock
        End SyncLock
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If IsDisposed = False
            IsClosed = True
            IsDisposed = True
            SyncLock WriteLock
                SyncLock ReadLock
                    NetSocket.Close()
                    LocalCSP.Dispose()
                    LocalTransform.Dispose()
                    RemoteCSP.Dispose()
                    RemoteTransform.Dispose()
                    NetSocket.Dispose()
                End SyncLock
            End SyncLock
        End If
    End Sub
End Class