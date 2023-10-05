Public Partial MustInherit Class BaseClient
    Public ReadOnly Property Connected As Boolean
        Get 
            Return BaseSocket.Connected
        End Get
    End Property
    
    Public Property IdentificationToken As Byte()
        Get 
            Return BaseIdentificationToken
        End Get
        Set
            BaseIdentificationToken = Value
        End Set
    End Property
    
    Public ReadOnly Property TrustedTokens As Byte()()
        Get 
            Return BaseIdentificationTokens.ToArray()
        End Get
    End Property

End Class