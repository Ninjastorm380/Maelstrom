Public Partial Class Socket
    Public Readonly Property Pending(Subsocket As UInt32) as Boolean
        Get
            PerformNetworkPumpRead()
            If Subsocket = UInt32.MaxValue Then Return False
            If Subsockets.Contains(Subsocket) Then Return Subsockets(Subsocket).Pending
            Return False
        End Get
    End Property
    
    Public Readonly Property Connected as Boolean
        Get
            Return BaseSocket.Connected
        End Get
    End Property
    
    Public Property Encrypted(Subsocket As UInt32) As Boolean
        Get
            If Subsocket = UInt32.MaxValue Then Return False
            If Subsockets.Contains(Subsocket) Then Return Subsockets(Subsocket).Encrypted
            Return False
        End Get
        Set
            If Subsocket = UInt32.MaxValue Then Return
            If Subsockets.Contains(Subsocket) Then Subsockets(Subsocket).Encrypted = Value
        End Set
    End Property
    
    Public Property Compressed(Subsocket As UInt32) As Boolean
        Get
            If Subsocket = UInt32.MaxValue Then Return False
            If Subsockets.Contains(Subsocket) Then Return Subsockets(Subsocket).Compressed
            Return False
        End Get
        Set
            If Subsocket = UInt32.MaxValue Then Return
            If Subsockets.Contains(Subsocket) Then Subsockets(Subsocket).Compressed = Value
        End Set
    End Property

End Class