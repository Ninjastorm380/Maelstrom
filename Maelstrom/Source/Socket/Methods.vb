Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography

Public Partial Class Socket
    Implements IDisposable

    Friend Function Init(TransportSocket as Sockets.Socket, ServerMode as Boolean) as Int32

        'Set up base variables
        NetSocket = TransportSocket
        NetSocket.SendBufferSize = 200000
        NetSocket.ReceiveBufferSize = 200000
        NetSocket.Blocking = True
        NetStream = New NetworkStream(NetSocket)
        RemoteEndpoint = CType(NetSocket.RemoteEndPoint, IPEndPoint)
        LocalEndpoint = CType(NetSocket.LocalEndPoint, IPEndPoint)

        'Determine the non-hosting port
        If ServerMode = True Then RemotePort = RemoteEndpoint.Port Else RemotePort = LocalEndpoint.Port

        'Initialize for handshake
        Dim UTCTimestamp as DateTime = DateTime.UtcNow()
        Init({UTCTimestamp.Year,
              UTCTimestamp.Month,
              UTCTimestamp.Day,
              UTCTimestamp.Hour,
              UTCTimestamp.Minute,
              Math.Floor(UTCTimestamp.Second/3),
              RemotePort}, NetStream, ServerMode)

        'Perform handshake
        LocalTimestamp = DateTime.Now
        Buffer.BlockCopy(MaelstromHandshakeHeader, 0, LocalProtocolHandshake, 0, 16)
        Buffer.BlockCopy(BitConverter.GetBytes(MaelstromHandshakeVersion(0)), 0, LocalProtocolHandshake, 16, 4)
        Buffer.BlockCopy(BitConverter.GetBytes(MaelstromHandshakeVersion(1)), 0, LocalProtocolHandshake, 20, 4)
        Buffer.BlockCopy(BitConverter.GetBytes(MaelstromHandshakeVersion(2)), 0, LocalProtocolHandshake, 24, 4)
        Buffer.BlockCopy(BitConverter.GetBytes(MaelstromHandshakeVersion(3)), 0, LocalProtocolHandshake, 28, 4)
        PackTimestamp(LocalTimestamp, LocalProtocolHandshake, 32)

        Write(LocalProtocolHandshake, 0, 64)
        Read(RemoteProtocolHandshake, 0, 64)

        For Index = 0 to MaelstromHandshakeHeader.Length - 1
            if LocalProtocolHandshake(Index) <> RemoteProtocolHandshake(Index) Then
                Dispose()
                Return 1
            End If
        Next

        Dim RemoteVersion(3) as Int32
        UnpackInt32(RemoteProtocolHandshake, RemoteVersion(0), 16)
        UnpackInt32(RemoteProtocolHandshake, RemoteVersion(1), 20)
        UnpackInt32(RemoteProtocolHandshake, RemoteVersion(2), 24)
        UnpackInt32(RemoteProtocolHandshake, RemoteVersion(3), 28)
        For Index = 0 to MaelstromHandshakeVersion.Length - 1
            if MaelstromHandshakeVersion(Index) <> RemoteVersion(Index)
                Dispose()
                Return 2
            End If
        Next

        UnpackTimestamp(RemoteProtocolHandshake, RemoteTimestamp, 32)

        'Re-initialize with full functionality
        Init({LocalTimestamp.Year,
              LocalTimestamp.Month,
              LocalTimestamp.Day,
              LocalTimestamp.Hour,
              LocalTimestamp.Minute,
              LocalTimestamp.Second,
              LocalTimestamp.Millisecond,
              RemotePort},
             {RemoteTimestamp.Year,
              RemoteTimestamp.Month,
              RemoteTimestamp.Day,
              RemoteTimestamp.Hour,
              RemoteTimestamp.Minute,
              RemoteTimestamp.Second,
              RemoteTimestamp.Millisecond,
              RemotePort},
             NetStream)

        Return 0
    End Function

    Private Sub Init(LocalSeed as Int32(), Stream as NetworkStream, Server as Boolean)
        LocalRNGMan = New RNG(LocalSeed)
        NetStream = Stream

        If Server = True Then
            LocalRNGMan.Next(LocalKey)
            LocalRNGMan.Next(LocalIV)
            LocalRNGMan.Next(RemoteKey)
            LocalRNGMan.Next(RemoteIV)
        Else
            LocalRNGMan.Next(RemoteKey)
            LocalRNGMan.Next(RemoteIV)
            LocalRNGMan.Next(LocalKey)
            LocalRNGMan.Next(LocalIV)
        End If

        LocalCSP = New AesCryptoServiceProvider With {.Key = LocalKey, .IV = LocalIV, .Padding = PaddingMode.None}
        RemoteCSP = New AesCryptoServiceProvider With {.Key = RemoteKey, .IV = RemoteIV, .Padding = PaddingMode.None}

        LocalEncryptor = LocalCSP.CreateEncryptor()
        RemoteDecryptor = RemoteCSP.CreateDecryptor()
    End Sub

    Private Sub Init(LocalSeed as Int32(), RemoteSeed as Int32(), Stream as NetworkStream)
        LocalRNGMan = New RNG(LocalSeed)
        RemoteRNGMan = New RNG(RemoteSeed)
        NetStream = Stream

        LocalRNGMan.Next(LocalKey)
        LocalRNGMan.Next(LocalIV)
        RemoteRNGMan.Next(RemoteKey)
        RemoteRNGMan.Next(RemoteIV)

        LocalCSP = New AesCryptoServiceProvider With {.Key = LocalKey, .IV = LocalIV, .Padding = PaddingMode.None}
        RemoteCSP = New AesCryptoServiceProvider With {.Key = RemoteKey, .IV = RemoteIV, .Padding = PaddingMode.None}

        LocalEncryptor = LocalCSP.CreateEncryptor()
        RemoteDecryptor = RemoteCSP.CreateDecryptor()
    End Sub

    
    Private Sub Read(byval Buffer as Byte(), Offset as Int32, Count as Int32)
        If RemoteTransformBuffer.Length < Count Then Redim RemoteTransformBuffer(Count - 1)
        NetSocket.Receive(RemoteTransformBuffer,0,Count, SocketFlags.None)
        RemoteDecryptor.TransformBlock(RemoteTransformBuffer,0, Count,Buffer,Offset)
    End Sub
    
    
    Private Sub Write(byval Buffer as Byte(), Offset as Int32, Count as Int32)
        If LocalTransformBuffer.Length < Count Then Redim LocalTransformBuffer(Count - 1)
        LocalEncryptor.TransformBlock(Buffer,Offset, Count,LocalTransformBuffer,0)
        NetSocket.Send(LocalTransformBuffer, 0,Count,SocketFlags.None)
    End Sub


    Public Sub ReadArray(Index as Int32, Byref Output as Byte())
        If Manager.Contains(Index) = False Then _
            Throw New ArgumentException("Parameter 'Index' referrs to a non-existent stream!")
        If Output IsNot Nothing Then Throw New ArgumentException("Parameter 'Output' must be nothing!")
        SyncLock ReadLock
            If Manager.Value(Index).Length >= 32
                Manager.Value(Index).Read(ReadDataHeader, 32, 0)
                UnpackInt32(ReadDataHeader, ReadPaddedLength, 0)
                UnpackInt32(ReadDataHeader, ReadDataLength, 4)
                UnpackInt32(ReadDataHeader, ReadIsMuxed, 8)
                UnpackInt32(ReadDataHeader, ReadIsJagged, 12)
                UnpackInt32(ReadDataHeader, ReadBlockCount, 16)
                
                If Manager.Value(Index).Length < ReadPaddedLength Then Return
                If ReadPaddedData.Length < ReadPaddedLength Then Redim ReadPaddedData(ReadPaddedLength - 1)
                Manager.Value(Index).Read(ReadPaddedData, ReadPaddedLength, 32)

                If ReadIsJagged <> - 1 Then
                    Return
                End If
                If Index <> ReadIsMuxed Then 
                    Return
                End If

                ReDim Output(ReadDataLength - 1)
                Buffer.BlockCopy(ReadPaddedData, 0, Output, 0, ReadDataLength)
                Manager.Value(Index).Dump(ReadPaddedLength + 32)
            Else
                If WaitForData(32) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                Read(ReadDataHeader, 0, 32)
                UnpackInt32(ReadDataHeader, ReadPaddedLength, 0)
                UnpackInt32(ReadDataHeader, ReadDataLength, 4)
                UnpackInt32(ReadDataHeader, ReadIsMuxed, 8)
                UnpackInt32(ReadDataHeader, ReadIsJagged, 12)
                UnpackInt32(ReadDataHeader, ReadBlockCount, 16)
                
                If ReadPaddedData.Length < ReadPaddedLength Then Redim ReadPaddedData(ReadPaddedLength - 1)
                If ReadBlockCount = 1 Then
                    If WaitForData(ReadPaddedLength) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                    Read(ReadPaddedData, 0, ReadPaddedLength)
                Else 
                    ReadLoopIndexer = 0
                    Do until ReadLoopIndexer >= ReadPaddedLength
                        If WaitForData(ReadBlockSize) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                        Read(ReadPaddedData, ReadLoopIndexer, ReadBlockSize)
                        ReadLoopIndexer += ReadBlockSize
                    Loop
                End If
                
                If ReadIsJagged <> - 1 Then
                    If Manager.Contains(ReadIsMuxed) = True
                        Manager.Value(ReadIsMuxed).Write(ReadDataHeader, 32)
                        Manager.Value(ReadIsMuxed).Write(ReadPaddedData, ReadPaddedLength)
                    End If
                    Return
                End If

                If Index <> ReadIsMuxed Then
                    If Manager.Contains(ReadIsMuxed) = True
                        Manager.Value(ReadIsMuxed).Write(ReadDataHeader, 32)
                        Manager.Value(ReadIsMuxed).Write(ReadPaddedData, ReadPaddedLength)
                    End If
                    Return
                End If

                ReDim Output(ReadDataLength - 1)
                Buffer.BlockCopy(ReadPaddedData, 0, Output, 0, ReadDataLength)
            End If
        End SyncLock
    End Sub


    Public Sub WriteArray(Index as Int32, Byref Input as Byte())
        If Manager.Contains(Index) = False Then _
            Throw New ArgumentException("Parameter 'Index' referrs to a non-existent stream!")
        If Input Is Nothing Then Throw New ArgumentException("Parameter 'Input' must not be nothing!")
        SyncLock WriteLock
            WriteDataLength = Input.Length
            WritePaddedLength = GetNearestBlockSize(WriteDataLength)
            WriteBlockCount = Math.Ceiling(WritePaddedLength / WriteBlockSize)
            
            If WriteBlockCount > 1 Then
                WritePaddedLength = WriteBlockSize * WriteBlockCount
            End If
            
            
            If WritePaddedData.Length < WritePaddedLength Then Redim WritePaddedData(WritePaddedLength - 1)
            Buffer.BlockCopy(Input, 0, WritePaddedData, 0, WriteDataLength)
            PackInt32(WritePaddedLength, WriteDataHeader, 0)
            PackInt32(WriteDataLength, WriteDataHeader, 4)
            PackInt32(Index, WriteDataHeader, 8)
            PackInt32(-1, WriteDataHeader, 12)
            PackInt32(WriteBlockCount, WriteDataHeader, 16)
            Write(WriteDataHeader, 0, 32)
            If WriteBlockCount = 1 Then
                Write(WritePaddedData, 0, WritePaddedLength)
            Else 
                WriteLoopIndexer = 0
                Do until WriteLoopIndexer >= WritePaddedLength
                    Write(WritePaddedData, WriteLoopIndexer, WriteBlockSize)
                    WriteLoopIndexer += WriteBlockSize
                Loop
            End If
            

        End SyncLock
    End Sub

    Public Sub ReadJagged(Index as Int32, Byref Output as Byte()())
        If Manager.Contains(Index) = False Then _
            Throw New ArgumentException("Parameter 'Index' referrs to a non-existent stream!")
        If Output IsNot Nothing Then Throw New ArgumentException("Parameter 'Output' must be nothing!")
        SyncLock ReadLock
            If Manager.Value(Index).Length >= 32
                Manager.Value(Index).Read(ReadDataHeader, 32, 0)
                UnpackInt32(ReadDataHeader, ReadPaddedLength, 0)
                UnpackInt32(ReadDataHeader, ReadDataLength, 4)
                UnpackInt32(ReadDataHeader, ReadIsMuxed, 8)
                UnpackInt32(ReadDataHeader, ReadIsJagged, 12)
                UnpackInt32(ReadDataHeader, ReadBlockCount, 16)
                
                If Manager.Value(Index).Length < ReadPaddedLength Then Return
                If ReadPaddedData.Length < ReadPaddedLength Then Redim ReadPaddedData(ReadPaddedLength - 1)
                Manager.Value(Index).Read(ReadPaddedData, ReadPaddedLength, 32)

                If ReadIsJagged = - 1 Then Return
                If Index <> ReadIsMuxed Then Return

                ReDim Output(ReadIsJagged - 1)
                ReadJaggedIndexer = 0
                ReadJaggedCounter = 0
                Do Until ReadJaggedIndexer > ReadIsJagged - 1
                    UnpackInt32(ReadPaddedData, ReadJaggedIndexLength, ReadJaggedCounter)
                    ReadJaggedCounter += 4
                    Redim Output(ReadJaggedIndexer)(ReadJaggedIndexLength - 1)
                    Buffer.BlockCopy(ReadPaddedData, ReadJaggedCounter, Output(ReadJaggedIndexer), 0,
                                     ReadJaggedIndexLength)
                    ReadJaggedCounter += ReadJaggedIndexLength
                    ReadJaggedIndexer += 1
                Loop
                Manager.Value(ReadIsMuxed).Dump(ReadPaddedLength + 32)
            Else
                If WaitForData(32) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                Read(ReadDataHeader, 0, 32)
                UnpackInt32(ReadDataHeader, ReadPaddedLength, 0)
                UnpackInt32(ReadDataHeader, ReadDataLength, 4)
                UnpackInt32(ReadDataHeader, ReadIsMuxed, 8)
                UnpackInt32(ReadDataHeader, ReadIsJagged, 12)
                UnpackInt32(ReadDataHeader, ReadBlockCount, 16)
                
                If ReadPaddedData.Length < ReadPaddedLength Then Redim ReadPaddedData(ReadPaddedLength - 1)
                If ReadBlockCount = 1 Then
                    If WaitForData(ReadPaddedLength) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                    Read(ReadPaddedData, 0, ReadPaddedLength)
                Else 
                    ReadLoopIndexer = 0
                    Do until ReadLoopIndexer >= ReadPaddedLength
                        If WaitForData(ReadBlockSize) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                        Read(ReadPaddedData, ReadLoopIndexer, ReadBlockSize)
                        ReadLoopIndexer += ReadBlockSize
                    Loop
                End If
                
                If ReadIsJagged = - 1 Then
                    If Manager.Contains(ReadIsMuxed) = True
                        Manager.Value(ReadIsMuxed).Write(ReadDataHeader, 32)
                        Manager.Value(ReadIsMuxed).Write(ReadPaddedData, ReadPaddedLength)
                    End If
                    Return
                End If

                If Index <> ReadIsMuxed Then
                    If Manager.Contains(ReadIsMuxed) = True
                        Manager.Value(ReadIsMuxed).Write(ReadDataHeader, 32)
                        Manager.Value(ReadIsMuxed).Write(ReadPaddedData, ReadPaddedLength)
                    End If
                    Return
                End If

                ReDim Output(ReadIsJagged - 1)
                ReadJaggedIndexer = 0
                ReadJaggedCounter = 0
                Do Until ReadJaggedIndexer > ReadIsJagged - 1
                    UnpackInt32(ReadPaddedData, ReadJaggedIndexLength, ReadJaggedCounter)
                    ReadJaggedCounter += 4
                    Redim Output(ReadJaggedIndexer)(ReadJaggedIndexLength - 1)
                    Buffer.BlockCopy(ReadPaddedData, ReadJaggedCounter, Output(ReadJaggedIndexer), 0,
                                     ReadJaggedIndexLength)
                    ReadJaggedCounter += ReadJaggedIndexLength
                    ReadJaggedIndexer += 1
                Loop
            End If
        End SyncLock
    End Sub

    Public Sub WriteJagged(Index as Int32, Byref Input as Byte()())
        If Manager.Contains(Index) = False Then Throw New ArgumentException("Parameter 'Index' referrs to a non-existent stream!")
        If Input Is Nothing Then Throw New ArgumentException("Parameter 'Input' must not be nothing!")
        SyncLock WriteLock
            WriteDataLength = input.Length * 4
            WriteJaggedIndexer = 0
            Do Until WriteJaggedIndexer > Input.Length - 1
                WriteDataLength += Input(WriteJaggedIndexer).Length
                WriteJaggedIndexer += 1
            Loop
            WritePaddedLength = GetNearestBlockSize(WriteDataLength)
            WriteBlockCount = Math.Ceiling(WritePaddedLength / WriteBlockSize)
            
            If WriteBlockCount > 1 Then
                WritePaddedLength = WriteBlockSize * WriteBlockCount
            End If
            
            
            If WritePaddedData.Length < WritePaddedLength Then Redim WritePaddedData(WritePaddedLength - 1)
            PackInt32(WritePaddedLength, WriteDataHeader, 0)
            PackInt32(WriteDataLength, WriteDataHeader, 4)
            PackInt32(Index, WriteDataHeader, 8)
            PackInt32(Input.Length, WriteDataHeader, 12)
            PackInt32(WriteBlockCount, WriteDataHeader, 16)
            WriteJaggedIndexer = 0
            WriteJaggedCounter = 0
            Do Until WriteJaggedIndexer > Input.Length - 1
                PackInt32(Input(WriteJaggedIndexer).Length, WritePaddedData, WriteJaggedCounter)
                WriteJaggedCounter += 4
                Buffer.BlockCopy(Input(WriteJaggedIndexer), 0, WritePaddedData, WriteJaggedCounter,
                                 Input(WriteJaggedIndexer).Length)
                WriteJaggedCounter += Input(WriteJaggedIndexer).Length
                WriteJaggedIndexer += 1
            Loop

            Write(WriteDataHeader, 0, 32)
            If WriteBlockCount = 1 Then
                Write(WritePaddedData, 0, WritePaddedLength)
            Else 
                WriteLoopIndexer = 0
                Do until WriteLoopIndexer >= WritePaddedLength
                    Write(WritePaddedData, WriteLoopIndexer, WriteBlockSize)
                    WriteLoopIndexer += WriteBlockSize
                Loop
            End If
        End SyncLock
    End Sub

    Public Function Available(Index as Int32) As Boolean
        If Manager.Contains(Index) = False Then _
            Throw New ArgumentException("Parameter 'Index' referrs to a non-existent stream!")
        SyncLock ReadLock
            If Manager.Value(Index).Length < 32
                If NetSocket.Available = 0 then Return False
                If WaitForData(32) = False Then Return False
                Read(SeekDataHeader, 0, 32)
                UnpackInt32(SeekDataHeader, SeekPaddedLength, 0)
                UnpackInt32(SeekDataHeader, SeekDataLength, 4)
                UnpackInt32(SeekDataHeader, SeekIsMuxed, 8)
                UnpackInt32(SeekDataHeader, SeekIsJagged, 12)
                UnpackInt32(SeekDataHeader, SeekBlockCount, 16)
                
                If SeekPaddedData.Length < SeekPaddedLength Then Redim SeekPaddedData(SeekPaddedLength - 1)
                If SeekBlockCount = 1 Then
                    If WaitForData(SeekPaddedLength) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                    Read(SeekPaddedData, 0, SeekPaddedLength)
                Else 
                    SeekLoopIndexer = 0
                    Do until SeekLoopIndexer >= SeekPaddedLength
                        If WaitForData(SeekBlockSize) = False Then Throw New TimeoutException("Timed out while waiting for data!")
                        Read(SeekPaddedData, SeekLoopIndexer, SeekBlockSize)
                        SeekLoopIndexer += SeekBlockSize
                    Loop
                End If

                If Manager.Contains(SeekIsMuxed) = True
                    Manager.Value(SeekIsMuxed).Write(SeekDataHeader, 32)
                    Manager.Value(SeekIsMuxed).Write(SeekPaddedData, SeekPaddedLength)
                End If
                Return (Index = SeekIsMuxed) And Manager.Contains(SeekIsMuxed)
            Else
                Manager.Value(Index).Read(SeekDataHeader, 32, 0)
                UnpackInt32(SeekDataHeader, SeekPaddedLength, 0)
                If Manager.Value(Index).Length < 32 + SeekPaddedLength Then Return False Else Return True 
            End If
        End SyncLock
    End Function

    Private Function WaitForData(Amount as Int32) as Boolean
        Dim WaitCounter = 0
        Dim WaitLimit As Double = (ReadTimeout * 30)/1000
        Dim WaitGovernor as new Governor(30)
        Do While True 
            Dim Available as Int32 = NetSocket.Available
            If WaitCounter = 0 Then
                If IsConnected = False Then
                    Return False
                End If
            End If
            
            If Available >= Amount Then
                Exit Do
            End If
            
            If WaitCounter >= WaitLimit Then
                Return False
            End If
            

            
            
            WaitCounter += 1
            WaitGovernor.Limit()
            
        Loop
        Return True
    End Function

    Private Function IsConnected as Boolean
        If Closing = True Then Return False
        If NetSocket is Nothing Then Return False
        If NetSocket.Connected = False Then Return False

        DataIsAvailable = NetSocket.Poll(0, SelectMode.SelectRead)
        DataIsNotAvailable = (NetSocket.Available = 0)

        If DataIsAvailable = False
            If DataIsNotAvailable = False
                Return True
            Else
                Return True
            End If
        Else
            If DataIsNotAvailable = False
                Return True
            Else
                Return False
            End If
        End If
    End Function

    Friend Sub Close
        If Closing = False Then
            Closing = True
            Dim AsyncThread as new Threading.Thread(
                sub()
                    Do Until Manager.Length = 0
                        Threading.Thread.Sleep(333)
                    Loop
                    
                    NetSocket.Close()
                    Dispose()
                    Closing = False
                End Sub)
            AsyncThread.Start()
        End If
    End Sub
    
    Public Sub CreateStream(Index as Int32)
        If Index < 0 then Throw New ArgumentException("Index must be non-negative!")
        SyncLock DictLock
            Manager.Add(Index, New QueueStream())
        End SyncLock
    End Sub

    Public Sub RemoveStream(Index as Int32)
        If Index < 0 then Throw New ArgumentException("Index must be non-negative!")
        SyncLock DictLock
            Manager.Remove(Index)
        End SyncLock
    End Sub

    Public Function StreamExists(Index as Int32) As Boolean
        If Index < 0 then Throw New ArgumentException("Index must be non-negative!")
        SyncLock DictLock
            Return Manager.Contains(Index)
        End SyncLock
    End Function
    
    Public Function GetNextFreeStream as Int32
        For x = 0 to Int32.MaxValue - 2
            If StreamExists(x) = False Then Return x
        Next
        Return -1
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If Disposed = False
            Try : LocalEncryptor.Dispose() : Catch : End Try
            Try : RemoteDecryptor.Dispose() : Catch : End Try
            Try : LocalCSP.Dispose() : Catch : End Try
            Try : RemoteCSP.Dispose() : Catch : End Try
            Disposed = True
        End If
    End Sub
End Class