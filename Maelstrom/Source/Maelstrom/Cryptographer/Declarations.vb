Friend Partial Class Cryptographer
    Private ReadOnly CSP as Security.Cryptography.Aes
    Private ReadOnly Transform as Security.Cryptography.ICryptoTransform
    Private Key(31) as Byte
    Private IV(15) as Byte
End Class