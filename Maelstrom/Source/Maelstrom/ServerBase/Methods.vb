Imports System.Threading

Public Partial MustInherit Class ServerBase
    Public Sub New
        NetSocket = New Lightning.Socket
        SocketLookup = New Dictionary(Of Lightning.Socket,Socket)
    End Sub
    
    Public Sub Listen(Endpoint As Net.IPEndPoint)
        If NetSocket.Listening = False Then
            NetSocket.Listen(Endpoint)
        End If
    End Sub
    
    Public Sub Deafen()
        If NetSocket.Listening = True Then
            NetSocket.Deafen
        End If
    End Sub
    
    Private Sub SocketConnecting(NewSocket As Lightning.Socket) Handles NetSocket.SocketConnecting
        SocketLookup.Add(NewSocket, New Socket(NewSocket))
        OnLoading(SocketLookup(NewSocket))
    End Sub
    
    Private Sub SocketConnected(NewSocket As Lightning.Socket) Handles NetSocket.SocketConnected
        OnLoaded(SocketLookup(NewSocket))
    End Sub
    
    Private Sub SocketDisconnecting(NewSocket As Lightning.Socket) Handles NetSocket.SocketDisconnecting
        OnUnloading(SocketLookup(NewSocket))
    End Sub
    
    Private Sub SocketDisconnected(NewSocket As Lightning.Socket) Handles NetSocket.SocketDisconnected
        OnUnloaded(SocketLookup(NewSocket))
        SocketLookup.Remove(NewSocket)
    End Sub
    
    Private Sub SocketListenerBinding() Handles NetSocket.SocketListenerBinding
        OnListenerBinding()
    End Sub
    
    Private Sub SocketListenerBound() Handles NetSocket.SocketListenerBound
        OnListenerBound()
    End Sub
    
    Private Sub SocketListenerUnbinding() Handles NetSocket.SocketListenerUnbinding
        OnListenerUnbinding()
    End Sub
    
    Private Sub SocketListenerUnbound() Handles NetSocket.SocketListenerUnbound
        OnListenerUnbound()
    End Sub
    
    Protected MustOverride Sub OnLoading(Socket As Socket)
    Protected MustOverride Sub OnLoaded(Socket As Socket)
    Protected MustOverride Sub OnUnloading(Socket As Socket)
    Protected MustOverride Sub OnUnloaded(Socket As Socket)
    Protected MustOverride Sub OnListenerBinding()
    Protected MustOverride Sub OnListenerBound()
    Protected MustOverride Sub OnListenerUnbinding()
    Protected MustOverride Sub OnListenerUnbound()

End Class