Public MustInherit Class ClientBase
    Private Client As TCPClient

    Public Sub Start(Endpoint As Net.IPEndPoint)
        If Client Is Nothing Then
            Client = New TCPClient(Endpoint)
            Dim AsyncLaunch As New Threading.Thread(AddressOf Prerun) : AsyncLaunch.Start()
        Else
            If Client.Connected = False Then
                Client.Dispose()
                Client = New TCPClient(Endpoint)
                Dim AsyncLaunch As New Threading.Thread(AddressOf Prerun) : AsyncLaunch.Start()
            End If
        End If
    End Sub
    Public Sub [Stop]()
        Client.Close()
    End Sub
    Private Sub Prerun()
        Run(Client)
    End Sub
    Public MustOverride Sub Run(ByVal Client As TCPClient)
End Class
Public MustInherit Class ServerBase
    Private Listener As Net.Sockets.TcpListener
    Private Online As Boolean = False
    Private HoldingTMP As TCPClient
    Public Event ServerStarting As EventHandler
    Public Event ServerStopping As EventHandler
    Public Sub Start(Endpoint As Net.IPEndPoint)
        SyncLock Me
            If Online = False Then
                Listener = New Net.Sockets.TcpListener(Endpoint)
                Listener.Server.SetSocketOption(Net.Sockets.SocketOptionLevel.IPv6, Net.Sockets.SocketOptionName.IPv6Only, False)
                Dim ListenerThread As New Threading.Thread(AddressOf ListenerMethod)
                RaiseEvent ServerStarting(Me, New EventArgs)
                Listener.Start()
                ListenerThread.Start()
                Online = True
            End If
        End SyncLock
    End Sub
    Public Sub [Stop]()
        SyncLock Me
            If Online = True Then
                RaiseEvent ServerStopping(Me, New EventArgs)
                Listener.Stop()
                Online = False
            End If
        End SyncLock
    End Sub

    Private Sub ListenerMethod()
        Dim Limiter As New ThreadLimiter(50)
        Do While Online = True
            SyncLock Me
                If Online = False Then Exit Do
                If Listener.Pending = True Then
                    HoldingTMP = New TCPClient(Listener.AcceptSocket)
                    Dim AsyncLaunch As New Threading.Thread(AddressOf Prerun) : AsyncLaunch.Start()
                End If
            End SyncLock
            Limiter.Limit()
        Loop
    End Sub
    Private Sub Prerun()
        Dim Client As TCPClient = HoldingTMP
        Run(Client)
    End Sub
    Public MustOverride Sub Run(ByVal Client As TCPClient)
End Class