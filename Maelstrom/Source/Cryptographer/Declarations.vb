Friend Partial Class Cryptographer
    Private CSP as System.Security.Cryptography.AesCryptoServiceProvider
    Private Transform as System.Security.Cryptography.ICryptoTransform
    Private Key(31) as Byte
    Private IV(15) as Byte
    Private TransformType As Type
End Class