Friend Partial Class Subsocket : Implements IDisposable
    Public ReadOnly Property Disposed As Boolean
        Get
            Return BaseDisposed
        End Get
    End Property

    Public Property ID As UInt32
        Get
            Return BaseID
        End Get
        Set
            BaseID = Value
        End Set
    End Property
    
    Public ReadOnly Property Pending As Boolean
        Get
            If BaseDisposed = True Then Return False
            Return InternalBuffer.Length > Header.ByteLength
        End Get
    End Property

    Public Property Compressed As Boolean
        Get
            Return BaseCompressed
        End Get
        Set
            BaseCompressed = Value
        End Set
    End Property
    
    Public Property Encrypted As Boolean
        Get
            Return BaseEncrypted
        End Get
        Set
            BaseEncrypted = Value
        End Set
    End Property
End Class