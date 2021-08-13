Namespace Bases
    Public MustInherit Class Server
        Private Listener As Net.Sockets.TcpListener
        Private ListenerThread As Threading.Thread
        Private OnlineBase As Boolean = False
        Private TCPTemp As TCPClient
        Protected ReadOnly Property Online As Boolean
            Get
                Return OnlineBase
            End Get
        End Property
        Public Sub Start(Endpoint As Net.IPEndPoint)
            If Online = False Then
                ServerStarting()
                Listener = New Net.Sockets.TcpListener(Endpoint)
                'Listener.Server.SetSocketOption(Net.Sockets.SocketOptionLevel.IPv6, Net.Sockets.SocketOptionName.IPv6Only, False)
                ListenerThread = New Threading.Thread(AddressOf ListenerMethod)
                OnlineBase = True
                Listener.Start()
                ListenerThread.Start()
                ServerStarted()
            End If
        End Sub
        Public Sub [Stop]()
            If Online = True Then
                ServerStopping()
                Listener.Stop()
                OnlineBase = False
                Listener.Server.Dispose()
                ListenerThread = Nothing
                ServerStopped()
            End If
        End Sub
        Private Sub ListenerMethod()
            Dim Limiter As New Governors.LoopGovernor(20)
            Do While Online = True
                If Listener.Pending = True
                  TCPTemp = New TcpClient(Listener.AcceptSocket)
                    Dim AsyncLaunch As New Threading.Thread(AddressOf AsyncLaunchHelper) : AsyncLaunch.Start()
                End If
                Limiter.Limit()
            Loop
        End Sub
        Private Sub AsyncLaunchHelper()
            Dim Local As TCPClient = TCPTemp
            ClientConnected(Local)
            Main(Local)
            If Local IsNot Nothing AndAlso Local.Connected = True Then Local.Close()
            ClientDisconnected(Local)
        End Sub
        Protected Mustoverride Sub ServerStarting()
        Protected Mustoverride Sub ServerStopping()
        Protected Mustoverride Sub ServerStarted()
        Protected Mustoverride Sub ServerStopped()
        Protected Mustoverride Sub ClientConnected(Connection As TCPClient)
        Protected Mustoverride Sub ClientDisconnected(Connection As TCPClient)
        Protected Mustoverride Sub Main(Byval Connection As TCPClient)
    End Class
End Namespace
