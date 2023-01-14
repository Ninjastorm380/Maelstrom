

Public Partial Class Socket : Implements IDisposable
    
    Private Sub AsyncReadMethod()
        Synclock ReceiveLock
            If NetSocket.Available >= 32 Then 
                If NetSocket.Read(HeaderReceiveTransformBuffer, 0, 32, Net.Sockets.SocketFlags.None) <> 32
                    NetSocket.Disconnect()
                    Exit Sub
                End If
                HeaderReceiveCryptographer.TransformBlock(HeaderReceiveTransformBuffer, HeaderReceiveTransformBuffer, 0, 32)
                ReceiveCompressedFlag = HeaderReceiveTransformBuffer(0)
                ReceiveEncryptedFlag = HeaderReceiveTransformBuffer(1)
                UnpackUInt32(HeaderReceiveTransformBuffer, 02, ReceiveSubsocket)
                UnpackUInt32(HeaderReceiveTransformBuffer, 06, ReceiveCompressedLength)
                UnpackUInt32(HeaderReceiveTransformBuffer, 10, ReceiveEncryptedLength)
                UnpackUInt32(HeaderReceiveTransformBuffer, 14, ReceiveRawLength)
                If ReceiveEncryptedFlag <> 0 And ReceiveCompressedFlag <> 0 Then
                    If ReceiveTransformBuffer.Length < ReceiveEncryptedLength Then ReDim ReceiveTransformBuffer(ReceiveEncryptedLength - 1)
                    If NetSocket.Read(ReceiveTransformBuffer, 0, ReceiveEncryptedLength, Net.Sockets.SocketFlags.None) <> ReceiveEncryptedLength
                        NetSocket.Disconnect()
                        Exit Sub
                    End If
                    ReceiveCryptographer.TransformBlock(ReceiveTransformBuffer,ReceiveTransformBuffer, 0, ReceiveEncryptedLength)
                    Compression.Decompress(ReceiveTransformBuffer, ReceiveTransformBuffer, ReceiveCompressedLength)
                ElseIf ReceiveEncryptedFlag <> 0 And ReceiveCompressedFlag = 0 Then
                    If ReceiveTransformBuffer.Length < ReceiveEncryptedLength Then ReDim ReceiveTransformBuffer(ReceiveEncryptedLength - 1)
                    If NetSocket.Read(ReceiveTransformBuffer, 0, ReceiveEncryptedLength, Net.Sockets.SocketFlags.None) <> ReceiveEncryptedLength
                        NetSocket.Disconnect()
                        Exit Sub
                    End If
                    ReceiveCryptographer.TransformBlock(ReceiveTransformBuffer,ReceiveTransformBuffer, 0, ReceiveEncryptedLength)
                ElseIf ReceiveEncryptedFlag = 0 And ReceiveCompressedFlag <> 0 Then
                    If ReceiveTransformBuffer.Length < ReceiveCompressedLength Then ReDim ReceiveTransformBuffer(ReceiveCompressedLength - 1)
                    If NetSocket.Read(ReceiveTransformBuffer, 0, ReceiveCompressedLength, Net.Sockets.SocketFlags.None) <> ReceiveCompressedLength
                        NetSocket.Disconnect()
                        Exit Sub
                    End If
                    Compression.Decompress(ReceiveTransformBuffer, ReceiveTransformBuffer, ReceiveCompressedLength)
                ElseIf ReceiveEncryptedFlag = 0 And ReceiveCompressedFlag = 0 Then
                    If ReceiveTransformBuffer.Length < ReceiveRawLength Then ReDim ReceiveTransformBuffer(ReceiveRawLength - 1)
                    If NetSocket.Read(ReceiveTransformBuffer, 0, ReceiveRawLength, Net.Sockets.SocketFlags.None) <> ReceiveRawLength
                        NetSocket.Disconnect()
                        Exit Sub
                    End If
                End If
                If Exists(ReceiveSubsocket) = True Then
                    SyncLock SubSocketBuffers(ReceiveSubsocket)
                        SubSocketBuffers(ReceiveSubsocket).Write(HeaderReceiveTransformBuffer, 0, 32)
                        SubSocketBuffers(ReceiveSubsocket).Write(ReceiveTransformBuffer, 0, ReceiveRawLength)
                    End SyncLock
                End If
            End If
        End SyncLock
    End Sub
    
    
    Public Sub Read(Byval Subsocket as UInt32, ByRef Data as Byte())
        If Data Is Nothing Then Throw New ArgumentException("Reference cannot be nothing!", "Data")
        If Exists(Subsocket) = False Then Throw New ArgumentException("The requested subsocket does not exist!", "Subsocket")
        AsyncReadMethod()
        SyncLock SubsocketBuffers(Subsocket)
            SubsocketBuffers(Subsocket).Read(HeaderReadBuffer, 0, 0, 32)
            UnpackUInt32(HeaderReadBuffer, 14, ReadRawLength)
            If Data.Length <> ReadRawLength Then ReDim Data(ReadRawLength - 1)
            SubsocketBuffers(Subsocket).Read(Data, 0, 32, ReadRawLength)
            SubsocketBuffers(Subsocket).Shift(32 + ReadRawLength)
        End SyncLock
    End Sub
        
    Public Sub Write(Byval Subsocket as UInt32, ByRef Data as Byte())
        If Data Is Nothing Then Throw New ArgumentException("Reference cannot be nothing!", "Data")
        If Exists(Subsocket) = False Then Throw New ArgumentException("The requested subsocket does not exist!", "Subsocket")
        SyncLock SendLock
            SendRawLength = Data.Length
            SendCompressedFlag = SubsocketCompressionFlags(Subsocket) 
            SendEncryptedFlag = SubsocketEncryptionFlags(Subsocket) 
            SendSubsocket = Subsocket
            SendCompressedLength = 0
            SendEncryptedLength = 0
                
            If SendTransformBuffer.Length < SendRawLength Then ReDim SendTransformBuffer(SendRawLength - 1)
            Buffer.BlockCopy(Data, 0, SendTransformBuffer, 0, SendRawLength)
            If SendEncryptedFlag <> 0 And SendCompressedFlag <> 0 Then
                SendCompressedLength = Compression.Compress(SendTransformBuffer, SendTransformBuffer, SendRawLength)
                SendEncryptedLength = Math.Ceiling(SendCompressedLength / AESBlockSize) * AESBlockSize
                SendCryptographer.TransformBlock(SendTransformBuffer, SendTransformBuffer, 0, SendEncryptedLength)
            ElseIf SendEncryptedFlag <> 0 And SendCompressedFlag = 0 Then
                SendEncryptedLength = Math.Ceiling(SendRawLength / AESBlockSize) * AESBlockSize
                
                SendCryptographer.TransformBlock(SendTransformBuffer, SendTransformBuffer, 0, SendEncryptedLength)
            ElseIf SendEncryptedFlag = 0 And SendCompressedFlag <> 0 Then
                SendCompressedLength = Compression.Compress(SendTransformBuffer, SendTransformBuffer, SendRawLength)
            End If
            
            HeaderSendTransformBuffer(0) = SendCompressedFlag
            HeaderSendTransformBuffer(1) = SendEncryptedFlag
            PackUInt32(HeaderSendTransformBuffer, 02, SendSubsocket)
            PackUInt32(HeaderSendTransformBuffer, 06, SendCompressedLength)
            PackUInt32(HeaderSendTransformBuffer, 10, SendEncryptedLength)
            PackUInt32(HeaderSendTransformBuffer, 14, SendRawLength)
            
            HeaderSendCryptographer.TransformBlock(HeaderSendTransformBuffer, HeaderSendTransformBuffer, 0, 32)
            NetSocket.Write(HeaderSendTransformBuffer, 0, 32, Net.Sockets.SocketFlags.None)
            If SendEncryptedFlag <> 0 And SendCompressedFlag <> 0 Then
                If SendEncryptedLength >= LargeWriteThreshold Then 
                    Dim AsyncRead As New Threading.Thread(AddressOf AsyncReadMethod)
                    AsyncRead.Start()
                End If
                NetSocket.Write(SendTransformBuffer, 0, SendEncryptedLength, Net.Sockets.SocketFlags.None)
            ElseIf SendEncryptedFlag <> 0 And SendCompressedFlag = 0 Then
                If SendEncryptedLength >= LargeWriteThreshold Then 
                    Dim AsyncRead As New Threading.Thread(AddressOf AsyncReadMethod)
                    AsyncRead.Start()
                End If
                NetSocket.Write(SendTransformBuffer, 0, SendEncryptedLength, Net.Sockets.SocketFlags.None)
            ElseIf SendEncryptedFlag = 0 And SendCompressedFlag <> 0 Then
                If SendCompressedLength >= LargeWriteThreshold Then
                    Dim AsyncRead As New Threading.Thread(AddressOf AsyncReadMethod)
                    AsyncRead.Start()
                End If
                NetSocket.Write(SendTransformBuffer, 0, SendCompressedLength, Net.Sockets.SocketFlags.None)
            ElseIf SendEncryptedFlag = 0 And SendCompressedFlag = 0 Then
                If SendRawLength >= LargeWriteThreshold Then
                    Dim AsyncRead As New Threading.Thread(AddressOf AsyncReadMethod)
                    AsyncRead.Start()
                End If
                NetSocket.Write(SendTransformBuffer, 0, SendRawLength, Net.Sockets.SocketFlags.None)
            End If
        End SyncLock
    End Sub
        
    Public Sub Add(Byval Subsocket as UInt32)
        SyncLock BufferLock
            SubsocketBuffers.Add(Subsocket, New QueueStream(Of Byte))
            SubsocketEncryptionFlags.Add(Subsocket, 0)
            SubsocketCompressionFlags.Add(Subsocket, 0)
        End SyncLock
    End Sub
        
    Public Sub Remove(Byval Subsocket as UInt32)
        SyncLock BufferLock
            SubsocketBuffers.Remove(Subsocket)
            SubsocketEncryptionFlags.Remove(Subsocket)
            SubsocketCompressionFlags.Remove(Subsocket)
        End SyncLock
    End Sub
        
    Public Function Exists(Byval Subsocket as UInt32) As Boolean
        Return SubsocketBuffers.Contains(Subsocket)
    End Function
        
    Public Sub Disconnect()
        SyncLock ReceiveLock
            SyncLock SendLock
                NetSocket.Disconnect()
            End SyncLock
        End SyncLock
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock ReceiveLock
            SyncLock SendLock
                NetSocket.Dispose()
            End SyncLock
        End SyncLock
    End Sub
End Class
