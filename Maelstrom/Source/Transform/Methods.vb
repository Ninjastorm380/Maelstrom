Friend Partial Class Transform : Implements IDisposable
    Friend Sub New(LocalHeaderSeed As Byte(), RemoteHeaderSeed As Byte(), LocalDataSeed As Byte(), RemoteDataSeed As Byte())
        Dim LocalHeaderRandom As New Lightning.Random(LocalHeaderSeed)
        Array.Resize(HeaderPackKey, 32) : LocalHeaderRandom.ByteNext(HeaderPackKey)
        Array.Resize(HeaderPackIV, 16) : LocalHeaderRandom.ByteNext(HeaderPackIV)
        LocalHeaderRandom = Nothing
        
        Dim RemoteHeaderRandom As New Lightning.Random(RemoteHeaderSeed)
        Array.Resize(HeaderUnpackKey, 32) : RemoteHeaderRandom.ByteNext(HeaderUnpackKey)
        Array.Resize(HeaderUnpackIV, 16) : RemoteHeaderRandom.ByteNext(HeaderUnpackIV)
        RemoteHeaderRandom = Nothing
        
        Dim LocalDataRandom As New Lightning.Random(LocalDataSeed)
        Array.Resize(DataPackKey, 32) : LocalDataRandom.ByteNext(DataPackKey)
        Array.Resize(DataPackIV, 16) : LocalDataRandom.ByteNext(DataPackIV)
        LocalDataRandom = Nothing
        
        Dim RemoteDataRandom As New Lightning.Random(RemoteDataSeed)
        Array.Resize(DataUnpackKey, 32) : RemoteDataRandom.ByteNext(DataUnpackKey)
        Array.Resize(DataUnpackIV, 16) : RemoteDataRandom.ByteNext(DataUnpackIV)
        RemoteDataRandom = Nothing
        
        HeaderPackAlgorithm = Security.Cryptography.Aes.Create() : With HeaderPackAlgorithm
            .Key = HeaderPackKey
            .IV  = HeaderPackIV
            .Padding = Security.Cryptography.PaddingMode.None
        End With : HeaderPackTransform = HeaderPackAlgorithm.CreateEncryptor
        
        HeaderUnpackAlgorithm = Security.Cryptography.Aes.Create() : With HeaderUnpackAlgorithm
            .Key = HeaderUnpackKey
            .IV  = HeaderUnpackIV
            .Padding = Security.Cryptography.PaddingMode.None
        End With : HeaderUnpackTransform = HeaderUnpackAlgorithm.CreateDecryptor
        
        DataPackAlgorithm = Security.Cryptography.Aes.Create() : With DataPackAlgorithm
            .Key = DataPackKey
            .IV  = DataPackIV
            .Padding = Security.Cryptography.PaddingMode.None
        End With : DataPackTransform = DataPackAlgorithm.CreateEncryptor
        
        DataUnpackAlgorithm = Security.Cryptography.Aes.Create() : With DataUnpackAlgorithm
            .Key = DataUnpackKey
            .IV  = DataUnpackIV
            .Padding = Security.Cryptography.PaddingMode.None
        End With : DataUnpackTransform = DataUnpackAlgorithm.CreateDecryptor
    End Sub

    Friend Sub Dispose() Implements IDisposable.Dispose
        HeaderPackAlgorithm   .Dispose
        HeaderUnpackAlgorithm .Dispose
        HeaderPackTransform   .Dispose
        HeaderUnpackTransform .Dispose
        DataPackAlgorithm     .Dispose
        DataUnpackAlgorithm   .Dispose
        DataPackTransform     .Dispose
        DataUnpackTransform   .Dispose
        
        Array.Clear(HeaderPackKey,   0, 32) : HeaderPackKey   = Nothing
        Array.Clear(HeaderPackIV,    0, 16) : HeaderPackIV    = Nothing
        Array.Clear(HeaderUnpackKey, 0, 32) : HeaderUnpackKey = Nothing
        Array.Clear(HeaderUnpackIV,  0, 16) : HeaderUnpackIV  = Nothing
        Array.Clear(DataPackKey,     0, 32) : DataPackKey     = Nothing
        Array.Clear(DataPackIV,      0, 16) : DataPackIV      = Nothing
        Array.Clear(DataUnpackKey,   0, 32) : DataUnpackKey   = Nothing
        Array.Clear(DataUnpackIV,    0, 16) : DataUnpackIV    = Nothing
    End Sub
End Class