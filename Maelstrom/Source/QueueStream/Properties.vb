Partial Friend Class QueueStream (Of T)

    ''' <summary>
    ''' Current length of this QueueStream instance.
    ''' </summary>
    ''' <remarks></remarks>
    Public Readonly Property Length as Int32
        get
            Return WritePointer
        End get
    End Property

    ''' <summary>
    ''' Readonly view of the internal buffer.
    ''' </summary>
    ''' <remarks></remarks>
    Public Readonly Property Buffer as T()
        get
            Return BaseBuffer
        End get
    End Property
End Class