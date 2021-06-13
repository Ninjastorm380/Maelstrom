Public Class TestClient : Inherits Networking.ClientBase
    Private Client As Networking.TcpClient = Nothing
    Public Sub TestAutoReconnect()
        Client.Close()
    End Sub
    Protected Friend Sub IsReconnecting() Handles Me.Reconnecting
        MsgBox("lost connection, but we reconnected successfully!", MsgBoxStyle.OkOnly, "TestForm - test client")
    End Sub
    Public Sub TestCommand()
        If Client IsNot Nothing AndAlso Client.Connected = True Then
            MsgBox("sending test message...", MsgBoxStyle.OkOnly, "TestForm - test client")
            Client.WriteJagged({Text.Encoding.ASCII.GetBytes("test message")})
        End If
    End Sub
    Public Overrides Sub Run(Client As Networking.TcpClient)
        Client.UseBufferedChannels = False
        Me.Client = Client
        Dim Limiter As New Networking.ThreadLimiter(10)
        Do While Client.Connected = True
            If Client.HasMessage = True Then
                Dim ReceivedData As Byte()() = Nothing
                Client.ReadJagged(ReceivedData)
                Dim ReceivedMessage As String = Text.Encoding.ASCII.GetString(ReceivedData(0))
                Select Case ReceivedMessage
                    Case "test response"
                        MsgBox("client received test response!", MsgBoxStyle.OkOnly, "TestForm - test client")
                End Select
            End If
            Limiter.Limit()
        Loop
    End Sub
End Class
