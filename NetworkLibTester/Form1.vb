Public Class Form1
    Private WithEvents TestServer1 As New TestServer
    Private WithEvents TestClient1 As New TestClient
    Private LoopBackV6 As Net.IPEndPoint = New Net.IPEndPoint(Net.IPAddress.IPv6Loopback, 12700)
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        TestServer1.Start(LoopBackV6)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        TestServer1.Stop()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        TestClient1.Connect(LoopBackV6)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        TestClient1.Disconnect()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        TestClient1.TestCommand()
    End Sub
End Class
