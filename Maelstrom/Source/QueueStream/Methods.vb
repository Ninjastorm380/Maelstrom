Partial Public Class QueueStream (Of T)

    ''' <summary>
    ''' Creates a new QueueStream instance.
    ''' </summary>
    ''' <param name="InitialCapacity">Initial capacity of the QueueStream, which will automatically expand as needed.</param>
    ''' <remarks></remarks>
    Public Sub New(Optional InitialCapacity As UInt32 = 65536)
        redim InternalBuffer(InitialCapacity - 1)
    End Sub

    ''' <summary>
    ''' Performs a non-volatile copy.
    ''' </summary>
    ''' <param name="Output">Array to store copied data in.</param>
    ''' <param name="Count">Number of elements to copy.</param>
    ''' <param name="Seek">Offset to copy from.</param>
    ''' <remarks></remarks>
    Public Sub Read(ByRef Output As T(), Count As UInt32, Optional Seek as UInt32 = 0)
        BlockCopy(InternalBuffer, Seek, Output, 0, Count)
    End Sub

    ''' <summary>
    ''' Shifts data by the specfied amount of bytes towards the start of the QueueStream, overwriting existing data at that location.
    ''' </summary>
    ''' <param name="Count">Number of elements to shift by.</param>
    ''' <remarks></remarks>
    Public Sub Shift(Count As UInt32)
        FutureWriteOffset = WriteOffset - Count
        if FutureWriteOffset > 0 Then BlockCopy(InternalBuffer, Count, InternalBuffer, 0, FutureWriteOffset)
        WriteOffset = FutureWriteOffset
    End Sub

    ''' <summary>
    ''' Copies data to the end of a QueueStream, expanding capacity as necessary.
    ''' </summary>
    ''' <param name="Input">Array to copy in.</param>
    ''' <param name="Count">Number of elements to copy from Input.</param>
    ''' <remarks></remarks>
    Public Sub Write(ByRef Input as T(), Count as UInt32)
        FutureWriteOffset = WriteOffset + Count
        If InternalBuffer.Length < FutureWriteOffset Then ReDim Preserve InternalBuffer(FutureWriteOffset - 1)
        BlockCopy(Input, 0, InternalBuffer, WriteOffset, Count)
        WriteOffset = FutureWriteOffset
    End Sub

    ''' <summary>
    ''' Helper method for copying to and from arrays
    ''' </summary>
    ''' <param name="Source">Source array to copy elements from.</param>
    ''' <param name="SourceOffset">Offset in Source to begin copying elements from.</param>
    ''' <param name="Destination">Destination array to copy elements to.</param>
    ''' <param name="DestinationOffset">Offset in Destination to begin copying elements to.</param>
    ''' <param name="Count">Number of elements to copy.</param>
    ''' <remarks></remarks>
    Private Sub BlockCopy(Byref Source As T(), ByVal SourceOffset As UInt32, ByRef Destination As T(),
                          ByVal DestinationOffset As UInt32, ByVal Count As UInt32)
        For Index = SourceOffset To SourceOffset + Count - 1
            Destination((Index - SourceOffset) + DestinationOffset) = Source(Index)
        Next
    End Sub
End Class