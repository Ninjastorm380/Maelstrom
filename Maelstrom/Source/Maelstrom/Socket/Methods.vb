Imports System.Net.Sockets

Public Partial Class Socket : Implements IDisposable
    Friend Sub New(Base As Lightning.Socket)
        ReadLock = New Object
        WriteLock = New Object
        BaseSocket = Base
        Subsockets = New Lightning.Dictionary(Of UInt32, Subsocket)
        BaseDisposed = False
    End Sub

    Private Sub PerformNetworkPumpRead()
        Dim ReadHeader As New Header
        SyncLock ReadLock
            If BaseSocket.Available <= 0 Then Return
            BaseSocket.Read(ReadHeaderBuffer, 0, Header.ByteLength, SocketFlags.None)
            Dim Subsocket As UInt32 : Lightning.UnpackUInt32(ReadHeaderBuffer, 0, Subsocket)
            Subsockets(Subsocket).DecryptHeader(ReadHeaderBuffer, ReadHeaderBuffer, 4, Header.ByteLength - 4)
            ReadHeader.Unpack(ReadHeaderBuffer)
            If ReadDataBuffer Is Nothing OrElse ReadDataBuffer.Length < ReadHeader.WriteLength Then Array.Resize(ReadDataBuffer, ReadHeader.WriteLength)
            BaseSocket.Read(ReadDataBuffer, 0, ReadHeader.WriteLength, SocketFlags.None)
            
            If ReadHeader.CompressedLength = 0 And ReadHeader.EncryptedLength > 0 Then
                Subsockets(ReadHeader.Subsocket).DecryptData(ReadDataBuffer, ReadDataBuffer, 0, ReadHeader.EncryptedLength)
            ElseIf ReadHeader.CompressedLength > 0 And ReadHeader.EncryptedLength = 0 Then
                Decompress(ReadDataBuffer, ReadDataBuffer, ReadHeader.CompressedLength)
            ElseIf ReadHeader.CompressedLength > 0 And ReadHeader.EncryptedLength > 0 Then
                Subsockets(ReadHeader.Subsocket).DecryptData(ReadDataBuffer, ReadDataBuffer, 0, ReadHeader.EncryptedLength)
                Decompress(ReadDataBuffer, ReadDataBuffer, ReadHeader.CompressedLength)
            End If
            
            Subsockets(ReadHeader.Subsocket).Write(ReadHeader, ReadDataBuffer)
        End SyncLock 
        ReadHeader = Nothing
    End Sub
    
    Public Sub Read(Byval Subsocket As UInt32, ByRef Data As Byte())
        Subsockets(Subsocket).Read(New Header, Data)
    End Sub
    
    Public Sub Write(Byval Subsocket As UInt32, ByVal Data As Byte())
        If Subsockets.Contains(Subsocket) = False Then Return
        Dim WriteHeader As New Header
        WriteHeader.Subsocket = Subsocket
        WriteHeader.RawLength = Data.Length
        WriteHeader.WriteLength = Data.Length
        WriteHeader.CompressedLength = 0
        WriteHeader.EncryptedLength = 0
        
        SyncLock WriteLock
            If WriteDataBuffer Is Nothing OrElse WriteDataBuffer.Length < Data.Length Then Array.Resize(WriteDataBuffer, GetBlockSize(Data.Length))
            Buffer.BlockCopy(Data, 0, WriteDataBuffer, 0, Data.Length)
            WriteHeader.Pack(WriteHeaderBuffer)
            If Subsockets(Subsocket).Compressed = False And Subsockets(Subsocket).Encrypted = True Then
                WriteHeader.EncryptedLength = GetBlockSize(Data.Length)
                WriteHeader.WriteLength = WriteHeader.EncryptedLength
                Subsockets(Subsocket).EncryptData(WriteDataBuffer, WriteDataBuffer, 0, WriteHeader.EncryptedLength)
            ElseIf Subsockets(Subsocket).Compressed = True And Subsockets(Subsocket).Encrypted = False Then
                WriteHeader.CompressedLength = Compress(WriteDataBuffer, WriteDataBuffer, Data.Length)
                WriteHeader.WriteLength = WriteHeader.CompressedLength
            ElseIf Subsockets(Subsocket).Compressed = True And Subsockets(Subsocket).Encrypted = True Then
                WriteHeader.CompressedLength = Compress(WriteDataBuffer, WriteDataBuffer, Data.Length)
                WriteHeader.EncryptedLength = GetBlockSize(WriteHeader.CompressedLength)
                WriteHeader.WriteLength = WriteHeader.EncryptedLength
                Subsockets(Subsocket).EncryptData(WriteDataBuffer, WriteDataBuffer, 0, WriteHeader.EncryptedLength)
            End If
            WriteHeader.Pack(WriteHeaderBuffer)
            Subsockets(Subsocket).EncryptHeader(WriteHeaderBuffer, WriteHeaderBuffer, 4, Header.ByteLength - 4)
            BaseSocket.Write(WriteHeaderBuffer, 0, Header.ByteLength, SocketFlags.None)
            BaseSocket.Write(WriteDataBuffer, 0, WriteHeader.WriteLength, SocketFlags.None)
        End SyncLock
        
        WriteHeader = Nothing
    End Sub
    
    Public Function Add(Subsocket As UInt32) As Boolean
        If Subsockets.Contains(Subsocket) = False Then
            Dim SyncResult As Boolean = False
            SyncLock ReadLock : SyncLock WriteLock
                Dim NewSubsocket As New Subsocket(Subsocket)
                SyncResult = NewSubsocket.Sync(BaseSocket)
                If SyncResult = True Then Subsockets.Add(Subsocket, NewSubsocket)
            End SyncLock : End SyncLock
            Return SyncResult
        Else
            Return True
        End If
    End Function
    
    Public Sub Remove(Subsocket As UInt32)
        If Subsockets.Contains(Subsocket) = True Then
            Subsockets(Subsocket).Dispose()
            Subsockets.Remove(Subsocket)
        End If
    End Sub
    
    Public Sub Disconnect()
        BaseSocket.Disconnect()
    End Sub
    
    Private Function GetBlockSize(RawSize As Int32) As Int32
        Return CInt(Math.Ceiling(RawSize / AESBlockSize)) * AESBlockSize
    End Function
    
    Public Sub Dispose() Implements IDisposable.Dispose
        If BaseDisposed = True Then Return
        For Each Item In Subsockets.Values
            Item.Dispose()
        Next
        ReadLock = Nothing
        WriteLock = Nothing
        BaseSocket.Dispose()
        BaseDisposed = True
    End Sub
End Class