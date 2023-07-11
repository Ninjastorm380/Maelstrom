Public Partial MustInherit Class ClientBase
    Public ReadOnly Property Connected As Boolean
        Get
            Return NetSocket.Connected
        End Get
    End Property
End Class