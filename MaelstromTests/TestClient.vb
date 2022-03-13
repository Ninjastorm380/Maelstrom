Imports Maelstrom

Friend Class TestClient : Inherits ClientBase
    Dim P(3)() as Byte

    Protected Overrides Sub OnConnectionErrored(Socket As Socket, SError As SocketError)
        Console.WriteLine("DEBUG - CLIENT: client connection setup errored: " + SError.ToString())
    End Sub

    Protected Overrides Sub OnConnectionForged(Socket As Socket)

        Console.WriteLine("DEBUG - CLIENT: connected to server")

    End Sub

    Protected Overrides Sub OnConnectionMain(Socket As Socket)
        Dim Governor as new Governor(10)
        for x= 0 to P.Length -1 
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
        for x = 0 to 0
            CreateAsyncInstance(Socket,x)
        Next
        

        Do While Socket.Connected = True
            Governor.Limit()
        Loop
    End Sub

    Protected Overrides Sub OnConnectionReleased(Socket As Socket)
        Console.WriteLine("DEBUG - CLIENT: disconnected from server")  
    End Sub
    
    Private Sub CreateAsyncInstance(Socket as Socket, Index as Int32)
        Dim AsyncThread as new Threading.Thread(
            Sub()
                Dim Governor as new Governor(1)
                Socket.CreateStream(Index)
                Socket.WriteJagged(Index, P)
                Do While Socket.Connected = True
                    If Socket.Available(Index) = True
                        Dim Buffer as Byte()() = Nothing
                        Socket.ReadJagged(Index, Buffer)
                        If Buffer IsNot Nothing Then
                            Console.writeline("DEBUG - CLIENT: received ping through muxed connection " + Index.ToString() + ".")
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