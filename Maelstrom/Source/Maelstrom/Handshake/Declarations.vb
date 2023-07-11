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
    
    Private Const MaxSyncAttempts As UInt32 = 3
    Private CurrentSyncAttempt As UInt32 = 0
    Private CurrentSyncResult As UInt32 = 3
    Private Const SyncIncrement As UInt32 = 1
    Private SignatureDataIntact As Boolean = False
    Private SignatureVersionMatched As Boolean = False
End Class