Public Partial Class QueueStream(Of T)
    Public ReadOnly Property Length As UInt32
        Get
            Return WritePointer
        End Get
    End Property
    Public Property Position As UInt32
End Class