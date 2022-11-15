    Friend Partial Class BaseSocket
        Private NetSocket As Net.Sockets.Socket = New Net.Sockets.Socket(Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        Private IsListening As System.Boolean = False
        Private IsConnected As System.Boolean = False
        Private IsBaseDisposed As System.Boolean = False
        Private ReadOnly ListenerGovernor As Governor = New Governor(10)
        
        Private ReadOnly ReadGovernor As Governor = New Governor(30)
        Private ReadOnly ReadLock As System.Object = New System.Object()
        Private ReadRetryResult As UInt32 = 0
        Private ReadRetryCounter As UInt32 = 0
        Private ReadRetryCurrent As Double = 0.0
        Private ReadRetryMax As Double = 10.0
        Private ReadAvailableSnapshot As UInt32 = 0
        
        Private ReadOnly WriteGovernor As Governor = New Governor(30)
        Private ReadOnly WriteLock As System.Object = New System.Object()
        Private WriteRetryResult As UInt32 = 0
        Private WriteRetryCounter As UInt32 = 0
        Private WriteRetryCurrent As Double = 0.0
        Private WriteRetryMax As Double = 10.0

    End Class
