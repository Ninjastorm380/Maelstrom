    
''' <summary>
''' A highly efficent queue.
''' </summary>
''' <remarks></remarks>
Partial Friend Class QueueStream (Of T)

    ''' <summary>
    ''' Array. Used for storing data from write calls, acts as the source for read calls, and is managed by shift calls.
    ''' Write calls will expand this array as necessary.
    ''' </summary>
    ''' <remarks></remarks>
    Private BaseBuffer(65535) As T

    ''' <summary>
    ''' 32 bit integer. Stores the current location for writing into InternalBuffer.
    ''' </summary>
    ''' <remarks></remarks>
    Private WritePointer As UInt32 = 0

    ''' <summary>
    ''' 32 bit integer. Stores the future location for writing into InternalBuffer.
    ''' WriteOffset is set to the value of FutureWriteOffset after every shift operation and every write operation.
    ''' </summary>
    ''' <remarks></remarks>
    Private WritePointerTemp As UInt32 = 0
End Class