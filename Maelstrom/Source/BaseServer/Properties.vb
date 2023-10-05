Public Partial MustInherit Class BaseServer
    Public ReadOnly Property Listening As Boolean
        Get 
            Return BaseSocket.Listening
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
    
    Public ReadOnly Property BlockedTokens As Byte()()
        Get 
            Return BaseIdentificationTokens.ToArray()
        End Get
    End Property

End Class