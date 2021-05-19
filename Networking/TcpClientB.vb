Public Class TcpClient : Inherits Net.Sockets.TcpClient
    Private LEndpoint As Net.IPEndPoint
    Private REndpoint As Net.IPEndPoint
    Private BaseStream As Net.Sockets.NetworkStream
    Private AESR As Security.Cryptography.AesCryptoServiceProvider
    Private AESW As Security.Cryptography.AesCryptoServiceProvider
    Private AESR_TRANSFORM As Security.Cryptography.ICryptoTransform
    Private AESW_TRANSFORM As Security.Cryptography.ICryptoTransform
    Private AESR_SYNC As Object = New Object
    Private AESW_SYNC As Object = New Object
    Private AESR_STREAM As System.Security.Cryptography.CryptoStream
    Private AESW_STREAM As System.Security.Cryptography.CryptoStream
    Private AESR_BYTEBLOCKSIZE As Integer
    Private AESW_BYTEBLOCKSIZE As Integer
    Private PingReplyReceived As Boolean = False
    Private ReadBuffer As MemoryQueueStream
    Private WriteBuffer As MemoryQueueStream
    Private BaseConnected As Boolean = False
    Public ReadOnly Property HasMessage As Boolean
        Get
            SyncLock AESR_SYNC
                Return ReadBuffer.Length > 0
            End SyncLock
        End Get
    End Property

    Public Sub New(Socket As Net.Sockets.Socket)
        MyBase.New(Socket.AddressFamily)
        Client = Socket
        Initialize()
    End Sub

    'creates and connects a brand new TCPClient.'
    Public Sub New(Endpoint As Net.IPEndPoint)
        MyBase.New(Endpoint.AddressFamily)
        MyBase.Connect(Endpoint)
        Initialize()
    End Sub

    Public Shadows ReadOnly Property Connected As Boolean
        Get
            If BaseConnected = False Then Return False
            SyncLock AESR_SYNC
                SyncLock AESW_SYNC
                    If Client.Connected = False Then
                        BaseConnected = False
                        Return False
                    End If
                    Dim Flag1 As Boolean = Client.Poll(0, Net.Sockets.SelectMode.SelectRead)
                    Dim Flag2 As Boolean = (Client.Available = 0)
                    If Not Flag1 Then Return True
                    If Not Flag2 Then Return True
                    If Client IsNot Nothing Then
                        Dim PingTimeoutWatchLimiter As New ThreadLimiter(40)
                        Dim PingTimeoutLimit As Integer = 16
                        Dim PingTimeoutCount As Integer = 0
                        PingReplyReceived = False
                        SendMessageInternal(1)
                        Do Until PingTimeoutLimit = PingTimeoutCount
                            If PingReplyReceived = True Then
                                PingReplyReceived = False
                                Return True
                            End If
                            PingTimeoutCount += 1
                            PingTimeoutWatchLimiter.Limit()
                        Loop
                        PingReplyReceived = False
                    End If
                    BaseConnected = False
                    Return False
                End SyncLock
            End SyncLock
        End Get
    End Property
    Private Sub Initialize()
        BaseConnected = True
        REndpoint = CType(Client.RemoteEndPoint, Net.IPEndPoint)
        LEndpoint = CType(Client.LocalEndPoint, Net.IPEndPoint)
        BaseStream = Me.GetStream
        ReadBuffer = New MemoryQueueStream
        WriteBuffer = New MemoryQueueStream

        Dim Flusher As New Threading.Thread(AddressOf BufferFlusherLoop)
        Flusher.Start()

        SyncLock AESR_SYNC
            SyncLock AESW_SYNC
                If AESR_TRANSFORM IsNot Nothing Then AESR_TRANSFORM.Dispose()
                If AESW_TRANSFORM IsNot Nothing Then AESW_TRANSFORM.Dispose()
                If AESR IsNot Nothing Then AESR.Dispose()
                If AESW IsNot Nothing Then AESW.Dispose()
                Dim PMAXPORTION As Integer = CInt(Math.Floor(Int32.MaxValue / 7))
                Dim PSnapshot As DateTime = DateTime.UtcNow
                Dim PRYERNG As New Random(PSnapshot.Year)
                Dim PRMORNG As New Random(PSnapshot.Month)
                Dim PRDARNG As New Random(PSnapshot.Day)
                Dim PRHORNG As New Random(PSnapshot.Hour)
                Dim PRMIRNG As New Random(PSnapshot.Minute)
                Dim PRSERNG As New Random(PSnapshot.Second)
                Dim PRPORTRNG As New Random(REndpoint.Port)
                Dim PReaderRNGInitSeed As Integer = PRYERNG.Next(PMAXPORTION) +
                                                    PRMORNG.Next(PMAXPORTION) +
                                                    PRDARNG.Next(PMAXPORTION) +
                                                    PRHORNG.Next(PMAXPORTION) +
                                                    PRMIRNG.Next(PMAXPORTION) +
                                                    PRSERNG.Next(PMAXPORTION) +
                                                    PRPORTRNG.Next(PMAXPORTION)
                Dim PReaderInit As New Random(PReaderRNGInitSeed)
                Dim PWYERNG As New Random(PSnapshot.Year)
                Dim PWMORNG As New Random(PSnapshot.Month)
                Dim PWDARNG As New Random(PSnapshot.Day)
                Dim PWHORNG As New Random(PSnapshot.Hour)
                Dim PWMIRNG As New Random(PSnapshot.Minute)
                Dim PWSERNG As New Random(PSnapshot.Second)
                Dim PWPORTRNG As New Random(LEndpoint.Port)
                Dim PWriterRNGInitSeed As Integer = PWYERNG.Next(PMAXPORTION) +
                                                    PWMORNG.Next(PMAXPORTION) +
                                                    PWDARNG.Next(PMAXPORTION) +
                                                    PWHORNG.Next(PMAXPORTION) +
                                                    PWMIRNG.Next(PMAXPORTION) +
                                                    PWSERNG.Next(PMAXPORTION) +
                                                    PWPORTRNG.Next(PMAXPORTION)
                Dim PWriterInit As New Random(PWriterRNGInitSeed)
                Dim PRKey(31) As Byte
                Dim PRIV(15) As Byte
                Dim PWKey(31) As Byte
                Dim PWIV(15) As Byte
                PReaderInit.NextBytes(PRKey)
                PReaderInit.NextBytes(PRIV)
                PWriterInit.NextBytes(PWKey)
                PWriterInit.NextBytes(PWIV)

                AESR = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PRKey, .IV = PRIV, .Padding = Security.Cryptography.PaddingMode.None}
                AESW = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PWKey, .IV = PWIV, .Padding = Security.Cryptography.PaddingMode.None}
                AESR_TRANSFORM = AESR.CreateDecryptor
                AESW_TRANSFORM = AESW.CreateEncryptor
                AESR_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESR_TRANSFORM, Security.Cryptography.CryptoStreamMode.Read)
                AESW_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESW_TRANSFORM, Security.Cryptography.CryptoStreamMode.Write)
                AESR_BYTEBLOCKSIZE = CInt(AESR.BlockSize / 8)
                AESW_BYTEBLOCKSIZE = CInt(AESW.BlockSize / 8)
            End SyncLock
        End SyncLock

        Dim Snapshot As DateTime = DateTime.Now
        WriteJagged({BitConverter.GetBytes(Snapshot.Year),
                     BitConverter.GetBytes(Snapshot.Month),
                     BitConverter.GetBytes(Snapshot.Day),
                     BitConverter.GetBytes(Snapshot.Hour),
                     BitConverter.GetBytes(Snapshot.Minute),
                     BitConverter.GetBytes(Snapshot.Second),
                     BitConverter.GetBytes(Snapshot.Millisecond)})
        Do Until ReadBuffer.Length > 0

        Loop
        Dim RemoteDate As Byte()() = Nothing : ReadJagged(RemoteDate)
        Dim RYE As Int32 : RYE = BitConverter.ToInt32(RemoteDate(0), 0)
        Dim RMO As Int32 : RMO = BitConverter.ToInt32(RemoteDate(1), 0)
        Dim RDA As Int32 : RDA = BitConverter.ToInt32(RemoteDate(2), 0)
        Dim RHO As Int32 : RHO = BitConverter.ToInt32(RemoteDate(3), 0)
        Dim RMI As Int32 : RMI = BitConverter.ToInt32(RemoteDate(4), 0)
        Dim RSE As Int32 : RSE = BitConverter.ToInt32(RemoteDate(5), 0)
        Dim RMS As Int32 : RMS = BitConverter.ToInt32(RemoteDate(6), 0)

        SyncLock AESR_SYNC
            SyncLock AESW_SYNC
                If AESR_TRANSFORM IsNot Nothing Then AESR_TRANSFORM.Dispose()
                If AESW_TRANSFORM IsNot Nothing Then AESW_TRANSFORM.Dispose()
                If AESR IsNot Nothing Then AESR.Dispose()
                If AESW IsNot Nothing Then AESW.Dispose()
                Dim MAXPORTION As Integer = CInt(Math.Floor(Int32.MaxValue / 8))
                Dim RYERNG As New Random(RYE)
                Dim RMORNG As New Random(RMO)
                Dim RDARNG As New Random(RDA)
                Dim RHORNG As New Random(RHO)
                Dim RMIRNG As New Random(RMI)
                Dim RSERNG As New Random(RSE)
                Dim RMSRNG As New Random(RMS)
                Dim RPORTRNG As New Random(LEndpoint.Port)
                Dim ReaderRNGInitSeed As Integer = RYERNG.Next(MAXPORTION) +
                                                   RMORNG.Next(MAXPORTION) +
                                                   RDARNG.Next(MAXPORTION) +
                                                   RHORNG.Next(MAXPORTION) +
                                                   RMIRNG.Next(MAXPORTION) +
                                                   RSERNG.Next(MAXPORTION) +
                                                   RMSRNG.Next(MAXPORTION) +
                                                   RPORTRNG.Next(MAXPORTION)
                Dim ReaderInit As New Random(ReaderRNGInitSeed)
                Dim WYERNG As New Random(Snapshot.Year)
                Dim WMORNG As New Random(Snapshot.Month)
                Dim WDARNG As New Random(Snapshot.Day)
                Dim WHORNG As New Random(Snapshot.Hour)
                Dim WMIRNG As New Random(Snapshot.Minute)
                Dim WSERNG As New Random(Snapshot.Second)
                Dim WMSRNG As New Random(Snapshot.Millisecond)
                Dim WPORTRNG As New Random(REndpoint.Port)
                Dim WriterRNGInitSeed As Integer = WYERNG.Next(MAXPORTION) +
                                                   WMORNG.Next(MAXPORTION) +
                                                   WDARNG.Next(MAXPORTION) +
                                                   WHORNG.Next(MAXPORTION) +
                                                   WMIRNG.Next(MAXPORTION) +
                                                   WSERNG.Next(MAXPORTION) +
                                                   WMSRNG.Next(MAXPORTION) +
                                                   WPORTRNG.Next(MAXPORTION)
                Dim WriterInit As New Random(WriterRNGInitSeed)
                Dim RKey(31) As Byte
                Dim RIV(15) As Byte
                Dim WKey(31) As Byte
                Dim WIV(15) As Byte
                ReaderInit.NextBytes(RKey)
                ReaderInit.NextBytes(RIV)
                WriterInit.NextBytes(WKey)
                WriterInit.NextBytes(WIV)
                AESR = New Security.Cryptography.AesCryptoServiceProvider With {.Key = RKey, .IV = RIV, .Padding = Security.Cryptography.PaddingMode.None}
                AESW = New Security.Cryptography.AesCryptoServiceProvider With {.Key = WKey, .IV = WIV, .Padding = Security.Cryptography.PaddingMode.None}
                AESR_TRANSFORM = AESR.CreateDecryptor
                AESW_TRANSFORM = AESW.CreateEncryptor
                AESR_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESR_TRANSFORM, Security.Cryptography.CryptoStreamMode.Read)
                AESW_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESW_TRANSFORM, Security.Cryptography.CryptoStreamMode.Write)
                AESR_BYTEBLOCKSIZE = CInt(AESR.BlockSize / 8)
                AESW_BYTEBLOCKSIZE = CInt(AESW.BlockSize / 8)
            End SyncLock
        End SyncLock
    End Sub


    Private Sub BufferFlusherLoop()
        Dim Limiter As New ThreadLimiter(50)
        Do While Connected = True
            SyncLock AESR_SYNC
                If Available > 0 Then
                    Dim DataPacket(Available - 1) As Byte
                    AESR_STREAM.Read(DataPacket, 0, Available)
                    Dim ControlByte As Byte = DataPacket(0)
                    Dim LengthBytes As Byte() = {DataPacket(1), DataPacket(2), DataPacket(3), DataPacket(4)}
                    Dim Length As Int32 = BitConverter.ToInt32(LengthBytes, 0)
                    Select Case ControlByte
                        Case 0
                            If Length > 0 Then
                                Dim Data(Length - 1) As Byte
                                Buffer.BlockCopy(DataPacket, 5, Data, 0, Length)
                                ReadBuffer.Write(Data, 0, Length)
                            End If
                        Case 1
                            SyncLock AESW_SYNC
                                SendMessageInternal(2)
                            End SyncLock
                        Case 2
                            PingReplyReceived = True
                    End Select
                End If
            End SyncLock
            SyncLock AESW_SYNC
                If WriteBuffer.Length > 0 Then
                    Dim DataPacketPayload(CInt(WriteBuffer.Length) - 1) As Byte
                    WriteBuffer.Read(DataPacketPayload, 0, DataPacketPayload.Length)
                    SendMessageInternal(0, DataPacketPayload)
                End If
            End SyncLock
            Limiter.Limit()
        Loop
    End Sub

    Private Sub SendMessageInternal(Code As Byte, Optional ByRef Data As Byte() = Nothing)
        If BaseConnected = True Then
            Dim LengthBytes As Byte() = BitConverter.GetBytes(0)
            Dim Length As Int32 = 0
            If Data IsNot Nothing Then
                Length = Data.Length
                LengthBytes = BitConverter.GetBytes(Length)
            End If

            Dim ByteSize As Integer = 5 + Length
            Dim TotalSize As Integer = CInt(Math.Ceiling(ByteSize / AESW_BYTEBLOCKSIZE)) * AESW_BYTEBLOCKSIZE
            Dim DataPacket(TotalSize - 1) As Byte
            DataPacket(0) = Code
            DataPacket(1) = LengthBytes(0)
            DataPacket(2) = LengthBytes(1)
            DataPacket(3) = LengthBytes(2)
            DataPacket(4) = LengthBytes(3)
            If Data IsNot Nothing AndAlso Length > 0 Then Buffer.BlockCopy(Data, 0, DataPacket, 5, Length)
            AESW_STREAM.Write(DataPacket, 0, TotalSize)
            AESW_STREAM.Flush()
        End If
    End Sub



    Public Sub WriteJagged(ByRef input As Byte()())
        SyncLock AESW_SYNC
            Dim ParentLengthBytes As Byte() = BitConverter.GetBytes(input.Length)
            WriteBuffer.Write(ParentLengthBytes, 0, 4)
            For x = 0 To input.Length - 1
                WriteBuffer.Write(BitConverter.GetBytes(input(x).Length), 0, 4)
                WriteBuffer.Write(input(x), 0, input(x).Length)
            Next
        End SyncLock
    End Sub
    Public Sub ReadJagged(ByRef output As Byte()())
        SyncLock AESR_STREAM
            Dim ParentLengthBytes(3) As Byte, ParentLength As Int32 = Nothing
            ReadBuffer.Read(ParentLengthBytes, 0, 4)
            ParentLength = BitConverter.ToInt32(ParentLengthBytes, 0)
            ReDim output(ParentLength - 1)
            For x = 0 To ParentLength - 1
                Dim ChildLengthBytes(3) As Byte, ChildLength As Int32 = Nothing
                ReadBuffer.Read(ChildLengthBytes, 0, 4)
                ChildLength = BitConverter.ToInt32(ChildLengthBytes, 0)
                ReDim output(x)(ChildLength - 1)
                ReadBuffer.Read(output(x), 0, ChildLength)
            Next
        End SyncLock
    End Sub
    Public Shadows Sub Close()
        SyncLock AESR_SYNC
            SyncLock AESW_SYNC
                MyBase.Close()
                AESR_TRANSFORM.Dispose()
                AESW_TRANSFORM.Dispose()
                AESR.Dispose()
                AESW.Dispose()
            End SyncLock
        End SyncLock
    End Sub
End Class
