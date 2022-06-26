Partial Friend Class QueueStream
    
    ''' <summary>
    ''' Current length of byte queue.
    ''' </summary>
    ''' <remarks></remarks>
    Public Readonly Property Length as Int32
        get
            Return WriteOffset
        End get
    End Property
End Class