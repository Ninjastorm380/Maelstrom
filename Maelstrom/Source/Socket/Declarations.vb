    Public Partial Class Socket
        Private SubsocketBuffers As Dictionary(Of UInt32, QueueStream(Of Byte)) = New Dictionary(Of UInt32, QueueStream(Of Byte))
        Private SubsocketCompressionFlags As Dictionary(Of UInt32, Byte) = New Dictionary(Of UInt32, Byte)
        Private SubsocketEncryptionFlags As Dictionary(Of UInt32, Byte) = New Dictionary(Of UInt32, Byte)

        Private ReadOnly BufferLock as Object = New Object
        
        Friend NetSocket As BaseSocket
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
