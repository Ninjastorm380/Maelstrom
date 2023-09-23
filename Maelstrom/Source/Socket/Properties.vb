Public Partial Class Socket
    Public ReadOnly Property Available(ByVal Subsocket As UInt32) As Boolean
        Get 
            If Connected = False Then Return False 
            Pump() 
            Return Subsockets(Subsocket).Buffer.Length > Header.RequiredBufferLength 
        End Get
    End Property
    
    Public ReadOnly Property Connected As Boolean
        Get 
            If BaseSocket Is Nothing Then Return False
            Return BaseSocket.Connected
        End Get
    End Property
    
    Public Property Compressed(Subsocket As UInt32) As Boolean
        Get 
            If Connected = False Then Return False
            EnsureSubsocket(Subsocket)
            Return Subsockets(Subsocket).Compressed 
        End Get
        Set
            If Connected = False Then Return
            EnsureSubsocket(Subsocket)
            Subsockets(Subsocket).Compressed = Value
        End Set
    End Property
    
    Public Property Encrypted(Subsocket As UInt32) As Boolean
        Get 
            If Connected = False Then Return False
            EnsureSubsocket(Subsocket)
            Return Subsockets(Subsocket).Encrypted
        End Get
        Set
            If Connected = False Then Return
            EnsureSubsocket(Subsocket)
            Subsockets(Subsocket).Encrypted = Value
        End Set
    End Property


End Class