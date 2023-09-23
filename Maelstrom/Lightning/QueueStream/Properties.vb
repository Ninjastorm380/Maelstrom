Namespace Lightning
    Friend Partial Class QueueStream(Of T)
        Public ReadOnly Property Length As Int32
            Get
                Return WritePointer
            End Get
        End Property
        Public Property Position As Int32
    End Class
End NameSpace