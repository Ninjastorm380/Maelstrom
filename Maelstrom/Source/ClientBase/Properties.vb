Public Partial Class ClientBase
    
    ''' <summary>
    ''' Boolean. Returns true if the client is connected to a server, otherwise false.
    ''' </summary>
    ''' <remarks></remarks>
    Public Readonly Property Connected as Boolean
        Get
            If MSocket IsNot Nothing Then Return MSocket.Connected
            Return False
        End Get
    End Property
End Class