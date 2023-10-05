Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Maelstrom.Lightning

Friend Partial Class Handshake
    Friend Sub New(IdentityTokens As Byte()(), IdentityToken As Byte())
        Me.LocalIdentityTokens.Clear()
        Me.LocalIdentityTokens.AddRange(IdentityTokens)
        LocalIdentityToken = IdentityToken
    End Sub
    
    Friend Function GetRemoteIdentificationToken() As Byte()
        If SyncResult = Result.NotReady Then Return Nothing
        Return RemoteIdentityToken
    End Function
    
    Friend Function GetLocalIdentificationToken() As Byte()
        If SyncResult = Result.NotReady Then Return Nothing
        Return LocalIdentityToken
    End Function
    
    Friend Function Perform(Socket As Lightning.Socket) As Result
        Array.Resize(LocalTransformBuffer, 516)
        Array.Resize(RemoteTransformBuffer, 516)
        
        Array.Resize(LocalHeaderSeed, 256)
        Array.Resize(LocalDataSeed, 256)
        Array.Resize(RemoteHeaderSeed, 256)
        Array.Resize(RemoteDataSeed, 256)
        Array.Resize(RemoteIdentityToken, IdentityTokenSize)
        Timeout = TimeSpan.FromMilliseconds(1000)
        SyncResult = Result.NotReady
        Watchdog = New Stopwatch
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
        Watchdog.Restart()
        LambdaThread(Of Lightning.Socket, Byte()).Start(AddressOf AsyncSend, Socket, LocalTransformBuffer)
        Do Until Socket.Available > 0 Or Socket.Connected = False Or Watchdog.Elapsed >= Timeout
            LambdaThread.Yield()
        Loop
        Dim ReadAmountA As Int32 = Socket.Read(RemoteTransformBuffer, 0, RemoteTransformBuffer.Length, Net.Sockets.SocketFlags.None, Nothing)
        If ReadAmountA < RemoteTransformBuffer.Length OrElse Watchdog.Elapsed >= Timeout  Then
            If SyncResult = Result.NotReady And Socket.Connected = True Then SyncResult = Result.TransferFailure
            If SyncResult = Result.NotReady And Socket.Connected = False Then SyncResult = Result.SocketDead
        End If
        
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
        
        'Derive encryption logic from secret shared seeds.
        Dim AESEncryptor As Security.Cryptography.Aes = Security.Cryptography.Aes.Create()
        AESEncryptor.Padding = Security.Cryptography.PaddingMode.None
        With New Random(EncryptionSeed)
            .ByteNext(LocalHeaderSeed)
            .ByteNext(LocalDataSeed)
            .ByteNext(AESEncryptor.Key)
            .ByteNext(AESEncryptor.IV)
        End With
        
        'Derive decryption logic from secret shared seeds.
        Dim AESDecryptor As Security.Cryptography.Aes = Security.Cryptography.Aes.Create()
        AESDecryptor.Padding = Security.Cryptography.PaddingMode.None
        With New Random(DecryptionSeed)
            .ByteNext(RemoteHeaderSeed)
            .ByteNext(RemoteDataSeed)
            .ByteNext(AESDecryptor.Key)
            .ByteNext(AESDecryptor.IV)
        End With
        
        'Prepare for secure validation of the newly established connection.
        Dim AESEncryptorTransform As Security.Cryptography.ICryptoTransform = AESEncryptor.CreateEncryptor()
        Dim AESDecryptorTransform As Security.Cryptography.ICryptoTransform = AESDecryptor.CreateDecryptor()
        Array.Resize(LocalTransformBuffer, HeaderReference.Length + IdentityTokenSize)
        Array.Resize(RemoteTransformBuffer, HeaderReference.Length + IdentityTokenSize)
        
        'Pack up validation data.
        Buffer.BlockCopy(HeaderReference, 0, LocalTransformBuffer, 0, HeaderReference.Length)
        Buffer.BlockCopy(LocalIdentityToken, 0, LocalTransformBuffer, HeaderReference.Length, IdentityTokenSize)
        
        'Securely exchange validation data.
        AESEncryptorTransform.TransformBlock(LocalTransformBuffer, 0, LocalTransformBuffer.Length, LocalTransformBuffer, 0)
        Watchdog.Restart()
        LambdaThread(Of Lightning.Socket, Byte()).Start(AddressOf AsyncSend, Socket, LocalTransformBuffer)
        Do Until Socket.Available > 0 Or Socket.Connected = False Or Watchdog.Elapsed >= Timeout
            LambdaThread.Yield()
        Loop
        Dim ReadAmountB As Int32 = Socket.Read(RemoteTransformBuffer, 0, RemoteTransformBuffer.Length, Net.Sockets.SocketFlags.None, Nothing)
        If ReadAmountB < RemoteTransformBuffer.Length OrElse Watchdog.Elapsed >= Timeout  Then
            If SyncResult = Result.NotReady And Socket.Connected = True Then SyncResult = Result.TransferFailure
            If SyncResult = Result.NotReady And Socket.Connected = False Then SyncResult = Result.SocketDead
        End If
        AESDecryptorTransform.TransformBlock(RemoteTransformBuffer, 0, RemoteTransformBuffer.Length, RemoteTransformBuffer, 0)
        
        'Perform validation.
        If Compare(RemoteTransformBuffer, HeaderReference, HeaderReference.Length) = True Then
            Buffer.BlockCopy(RemoteTransformBuffer, HeaderReference.Length, RemoteIdentityToken, 0, IdentityTokenSize)
            If LocalIdentityTokens.Count > 0 
                For Each Item In LocalIdentityTokens
                    Dim Compared As Boolean = Compare(RemoteIdentityToken, Item, IdentityTokenSize)
                    If Compared = True And Socket.ClientSide = True
                        If SyncResult = Result.NotReady Then SyncResult = Result.Ok 'LocalIdentityTokens functions as a trust-list on a client. Proceed if remote token is found within it.
                    Else If Compared = False And Socket.ClientSide = False
                        If SyncResult = Result.NotReady Then SyncResult = Result.Ok 'LocalIdentityTokens functions as a deny-list on a server. Proceed if remote token is not found within it.
                    Else
                        If SyncResult = Result.NotReady Then SyncResult = Result.IdentityFailure 'Identity is untrustworthy. Do not proceed.
                    End If
                Next
            Else
                If Socket.ClientSide = True
                    If SyncResult = Result.NotReady Then SyncResult = Result.IdentityFailure 'Do not proceed if LocalIdentityTokens list is empty on a client.
                Else
                    If SyncResult = Result.NotReady Then SyncResult = Result.Ok 'proceed if LocalIdentityTokens list is empty on a server.
                End If
            End If
        Else
            If SyncResult = Result.NotReady Then SyncResult = Result.ValidationFailure
        End If
        
        AESDecryptorTransform.Dispose()
        AESEncryptorTransform.Dispose()
        AESDecryptor.Dispose()
        AESEncryptor.Dispose()
        
        If Socket.Connected = False And SyncResult = Result.Ok Then SyncResult = Result.SocketDead
        Return SyncResult
    End Function
    
    Friend Function GetTransform() As Transform
        If SyncResult <> Result.Ok Then Return Nothing
        Return New Transform(LocalHeaderSeed, RemoteHeaderSeed, LocalDataSeed, RemoteDataSeed)
    End Function
    
    Private Sub AsyncSend(Socket As Lightning.Socket, Buffer As Byte())
        Socket.Write(Buffer, 0, Buffer.Length, Net.Sockets.SocketFlags.None, Nothing)
    End Sub
    
    <MethodImpl(MethodImplOptions.NoInlining And MethodImplOptions.NoOptimization)>
    Private Function Compare(A As Byte(), B As Byte(), Length As Int32) As Boolean
        Dim Equals = True
        For Index = 0 To Length - 1
            If A(Index) <> B(Index) Then Equals = False
        Next
        Return Equals
    End Function
    
    Public Shared Function GenerateToken() As Byte()
        Dim NewToken(IdentityTokenSize - 1) As Byte
        Dim Noise As Security.Cryptography.RandomNumberGenerator = Security.Cryptography.RandomNumberGenerator.Create()
        Noise.GetBytes(NewToken) : Noise.Dispose()
        Return NewToken
    End Function
End Class