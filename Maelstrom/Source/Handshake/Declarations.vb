Friend Partial Class Handshake
    Private Shared ReadOnly HeaderReference as Byte() = {109, 97, 101, 108, 115, 116, 114, 111, 109, 115, 101, 99, 117, 114, 101, 100,   0,0,0,3, 0,0,0,0, 0,0,0,0, 0,0,0,0}
    Private Shared ReadOnly IdentityTokenSize As Int32 = 32
    Private ReadOnly LocalIdentityTokens As New List(Of Byte())
    Private ReadOnly LocalIdentityToken As Byte()
    Private LocalTransformBuffer As Byte()
    Private RemoteTransformBuffer As Byte()
    
    Private LocalKey As Byte()
    Private LocalIV As Byte()
                    
    Private RemoteKey As Byte()
    Private RemoteIV As Byte()
    
    Private LocalHeaderSeed As Byte()
    Private LocalDataSeed As Byte()
    
    Private RemoteHeaderSeed As Byte()
    Private RemoteDataSeed As Byte()
    
    Private CRNG As Security.Cryptography.RandomNumberGenerator
    Private LocalBlockCipher As Security.Cryptography.Aes
    Private LocalTransform As Security.Cryptography.ICryptoTransform
    Private RemoteBlockCipher As Security.Cryptography.Aes
    Private RemoteTransform As Security.Cryptography.ICryptoTransform
    
    Private AttemptCounter As Int32
    Private Watchdog As Stopwatch
    Private SyncResult As Result
    Private Timeout As Timespan

    Private RemoteIdentityToken As Byte()
End Class