Public Class TestClient : Inherits Networking.Bases.Client
    Private Client As Networking.TcpClient = Nothing
    Private IsReconnectingFlag As Boolean = False
    Public Sub TestAutoReconnect()
        Client.Close()
    End Sub
    Protected Friend Sub IsReconnecting() Handles Me.Reconnecting
        IsReconnectingFlag = True
        MsgBox("lost connection! attempting to reconnect...", MsgBoxStyle.OkOnly, "TestForm - test client")
    End Sub
    Public Sub TestCommand()
        If Client IsNot Nothing AndAlso Client.Connected = True Then
            MsgBox("sending test message...", MsgBoxStyle.OkOnly, "TestForm - test client")
            Client.WriteJagged({Text.Encoding.ASCII.GetBytes("test message")})
        End If
    End Sub
    Public Overrides Sub Run(Client As Networking.TcpClient)
        If IsReconnectingFlag = True Then
            IsReconnectingFlag = False
            MsgBox("successfully reconnected!", MsgBoxStyle.OkOnly, "TestForm - test client")
        End If
        Client.UseBufferedChannels = False
        Me.Client = Client
        Dim Limiter As New Networking.Governors.LoopGovernor(10)
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
