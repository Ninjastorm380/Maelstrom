Imports System.Security.Cryptography
Imports System.Numerics
Namespace Lightning
    Friend Partial Class KeyExchange : Implements IDisposable
        Public Sub New(Optional P As BigInteger = Nothing, Optional G As BigInteger = Nothing)
            If P = Nothing Then P = DefaultP
            If G = Nothing Then G = DefaultG
            Me.P = P : Me.G = G
            Dim PrivateKeyBytes(255) As Byte
            Dim Noise As RandomNumberGenerator = RandomNumberGenerator.Create()
            Noise.GetBytes(PrivateKeyBytes) : Noise.Dispose()
        
            PrivateKey = BigInteger.Abs(New BigInteger(PrivateKeyBytes))
            PublicKey =  BigInteger.ModPow(G, PrivateKey, P)
            Array.Clear(PrivateKeyBytes, 0, PrivateKeyBytes.Length) : PrivateKeyBytes = Nothing
        End Sub
    
        Public Function Derive(Input As BigInteger) As BigInteger
            Return BigInteger.ModPow(Input, PrivateKey, P)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            P = Nothing
            G = Nothing
            PublicKey = Nothing
            PrivateKey = Nothing
        End Sub
    End Class
End Namespace
