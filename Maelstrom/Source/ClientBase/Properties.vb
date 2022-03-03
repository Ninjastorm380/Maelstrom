Public Partial Class ClientBase
    Public Readonly Property Connected as Boolean
        Get
            If MSocket IsNot Nothing Then Return MSocket.Connected
            Return False
        End Get
    End Property
End Class