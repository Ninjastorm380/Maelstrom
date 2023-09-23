Friend Partial Class Header
    Public Sub New(Transform As Transform)
        TransformReference = Transform
    End Sub
    
    Public Sub Pack(ByRef Buffer As Byte(), Offset As Int32)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Subsocket),   0, Buffer, 00 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Compressed),  0, Buffer, 04 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Encrypted),   0, Buffer, 08 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Raw),         0, Buffer, 12 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Transformed), 0, Buffer, 16 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Max),         0, Buffer, 20 + Offset, 4)
        TransformReference.HeaderPackTransform.TransformBlock(Buffer, 0 + Offset, RequiredBufferLength, Buffer, 0 + Offset)
    End Sub
    
    Public Sub Unpack(ByRef Buffer As Byte(), Offset As Int32)
        TransformReference.HeaderUnpackTransform.TransformBlock(Buffer, 0 + Offset, RequiredBufferLength, Buffer, 0 + Offset)
        Subsocket   = BitConverter.ToUInt32(Buffer, 00 + Offset)
        Compressed  = BitConverter.ToInt32 (Buffer, 04 + Offset)
        Encrypted   = BitConverter.ToInt32 (Buffer, 08 + Offset)
        Raw         = BitConverter.ToInt32 (Buffer, 12 + Offset)
        Transformed = BitConverter.ToInt32 (Buffer, 16 + Offset)
        Max         = BitConverter.ToInt32 (Buffer, 20 + Offset)
    End Sub
    
    Public Sub PackRaw(ByRef Buffer As Byte(), Offset As Int32)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Subsocket),   0, Buffer, 00 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Compressed),  0, Buffer, 04 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Encrypted),   0, Buffer, 08 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Raw),         0, Buffer, 12 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Transformed), 0, Buffer, 16 + Offset, 4)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Max),         0, Buffer, 20 + Offset, 4)
    End Sub
    
    Public Sub UnpackRaw(ByRef Buffer As Byte(), Offset As Int32)
        Subsocket   = BitConverter.ToUInt32(Buffer, 00 + Offset)
        Compressed  = BitConverter.ToInt32 (Buffer, 04 + Offset)
        Encrypted   = BitConverter.ToInt32 (Buffer, 08 + Offset)
        Raw         = BitConverter.ToInt32 (Buffer, 12 + Offset)
        Transformed = BitConverter.ToInt32 (Buffer, 16 + Offset)
        Max         = BitConverter.ToInt32 (Buffer, 20 + Offset)
    End Sub
End Class