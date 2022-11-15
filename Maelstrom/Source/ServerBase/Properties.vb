Public Partial MustInherit Class ServerBase
    Public ReadOnly Property Listening As Boolean
        Get
            Return BaseSocket.Listening
        End Get
    End Property

End Class