Friend Partial Class QueueStream(Of T)
    Public ReadOnly Property Length As UInt32
        Get
            Return WritePointer
        End Get
    End Property
End Class