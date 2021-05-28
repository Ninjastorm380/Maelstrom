Public Class TestServer : Inherits Networking.ServerBase

    Public Overrides Sub Run(Client As Networking.TCPClient)
        Dim Limiter As New Networking.ThreadLimiter(60)
        Do While Client.Connected = True
            If Client.HasMessage = True Then
                Dim ReceivedData As Byte()() = Nothing
                Client.ReadJagged(ReceivedData)
                Dim ReceivedMessage As String = Text.Encoding.ASCII.GetString(ReceivedData(0))
                Select Case ReceivedMessage
                    Case "test message"
                        Client.WriteJagged({Text.Encoding.ASCII.GetBytes("test response")})
                End Select
            End If
            Limiter.Limit()
        Loop
        Debug.Print("connection to server closed properly!")
    End Sub
End Class
