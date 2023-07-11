Imports System.Runtime.CompilerServices

Friend Partial Class Handshake
    <MethodImpl(MethodImplOptions.Synchronized)>
    Public Function Perform(Socket As Lightning.Socket) As Result
        CurrentSyncAttempt = 0
        CurrentSyncResult = 3
        Array.Resize(LocalHandshakeBuffer, 128)
        Array.Resize(RemoteHandshakeBuffer, 128)
        Array.Resize(LocalHandshakeTransformBuffer, 128)
        Array.Resize(RemoteHandshakeTransformBuffer, 128)
        Array.Resize(LocalHeaderRandomData, 48)
        Array.Resize(RemoteHeaderRandomData, 48)
        Array.Resize(LocalDataRandomData, 48)
        Array.Resize(RemoteDataRandomData, 48)
        TemporaryCryptoRandom = Security.Cryptography.RandomNumberGenerator.Create()
        TemporaryCryptoRandom.GetBytes(LocalHeaderRandomData)
        TemporaryCryptoRandom.GetBytes(LocalDataRandomData)
        TemporaryCryptoRandom.Dispose()
        Do
            CurrentSyncAttempt += SyncIncrement
            TemporaryUTCTime = DateTime.UtcNow

            TemporaryRandom = New Lightning.Random({
                    TemporaryUTCTime.Year,
                    TemporaryUTCTime.Month,
                    TemporaryUTCTime.Day,
                    TemporaryUTCTime.Hour,
                    TemporaryUTCTime.Minute,
                    TemporaryUTCTime.Second,
                    TemporaryUTCTime.Millisecond\200})
            If Socket.ClientBound = True Then
                TemporaryLocalCryptographer = New Cryptographer(TemporaryRandom, Cryptographer.Type.Encryption)
                TemporaryRemoteCryptographer = New Cryptographer(TemporaryRandom, Cryptographer.Type.Decryption)
            Else
                TemporaryRemoteCryptographer = New Cryptographer(TemporaryRandom, Cryptographer.Type.Decryption)
                TemporaryLocalCryptographer = New Cryptographer(TemporaryRandom, Cryptographer.Type.Encryption)
            End If
            
            Buffer.BlockCopy(MaelstromHeaderRefrence, 0, LocalHandshakeBuffer, 0, 32)
            Buffer.BlockCopy(LocalHeaderRandomData, 0, LocalHandshakeBuffer, 32, 48)
            Buffer.BlockCopy(LocalDataRandomData, 0, LocalHandshakeBuffer, 80, 48)
            
            TemporaryLocalCryptographer.TransformBlock(LocalHandshakeBuffer, LocalHandshakeTransformBuffer, 0, 128)
            Socket.Write(LocalHandshakeTransformBuffer, 0, 128, Net.Sockets.SocketFlags.None)
            Do Until Socket.Available >= 128 : Loop
            Socket.Read(RemoteHandshakeTransformBuffer, 0, 128, Net.Sockets.SocketFlags.None)
            TemporaryRemoteCryptographer.TransformBlock(RemoteHandshakeTransformBuffer, RemoteHandshakeBuffer, 0, 128)
            
            SignatureDataIntact = Lightning.BinaryCompare(MaelstromHeaderRefrence, RemoteHandshakeBuffer, 0, 16)
            SignatureVersionMatched = Lightning.BinaryCompare(MaelstromHeaderRefrence, RemoteHandshakeBuffer, 16, 16)
            
            If SignatureDataIntact = True And SignatureVersionMatched = True
                CurrentSyncResult = 0
                Buffer.BlockCopy(RemoteHandshakeBuffer, 32, RemoteHeaderRandomData, 0, 48)
                Buffer.BlockCopy(RemoteHandshakeBuffer, 80, RemoteDataRandomData, 0, 48)
            ElseIF SignatureDataIntact = True And SignatureVersionMatched = False
                CurrentSyncResult = 2
                Threading.Thread.Sleep(10)
            Else
                CurrentSyncResult = 1
                Threading.Thread.Sleep(60)
            End If
            
            TemporaryLocalCryptographer.Dispose()
            TemporaryRemoteCryptographer.Dispose()
        Loop Until CurrentSyncAttempt >= MaxSyncAttempts Or CurrentSyncResult = 0
        Return CType(CurrentSyncResult, Result)
    End Function
    
    Public Function GetSeeds() As Byte()()
        If CurrentSyncResult <> 0 Then Return Nothing
        Return {LocalHeaderRandomData, RemoteHeaderRandomData, LocalDataRandomData, RemoteDataRandomData}
    End Function
End Class