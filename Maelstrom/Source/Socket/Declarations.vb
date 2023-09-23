Public Partial Class Socket
    Private TransformReference As Transform
    Private BaseSocket As Lightning.Socket
    Private Subsockets As Dictionary(Of UInt32, Subsocket)

    Private ReadHeaderBuffer As Byte()

    
    Private WriteHeaderBuffer As Byte()
    
    
    Private BufferHeaderBuffer As Byte()
    
    Private ReadLock As Object
    Private WriteLock As Object
    Private BufferLock As Object
    
End Class