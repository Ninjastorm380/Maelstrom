Friend Partial Class Subsocket : Implements IDisposable
    Public Sub New
        Buffer = New Lightning.QueueStream(Of Byte)
        BufferLock = New Object
            
        Compressed = True
        Encrypted = True
        Array.Resize(ReadDataBuffer,   524288)
        Array.Resize(WriteDataBuffer,  524288)
    End Sub
    
    Public Sub Dispose() Implements IDisposable.Dispose
        Buffer.Clear()
        Buffer = Nothing
        BufferLock = Nothing
    End Sub
End Class