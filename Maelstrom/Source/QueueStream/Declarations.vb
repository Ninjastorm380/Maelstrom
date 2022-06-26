    ''' <summary>
    ''' A highly efficent queue for bytes.
    ''' </summary>
    ''' <remarks></remarks>
Partial Friend Class QueueStream
    ''' <summary>
    ''' Byte array. Used for storing data from write calls, acts as the source for read calls, and is managed by shift calls.
    ''' Write calls will expand this array as necessary.
    ''' </summary>
    ''' <remarks></remarks>
    Private InternalBuffer(65535) As Byte
    
    ''' <summary>
    ''' 32 bit integer. Stores the current offset for writing into InternalBuffer.
    ''' </summary>
    ''' <remarks></remarks>
    Private WriteOffset As Int32 = 0
    
    ''' <summary>
    ''' 32 bit integer. Stores the future offset for writing into InternalBuffer.
    ''' WriteOffset is set to the value of FutureWriteOffset after every shift operation and every write operation.
    ''' </summary>
    ''' <remarks></remarks>
    Private FutureWriteOffset As Int32 = 0
End Class