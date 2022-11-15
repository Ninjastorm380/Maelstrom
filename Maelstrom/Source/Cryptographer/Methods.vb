Friend Partial Class Cryptographer : Implements IDisposable
    Friend Enum [Type] As Byte
        Encryption = 0
        Decryption = 1
    End Enum
    
    
    Friend Sub New(RNG As VariableRNG, Type As [Type])
        Key = RNG.Next(32)
        IV = RNG.Next(16)
        CSP = New System.Security.Cryptography.AesCryptoServiceProvider With {.Key = Key, .IV = IV, .Padding = System.Security.Cryptography.PaddingMode.None}
        If Type = Type.Decryption Then Transform = CSP.CreateDecryptor()
        If Type = Type.Encryption Then Transform = CSP.CreateEncryptor()
        TransformType = Type
    End Sub
    
    Public Sub TransformBlock(ByRef Input As Byte(), ByRef Output As Byte(), ByVal Offset As UInt32, Byval Length As UInt32)
        Transform.TransformBlock(Input, Offset, Length, Output, Offset)
    End Sub
    
    Public Sub Dispose() Implements IDisposable.Dispose
        CSP.Dispose()
        Transform.Dispose()
        IV = Nothing
        Key = Nothing
    End Sub
End Class