Public Partial MustInherit Class ServerBase
    Public ReadOnly Property Listening As Boolean
        Get
            Return NetSocket.Listening
        End Get
    End Property
End Class