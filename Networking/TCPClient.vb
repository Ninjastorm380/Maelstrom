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
    Private ReadBuffers(255) As MemoryQueueStream
    Private WriteBuffers(255) As MemoryQueueStream
    Private ChannelActiveBase(255) As Boolean
    Private BaseConnected As Boolean = False
    Public ReadOnly Property HasMessage(Optional Channel As Byte = 0) As Boolean
        Get
            SyncLock AESR_SYNC
                If ReadBuffers(Channel) Is Nothing Then Return False
                Return ReadBuffers(Channel).Length > 0
            End SyncLock
        End Get
    End Property
    Public Property BufferFlushRate As Integer = 25
    Public Sub ActivateChannel(Channel As Byte)
        ChannelActiveBase(Channel) = True
    End Sub
    Public Sub DeactivateChannel(Channel As Byte)
        ChannelActiveBase(Channel) = False
    End Sub
    Public Sub New(Socket As Net.Sockets.Socket)
        MyBase.New(Socket.AddressFamily)
        Client = Socket
        Initialize()
    End Sub
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
                        If BaseConnected = False Then Return False
                        SendMessage(0, 1)
                        Do Until PingTimeoutLimit = PingTimeoutCount Or BaseConnected = False
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
        Client.SendBufferSize = Integer.MaxValue
        Client.ReceiveBufferSize = Integer.MaxValue
        BaseStream = Me.GetStream

        For x = 0 To 255
            ReadBuffers(x) = New MemoryQueueStream
            WriteBuffers(x) = New MemoryQueueStream
        Next
        ChannelActiveBase(0) = True
        For x = 1 To 255
            ChannelActiveBase(x) = False
        Next
        Dim Flusher As New Threading.Thread(AddressOf BufferFlusherLoop)
        Flusher.Start()

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

        SyncLock AESR_SYNC
            SyncLock AESW_SYNC
                If AESR_TRANSFORM IsNot Nothing Then AESR_TRANSFORM.Dispose()
                If AESW_TRANSFORM IsNot Nothing Then AESW_TRANSFORM.Dispose()
                If AESR IsNot Nothing Then AESR.Dispose()
                If AESW IsNot Nothing Then AESW.Dispose()
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

        PRKey = Nothing
        PWKey = Nothing
        PRIV = Nothing
        PWIV = Nothing
        PWriterRNGInitSeed = Nothing
        PReaderRNGInitSeed = Nothing
        PWYERNG = Nothing
        PWMORNG = Nothing
        PWDARNG = Nothing
        PWHORNG = Nothing
        PWMIRNG = Nothing
        PWSERNG = Nothing
        PWPORTRNG = Nothing
        PRYERNG = Nothing
        PRMORNG = Nothing
        PRDARNG = Nothing
        PRHORNG = Nothing
        PRMIRNG = Nothing
        PRSERNG = Nothing
        PRPORTRNG = Nothing
        PSnapshot = Nothing
        PReaderInit = Nothing
        PWriterInit = Nothing
        PMAXPORTION = Nothing

        If BaseConnected = False Then Exit Sub
        Dim Snapshot As DateTime = DateTime.Now
        WriteJagged({BitConverter.GetBytes(Snapshot.Year),
                     BitConverter.GetBytes(Snapshot.Month),
                     BitConverter.GetBytes(Snapshot.Day),
                     BitConverter.GetBytes(Snapshot.Hour),
                     BitConverter.GetBytes(Snapshot.Minute),
                     BitConverter.GetBytes(Snapshot.Second),
                     BitConverter.GetBytes(Snapshot.Millisecond)})
        Dim loopLimiter As New ThreadLimiter(50)
        Do Until HasMessage(0) = True Or BaseConnected = False : loopLimiter.Limit() : Loop
        If BaseConnected = False Then Exit Sub
        Dim RemoteDate As Byte()() = Nothing : ReadJagged(RemoteDate)
        Dim RYE As Int32 : RYE = BitConverter.ToInt32(RemoteDate(0), 0)
        Dim RMO As Int32 : RMO = BitConverter.ToInt32(RemoteDate(1), 0)
        Dim RDA As Int32 : RDA = BitConverter.ToInt32(RemoteDate(2), 0)
        Dim RHO As Int32 : RHO = BitConverter.ToInt32(RemoteDate(3), 0)
        Dim RMI As Int32 : RMI = BitConverter.ToInt32(RemoteDate(4), 0)
        Dim RSE As Int32 : RSE = BitConverter.ToInt32(RemoteDate(5), 0)
        Dim RMS As Int32 : RMS = BitConverter.ToInt32(RemoteDate(6), 0)
        If BaseConnected = False Then Exit Sub

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

        SyncLock AESR_SYNC
            SyncLock AESW_SYNC
                If AESR_TRANSFORM IsNot Nothing Then AESR_TRANSFORM.Dispose()
                If AESW_TRANSFORM IsNot Nothing Then AESW_TRANSFORM.Dispose()
                If AESR IsNot Nothing Then AESR.Dispose()
                If AESW IsNot Nothing Then AESW.Dispose()
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

        RKey = Nothing
        WKey = Nothing
        RIV = Nothing
        WIV = Nothing
        WriterRNGInitSeed = Nothing
        ReaderRNGInitSeed = Nothing
        WYERNG = Nothing
        WMORNG = Nothing
        WDARNG = Nothing
        WHORNG = Nothing
        WMIRNG = Nothing
        WSERNG = Nothing
        WPORTRNG = Nothing
        RYERNG = Nothing
        RMORNG = Nothing
        RDARNG = Nothing
        RHORNG = Nothing
        RMIRNG = Nothing
        RSERNG = Nothing
        RPORTRNG = Nothing
        Snapshot = Nothing
        ReaderInit = Nothing
        WriterInit = Nothing
        MAXPORTION = Nothing
    End Sub
    Private Sub BufferFlusherLoop()
        Dim Limiter As New ThreadLimiter(BufferFlushRate)
        Do While Connected = True
            Limiter.IterationsPerSecond = BufferFlushRate
            If Available > 0 Then
                Dim Code As Byte = 0
                Dim Channel As Byte = 0
                Dim ReceivedData As Byte() = Nothing
                Dim AboutToReset As Boolean = False
                SyncLock AESR_SYNC
                    ReceiveMessage(Channel, Code, ReceivedData, AboutToReset)
                End SyncLock
                If AboutToReset = True Then
                    Dim AsyncDisconnector As New Threading.Thread(Sub() Close()) : AsyncDisconnector.Start()
                    Exit Do
                End If
                Select Case Code
                    Case 0
                        SyncLock ReadBuffers(Channel)
                            If ChannelActiveBase(Channel) = True Then ReadBuffers(Channel).Write(ReceivedData, 0, ReceivedData.Length)
                        End SyncLock
                    Case 1
                        SyncLock AESW_SYNC
                            SendMessage(0, 2)
                        End SyncLock
                    Case 2
                        PingReplyReceived = True
                End Select
            End If
            For x = 0 To 255
                If WriteBuffers(x).Length > 0 Then
                    SyncLock WriteBuffers(x)
                        Dim Data(CInt(WriteBuffers(x).Length) - 1) As Byte
                        WriteBuffers(x).Read(Data, 0, Data.Length)
                        SyncLock AESW_SYNC
                            If ChannelActiveBase(x) = True Then
                                SendMessage(CByte(x), 0, Data)
                            End If
                        End SyncLock
                    End SyncLock
                End If
            Next
            Limiter.Limit()
        Loop
    End Sub
    Private Sub SendMessage(ByRef Channel As Byte, ByRef Code As Byte, Optional ByRef Data As Byte() = Nothing)
        If BaseConnected = True Then
            Dim Length As Int32 = 0
            Dim LengthBytes As Byte() = BitConverter.GetBytes(Length)
            Dim PaddedLength As Int32 = 0
            Dim PaddedLengthBytes As Byte() = BitConverter.GetBytes(PaddedLength)
            Dim PaddedData As Byte() = Nothing
            If Data IsNot Nothing AndAlso Data.Length > 0 Then
                Length = Data.Length
                LengthBytes = BitConverter.GetBytes(Length)
                PaddedLength = CInt(Math.Ceiling(Length / AESW_BYTEBLOCKSIZE)) * AESW_BYTEBLOCKSIZE
                PaddedLengthBytes = BitConverter.GetBytes(PaddedLength)
                ReDim PaddedData(PaddedLength - 1)
                Buffer.BlockCopy(Data, 0, PaddedData, 0, Length)
            End If
            Dim Header(15) As Byte
            Header(0) = Channel
            Header(1) = Code
            Buffer.BlockCopy(LengthBytes, 0, Header, 2, 4)
            Buffer.BlockCopy(PaddedLengthBytes, 0, Header, 6, 4)
            SyncLock AESW_SYNC
                Try
                    AESW_STREAM.Write(Header, 0, 16)
                    If Length > 0 Then AESW_STREAM.Write(PaddedData, 0, PaddedLength)
                    AESW_STREAM.Flush()
                Catch ex As System.IO.IOException
                End Try
            End SyncLock
        End If
    End Sub
    Private Sub ReceiveMessage(ByRef Channel As Byte, ByRef Code As Byte, Optional ByRef Data As Byte() = Nothing, Optional ByRef ResetPending As Boolean = False)
        If BaseConnected = True Then
            SyncLock AESR_SYNC
                Dim Header(15) As Byte
                Dim LengthBytes(3) As Byte
                Dim Length As Int32
                Dim PaddedLengthBytes(3) As Byte
                Dim PaddedLength As Int32
                If AESR_STREAM Is Nothing Then ResetPending = True
                If ResetPending = False Then
                    AESR_STREAM.Read(Header, 0, 16)
                    Channel = Header(0)
                    Code = Header(1)
                    Buffer.BlockCopy(Header, 2, LengthBytes, 0, 4)
                    Buffer.BlockCopy(Header, 6, PaddedLengthBytes, 0, 4)
                    Length = BitConverter.ToInt32(LengthBytes, 0)
                    PaddedLength = BitConverter.ToInt32(PaddedLengthBytes, 0)
                End If
                If Math.Sign(Length) = -1 OrElse Math.Sign(PaddedLength) = -1 Then ResetPending = True
                If ResetPending = False Then
                    Dim Results As Integer = PaddedLength - Length
                    If Results < 0 OrElse Results >= 16 Then ResetPending = True
                End If
                If ResetPending = True Then Exit Sub
                If Length > 0 And ResetPending = False Then
                    ReDim Data(Length - 1)
                    Dim PaddedData(PaddedLength - 1) As Byte
                    AESR_STREAM.Read(PaddedData, 0, PaddedLength)
                    Buffer.BlockCopy(PaddedData, 0, Data, 0, Length)
                End If
            End SyncLock
        End If
    End Sub
    Public Sub WriteJagged(ByRef input As Byte()(), Optional ByRef Channel As Byte = 0)
        SyncLock WriteBuffers(Channel)
            Dim ParentLengthBytes As Byte() = BitConverter.GetBytes(input.Length)
            WriteBuffers(Channel).Write(ParentLengthBytes, 0, 4)
            For x = 0 To input.Length - 1
                WriteBuffers(Channel).Write(BitConverter.GetBytes(input(x).Length), 0, 4)
                WriteBuffers(Channel).Write(input(x), 0, input(x).Length)
            Next
        End SyncLock
    End Sub
    Public Sub ReadJagged(ByRef output As Byte()(), Optional ByRef Channel As Byte = 0)
        SyncLock ReadBuffers(Channel)
            Dim ParentLengthBytes(3) As Byte, ParentLength As Int32 = Nothing
            ReadBuffers(Channel).Read(ParentLengthBytes, 0, 4)
            ParentLength = BitConverter.ToInt32(ParentLengthBytes, 0)
            ReDim output(ParentLength - 1)
            For x = 0 To ParentLength - 1
                Dim ChildLengthBytes(3) As Byte, ChildLength As Int32 = Nothing
                ReadBuffers(Channel).Read(ChildLengthBytes, 0, 4)
                ChildLength = BitConverter.ToInt32(ChildLengthBytes, 0)
                ReDim output(x)(ChildLength - 1)
                ReadBuffers(Channel).Read(output(x), 0, ChildLength)
            Next
        End SyncLock
    End Sub
    Public Shadows Sub Close()
        SyncLock AESR_SYNC
            SyncLock AESW_SYNC
                BaseConnected = False
                MyBase.Close()

                BaseStream.Dispose()
                For x = 0 To 255
                    ReadBuffers(x).Dispose()
                    WriteBuffers(x).Dispose()
                Next
                AESR_TRANSFORM.Dispose()
                AESW_TRANSFORM.Dispose()
                AESR.Dispose()
                AESW.Dispose()
                AESR_BYTEBLOCKSIZE = Nothing
                AESW_BYTEBLOCKSIZE = Nothing
                Client.Dispose()
                Dispose()
            End SyncLock
        End SyncLock
    End Sub
End Class
