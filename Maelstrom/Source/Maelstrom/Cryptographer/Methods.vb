Friend Partial Class Cryptographer : Implements IDisposable
    Friend Enum [Type] As Byte
        Encryption = 0
        Decryption = 1
    End Enum
    
    Friend Sub New(RNG As Lightning.Random, Type As [Type])
        Key = RNG.Next(32)
        IV = RNG.Next(16)
        CSP = Security.Cryptography.Aes.Create()
        CSP.Key = Key
        CSP.IV = IV
        CSP.Padding = Security.Cryptography.PaddingMode.None
        If Type = Type.Decryption Then Transform = CSP.CreateDecryptor()
        If Type = Type.Encryption Then Transform = CSP.CreateEncryptor()
    End Sub
    
    Public Sub TransformBlock(ByRef Input As Byte(), ByRef Output As Byte(), ByVal Offset As Integer, Byval Length As Integer)
        Transform.TransformBlock(Input, Offset, Length, Output, Offset)
    End Sub
    
    Public Sub Dispose() Implements IDisposable.Dispose
        CSP.Dispose()
        Transform.Dispose()
        IV = Nothing
        Key = Nothing
    End Sub
End Class