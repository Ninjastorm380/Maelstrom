Public Partial MustInherit Class BaseServer
    Public Sub Listen(Endpoint As Net.IPEndPoint)
        BaseSocket.Listen(Endpoint, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
    End Sub
    
    Public Sub Deafen()
        BaseSocket.Deafen()
    End Sub
    
    Private Sub OnConnect(Socket As Lightning.Socket) Handles BaseSocket.OnConnect
        With New Handshake : If .Perform(Socket) = Result.Ok Then
            #IF DEBUG 
            Console.WriteLine("  Server:   launching server-side code...")
            #END IF
            Dim HandshakeTransform As Transform = .GetTransform
            SocketLookup.Add(Socket, New Socket(Socket, HandshakeTransform))
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnConnect, SocketLookup(Socket), "Maelstrom server OnConnect client event thread")
        Else
            #IF DEBUG 
            Console.WriteLine("  Server:   dropping unknown client...")
            #END IF
            Socket.Disconnect()
        End If : End With

    End Sub
    
    Private Sub OnDisconnect(Socket As Lightning.Socket) Handles BaseSocket.OnDisconnect
        If SocketLookup.ContainsKey(Socket) = True Then
            Lightning.LambdaThread(Of Socket).Start(AddressOf OnDisconnect, SocketLookup(Socket), "Maelstrom server OnDisconnect client event thread")
            SocketLookup(Socket).Dispose()
            SocketLookup.Remove(Socket)
            Socket.Dispose()
        Else
            Lightning.LambdaThread(Of Net.IPEndPoint).Start(AddressOf OnDrop, Socket.RemoteEndpoint, "Maelstrom server OnDrop client event thread")
            Socket.Dispose()
        End If
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
    Protected MustOverride Sub OnDrop(Endpoint As Net.IPEndPoint)
    
    
End Class