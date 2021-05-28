Public MustInherit Class ClientBase
    Private Client As TcpClient
    Private PurposeClosed As Boolean = False
    Private Endpoint As Net.IPEndPoint

    Public Sub Connect(Endpoint As Net.IPEndPoint)
        Me.Endpoint = Endpoint
        PurposeClosed = False
        If Client Is Nothing Then
            Client = New TcpClient(Endpoint)
            Dim AsyncLaunch As New Threading.Thread(AddressOf Prerun) : AsyncLaunch.Start()
        Else
            If Client.Connected = False Then
                Client.Dispose()
                Client = New TcpClient(Endpoint)
                Dim AsyncLaunch As New Threading.Thread(AddressOf Prerun) : AsyncLaunch.Start()
            End If
        End If
    End Sub
    Public Sub Disconnect()
        PurposeClosed = True
        Client.Close()
        Endpoint = Nothing
        Client.Dispose()
        Client = Nothing
    End Sub
    Private Sub Prerun()
        Run(Client)
        If PurposeClosed = False Then Connect(Endpoint)
    End Sub

    Public MustOverride Sub Run(ByVal Client As TCPClient)
End Class
Public MustInherit Class ServerBase
    Private Listener As Net.Sockets.TcpListener
    Private OnlineBase As Boolean = False
    Private HoldingTMP As TcpClient
    Public Event ServerStarting As EventHandler
    Public Event ServerStopping As EventHandler
    Public ReadOnly Property Online As Boolean
        Get
            Return OnlineBase
        End Get
    End Property
    Public Sub Start(Endpoint As Net.IPEndPoint)
        SyncLock Me
            If Online = False Then
                Listener = New Net.Sockets.TcpListener(Endpoint)
                Listener.Server.SetSocketOption(Net.Sockets.SocketOptionLevel.IPv6, Net.Sockets.SocketOptionName.IPv6Only, False)
                Dim ListenerThread As New Threading.Thread(AddressOf ListenerMethod)
                RaiseEvent ServerStarting(Me, New EventArgs)
                Listener.Start()
                ListenerThread.Start()
                OnlineBase = True
            End If
        End SyncLock
    End Sub
    Public Sub [Stop]()
        SyncLock Me
            If Online = True Then
                RaiseEvent ServerStopping(Me, New EventArgs)
                Listener.Stop()
                Listener.Server.Dispose()
                OnlineBase = False
            End If
        End SyncLock
    End Sub

    Private Sub ListenerMethod()
        Dim Limiter As New ThreadLimiter(50)
        Do While Online = True
            SyncLock Me
                If Online = False Then Exit Do
                If Listener.Pending = True Then
                    HoldingTMP = New TcpClient(Listener.AcceptSocket)
                    Dim AsyncLaunch As New Threading.Thread(AddressOf Prerun) : AsyncLaunch.Start()
                End If
            End SyncLock
            Limiter.Limit()
        Loop
    End Sub
    Private Sub Prerun()
        Dim Client As TcpClient = HoldingTMP
        HoldingTMP = Nothing
        Run(Client)
        Client = Nothing
    End Sub
    Public MustOverride Sub Run(ByVal Client As TcpClient)
End Class