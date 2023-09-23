Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Maelstrom.Lightning

Friend Partial Class Handshake
    
    Friend Function Perform(Socket As Lightning.Socket) As Result
        Array.Resize(LocalTransformBuffer, 516)
        Array.Resize(RemoteTransformBuffer, 516)
        
        Array.Resize(LocalHeaderSeed, 256)
        Array.Resize(LocalDataSeed, 256)
        Array.Resize(RemoteHeaderSeed, 256)
        Array.Resize(RemoteDataSeed, 256)
        
        Timeout = TimeSpan.FromMilliseconds(1000)
        SyncResult = Result.ValidationFailure
        Watchdog = Stopwatch.StartNew()
        'Generate local public and private keys.
        Dim LocalEncryptionExchange As New KeyExchange
        Dim LocalDecryptionExchange As New KeyExchange
        
        'Retrieve local public keys.
        Dim LocalEncryptionPublicKey As BigInteger = LocalEncryptionExchange.PublicKey
        Dim LocalDecryptionPublicKey As BigInteger = LocalDecryptionExchange.PublicKey
        
        'Prepare local public key buffers.
        Dim LocalEncryptionPublicKeyBuffer As Byte() = LocalEncryptionPublicKey.ToByteArray()
        Dim LocalDecryptionPublicKeyBuffer As Byte() = LocalDecryptionPublicKey.ToByteArray()
        
        'Determine padding requirements for the local public key buffers. Maximum length of 257 bytes of padding.
        Dim LocalEncryptionTokenPaddingLength As Byte = CByte(257 - LocalEncryptionPublicKeyBuffer.Length)
        Dim LocalDecryptionTokenPaddingLength As Byte = CByte(257 - LocalDecryptionPublicKeyBuffer.Length)
        
        'Pack local padding values.
        LocalTransformBuffer(000) = LocalEncryptionTokenPaddingLength
        LocalTransformBuffer(257) = LocalDecryptionTokenPaddingLength
        
        'Pack local public key buffers.
        Buffer.BlockCopy(LocalEncryptionPublicKeyBuffer, 0, LocalTransformBuffer, 1, LocalEncryptionPublicKeyBuffer.Length)
        Buffer.BlockCopy(LocalDecryptionPublicKeyBuffer, 0, LocalTransformBuffer, 258, LocalDecryptionPublicKeyBuffer.Length)
        
        'Exchange data.
        LambdaThread(Of Lightning.Socket, Byte()).Start(AddressOf AsyncSend, Socket, LocalTransformBuffer)
        Do Until Socket.Available > 0 Or Socket.Connected = False Or Watchdog.Elapsed >= Timeout
            LambdaThread.Yield()
        Loop
        Socket.Read(RemoteTransformBuffer, 0, 516, Net.Sockets.SocketFlags.None, Nothing)
        
        'Unpack remote padding values.
        Dim RemoteEncryptionTokenPaddingLength As Byte = RemoteTransformBuffer(0)
        Dim RemoteDecryptionTokenPaddingLength As Byte = RemoteTransformBuffer(257)
        
        'Prepare remote public key buffers.
        Dim RemoteEncryptionPublicKeyBuffer((257 - RemoteEncryptionTokenPaddingLength) - 1) As Byte
        Dim RemoteDecryptionPublicKeyBuffer((257 - RemoteDecryptionTokenPaddingLength) - 1) As Byte
        
        'Unpack remote public key buffers.
        Buffer.BlockCopy(RemoteTransformBuffer, 1, RemoteEncryptionPublicKeyBuffer, 0, RemoteEncryptionPublicKeyBuffer.Length)
        Buffer.BlockCopy(RemoteTransformBuffer, 258, RemoteDecryptionPublicKeyBuffer, 0, RemoteDecryptionPublicKeyBuffer.Length)
        
        'Prepare remote public keys.
        Dim RemoteEncryptionPublicKey As New BigInteger(RemoteEncryptionPublicKeyBuffer)
        Dim RemoteDecryptionPublicKey As New BigInteger(RemoteDecryptionPublicKeyBuffer)
        
        'Derive shared secret seeds.
        Dim DecryptionSeedValue As BigInteger, EncryptionSeedValue As BigInteger
        If Socket.ClientSide = True Then
            EncryptionSeedValue = LocalEncryptionExchange.Derive(RemoteEncryptionPublicKey)
            DecryptionSeedValue = LocalDecryptionExchange.Derive(RemoteDecryptionPublicKey)
        Else
            EncryptionSeedValue = LocalDecryptionExchange.Derive(RemoteDecryptionPublicKey)
            DecryptionSeedValue = LocalEncryptionExchange.Derive(RemoteEncryptionPublicKey)
        End If
        Dim EncryptionSeed As Byte() = EncryptionSeedValue.ToByteArray()
        Dim DecryptionSeed As Byte()  = DecryptionSeedValue.ToByteArray()
        
        Dim AESEncryptor As Security.Cryptography.Aes = Security.Cryptography.Aes.Create()
        'Derive encryption seeds from secret shared seeds.
        With New Random(EncryptionSeed)
            .ByteNext(LocalHeaderSeed)
            .ByteNext(LocalDataSeed)
            .ByteNext(AESEncryptor.Key)
            .ByteNext(AESEncryptor.IV)
        End With
        
        Dim AESDecryptor As Security.Cryptography.Aes = Security.Cryptography.Aes.Create()
        'Derive decryption seeds from secret shared seeds.
        With New Random(DecryptionSeed)
            .ByteNext(RemoteHeaderSeed)
            .ByteNext(RemoteDataSeed)
            .ByteNext(AESDecryptor.Key)
            .ByteNext(AESDecryptor.IV)
        End With
        
        Dim AESEncryptorTransform As Security.Cryptography.ICryptoTransform = AESEncryptor.CreateEncryptor()
        Dim AESDecryptorTransform As Security.Cryptography.ICryptoTransform = AESDecryptor.CreateDecryptor()
        
        Array.Resize(LocalTransformBuffer, MaelstromHeaderReference.Length)
        Array.Resize(RemoteTransformBuffer, MaelstromHeaderReference.Length)
        
        Buffer.BlockCopy(MaelstromHeaderReference, 0, LocalTransformBuffer, 0, MaelstromHeaderReference.Length)
        AESEncryptorTransform.TransformBlock(LocalTransformBuffer, 0, MaelstromHeaderReference.Length, LocalTransformBuffer, 0)
        LambdaThread(Of Lightning.Socket, Byte()).Start(AddressOf AsyncSend, Socket, LocalTransformBuffer)
        Do Until Socket.Available > 0 Or Socket.Connected = False Or Watchdog.Elapsed >= Timeout
            LambdaThread.Yield()
        Loop
        Socket.Read(RemoteTransformBuffer, 0, MaelstromHeaderReference.Length, Net.Sockets.SocketFlags.None, Nothing)
        AESDecryptorTransform.TransformBlock(RemoteTransformBuffer, 0, MaelstromHeaderReference.Length, RemoteTransformBuffer, 0)
        
        If Watchdog.Elapsed >= Timeout Then
            SyncResult = Result.ValidationFailure
        Else
            If Validate(RemoteTransformBuffer, MaelstromHeaderReference, MaelstromHeaderReference.Length) = True Then
                SyncResult = Result.Ok
            Else
                SyncResult = Result.ValidationFailure
            End If
        End If
        
        AESDecryptorTransform.Dispose()
        AESEncryptorTransform.Dispose()
        AESDecryptor.Dispose()
        AESEncryptor.Dispose()
        Return SyncResult
    End Function
    
    Friend Function GetTransform() As Transform
        If SyncResult = Result.ValidationFailure Then Return Nothing
        Return New Transform(LocalHeaderSeed, RemoteHeaderSeed, LocalDataSeed, RemoteDataSeed)
    End Function
    
    Private Sub AsyncSend(Socket As Lightning.Socket, Buffer As Byte())
        Socket.Write(Buffer, 0, Buffer.Length, Net.Sockets.SocketFlags.None, Nothing)
    End Sub
    
    <MethodImpl(MethodImplOptions.NoInlining And MethodImplOptions.NoOptimization)>
    Private Function Validate(A As Byte(), B As Byte(), Length As Int32) As Boolean
        Dim Validated = True
        For Index = 0 To Length - 1
            If A(Index) <> B(Index) Then Validated = False
        Next
        Return Validated
    End Function
End Class