Imports Maelstrom

Friend Class TestServer : Inherits ServerBase
    Dim P(3)() as Byte
    Protected Overrides Sub OnConnectionErrored(Socket As Socket, SError As SocketError)
        Console.WriteLine("DEBUG - SERVER: client connection setup errored: " + SError.ToString())
    End Sub

    Protected Overrides Sub OnConnectionForged(Socket As Socket)
        Console.WriteLine("DEBUG - SERVER: client has connected")
    End Sub

    Protected Overrides Sub OnConnectionMain(Socket As Socket)
        Dim Governor as new Governor(10)
        for x = 0 to 0
            CreateAsyncInstance(Socket,x)
        Next
        Do While Socket.Connected = True And Online = True
            Governor.Limit()
        Loop
    End Sub

    Protected Overrides Sub OnConnectionReleased(Socket As Socket)
        Console.WriteLine("DEBUG - SERVER: client has disconnected")
    End Sub

    Protected Overrides Sub OnServerOnline()

        for x= 0 to P.Length - 2
            P(x)= {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                   1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        Next
        P(P.Length - 1) = io.File.ReadAllBytes("./DefaultBioBackground.rgb")
        Console.WriteLine("DEBUG - SERVER: server is now online")
    End Sub

    Protected Overrides Sub OnServerOffline()
        Console.WriteLine("DEBUG - SERVER: server is now offline")
    End Sub
    
    Private Sub CreateAsyncInstance(Socket as Socket, Index as Int32)
        Dim AsyncThread as new Threading.Thread(
            Sub()
                Dim Governor as new Governor(1)
                Socket.CreateStream(Index)
                Socket.WriteJagged(Index, P)
                Do While Socket.Connected = True And Online = True
                    If Socket.Available(Index) = True 
                        Dim Buffer as Byte()() = Nothing
                        Socket.ReadJagged(Index, Buffer)
                        If Buffer IsNot Nothing Then
                            Console.writeline("DEBUG - SERVER: received ping through muxed connection " + Index.ToString() + ".")
                            Socket.WriteJagged(Index, P)
                        End If
                    End If
                    Governor.Limit()
                Loop
                Socket.RemoveStream(Index)
            End Sub)
        AsyncThread.Start()
    End Sub
End Class