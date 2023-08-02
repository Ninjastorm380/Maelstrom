Friend Partial Class Handshake
    Private TemporaryUTCTime As DateTime
    Private TemporaryRandom As Lightning.Random
    Private TemporaryLocalCryptographer As Cryptographer
    Private TemporaryRemoteCryptographer As Cryptographer
    Private TemporaryCryptoRandom As Security.Cryptography.RandomNumberGenerator
    
    Private LocalHeaderRandomData As Byte()
    Private RemoteHeaderRandomData As Byte()
    Private LocalDataRandomData As Byte()
    Private RemoteDataRandomData As Byte()
    Private LocalHandshakeBuffer As Byte()
    Private RemoteHandshakeBuffer As Byte()
    Private LocalHandshakeTransformBuffer As Byte()
    Private RemoteHandshakeTransformBuffer As Byte()
    
    Private Const MaxSyncDuration As UInt32 = 1000
    Private CurrentSyncAttempt As UInt32 = 0
    Private CurrentSyncResult As UInt32 = 3
    Private SignatureDataIntact As Boolean = False
    Private SignatureVersionMatched As Boolean = False
End Class