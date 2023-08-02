Friend Partial Structure Header
    Public Property Subsocket As UInt32
        Get
            Return BaseSubsocket
        End Get
        Set
            BaseSubsocket = Value
        End Set
    End Property
    
    Public Property Command As UInt32
        Get
            Return BaseCommand
        End Get
        Set
            BaseCommand = Value
        End Set
    End Property

    
    Public Property CompressedLength As Int32
        Get
            Return BaseCompressedLength
        End Get
        Set
            BaseCompressedLength = Value
        End Set
    End Property
    
    Public Property EncryptedLength As Int32
        Get
            Return BaseEncryptedLength
        End Get
        Set
            BaseEncryptedLength = Value
        End Set
    End Property
    
    Public Property RawLength As Int32
        Get
            Return BaseRawLength
        End Get
        Set
            BaseRawLength = Value
        End Set
    End Property
    
    Public Property WriteLength As Int32
        Get
            Return BaseWriteLength
        End Get
        Set
            BaseWriteLength = Value
        End Set
    End Property

End Structure