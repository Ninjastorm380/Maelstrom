Imports Maelstrom.Lightning

Public Partial Class Socket
    Private ReadOnly BaseSocket As Lightning.Socket
    Private ReadOnly Subsockets As Lightning.Dictionary(Of UInt32, Subsocket)
    

    Dim ReadHeaderBuffer(Header.ByteLength - 1) As Byte
    Dim ReadDataBuffer As Byte()
    
    
    Dim WriteHeaderBuffer(Header.ByteLength - 1) As Byte
    Dim WriteDataBuffer As Byte()
    
    
    Dim BufferHeaderBuffer(Header.ByteLength - 1) As Byte
    Dim BufferDataBuffer As Byte()
    
    Private ReadLock As Object
    Private WriteLock As Object
    Private BaseDisposed As Boolean
    
End Class