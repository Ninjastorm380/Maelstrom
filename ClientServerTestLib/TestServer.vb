Imports Networking

Public Class TestServer
    Inherits Bases.Server

    Protected Overrides Sub ServerStarting()
        Console.WriteLine("Server: starting...")
    End Sub

    Protected Overrides Sub ServerStopping()
        Console.WriteLine("Server: stopping...")
    End Sub

    Protected Overrides Sub ServerStarted()
        Console.WriteLine("Server: started")
    End Sub

    Protected Overrides Sub ServerStopped()
        Console.WriteLine("Server: stopped")
    End Sub

    Protected Overrides Sub ClientConnected(Connection As TCPClient)
        Console.WriteLine("Server: client connected to server")
    End Sub

    Protected Overrides Sub ClientDisconnected(Connection As TCPClient)
        Console.WriteLine("Server: client disconnected from server")
    End Sub

    Protected Overrides Sub Main(Connection As TCPClient)
        Dim Governor As New Governors.LoopGovernor(25)
        Connection.AddChannel(0)
        Dim IsRunningIntegrityTest As Boolean = False
        Do While Connection.Connected = True And Online = True
            Governor.LoopsPerSecond = Connection.AutoCurrentDynamicSpeed
            If Connection.Available(0) > 0 Then
                Dim Input As Byte()() = Nothing
                Connection.ReadJagged(Input,0)
                Dim Command As String = Text.Encoding.ASCII.GetString(Input(0))
                Select Case Command 
                    Case "server.commands.test"
                        Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.commands.test.return"),BitConverter.GetBytes(0)},0)
                    Case "server.integrity.test.begin"
                        If IsRunningIntegrityTest = False Then
                            IsRunningIntegrityTest = true
                            Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.ping")},0)

                            Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.begin.return"),BitConverter.GetBytes(0)},0)
                        Else 
                            Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.begin.return"),BitConverter.GetBytes(1)},0)
                        End If
                       
                    Case "server.integrity.test.end"
                        If IsRunningIntegrityTest = true Then
                            IsRunningIntegrityTest = false
                            Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.end.return"),BitConverter.GetBytes(0)},0)
                        Else 
                            Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.end.return"),BitConverter.GetBytes(1)},0)
                        End If
                    Case "server.integrity.test.pong"
                        
                        If IsRunningIntegrityTest = True Then
                            Console.WriteLine("Server: received client pong, and continue is true. pinging...")
                            Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.ping")},0)
                        Else 
                            Console.WriteLine("Server: received client pong, and continue is false. stopping test.") 
                        End If
                End Select
            End If
            Governor.Limit()
        Loop
        Connection.RemoveChannel(0)
    End Sub
End Class