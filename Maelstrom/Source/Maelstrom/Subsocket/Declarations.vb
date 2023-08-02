Friend Class Subsocket : Implements IDisposable
    Private InternalBuffer As Lightning.QueueStream(Of Byte)
    Private InternalPackBuffer As Byte()
    Friend BufferLock As Object
    Private BaseDisposed As Boolean
    Private BaseID As UInt32
    Private BaseCompressed As Boolean
    Private BaseEncrypted As Boolean
    Private BaseDataWriteEncryptor As Cryptographer
    Private BaseDataReadDecryptor As Cryptographer
    Private BaseHeaderWriteEncryptor As Cryptographer
    Private BaseHeaderReadDecryptor As Cryptographer
End Class