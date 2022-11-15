Public Partial MustInherit Class ServerBase
    Public Sub New
        BaseSocket = New BaseSocket
    End Sub
    
    Public Sub Listen(Endpoint As Net.IPEndPoint)
        If BaseSocket.Listening = False Then
            BaseSocket.Listen(Endpoint)
        End If
    End Sub
    
    Public Sub Deafen()
        If BaseSocket.Listening = True Then
            BaseSocket.Deafen
        End If
    End Sub
    
    Private Sub SocketConnected(NewSocket As BaseSocket) Handles BaseSocket.SocketConnected
        Dim AsyncThread As New Threading.Thread(AddressOf Main)
        
        Dim WrapperSocket As New Socket
        
        Dim TempUTCTimestamp as DateTime
        Dim TempVRNG As VariableRNG
        Dim TempRetryCounter As Byte = 0
        Dim TempRetryResult As Byte = 3
        Dim TempRetryMax As Byte = 10
        
        Dim HandshakeSendBuffer(79) As Byte
        Dim HandshakeReceiveBuffer(79) As Byte
        Dim SendTimestamp as DateTime
        Dim ReceiveTimestamp as DateTime
        Dim SendVRNG As VariableRNG
        Dim ReceiveVRNG As VariableRNG
        Dim OpposingPortA As UInt32
        Dim OpposingPortB As UInt32
        Do
            TempRetryCounter += 1
            WrapperSocket.NetSocket = NewSocket
            TempUTCTimestamp = DateTime.UtcNow()
            TempVRNG = New VariableRNG({TempUTCTimestamp.Year,
                                        TempUTCTimestamp.Month,
                                        TempUTCTimestamp.Day,
                                        TempUTCTimestamp.Hour,
                                        TempUTCTimestamp.Minute,
                                        TempUTCTimestamp.Second,
                                        Math.Floor(TempUTCTimestamp.Millisecond/200)})
            WrapperSocket.ReceiveCryptographer = New Cryptographer(TempVRNG, Cryptographer.Type.Decryption)
            WrapperSocket.SendCryptographer = New Cryptographer(TempVRNG, Cryptographer.Type.Encryption)
            SendTimestamp = DateTime.Now
            Buffer.BlockCopy(MaelstromHeaderRefrence, 0, HandshakeSendBuffer, 0, 32)
            PackUInt32(HandshakeSendBuffer, 32, WrapperSocket.NetSocket.RemoteEndpoint.Port)
            PackUInt32(HandshakeSendBuffer, 36, WrapperSocket.NetSocket.LocalEndpoint.Port)
            PackTimestamp(HandshakeSendBuffer, 40, SendTimestamp)
            WrapperSocket.SendCryptographer.TransformBlock(HandshakeSendBuffer, HandshakeSendBuffer, 0, 80)
            WrapperSocket.NetSocket.Write(HandshakeSendBuffer, 0, 80, Net.Sockets.SocketFlags.None)
            WrapperSocket.NetSocket.Read(HandshakeReceiveBuffer, 0, 80, Net.Sockets.SocketFlags.None)
            WrapperSocket.ReceiveCryptographer.TransformBlock(HandshakeReceiveBuffer, HandshakeReceiveBuffer, 0, 80)
            If BinaryCompare(MaelstromHeaderRefrence, HandshakeReceiveBuffer, 0, 16) = False Then 
                TempRetryResult = 2
                Threading.Thread.Sleep(210)
            End If
            If BinaryCompare(MaelstromHeaderRefrence, HandshakeReceiveBuffer, 0, 16) = True And BinaryCompare(MaelstromHeaderRefrence, HandshakeReceiveBuffer, 16, 16) = False Then
                TempRetryResult = 1
                Threading.Thread.Sleep(10)
            End If
            If BinaryCompare(MaelstromHeaderRefrence, HandshakeReceiveBuffer, 0, 16) = True And BinaryCompare(MaelstromHeaderRefrence, HandshakeReceiveBuffer, 16, 16) = True Then
                TempRetryResult = 0
            End If
            WrapperSocket.SendCryptographer.Dispose()
            WrapperSocket.ReceiveCryptographer.Dispose()
            
        Loop Until TempRetryCounter >= TempRetryMax Or TempRetryResult = 0 Or WrapperSocket.NetSocket.Connected = False
        
        
        
        If TempRetryResult = 0 Then
            UnpackUInt32(HandshakeReceiveBuffer, 32, OpposingPortB)
            UnpackUInt32(HandshakeReceiveBuffer, 36, OpposingPortA)
            UnpackTimestamp(HandshakeReceiveBuffer,40, ReceiveTimestamp)
            SendVRNG = New VariableRNG({SendTimestamp.Year,
                                        SendTimestamp.Month,
                                        SendTimestamp.Day,
                                        SendTimestamp.Hour,
                                        SendTimestamp.Minute,
                                        SendTimestamp.Second,
                                        SendTimestamp.Millisecond,
                                        WrapperSocket.NetSocket.RemoteEndpoint.Port,
                                        WrapperSocket.NetSocket.LocalEndpoint.Port})
            ReceiveVRNG = New VariableRNG({ReceiveTimestamp.Year,
                                           ReceiveTimestamp.Month,
                                           ReceiveTimestamp.Day,
                                           ReceiveTimestamp.Hour,
                                           ReceiveTimestamp.Minute,
                                           ReceiveTimestamp.Second,
                                           ReceiveTimestamp.Millisecond,
                                           OpposingPortB,
                                           OpposingPortA})
            WrapperSocket.HeaderSendCryptographer = New Cryptographer(SendVRNG, Cryptographer.Type.Encryption)
            WrapperSocket.SendCryptographer = New Cryptographer(SendVRNG, Cryptographer.Type.Encryption)
            WrapperSocket.HeaderReceiveCryptographer = New Cryptographer(ReceiveVRNG, Cryptographer.Type.Decryption)
            WrapperSocket.ReceiveCryptographer = New Cryptographer(ReceiveVRNG, Cryptographer.Type.Decryption)
            AsyncThread.Start(WrapperSocket)
        Else
            WrapperSocket.NetSocket.Disconnect()
        End If
    End Sub
    
    Public MustOverride Sub Main(Socket As Socket)
End Class