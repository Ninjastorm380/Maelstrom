Public Class TCPClientb : Inherits Net.Sockets.TcpClient
    'private variables'
    Private BaseStream As Net.Sockets.NetworkStream
    Private AESR As Security.Cryptography.AesCryptoServiceProvider
    Private AESW As Security.Cryptography.AesCryptoServiceProvider
    Private AESR_TRANSFORM As Security.Cryptography.ICryptoTransform
    Private AESW_TRANSFORM As Security.Cryptography.ICryptoTransform
    Private AESR_SYNC As Object = New Object
    Private AESW_SYNC As Object = New Object
    Private AESR_STREAM As System.Security.Cryptography.CryptoStream
    Private AESW_STREAM As System.Security.Cryptography.CryptoStream
    Private MemReadBuffer As IO.MemoryStream
    Private MemWriteBuffer As IO.MemoryStream
    Private LEndpoint As Net.IPEndPoint
    Private REndpoint As Net.IPEndPoint

    'remote endpoint of current connection'
    Public ReadOnly Property RemoteEndpoint As Net.IPEndPoint
        Get
            Return REndpoint
        End Get
    End Property

    'local endpoint of current connection'
    Public ReadOnly Property LocalEndpoint As Net.IPEndPoint
        Get
            Return LEndpoint
        End Get
    End Property
    Public Property Encrypt As Boolean = True
    Public Shadows ReadOnly Property Connected As Boolean
        Get
            'synclock to prevent readjagged or writejagged from interfering.'
            SyncLock AESR_SYNC
                SyncLock AESW_SYNC
                    'check if we are already offline.'
                    If Client.Connected = False Then Return False

                    'check if we are online or offline with poll + available.'
                    Dim Flag1 As Boolean = Client.Poll(0, Net.Sockets.SelectMode.SelectRead)
                    Dim Flag2 As Boolean = (Client.Available = 0)
                    If Not Flag1 Then Return True
                    If Not Flag2 Then Return True

                    'poll + available can return a false negative, so we double check if we are actually disconnected by trying a non-blocking zero send.'
                    Dim blockingState As Boolean = Client.Blocking
                    Try
                        Dim tmp(0) As Byte
                        Client.Blocking = False
                        Dim result As Integer = Client.Send(tmp, 0, 0)
                        Client.Blocking = blockingState
                        If result = 1 Then Return True
                    Catch e As Net.Sockets.SocketException
                        If e.SocketErrorCode = Net.Sockets.SocketError.WouldBlock Then
                            Return True
                        Else
                            Return False
                        End If
                    Finally
                        Client.Blocking = blockingState
                    End Try
                    Return False
                End SyncLock
            End SyncLock
        End Get
    End Property

    'creates an existing TCPClient from a socket.'
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

    'common init path for both New() overloads.'
    Private Sub Initialize()
        'save endpoint info.'
        REndpoint = CType(Client.RemoteEndPoint, Net.IPEndPoint)
        LEndpoint = CType(Client.LocalEndPoint, Net.IPEndPoint)

        'get base stream.'
        BaseStream = Me.GetStream
        BaseStream.ReadTimeout = 1000
        BaseStream.WriteTimeout = 1000
        MemReadBuffer = New IO.MemoryStream
        MemWriteBuffer = New IO.MemoryStream

        'begin handshake for keyless AES encryption.'


        'constant for RNG preliminary max value (prevents int overload exception).'
        Dim PMAXPORTION As Integer = CInt(Math.Floor(Int32.MaxValue / 7))

        'get preliminary read keygen seed.'
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

        'get preliminary write keygen seed.'
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

        'get preliminary key and iv for AESR and AESW.'
        Dim PRKey(31) As Byte
        Dim PRIV(15) As Byte
        Dim PWKey(31) As Byte
        Dim PWIV(15) As Byte
        PReaderInit.NextBytes(PRKey)
        PReaderInit.NextBytes(PRIV)
        PWriterInit.NextBytes(PWKey)
        PWriterInit.NextBytes(PWIV)

        'create preliminary AES objects.'
        AESR = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PRKey, .IV = PRIV, .Padding = Security.Cryptography.PaddingMode.None}
        AESW = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PWKey, .IV = PWIV, .Padding = Security.Cryptography.PaddingMode.None}

        'create preliminary transforms.'
        AESR_TRANSFORM = AESR.CreateDecryptor
        AESW_TRANSFORM = AESW.CreateEncryptor

        'create preliminary streams.'
        AESR_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESR_TRANSFORM, Security.Cryptography.CryptoStreamMode.Read)
        AESW_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESW_TRANSFORM, Security.Cryptography.CryptoStreamMode.Write)

        'begin handshake.'
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

        'clean up all preliminary objects.'
        AESR_TRANSFORM.Dispose()
        AESW_TRANSFORM.Dispose()
        AESR.Dispose()
        AESW.Dispose()

        'constant for RNG max value (prevents int overload exception).'
        Dim MAXPORTION As Integer = CInt(Math.Floor(Int32.MaxValue / 8))

        'get read keygen seed.'
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

        'get write keygen seed.'
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

        'generate keys and ivs.'
        Dim RKey(31) As Byte
        Dim RIV(15) As Byte
        Dim WKey(31) As Byte
        Dim WIV(15) As Byte
        ReaderInit.NextBytes(RKey)
        ReaderInit.NextBytes(RIV)
        WriterInit.NextBytes(WKey)
        WriterInit.NextBytes(WIV)

        'create encryption objects.'
        AESR = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PRKey, .IV = PRIV, .Padding = Security.Cryptography.PaddingMode.None}
        AESW = New Security.Cryptography.AesCryptoServiceProvider With {.Key = PWKey, .IV = PWIV, .Padding = Security.Cryptography.PaddingMode.None}

        'create encryption transforms.'
        AESR_TRANSFORM = AESR.CreateDecryptor
        AESW_TRANSFORM = AESW.CreateEncryptor

        'create encryption streams.'
        AESR_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESR_TRANSFORM, Security.Cryptography.CryptoStreamMode.Read)
        AESW_STREAM = New Security.Cryptography.CryptoStream(BaseStream, AESW_TRANSFORM, Security.Cryptography.CryptoStreamMode.Write)
    End Sub

    'reads a jagged byte array from the network.'
    Public Sub ReadJagged(ByRef DataOut As Byte()())
        If Connected = True Then
            SyncLock AESR_SYNC
                If Encrypt = True Then
                    'read in length of padding'
                    Dim PaddingLengthBytes(3) As Byte, PaddingLength As Int32 = Nothing
                    AESR_STREAM.Read(PaddingLengthBytes, 0, 4)
                    PaddingLength = BitConverter.ToInt32(PaddingLengthBytes, 0)

                    'read in the jagged array'
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

                    'read padding into the void, if we have any.'
                    If PaddingLength > 0 Then
                        Dim Padding(PaddingLength - 1) As Byte
                        AESR_STREAM.Read(Padding, 0, PaddingLength)
                        Padding = Nothing
                    End If
                Else
                    'we have the option...'
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

    'writes a jagged byte array to the network.'
    Public Sub WriteJagged(ByRef DataIn As Byte()())
        If Connected = True Then
            SyncLock AESW_SYNC



                If Encrypt = True Then

                    'compute total send length.'
                    Dim ByteSize As Integer = 4 + 4 + (4 * DataIn.Length)
                    For Each x In DataIn
                        ByteSize += x.Length
                    Next

                    'do maths to check if we need any padding bytes. if PaddingAmount is zero then no padding required.'
                    Dim BlockSize As Double = (AESW.BlockSize / 8)
                    Dim BlocksUsed As Double = Math.Ceiling(ByteSize / BlockSize)
                    Dim TotalSize As Double = BlocksUsed * BlockSize
                    Dim PaddingAmount As Integer = CInt(TotalSize) - ByteSize

                    Dim Padding As Byte() = Nothing
                    Dim PaddingLengthBytes As Byte() = BitConverter.GetBytes(PaddingAmount)
                    If PaddingAmount > 0 Then ReDim Padding(PaddingAmount - 1)

                    'write jagged array.'
                    Dim ParentLengthBytes As Byte() = BitConverter.GetBytes(DataIn.Length)
                    AESW_STREAM.Write(PaddingLengthBytes, 0, 4)
                    AESW_STREAM.Write(ParentLengthBytes, 0, 4)
                    For x = 0 To DataIn.Length - 1
                        AESW_STREAM.Write(BitConverter.GetBytes(DataIn(x).Length), 0, 4)
                        AESW_STREAM.Write(DataIn(x), 0, DataIn(x).Length)
                    Next

                    'write padding, if neccessary.'
                    If PaddingAmount > 0 Then AESW_STREAM.Write(Padding, 0, Padding.Length)
                    AESW_STREAM.Flush()
                Else
                    'we have the option...'
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
    'shadow close so we can properly dispose the encryption objects'
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
