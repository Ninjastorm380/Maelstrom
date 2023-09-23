Friend Partial Class Subsocket
    Public Property ReadDataBuffer As Byte()
    Public Property WriteDataBuffer As Byte()
    Public Property BufferLock As Object
    Public Property Buffer As Lightning.QueueStream(Of Byte)
    Public Property Compressed As Boolean
    Public Property Encrypted As Boolean
End Class