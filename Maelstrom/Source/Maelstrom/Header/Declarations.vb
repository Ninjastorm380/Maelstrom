Friend Partial Structure Header
    Private BaseBuffer As Byte()
    Private BaseSubsocket As UInt32
    Private BaseCommand As UInt32
    Private BaseCompressedLength As Int32
    Private BaseEncryptedLength As Int32
    Private BaseRawLength As Int32
    Private BaseWriteLength As Int32
    Public Const ByteLength As Int32 = 36
End Structure