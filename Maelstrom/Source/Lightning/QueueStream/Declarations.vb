    Namespace Lightning
        Public Partial Class QueueStream(Of T) : Implements IDisposable
            Private BaseBuffer As T() = New T(65535) {}
            Private WritePointer As Int32 = 0
            Private IsDisposed As Boolean = True
        End Class
    End Namespace

