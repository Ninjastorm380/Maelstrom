Namespace Lightning
    Public Partial Class QueueStream(Of T) : Implements IDisposable
        Public ReadOnly Property Length As Int32
            Get
                Return WritePointer
            End Get
        End Property
        Public Property Position As UInt32
    End Class
End Namespace
