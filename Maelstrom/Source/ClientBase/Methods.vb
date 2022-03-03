Public MustInherit Partial Class ClientBase
    Public Sub Connect(Endpoint As Net.IPEndPoint)
        If MSocket Is Nothing Then MSocket = New Socket()
        If MSocket.Connected = False
            BSocket = new Net.Sockets.Socket(Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
            BSocket.Connect(Endpoint)
            Dim AsyncThread as New Threading.Thread(
                Sub()
                    Dim Result as Int32 = MSocket.Init(BSocket, False)
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
                End Sub)
            AsyncThread.Start()
        End If
    End Sub
    
    Public Sub Disconnect
        If MSocket IsNot Nothing AndAlso MSocket.Connected = True
            MSocket.Close()
        End If
    End Sub
    
    Protected Mustoverride Sub OnConnectionErrored(Byval Socket as Socket, Byval SError as SocketError)
    Protected Mustoverride Sub OnConnectionForged(Byval Socket as Socket)
    Protected Mustoverride Sub OnConnectionMain(Byval Socket as Socket)
    Protected Mustoverride Sub OnConnectionReleased(Byval Socket as Socket)
End Class