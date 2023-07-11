Imports System.Net.Sockets
Imports System.Net

Namespace Lightning
    Friend Partial Class Socket
        Private NetSocket As Sockets.Socket = New Sockets.Socket(SocketType.Stream, ProtocolType.Tcp)

        Private IsListening As System.Boolean = False
        Private IsConnected As System.Boolean = False
        Private IsBaseDisposed As System.Boolean = False
        Private DataAvailable As Boolean = False
        Private DataNotAvailable As Boolean = False
        Private BaseClientBound As Boolean = False

        Private ReadOnly ReadLock As System.Object = New System.Object()
        Private ReadTimeoutStopwatch As Stopwatch = New Stopwatch()

        Private ReadOnly WriteLock As System.Object = New System.Object()
        Private WriteTimeoutStopwatch As Stopwatch = New Stopwatch()
    End Class
End Namespace
