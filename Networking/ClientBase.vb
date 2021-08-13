Namespace Bases
    Public MustInherit Class Client
        Private Client As TcpClient = Nothing
        Public Sub Connect(Endpoint As Net.IPEndPoint)
            If Client Is Nothing OrElse Client.Connected = False
                ClientConnecting()
                If Client IsNot Nothing Then Client.Dispose()
                Client = new TCPClient(Endpoint)
                Dim AsyncLaunch As New Threading.Thread(AddressOf AsyncLaunchHelper) : AsyncLaunch.Start()
                ClientConnected()
            End If
        End Sub
        Public Sub Disconnect()
            If Client IsNot Nothing AndAlso Client.Connected = True
                ClientDisconnecting
                Client.Close()
            End If
        End Sub
        Private Sub AsyncLaunchHelper()
            Main(Client)
            ClientDisconnected
        End Sub
        Protected Mustoverride Sub ClientConnecting()
        Protected Mustoverride Sub ClientDisconnecting()
        Protected Mustoverride Sub ClientConnected()
        Protected Mustoverride Sub ClientDisconnected()
        Protected Mustoverride Sub Main(Byval Connection As TCPClient)
    End Class
End Namespace
