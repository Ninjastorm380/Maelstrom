Friend Partial Class Transform
    Private HeaderPackKey As Byte()
    Private HeaderUnpackKey As Byte()
    Private HeaderPackIV As Byte()
    Private HeaderUnpackIV As Byte()

    Private HeaderPackAlgorithm As Security.Cryptography.Aes
    Private HeaderUnpackAlgorithm As Security.Cryptography.Aes
    Friend HeaderPackTransform As Security.Cryptography.ICryptoTransform
    Friend HeaderUnpackTransform As Security.Cryptography.ICryptoTransform

    Private DataPackKey As Byte()
    Private DataUnpackKey As Byte()
    Private DataPackIV As Byte()
    Private DataUnpackIV As Byte()

    Private DataPackAlgorithm As Security.Cryptography.Aes
    Private DataUnpackAlgorithm As Security.Cryptography.Aes
    Friend DataPackTransform As Security.Cryptography.ICryptoTransform
    Friend DataUnpackTransform As Security.Cryptography.ICryptoTransform
End Class