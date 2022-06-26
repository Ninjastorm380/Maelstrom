Public MustInherit Partial Class ServerBase
    ''' <summary>
    ''' Boolean. Returns true if the server is listening for connections, otherwise false.
    ''' </summary>
    ''' <remarks></remarks>
    Protected ReadOnly Property Online As Boolean
        Get
            Return IsOnline
        End Get
    End Property

End Class