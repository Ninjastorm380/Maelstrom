Public Partial MustInherit Class BaseServer
    Public Sub New()
        BaseIdentificationToken = Handshake.GenerateToken()
    End Sub
    Public Sub Listen(Endpoint As Net.IPEndPoint)
        If Listening = False Then
            BaseSocket.Listen(Endpoint, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        End If

    End Sub
    
    Public Sub Deafen()
        If Listening = True Then
            BaseSocket.Deafen()
        End If
    End Sub
    
    Private Sub OnConnect(Socket As Lightning.Socket) Handles BaseSocket.OnConnect
        Dim Handshake As New Handshake(BaseIdentificationTokens.ToArray(), BaseIdentificationToken)
        Dim Result As Result = Handshake.Perform(Socket)
        Dim HandshakeTransform As Transform = Handshake.GetTransform
        SocketLookup.Add(Socket, New Socket(Socket, HandshakeTransform, Result, Handshake.GetLocalIdentificationToken(), Handshake.GetRemoteIdentificationToken()))
        
        If Result = Result.Ok Then
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnConnect, SocketLookup(Socket), "Maelstrom server OnConnect client event thread")
        Else
            Socket.Disconnect()
        End If
    End Sub
    
    Private Sub OnDisconnect(Socket As Lightning.Socket) Handles BaseSocket.OnDisconnect
        If SocketLookup(Socket).SyncResult = Result.Ok
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnDisconnect, SocketLookup(Socket), "Maelstrom server OnDisconnect client event thread")
        Else
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnDrop, SocketLookup(Socket), "Maelstrom server OnDrop client event thread")
        End If
        SocketLookup(Socket).Dispose()
        SocketLookup.Remove(Socket)
        Socket.Dispose()
    End Sub
    
    Private Sub OnListen(Socket As Lightning.Socket) Handles BaseSocket.OnListen
        Lightning.LambdaThread.Start(AddressOf OnListen, "Maelstrom server OnListen event thread")
    End Sub
    
    Private Sub OnDeafen(Socket As Lightning.Socket) Handles BaseSocket.OnDeafen
        Lightning.LambdaThread.Start(AddressOf OnDeafen, "Maelstrom server OnDeafen event thread")
    End Sub

    Protected MustOverride Sub OnConnect(Socket As Socket)
    Protected MustOverride Sub OnDisconnect(Socket As Socket)
    Protected MustOverride Sub OnListen()
    Protected MustOverride Sub OnDeafen()
    Protected MustOverride Sub OnDrop(Socket As Socket)
    
    Public Sub AddBlockedToken(Token As Byte())
        If BaseIdentificationTokens.Contains(Token) = False Then BaseIdentificationTokens.Add(Token)
    End Sub
    Public Sub RemoveBlockedToken(Token As Byte())
        If BaseIdentificationTokens.Contains(Token) = True Then BaseIdentificationTokens.Remove(Token)
    End Sub
    Public Function ContainsBlockedToken(Token As Byte()) As Int32
        If BaseIdentificationTokens.Contains(Token) = False Then Return -1
        Return BaseIdentificationTokens.IndexOf(Token)
    End Function
End Class