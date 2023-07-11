Imports Maelstrom.Lightning

Friend Partial Class Subsocket : Implements IDisposable
    Public Sub New(ID As UInt32)
        BufferLock = New Object
        SyncLock BufferLock
            InternalBuffer = New QueueStream(Of Byte)()
            Array.Resize(InternalPackBuffer, Header.ByteLength)
            BaseID = ID
            BaseDisposed = False
            BaseCompressed = False
            BaseEncrypted = False
        End SyncLock
    End Sub
    
    Public Sub Read(ByRef Header As Header, ByRef Data As Byte())
        SyncLock BufferLock
            InternalBuffer.Read(InternalPackBuffer, 0, 0, Header.ByteLength)
            Header.Unpack(InternalPackBuffer)
            If Data Is Nothing OrElse Data.Length < Header.RawLength Then Array.Resize(Data, Header.RawLength)
            InternalBuffer.Read(Data, 0, Header.ByteLength, Header.RawLength)
            InternalBuffer.Shift(Header.RawLength + Header.ByteLength)
        End SyncLock
    End Sub
    
    Public Sub Write(ByVal Header As Header, ByVal Data As Byte())
        SyncLock BufferLock
            Header.Pack(InternalPackBuffer)
            InternalBuffer.Write(InternalPackBuffer, 0, Header.ByteLength)
            InternalBuffer.Write(Data, 0, Header.RawLength)
        End SyncLock
    End Sub
    
    Public Sub EncryptData(ByRef Input As Byte(), ByRef Output As Byte(), ByVal Offset As Integer, Byval Length As Integer)
        BaseDataWriteEncryptor.TransformBlock(Input, Output, Offset, Length)
    End Sub
    
    Public Sub DecryptData(ByRef Input As Byte(), ByRef Output As Byte(), ByVal Offset As Integer, Byval Length As Integer)
        BaseDataReadDecryptor.TransformBlock(Input, Output, Offset, Length)
    End Sub
    
    Public Sub EncryptHeader(ByRef Input As Byte(), ByRef Output As Byte(), ByVal Offset As Integer, Byval Length As Integer)
        BaseHeaderWriteEncryptor.TransformBlock(Input, Output, Offset, Length)
    End Sub
    
    Public Sub DecryptHeader(ByRef Input As Byte(), ByRef Output As Byte(), ByVal Offset As Integer, Byval Length As Integer)
        BaseHeaderReadDecryptor.TransformBlock(Input, Output, Offset, Length)
    End Sub
    
    Public Function Sync(Socket As Lightning.Socket) As Boolean
        Dim SubsocketHandshake As New Handshake
        Dim SubsocketHandshakeResult As Handshake.Result = SubsocketHandshake.Perform(Socket)
        If SubsocketHandshakeResult = Handshake.Result.Ok Then
            Dim SubsocketRandoms As Byte()() = SubsocketHandshake.GetSeeds()
            
            If BaseDataWriteEncryptor IsNot Nothing Then BaseDataWriteEncryptor.Dispose()
            If BaseDataReadDecryptor IsNot Nothing Then BaseDataReadDecryptor.Dispose()
            If BaseHeaderWriteEncryptor IsNot Nothing Then BaseHeaderWriteEncryptor.Dispose()
            If BaseHeaderReadDecryptor IsNot Nothing Then BaseHeaderReadDecryptor.Dispose()
            
            BaseDataWriteEncryptor = New Cryptographer(New Random(SubsocketRandoms(0)), Cryptographer.Type.Encryption)
            BaseDataReadDecryptor = New Cryptographer(New Random(SubsocketRandoms(1)), Cryptographer.Type.Decryption)
            BaseHeaderWriteEncryptor = New Cryptographer(New Random(SubsocketRandoms(2)), Cryptographer.Type.Encryption)
            BaseHeaderReadDecryptor = New Cryptographer(New Random(SubsocketRandoms(3)), Cryptographer.Type.Decryption)
            Return True
        Else
            Return False
        End If
    End Function
    
    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock BufferLock
            InternalBuffer.Dispose()
            BaseDataWriteEncryptor.Dispose()
            BaseDataReadDecryptor.Dispose()
            BaseHeaderWriteEncryptor.Dispose()
            BaseHeaderReadDecryptor.Dispose()
            InternalBuffer = Nothing
            InternalPackBuffer = Nothing
            BaseID = Nothing
            BaseDisposed = True
        End SyncLock
        BufferLock = Nothing
    End Sub
End Class