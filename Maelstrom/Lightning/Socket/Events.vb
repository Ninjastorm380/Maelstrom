Namespace Lightning
    Friend Partial Class Socket
        Public Event OnConnect As SocketHandler
        Public Event OnDisconnect As SocketHandler
    
        Public Event OnListen As SocketHandler
        Public Event OnDeafen As SocketHandler
    End Class
End NameSpace