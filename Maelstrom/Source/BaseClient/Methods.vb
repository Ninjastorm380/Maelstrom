Public Partial MustInherit Class BaseClient
    Public Sub Connect(Endpoint As Net.IPEndPoint)
        BaseSocket.Connect(Endpoint, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
    End Sub
    
    Public Sub Disconnect()
        BaseSocket.Disconnect()
    End Sub
    
    Private Sub OnConnect(Socket As Lightning.Socket) Handles BaseSocket.OnConnect
        Dim Handshake As New Handshake
        If Handshake.Perform(Socket) = Result.Ok Then
            #IF DEBUG 
            Console.WriteLine("  Client:   launching client-side code...")
            #END IF
            Dim HandshakeTransform As Transform = Handshake.GetTransform
            SocketLookup.Add(Socket, New Socket(Socket, HandshakeTransform))
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnConnect, SocketLookup(Socket), "Maelstrom client connected event thread")
        Else
            #IF DEBUG 
            Console.WriteLine("  Client:   disconnecting from unknown server...")
            #END IF
            Socket.Disconnect()
        End If
    End Sub
    
    Private Sub OnDisconnect(Socket As Lightning.Socket) Handles BaseSocket.OnDisconnect
        If SocketLookup.ContainsKey(Socket) = True Then
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnDisconnect, SocketLookup(Socket), "Maelstrom client disconnected event thread")
            SocketLookup(Socket).Dispose()
            SocketLookup.Remove(Socket)
        Else
            Lightning.LambdaThread (Of Net.IPEndPoint).Start(AddressOf OnDrop, Socket.RemoteEndpoint, "Maelstrom client OnDrop event thread")
        End If
    End Sub

    Protected MustOverride Sub OnConnect(Socket As Socket)
    Protected MustOverride Sub OnDisconnect(Socket As Socket)
    Protected MustOverride Sub OnDrop(Endpoint As Net.IPEndPoint)
End Class