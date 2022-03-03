Friend Notinheritable Class QueueStream
    Private InternalBuffer(65535) As Byte
    Private WriteOffset As Int32 = 0
    Private FutureWriteOffset As Int32 = 0
    
    Public Readonly Property Length as Int32
        get
            Return WriteOffset
        End get
    End Property
    
    Public Sub Read(ByRef Output As Byte(), Count As Int32, Optional Seek as Int32 = 0)
        Buffer.BlockCopy(InternalBuffer,Seek,Output,0,Count)
    End Sub
    
    Public Sub Dump(Count As Int32)
        Buffer.BlockCopy(InternalBuffer,Count,InternalBuffer,0,InternalBuffer.Length - Count)
        Threading.Interlocked.Add(WriteOffset,-Count)
    End Sub
    
    Public Sub Write(ByRef Input as Byte(), Count as Int32)
            FutureWriteOffset = WriteOffset + Count
            If InternalBuffer.Length < FutureWriteOffset Then ReDim Preserve InternalBuffer(FutureWriteOffset - 1)
            Buffer.BlockCopy(Input, 0, InternalBuffer, WriteOffset, Count)
            WriteOffset = FutureWriteOffset
    End Sub
End Class