Public Class TCPClient : Inherits Net.Sockets.TcpClient
    Private BaseStream As Net.Sockets.NetworkStream
    Private AESR As Security.Cryptography.AesCryptoServiceProvider
    Private AESW As Security.Cryptography.AesCryptoServiceProvider
    Private AESR_TRANSFORM As Security.Cryptography.ICryptoTransform
    Private AESW_TRANSFORM As Security.Cryptography.ICryptoTransform
    Private AESR_SYNC As Object = New Object
    Private AESW_SYNC As Object = New Object
    Private AESR_STREAM As System.Security.Cryptography.CryptoStream
    Private AESW_STREAM As System.Security.Cryptography.CryptoStream

    Private LEndpoint As Net.IPEndPoint
    Private REndpoint As Net.IPEndPoint

    Public ReadOnly Property RemoteEndpoint As Net.IPEndPoint
        Get
            Return REndpoint
        End Get
    End Property
    Public ReadOnly Property LocalEndpoint As Net.IPEndPoint
        Get
            Return LEndpoint
        End Get
    End Property
    Public Property Encrypt As Boolean = True
    Public Shadows ReadOnly Property Connected As Boolean
        Get
            SyncLock AESR_SYNC
                SyncLock AESW_SYNC
                    If Client.Connected = False Then Return False
                    Dim Flag1 As Boolean = Client.Poll(0, Net.Sockets.SelectMode.SelectRead)
                    Dim Flag2 As Boolean = (Client.Available = 0)

                    If Not Flag1 Then Return True
                    If Not Flag2 Then Return True

                    Dim blockingState As Boolean = Client.Blocking
                    Try
                        Dim tmp(0) As Byte
                        Client.Blocking = False
                        Client.Send(tmp, 0, 0)
                        Return True
                    Catch e As Net.Sockets.SocketException
                        If e.SocketErrorCode = Net.Sockets.SocketError.WouldBlock Then
                            Return True
                        Else
                            Return False
                        End If
                    Finally
                        Client.Blocking = blockingState
                    End Try
                End SyncLock
            End SyncLock
        End Get
    End Property

    Public Sub New(Socket As Net.Sockets.Socket)
        MyBase.New
        Client = Socket
        Initialize()
    End Sub
    Public Sub New(Endpoint As Net.IPEndPoint)
        MyBase.New()
        MyBase.Connect(Endpoint)
        Initialize()
    End Sub
    Private Sub Initialize()
        REndpoint = CType(Client.RemoteEndPoint, Net.IPEndPoint)
        LEndpoint = CType(Client.LocalEndPoint, Net.IPEndPoint)
        BaseStream = Me.GetStream
        BaseStream.ReadTimeout = 1000
        BaseStream.WriteTimeout = 1000
        Dim PSnapshot As DateTime = DateTime.UtcNow
        Dim PRYERNG As New Random(PSnapshot.Year)
        Dim PRMORNG As New Random(PSnapshot.Month)
        Dim PRDARNG As New Random(PSnapshot.Day)
        Dim PRHORNG As New Random(PSnapshot.Hour)
        Dim PRMIRNG As New Random(PSnapshot.Minute)
        Dim PRSERNG As New Random(PSnapshot.Second)
        Dim PRPORTRNG As New Random(REndpoint.Port)

        Dim PMAXPORTION As Double = Int32.MaxValue / 7

        Dim PReaderRNGInitSeed As Integer = PRYERNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PRMORNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PRDARNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PRHORNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PRMIRNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PRSERNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PRPORTRNG.Next(CInt(Math.Floor(PMAXPORTION)))

        Dim PWYERNG As New Random(PSnapshot.Year)
        Dim PWMORNG As New Random(PSnapshot.Month)
        Dim PWDARNG As New Random(PSnapshot.Day)
        Dim PWHORNG As New Random(PSnapshot.Hour)
        Dim PWMIRNG As New Random(PSnapshot.Minute)
        Dim PWSERNG As New Random(PSnapshot.Second)
        Dim PWPORTRNG As New Random(LEndpoint.Port)
        Dim PWriterRNGInitSeed As Integer = PWYERNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PWMORNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PWDARNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PWHORNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PWMIRNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PWSERNG.Next(CInt(Math.Floor(PMAXPORTION))) +
                                            PWPORTRNG.Next(CInt(Math.Floor(PMAXPORTION)))

        Dim PReaderInit As New Random(PReaderRNGInitSeed)
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

        Dim Snapshot As DateTime = DateTime.Now
        WriteJagged({BitConverter.GetBytes(Snapshot.Year),
                     BitConverter.GetBytes(Snapshot.Month),
                     BitConverter.GetBytes(Snapshot.Day),
                     BitConverter.GetBytes(Snapshot.Hour),
                     BitConverter.GetBytes(Snapshot.Minute),
                     BitConverter.GetBytes(Snapshot.Second),
                     BitConverter.GetBytes(Snapshot.Millisecond)})

        Dim RemoteDate As Byte()() = Nothing : ReadJagged(RemoteDate)

        Dim RYE As Int32 : RYE = BitConverter.ToInt32(RemoteDate(0), 0)
        Dim RMO As Int32 : RMO = BitConverter.ToInt32(RemoteDate(1), 0)
        Dim RDA As Int32 : RDA = BitConverter.ToInt32(RemoteDate(2), 0)
        Dim RHO As Int32 : RHO = BitConverter.ToInt32(RemoteDate(3), 0)
        Dim RMI As Int32 : RMI = BitConverter.ToInt32(RemoteDate(4), 0)
        Dim RSE As Int32 : RSE = BitConverter.ToInt32(RemoteDate(5), 0)
        Dim RMS As Int32 : RMS = BitConverter.ToInt32(RemoteDate(6), 0)

        AESR_TRANSFORM.Dispose()
        AESW_TRANSFORM.Dispose()
        AESR.Dispose()
        AESW.Dispose()
        Dim RYERNG As New Random(RYE)
        Dim RMORNG As New Random(RMO)
        Dim RDARNG As New Random(RDA)
        Dim RHORNG As New Random(RHO)
        Dim RMIRNG As New Random(RMI)
        Dim RSERNG As New Random(RSE)
        Dim RMSRNG As New Random(RMS)
        Dim RPORTRNG As New Random(LEndpoint.Port)

        Dim MAXPORTION As Double = Int32.MaxValue / 8

        Dim ReaderRNGInitSeed As Integer = RYERNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RMORNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RDARNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RHORNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RMIRNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RSERNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RMSRNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           RPORTRNG.Next(CInt(Math.Floor(MAXPORTION)))

        Dim WYERNG As New Random(Snapshot.Year)
        Dim WMORNG As New Random(Snapshot.Month)
        Dim WDARNG As New Random(Snapshot.Day)
        Dim WHORNG As New Random(Snapshot.Hour)
        Dim WMIRNG As New Random(Snapshot.Minute)
        Dim WSERNG As New Random(Snapshot.Second)
        Dim WMSRNG As New Random(Snapshot.Millisecond)
        Dim WPORTRNG As New Random(REndpoint.Port)

        Dim WriterRNGInitSeed As Integer = WYERNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WMORNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WDARNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WHORNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WMIRNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WSERNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WMSRNG.Next(CInt(Math.Floor(MAXPORTION))) +
                                           WPORTRNG.Next(CInt(Math.Floor(MAXPORTION)))

        Dim ReaderInit As New Random(ReaderRNGInitSeed)
        Dim WriterInit As New Random(WriterRNGInitSeed)

        Dim RKey(31) As Byte
        Dim RIV(15) As Byte
        Dim WKey(31) As Byte
        Dim WIV(15) As Byte

        ReaderInit.NextBytes(RKey)
        ReaderInit.NextBytes(RIV)
        WriterInit.NextBytes(WKey)
        WriterInit.NextBytes(WIV)

        AESR = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PRKey, .IV = PRIV, .Padding = Security.Cryptography.PaddingMode.None}
        AESW = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PWKey, .IV = PWIV, .Padding = Security.Cryptography.PaddingMode.None}

        AESR_TRANSFORM = AESR.CreateDecryptor
        AESW_TRANSFORM = AESW.CreateEncryptor

        AESR_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESR_TRANSFORM, Security.Cryptography.CryptoStreamMode.Read)
        AESW_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESW_TRANSFORM, Security.Cryptography.CryptoStreamMode.Write)
    End Sub

    Public Sub ReadJagged(ByRef DataOut As Byte()())

        If Connected = True Then
            SyncLock AESR_SYNC
                If Encrypt = True Then
                    Dim PaddingLengthBytes(3) As Byte, PaddingLength As Int32 = Nothing
                    AESR_STREAM.Read(PaddingLengthBytes, 0, 4)
                    PaddingLength = BitConverter.ToInt32(PaddingLengthBytes, 0)
                    Dim ParentLengthBytes(3) As Byte, ParentLength As Int32 = Nothing
                    AESR_STREAM.Read(ParentLengthBytes, 0, 4)
                    ParentLength = BitConverter.ToInt32(ParentLengthBytes, 0)
                    ReDim DataOut(ParentLength - 1)
                    For x = 0 To ParentLength - 1
                        Dim ChildLengthBytes(3) As Byte, ChildLength As Int32 = Nothing
                        AESR_STREAM.Read(ChildLengthBytes, 0, 4)
                        ChildLength = BitConverter.ToInt32(ChildLengthBytes, 0)
                        ReDim DataOut(x)(ChildLength - 1)
                        AESR_STREAM.Read(DataOut(x), 0, ChildLength)
                    Next
                    If PaddingLength > 0 Then
                        Dim Padding(PaddingLength - 1) As Byte
                        AESR_STREAM.Read(Padding, 0, PaddingLength)
                    End If
                Else
                    Dim PaddingLengthBytes(3) As Byte, PaddingLength As Int32 = Nothing
                    BaseStream.Read(PaddingLengthBytes, 0, 4)
                    PaddingLength = BitConverter.ToInt32(PaddingLengthBytes, 0)
                    Dim ParentLengthBytes(3) As Byte, ParentLength As Int32 = Nothing
                    BaseStream.Read(ParentLengthBytes, 0, 4)
                    ParentLength = BitConverter.ToInt32(ParentLengthBytes, 0)
                    ReDim DataOut(ParentLength - 1)
                    For x = 0 To ParentLength - 1
                        Dim ChildLengthBytes(3) As Byte, ChildLength As Int32 = Nothing
                        BaseStream.Read(ChildLengthBytes, 0, 4)
                        ChildLength = BitConverter.ToInt32(ChildLengthBytes, 0)
                        ReDim DataOut(x)(ChildLength - 1)
                        BaseStream.Read(DataOut(x), 0, ChildLength)
                    Next
                End If
            End SyncLock
        End If
    End Sub
    Public Sub WriteJagged(ByRef DataIn As Byte()())
        If Connected = True Then
            '8B + n(4B) + n(xB) = ByteSize
            SyncLock AESW_SYNC
                If Encrypt = True Then
                    Dim ByteSize As Integer = 8 + (4 * DataIn.Length)
                    For Each x In DataIn
                        ByteSize += x.Length
                    Next
                    Dim BlockSize As Double = (AESW.BlockSize / 8)
                    Dim BlocksUsed As Double = Math.Ceiling(ByteSize / BlockSize)
                    Dim TotalSize As Double = BlocksUsed * BlockSize
                    Dim PaddingAmount As Integer = CInt(TotalSize) - ByteSize
                    Dim Padding As Byte() = Nothing
                    Dim PaddingLengthBytes As Byte() = BitConverter.GetBytes(PaddingAmount)
                    If PaddingAmount > 0 Then ReDim Padding(PaddingAmount - 1)
                    Dim ParentLengthBytes As Byte() = BitConverter.GetBytes(DataIn.Length)
                    AESW_STREAM.Write(PaddingLengthBytes, 0, 4)
                    AESW_STREAM.Write(ParentLengthBytes, 0, 4)
                    For x = 0 To DataIn.Length - 1
                        AESW_STREAM.Write(BitConverter.GetBytes(DataIn(x).Length), 0, 4)
                        AESW_STREAM.Write(DataIn(x), 0, DataIn(x).Length)
                    Next
                    If PaddingAmount > 0 Then AESW_STREAM.Write(Padding, 0, Padding.Length)
                    AESW_STREAM.Flush()
                Else
                    Dim PaddingLengthBytes As Byte() = BitConverter.GetBytes(0)
                    Dim ParentLengthBytes As Byte() = BitConverter.GetBytes(DataIn.Length)
                    BaseStream.Write(PaddingLengthBytes, 0, 4)
                    BaseStream.Write(ParentLengthBytes, 0, 4)
                    For x = 0 To DataIn.Length - 1
                        BaseStream.Write(BitConverter.GetBytes(DataIn(x).Length), 0, 4)
                        BaseStream.Write(DataIn(x), 0, DataIn(x).Length)
                    Next
                    BaseStream.Flush()
                End If
            End SyncLock

        End If
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
