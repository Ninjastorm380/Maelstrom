Imports Networking
Imports Networking.Bases
Imports Networking.Governors

Public Class TestClient
    inherits Client
    
    Dim CurrentTCPClient As TCPClient = Nothing
    
    
    Private ReadOnly CommandTestLock As New LockGovernor(of Byte()())
    Public Function CommandTest As Integer
        If CurrentTCPClient IsNot Nothing AndAlso CurrentTCPClient.Connected = True
            CurrentTCPClient.WriteJagged({Text.Encoding.ASCII.GetBytes("server.commands.test")},0)
            Dim Result As LockGovernor(of Byte()()).LockResult = CommandTestLock.Lock()
            Return BitConverter.ToInt32(Result.ReturnedObject(1),0)
        Else
            Return -1
        End If
    End Function

    Private ReadOnly IntegrityTestBeginLock As New LockGovernor(of Byte()())
    Public Function IntegrityTestBegin As Integer
        If CurrentTCPClient IsNot Nothing AndAlso CurrentTCPClient.Connected = True
            CurrentTCPClient.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.begin")},0)
            Dim Result As LockGovernor(of Byte()()).LockResult = IntegrityTestBeginLock.Lock()
            Return BitConverter.ToInt32(Result.ReturnedObject(1),0)
        Else
            Return -1
        End If
    End Function
    
    Private ReadOnly IntegrityTestEndLock As New LockGovernor(of Byte()())
    Public Function IntegrityTestEnd As Integer
        If CurrentTCPClient IsNot Nothing AndAlso CurrentTCPClient.Connected = True
            CurrentTCPClient.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.end")},0)
            Dim Result As LockGovernor(of Byte()()).LockResult = IntegrityTestEndLock.Lock()
            Return BitConverter.ToInt32(Result.ReturnedObject(1),0)
        Else
            Return -1
        End If
    End Function
    
    Protected Overrides Sub ClientConnecting()
        Console.WriteLine("Client: connecting to server...")
    End Sub

    Protected Overrides Sub ClientDisconnecting()
        Console.WriteLine("Client: disconnecting from server...")
    End Sub

    Protected Overrides Sub ClientConnected()
        Console.WriteLine("Client: connected to server")
    End Sub

    Protected Overrides Sub ClientDisconnected()
        Console.WriteLine("Client: disconnected from server")
    End Sub

    Protected Overrides Sub Main(Connection As TCPClient)
        Dim Governor As New LoopGovernor(25)
        CurrentTCPClient = Connection
        Connection.AddChannel(0)
        Do While Connection.Connected = True
            Governor.LoopsPerSecond = Connection.AutoCurrentDynamicSpeed
            If Connection.Available(0) > 0 Then
                Dim Input As Byte()() = Nothing
                Connection.ReadJagged(Input,0)
                Dim Command As String = Text.Encoding.ASCII.GetString(Input(0))
                Select Case Command 
                    Case "server.commands.test.return"
                        CommandTestLock.Unlock(Input)
                    Case "server.integrity.test.begin.return"
                        IntegrityTestBeginLock.Unlock(Input)
                    Case "server.integrity.test.end.return"
                        IntegrityTestEndLock.Unlock(Input)
                    Case "server.integrity.test.ping"
                        Console.WriteLine("Client: received server ping. ponging...")
                        Connection.WriteJagged({Text.Encoding.ASCII.GetBytes("server.integrity.test.pong")},0)
                End Select
            End If
            Governor.Limit()
        Loop
        Connection.RemoveChannel(0)
    End Sub
End Class