Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public MustInherit Partial Class ServerBase
    
    
    ''' <summary>
    ''' Starts the server on a specific endpoint.
    ''' </summary>
    ''' <param name="Endpoint">endpoint to listen on.</param>
    ''' <remarks></remarks>
    Public Sub Start(Endpoint As IPEndPoint)
        If IsOnline = False Then
            
            Listener = New TcpListener(Endpoint)
            ListenerThread = New Thread(AddressOf ListenerMethod)
            IsOnline = True
            Listener.Start()
            ListenerThread.Start()
        End If
    End Sub
    
    ''' <summary>
    ''' Stops the server, and disconnects any active clients.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub [Stop]()
        IsOnline = False
    End Sub
    
    Private Sub ListenerMethod()
        OnServerOnline()
        Dim Limiter As New Governor(10)
        Do While IsOnline = True
            If Listener.Pending = True 
                Dim AsyncLaunch As New Thread(Sub()
                    Dim MSocket as new Socket
                    Dim Result as SocketError = MSocket.Init(Listener.AcceptSocket, True)
                    Select Case Result
                        Case = 0
                            OnConnectionForged(MSocket)
                            Do Until MSocket.Connected = False Or IsOnline = False
                                Thread.Sleep(300)
                            Loop
                            If MSocket.Connected = true Then
                                MSocket.Close()
                                MSocket.Dispose()
                            End If
                            OnConnectionReleased(MSocket)
                        Case = 1
                            OnConnectionErrored(SocketError.HeaderMismatch)
                        Case = 2
                            OnConnectionErrored(SocketError.VersionMismatch)
                    End Select
                End Sub) : AsyncLaunch.Start()
            End If
            Limiter.Limit()
        Loop
        Listener.Stop()
        OnServerOffline()
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
    
    ''' <summary>
    ''' Overridable method. Treggers when the server is started.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Mustoverride Sub OnServerOnline()
    
    ''' <summary>
    ''' Overridable method. Triggers when the server is stopped.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Mustoverride Sub OnServerOffline()
End Class