Public Partial MustInherit Class BaseClient
    Public Sub New()
        BaseIdentificationToken = Handshake.GenerateToken()
    End Sub
    
    Public Sub Connect(Endpoint As Net.IPEndPoint)
        If Connected = False Then
            BaseSocket.Connect(Endpoint, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        End If
    End Sub
    
    Public Sub Disconnect()
        If Connected = True Then
            BaseSocket.Disconnect()
        End If
    End Sub
    
    Private Sub OnConnect(Socket As Lightning.Socket) Handles BaseSocket.OnConnect
        Dim Handshake As New Handshake(BaseIdentificationTokens.ToArray(), BaseIdentificationToken)
        Dim Result As Result = Handshake.Perform(Socket)
        Dim HandshakeTransform As Transform = Handshake.GetTransform
        SocketLookup.Add(Socket, New Socket(Socket, HandshakeTransform, Result, Handshake.GetLocalIdentificationToken(), Handshake.GetRemoteIdentificationToken()))
        
        If Result = Result.Ok Then
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnConnect, SocketLookup(Socket), "Maelstrom client OnConnect event thread")
        Else
            Socket.Disconnect()
        End If
    End Sub
    
    Private Sub OnDisconnect(Socket As Lightning.Socket) Handles BaseSocket.OnDisconnect
        If SocketLookup(Socket).SyncResult = Result.Ok
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnDisconnect, SocketLookup(Socket), "Maelstrom client OnDisconnect event thread")
        Else
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnDrop, SocketLookup(Socket), "Maelstrom client OnDrop event thread")
        End If
        SocketLookup(Socket).Dispose()
        SocketLookup.Remove(Socket)
        Socket.Dispose()
    End Sub

    Protected MustOverride Sub OnConnect(Socket As Socket)
    Protected MustOverride Sub OnDisconnect(Socket As Socket)
    Protected MustOverride Sub OnDrop(Socket As Socket)
    
    Public Sub AddTrustedToken(Token As Byte())
        If BaseIdentificationTokens.Contains(Token) = False Then BaseIdentificationTokens.Add(Token)
    End Sub
    Public Sub RemoveTrustedToken(Token As Byte())
        If BaseIdentificationTokens.Contains(Token) = True Then BaseIdentificationTokens.Remove(Token)
    End Sub
    Public Function ContainsTrustedToken(Token As Byte()) As Int32
        If BaseIdentificationTokens.Contains(Token) = False Then Return -1
        Return BaseIdentificationTokens.IndexOf(Token)
    End Function
End Class