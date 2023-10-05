Public Partial Class Socket : Implements IDisposable
    Friend Sub New(Socket As Lightning.Socket, Transform As Transform, Result As Result, LocalToken As Byte(), RemoteToken As Byte())
        Me.SyncResult = Result
        LocalIdentificationToken = LocalToken
        RemoteIdentificationToken = RemoteToken
        BaseSocket = Socket
        TransformReference = Transform
        Subsockets = New Dictionary(Of UInteger, Subsocket)
        
        Array.Resize(ReadHeaderBuffer,   Header.RequiredBufferLength)
        Array.Resize(WriteHeaderBuffer,  Header.RequiredBufferLength)
        Array.Resize(BufferHeaderBuffer, Header.RequiredBufferLength)
        
        ReadLock   = New Object
        WriteLock  = New Object
        BufferLock = New Object
    End Sub
    
    Private Sub Pump()
        SyncLock ReadLock
            If Connected = False Then Return
            
            Dim ReadHeader As New Header(TransformReference) : With ReadHeader
                If BaseSocket.Available < Header.RequiredBufferLength Then Return
                BaseSocket.Read(ReadHeaderBuffer, 0, Header.RequiredBufferLength, Net.Sockets.SocketFlags.None, Nothing)
                .Unpack(ReadHeaderBuffer, 0) : EnsureSubsocket(.Subsocket)
                Dim ReadDataBuffer As Byte() = Subsockets(.Subsocket).ReadDataBuffer
                If ReadDataBuffer Is Nothing OrElse ReadDataBuffer.Length < .Max Then Array.Resize(ReadDataBuffer, .Max) 
                Do Until BaseSocket.Available >= .Transformed Or Connected = False : Loop
                BaseSocket.Read(ReadDataBuffer, 0, .Transformed, Net.Sockets.SocketFlags.None, Nothing)
                If .Compressed = 0 And .Encrypted > 0
                    TransformReference.DataUnpackTransform.TransformBlock(ReadDataBuffer, 0, .Encrypted, ReadDataBuffer, 0)
                ElseIf .Compressed > 0 And .Encrypted = 0
                    Decompress(ReadDataBuffer, ReadDataBuffer, .Compressed)
                ElseIf .Compressed > 0 And .Encrypted > 0
                    TransformReference.DataUnpackTransform.TransformBlock(ReadDataBuffer, 0, .Encrypted, ReadDataBuffer, 0)
                    Decompress(ReadDataBuffer, ReadDataBuffer, .Compressed)
                End If
                SyncLock Subsockets(.Subsocket).BufferLock 
                    Subsockets(.Subsocket).Buffer.Write(ReadHeaderBuffer, 0, Header.RequiredBufferLength)
                    Subsockets(.Subsocket).Buffer.Write(ReadDataBuffer, 0, .Raw)
                End SyncLock
            End With
        End SyncLock
    End Sub
    
    Public Sub Read(ByVal Subsocket As UInt32, ByRef Data As Byte())
        SyncLock Subsockets(Subsocket).BufferLock : With New Header(TransformReference)
            Subsockets(Subsocket).Buffer.Read(BufferHeaderBuffer, 0, 0, Header.RequiredBufferLength)
            .UnpackRaw(BufferHeaderBuffer, 0) 
            If Data.Length < .Raw Then Array.Resize(Data, .Raw)
            Subsockets(Subsocket).Buffer.Read(Data, 0, Header.RequiredBufferLength, .Raw)
            Subsockets(Subsocket).Buffer.Shift(Header.RequiredBufferLength + .Raw)
        End With : End SyncLock
    End Sub
    
    Public Sub Write(ByVal Subsocket As UInt32, ByRef Data As Byte())
        EnsureSubsocket(Subsocket)
        SyncLock WriteLock : With New Header(TransformReference)
            If Connected = False Then Return
            .Subsocket   = Subsocket
            .Raw         = Data.Length
            .Compressed  = 0
            .Encrypted   = 0
            .Transformed = .Raw
            
            Dim WriteDataBuffer As Byte() = Subsockets(Subsocket).WriteDataBuffer
            If WriteDataBuffer Is Nothing OrElse WriteDataBuffer.Length < .Raw Then Array.Resize(WriteDataBuffer, .Raw)
            Buffer.BlockCopy(Data, 0, WriteDataBuffer, 0, .Raw)
            If Subsockets(.Subsocket).Compressed = False And Subsockets(.Subsocket).Encrypted = True
                .Encrypted   = GetBlockSize(.Raw)
                .Transformed = .Encrypted
                TransformReference.DataPackTransform.TransformBlock(WriteDataBuffer, 0, .Encrypted, WriteDataBuffer, 0)
            ElseIf Subsockets(.Subsocket).Compressed = True And Subsockets(.Subsocket).Encrypted = False
                .Compressed  = Compress(WriteDataBuffer, WriteDataBuffer, .Raw)
                .Transformed = .Compressed
            ElseIf Subsockets(.Subsocket).Compressed = True And Subsockets(.Subsocket).Encrypted = True
                .Compressed  = Compress(WriteDataBuffer, WriteDataBuffer, .Raw)
                .Encrypted   = GetBlockSize(.Compressed)
                .Transformed = .Encrypted
                TransformReference.DataPackTransform.TransformBlock(WriteDataBuffer, 0, .Encrypted, WriteDataBuffer, 0)
            End If
            .Max = Math.Max(Math.Max(.Compressed, .Encrypted), .Raw)
            .Pack(WriteHeaderBuffer, 0)
            
            BaseSocket.Write(WriteHeaderBuffer, 0, Header.RequiredBufferLength, Net.Sockets.SocketFlags.None, Nothing)
            BaseSocket.Write(WriteDataBuffer, 0, .Transformed, Net.Sockets.SocketFlags.None, Nothing)
            WriteDataBuffer = Nothing
        End With : End SyncLock
    End Sub
    Public Sub Create(Subsocket As UInt32)
        EnsureSubsocket(Subsocket)
    End Sub
    
    Public Function Exists(ByVal Subsocket As UInt32) As Boolean
        SyncLock BufferLock : Return Subsockets.ContainsKey(Subsocket) : End SyncLock
    End Function
    
    Public Sub Disconnect()
        SyncLock ReadLock : Synclock WriteLock : Synclock BufferLock
            If Connected = True Then BaseSocket.Disconnect()
        End SyncLock : End SyncLock : End SyncLock
    End Sub
    Private Sub EnsureSubsocket(Subsocket As UInt32)
        SyncLock BufferLock
            If Connected = False Then Return
            If Subsockets.ContainsKey(Subsocket) = False Then Subsockets.Add(Subsocket, New Subsocket)
        End SyncLock
    End Sub
    Private Function GetBlockSize(Size As Int32) As Int32
        Return CInt(Math.Ceiling(Size / 16)) * 16
    End Function
    Private Shared Function Compress(ByRef Input as Byte(), ByRef Output as Byte(), ByVal Count As Int32) As Int32
        Dim CompressedMemStream = New IO.MemoryStream()
        Dim Compressor = New IO.Compression.GZipStream(CompressedMemStream, IO.Compression.CompressionMode.Compress)
        
        Compressor.Write(Input, 0, Count)
        Compressor.Close()
        Dim TempBuffer = CompressedMemStream.ToArray()
        
        If Output.Length < TempBuffer.Length Then Redim Preserve Output(TempBuffer.Length - 1)
        Buffer.BlockCopy(TempBuffer,0,Output,0,TempBuffer.Length)
        
        Compressor.Dispose()
        CompressedMemStream.Dispose()
        Return TempBuffer.Length
    End Function
    Private Shared Sub Decompress(ByRef Input As Byte(), ByRef Output as Byte(), ByVal Count As Int32)
        Dim InternalBuffer As Byte() = Nothing
        Array.Resize(InternalBuffer, Count)
        Buffer.BlockCopy(Input, 0, InternalBuffer, 0, Count)
        Dim CompressedMemStream = New IO.MemoryStream(InternalBuffer)
        CompressedMemStream.Position = 0
        Dim Decompressor = New IO.Compression.GZipStream(CompressedMemStream, IO.Compression.CompressionMode.Decompress)
       
        Dim DecompressedMemStream = New IO.MemoryStream()
        
        Decompressor.CopyTo(DecompressedMemStream)
        Dim TempBuffer = DecompressedMemStream.ToArray()
        If Output.Length < TempBuffer.Length Then Redim Preserve Output(TempBuffer.Length - 1)
        Buffer.BlockCopy(TempBuffer,0,Output,0,TempBuffer.Length)
        
        CompressedMemStream.Dispose()
        Decompressor.Dispose()
        DecompressedMemStream.Dispose()
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock ReadLock : Synclock WriteLock : Synclock BufferLock
            Array.Clear(ReadHeaderBuffer,   0, 32) : ReadHeaderBuffer   = Nothing
            Array.Clear(WriteHeaderBuffer,  0, 16) : WriteHeaderBuffer  = Nothing
            Array.Clear(BufferHeaderBuffer, 0, 32) : BufferHeaderBuffer = Nothing
            For Each Entry In Subsockets : Entry.Value.Dispose : Next : Subsockets.Clear : Subsockets = Nothing
            If TransformReference IsNot Nothing Then TransformReference.Dispose : TransformReference = Nothing
        End SyncLock : End SyncLock : End SyncLock
        'ReadLock = Nothing : WriteLock = Nothing : BufferLock = Nothing
    End Sub
End Class