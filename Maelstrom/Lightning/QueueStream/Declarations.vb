Namespace Lightning
    Friend Partial Class QueueStream(Of T)
        Private BaseBuffer As T() = New T(65535) {}
        Private WritePointer As Int32 = 0
        Private IsDisposed As Boolean = True
    End Class
End NameSpace