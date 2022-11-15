Friend Partial Class BaseSocket
        Public ReadOnly Property Available As Int32
            Get
                If NetSocket Is Nothing Then Return 0
                If IsConnected = False Then Return 0
                If NetSocket.Connected = False Then Return 0
                Return NetSocket.Available
            End Get
        End Property

        Public ReadOnly Property Listening As System.Boolean
            Get
                Return IsListening
            End Get
        End Property
        
        Public Property RemoteEndpoint As Net.IPEndPoint
        Public Property LocalEndpoint As Net.IPEndPoint

        Public ReadOnly Property Connected As System.Boolean
            Get
                If NetSocket Is Nothing Then Return False
                If IsConnected = False Then Return False
                If NetSocket.Connected = False Then Return False
                SyncLock ReadLock
                    Dim DataAvailable = NetSocket.Poll(0, Net.Sockets.SelectMode.SelectRead)
                    Dim DataNotAvailable = (NetSocket.Available = 0)
                    IsConnected = Not (DataAvailable = True AndAlso DataNotAvailable = True)
                    Return IsConnected
                End SyncLock
            End Get
        End Property
    End Class
