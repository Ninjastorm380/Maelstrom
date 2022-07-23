Partial Public Class QueueStream (Of T)

    ''' <summary>
    ''' Current length of byte queue.
    ''' </summary>
    ''' <remarks></remarks>
    Public Readonly Property Length as Int32
        get
            Return WriteOffset
        End get
    End Property

    Public Readonly Property Buffer as T()
        get
            Return InternalBuffer
        End get
    End Property
End Class