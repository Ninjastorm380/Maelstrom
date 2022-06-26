Partial Friend Class QueueStream
    ''' <summary>
    ''' Creates a new QueueStream instance.
    ''' </summary>
    ''' <param name="InitialCapacity">Initial capacity of the QueueStream, which will automatically expand as needed.</param>
    ''' <remarks></remarks>
    Public Sub New(Optional InitialCapacity As Int32 = 65536)
        redim InternalBuffer(InitialCapacity - 1)
    End Sub
    
    ''' <summary>
    ''' Performs a non-volatile copy.
    ''' </summary>
    ''' <param name="Output">Byte array to store copied data in.</param>
    ''' <param name="Count">Number of bytes to copy.</param>
    ''' <param name="Seek">Offset to copy from.</param>
    ''' <remarks></remarks>
    Public Sub Read(ByRef Output As Byte(), Count As Int32, Optional Seek as Int32 = 0)
        Buffer.BlockCopy(InternalBuffer,Seek,Output,0,Count)
    End Sub
    
    ''' <summary>
    ''' Shifts data by the specfied amount of bytes towards the start of the QueueStream, overwriting existing data at that location.
    ''' </summary>
    ''' <param name="Count">Number of bytes to shift by.</param>
    ''' <remarks></remarks>
    Public Sub Shift(Count As Int32)
        FutureWriteOffset = WriteOffset - Count
        Buffer.BlockCopy(InternalBuffer,Count,InternalBuffer,0,InternalBuffer.Length - Count)
        WriteOffset = FutureWriteOffset
    End Sub
    
    ''' <summary>
    ''' Copies data to the end of a QueueStream, expanding capacity as necessary.
    ''' </summary>
    ''' <param name="Input">Byte array to copy in.</param>
    ''' <param name="Count">Number of bytes to copy from Input.</param>
    ''' <remarks></remarks>
    Public Sub Write(ByRef Input as Byte(), Count as Int32)
        FutureWriteOffset = WriteOffset + Count
        If InternalBuffer.Length < FutureWriteOffset Then ReDim Preserve InternalBuffer((FutureWriteOffset) - 1)
        Buffer.BlockCopy(Input, 0, InternalBuffer, WriteOffset, Count)
        WriteOffset = FutureWriteOffset
    End Sub
End Class