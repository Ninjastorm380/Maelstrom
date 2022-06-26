Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public MustInherit Partial Class ClientBase
    
    ''' <summary>
    ''' Connects the client to the server on the specified endpoint.
    ''' </summary>
    ''' <param name="Endpoint">The endpoint to connect to.</param>
    ''' <remarks></remarks>
    Public Sub Connect(Endpoint As IPEndPoint)
        If MSocket Is Nothing Then MSocket = New Socket()
        If MSocket.Connected = False
            BSocket = new Sockets.Socket(SocketType.Stream, ProtocolType.Tcp)
            BSocket.Connect(Endpoint)
            Dim AsyncThread as New Thread(
                Sub()
                    Dim Result as Int32 = MSocket.Init(BSocket, False)
                    Select Case Result
                        Case = 0
                            OnConnectionForged(MSocket)
                            Do Until MSocket.Connected = False 
                                Thread.Sleep(300)
                            Loop
                            MSocket.Dispose()
                            OnConnectionReleased(MSocket)
                        Case = 1
                            OnConnectionErrored(SocketError.HeaderMismatch)
                        Case = 2
                            OnConnectionErrored(SocketError.VersionMismatch)
                    End Select
                End Sub)
            AsyncThread.Start()
        End If
    End Sub
    
    ''' <summary>
    ''' Disconnects the client from the currently connected server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Disconnect()
        If MSocket IsNot Nothing AndAlso MSocket.Connected = True
            MSocket.Close()
        End If
    End Sub
    
    ''' <summary>
    ''' Overridable method. Triggers when a maelstrom socket encounters an error while attempting to connect.
    ''' </summary>
    ''' <param name="SError">Maelstrom error code.</param>
    ''' <remarks></remarks>
    Protected Mustoverride Sub OnConnectionErrored(Byval SError as SocketError)
    
    ''' <summary>
    ''' Overridable method. Triggers when a maelstrom socket successfully establishes a connection.
    ''' </summary>
    ''' <param name="Socket">The connected maelstrom socket.</param>
    ''' <remarks></remarks>
    Protected Mustoverride Sub OnConnectionForged(Byval Socket as Socket)
    
    ''' <summary>
    ''' Overridable method. Triggers when a maelstrom socket has completely disconnected.
    ''' </summary>
    ''' <param name="Socket">The disconnected maelstrom socket.</param>
    ''' <remarks></remarks>
    Protected Mustoverride Sub OnConnectionReleased(Byval Socket as Socket)

End Class