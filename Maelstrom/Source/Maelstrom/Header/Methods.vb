Imports System.Net.Security

Friend Partial Structure Header
    Friend Sub Pack(ByRef Buffer As Byte())
        If Buffer Is Nothing OrElse Buffer.Length <> ByteLength Then Array.Resize(Buffer, ByteLength)
        Lightning.PackUInt32(Buffer, 0, BaseSubsocket)
        Lightning.PackInt32(Buffer, 4, BaseCompressedLength)
        Lightning.PackInt32(Buffer, 8, BaseEncryptedLength)
        Lightning.PackInt32(Buffer, 12, BaseRawLength)
        Lightning.PackInt32(Buffer, 16, BaseWriteLength)
    End Sub
    
    Friend Sub Unpack(ByVal Buffer As Byte())
        If Buffer Is Nothing Then
            BaseSubsocket = 0
            BaseCompressedLength = 0
            BaseEncryptedLength = 0
            BaseRawLength = 0
            BaseWriteLength = 0
            Return
        End If
        
        If Buffer.Length <> ByteLength Then Array.Resize(Buffer, ByteLength)
        Lightning.UnpackUInt32(Buffer, 0, BaseSubsocket)
        Lightning.UnpackInt32(Buffer, 4, BaseCompressedLength)
        Lightning.UnpackInt32(Buffer, 8, BaseEncryptedLength)
        Lightning.UnpackInt32(Buffer, 12, BaseRawLength)
        Lightning.UnpackInt32(Buffer, 16, BaseWriteLength)
    End Sub
End Structure