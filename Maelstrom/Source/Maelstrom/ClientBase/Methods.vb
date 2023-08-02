Imports System.Threading
Imports Maelstrom.Lightning

Public Partial MustInherit Class ClientBase
    Public Sub New
        NetSocket = New Lightning.Socket
        SocketLookup = New Dictionary(Of Lightning.Socket,Socket)
    End Sub
    
    Public Sub Connect(Endpoint As Net.IPEndPoint)
        If NetSocket.Connected = False Then
            NetSocket.Connect(Endpoint)
        End If
    End Sub
    
    Public Sub Disconnect()
        If SocketLookup(NetSocket).Connected = True Then
            SocketLookup(NetSocket).Disconnect
        End If
    End Sub
    
    Private Sub SocketConnecting(NewSocket As Lightning.Socket) Handles NetSocket.SocketConnecting
        SocketLookup.Add(NewSocket, New Socket(NewSocket))
        OnLoading(SocketLookup(NewSocket))
    End Sub
    
    Private Sub SocketConnected(NewSocket As Lightning.Socket) Handles NetSocket.SocketConnected
        SocketLookup(NewSocket).Setup(UInt32.MaxValue)
        OnLoaded(SocketLookup(NewSocket))
    End Sub
    
    Private Sub SocketDisconnecting(NewSocket As Lightning.Socket) Handles NetSocket.SocketDisconnecting
        OnUnloading(SocketLookup(NewSocket))
    End Sub
    
    Private Sub SocketDisconnected(NewSocket As Lightning.Socket) Handles NetSocket.SocketDisconnected
        SocketLookup(NewSocket).Teardown(UInt32.MaxValue)
        OnUnloaded(SocketLookup(NewSocket))
        SocketLookup.Remove(NewSocket)
        NewSocket.Dispose()
    End Sub
    
    Protected MustOverride Sub OnLoading(Socket As Socket)
    Protected MustOverride Sub OnLoaded(Socket As Socket)
    Protected MustOverride Sub OnUnloading(Socket As Socket)
    Protected MustOverride Sub OnUnloaded(Socket As Socket)

End Class