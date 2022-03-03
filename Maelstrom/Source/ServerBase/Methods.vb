Public MustInherit Partial Class ServerBase
    Public Sub Start(Endpoint As Net.IPEndPoint)
        If Online = False Then
            Listener = New Net.Sockets.TcpListener(Endpoint)
            ListenerThread = New Threading.Thread(AddressOf ListenerMethod)
            Online = True
            Listener.Start()
            ListenerThread.Start()
        End If
    End Sub
    
    Public Sub [Stop]()
        Online = False
    End Sub
    
    Private Sub ListenerMethod()
        OnServerOnline()
        Dim Limiter As New Governor(10)
        Do While Online = True
            If Listener.Pending = True 
                Dim AsyncLaunch As New Threading.Thread(Sub()
                    Dim MSocket as new Socket
                    Dim Result as Int32 = MSocket.Init(Listener.AcceptSocket, True)
                    Select Case Result
                        Case = 0
                            OnConnectionForged(MSocket)
                            OnConnectionMain(MSocket)
                            OnConnectionReleased(MSocket)
                        Case = 1
                            OnConnectionErrored(MSocket, SocketError.HeaderMismatch)
                        Case = 2
                            OnConnectionErrored(MSocket, SocketError.VersionMismatch)
                    End Select
                End Sub) : AsyncLaunch.Start()
            End If
            Limiter.Limit()
        Loop
        Listener.Stop()
        OnServerOffline()
    End Sub
    
    Protected Mustoverride Sub OnConnectionErrored(Byval Socket as Socket, Byval SError as SocketError)
    Protected Mustoverride Sub OnConnectionForged(Byval Socket as Socket)
    Protected Mustoverride Sub OnConnectionMain(Byval Socket as Socket)
    Protected Mustoverride Sub OnConnectionReleased(Byval Socket as Socket)
    Protected Mustoverride Sub OnServerOnline()
    Protected Mustoverride Sub OnServerOffline()
End Class