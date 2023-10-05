Namespace Lightning
    Friend Partial Class Socket
        Public ReadOnly Property Available As Int32
            Get
                If NetSocket Is Nothing Then Return 0
                If IsConnected = False Then Return 0
                If NetSocket.Connected = False Then Return 0
                Return NetSocket.Available
            End Get
        End Property

        Public ReadOnly Property Listening As Boolean
            Get
                Return IsListening
            End Get
        End Property

        Public Readonly Property ClientSide As Boolean
            Get
                Return BaseClientSide
            End get
        End Property

        Public ReadOnly Property Connected As System.Boolean
            Get
                SyncLock ReadLock
                    If NetSocket Is Nothing Then
                        IsConnected = False
                        Return False
                    End If
                    If IsConnected = False Then
                        IsConnected = False
                        Return False
                    End If
                    If NetSocket.Connected = False Then
                        IsConnected = False
                        Return False
                    End If
                
            
                    DataAvailable = (NetSocket.Poll(1, Net.Sockets.SelectMode.SelectRead))
                    DataNotAvailable = (NetSocket.Available = 0)

                    'IsConnected = Not (NetSocket.Poll(1, Net.Sockets.SelectMode.SelectRead) AndAlso (NetSocket.Available = 0))
                    If DataAvailable = True And DataNotAvailable = True Then
                        IsConnected = False
                    Else If DataAvailable = True And DataNotAvailable = False And NetSocket.Connected = True Then
                        IsConnected = True
                    Else If DataAvailable = False And DataNotAvailable = True And NetSocket.Connected = True Then
                        IsConnected = True
                    Else If DataAvailable = False And DataNotAvailable = True And NetSocket.Connected = True Then
                        IsConnected = True
                    End If
                End SyncLock
                Return IsConnected
            End Get
        End Property
    
        Public Property ReadTimeout As TimeSpan
        Public Property WriteTimeout As TimeSpan

        Public Property LocalEndpoint As Net.IPEndPoint
        Public Property RemoteEndpoint As Net.IPEndPoint
    End Class
End NameSpace