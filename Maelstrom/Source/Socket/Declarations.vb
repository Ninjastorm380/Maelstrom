    Public Partial Class Socket
        
        '|---------------------------------------------------------------------------------------------------------------|
        '| Maelstrom Generation 4 Programmable Protocol Refrence                                                         |
        '|----------------------------------------------------------|-------------------------|--------------------------|
        '| Raw Protocol Byte Region Descriptions                    | Encryption support for  | Compression support for  |
        '|                                                          | a specific byte region  | a specific byte region   |
        '|----------------------------------------------------------|-------------------------|--------------------------|
        '| Bytes 00000000-00000003 = Command ID                     | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000004-00000007 = Command Return Flag            | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000008-00000011 = Command Parameter 1            | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000012-00000015 = Command Parameter 2            | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000016-00000019 = Command Parameter 3            | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000020-00000023 = Command Data Encrypted Length  | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000024-00000027 = Command Data Compressed Length | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000028-00000031 = Command Data Raw Length        | Supports Encryption = T | Supports Compression = F |
        '| Bytes 00000032-???????? = Command Data (optional)        | Supports Encryption = T | Supports Compression = T |
        '| Bytes ??????+1-???MOD16 = Padding (optional)             | Supports Encryption = T | Supports Compression = F |
        '|----------------------------------------------------------|-------------------------|--------------------------|
       
        Private SubsocketBuffers As Lightning.Dictionary(Of UInt32, Lightning.QueueStream(Of Byte)) = New Lightning.Dictionary(Of UInt32, Lightning.QueueStream(Of Byte))
        Private SubsocketCompressionFlags As Lightning.Dictionary(Of UInt32, Byte) = New Lightning.Dictionary(Of UInt32, Byte)
        Private SubsocketEncryptionFlags As Lightning.Dictionary(Of UInt32, Byte) = New Lightning.Dictionary(Of UInt32, Byte)

        Private ReadOnly BufferLock as Object = New Object
        
        Friend NetSocket As Lightning.Socket
        Friend SendCryptographer As Cryptographer
        Friend ReceiveCryptographer As Cryptographer
        Friend HeaderSendCryptographer As Cryptographer
        Friend HeaderReceiveCryptographer As Cryptographer
        Private SendTransformBuffer(65535) as Byte
        Private HeaderSendTransformBuffer(31) as Byte
        Private SendRawLength As UInt32
        Private SendSubsocket As UInt32
        Private SendCompressedLength As UInt32
        Private SendEncryptedLength As UInt32
        Private SendCompressedFlag As Byte
        Private SendEncryptedFlag As Byte
        
        
        Private ReceiveTransformBuffer(65535) as Byte
        Private HeaderReceiveTransformBuffer(31) as Byte
        Private ReceiveRawLength As UInt32
        Private ReceiveSubsocket As UInt32
        Private ReceiveCompressedLength As UInt32
        Private ReceiveEncryptedLength As UInt32
        Private ReceiveCompressedFlag As Byte
        Private ReceiveEncryptedFlag As Byte
        Private ReceiverLaunched As Boolean = False
        
        Private ReadOnly SendLock as Object = New Object
        Private ReadOnly ReceiveLock as Object = New Object
        
        Private ReadRawLength As UInt32
        Private HeaderReadBuffer(31) as Byte

    End Class
