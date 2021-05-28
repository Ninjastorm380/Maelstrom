Public Class TestClient : Inherits Networking.ClientBase
    Private Client As Networking.TcpClient = Nothing
    Public Sub TestCommand()
        If Client IsNot Nothing AndAlso Client.Connected = True Then
            Client.WriteJagged({Text.Encoding.ASCII.GetBytes("test message")})
        End If
    End Sub
    Public Overrides Sub Run(Client As Networking.TcpClient)
        Me.Client = Client
        Dim Limiter As New Networking.ThreadLimiter(10)
        Do While Client.Connected = True
            If Client.HasMessage = True Then
                Dim ReceivedData As Byte()() = Nothing
                Client.ReadJagged(ReceivedData)
                Dim ReceivedMessage As String = Text.Encoding.ASCII.GetString(ReceivedData(0))
                Select Case ReceivedMessage
                    Case "test response"
                        Debug.Print("received test message!")
                End Select
            End If
            Limiter.Limit()
        Loop
    End Sub
End Class
