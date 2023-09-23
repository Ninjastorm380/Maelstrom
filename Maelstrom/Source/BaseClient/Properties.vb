Public Partial MustInherit Class BaseClient
    Public ReadOnly Property Connected As Boolean
        Get 
            Return BaseSocket.Connected
        End Get
    End Property
End Class