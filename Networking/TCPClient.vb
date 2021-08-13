Public Class TCPClient : Inherits Net.Sockets.TcpClient
    Private Class RWChannel
        Public Property ReadBuffer As MemoryQueueStream
        Public Property WriteBuffer As MemoryQueueStream

        Public Property Active As Boolean = False
        Public Sub New(Optional Active As Boolean = False, Optional BufferSize As Int32 = 16384)
            If Active = True Then
                Readbuffer = New MemoryQueueStream(Buffersize)
                Writebuffer = New MemoryQueueStream(Buffersize)
                Me.Active = Active
            End If
        End Sub
        Public Sub Activate(Optional BufferSize As Int32 = 16384)
            If Active = False Then
                Readbuffer = New MemoryQueueStream(Buffersize)
                Writebuffer = New MemoryQueueStream(Buffersize)
                Active = True
            End If
        End Sub
        Public Sub Deactivate()
            If Active = True Then
                Active = False
                Readbuffer.Dispose()
                Writebuffer.Dispose()
            End If
        End Sub
    End Class
    Private ReadOnly Channels As New Dictionary(Of Int32, RWChannel)(32)
    Private TCP_LocalEndpoint As Net.IPEndPoint
    Private TCP_RemoteEndpoint As Net.IPEndPoint
    Private TCP_Connected As Boolean = False
    Private TCP_Connected_Flag_A As Boolean
    Private TCP_Connected_Flag_B As Boolean
    Private TCP_Dynamic_Speed_Minimum As Int32 = 10
    Private TCP_Dynamic_Speed_Maximum As Int32 = 200
    Private TCP_Dynamic_Speed_Increment As Int32 = 7
    Private TCP_Dynamic_Speed_Decrement As Int32 = 2
    Private TCP_Dynamic_Speed_Sender_Do_Increase As Boolean = False
    Private TCP_Dynamic_Speed_Sender_Current_Speed As Int32 = 2
    Private TCP_Dynamic_Speed_Receiver_Do_Increase As Boolean = False
    Private TCP_Dynamic_Speed_Receiver_Current_Speed As Int32 = 2

    Private AES256CSP_Temporal_Sender_Timestamp As DateTime
    Private AES256CSP_Temporal_Receiver_Timestamp As DateTime
    Private AES256CSP_Temporal_Sender_TimestampBuffer(27) As Byte
    Private AES256CSP_Temporal_Receiver_TimestampBuffer(27) As Byte
    Private AES256CSP_Temporal_Sender_EncryptionGenerator As FastRNG
    Private AES256CSP_Temporal_Receiver_EncryptionGenerator As FastRNG

    Private AES256CSP_Temporal_Receiver_Key(31) As Byte
    Private AES256CSP_Temporal_Receiver_IV(15) As Byte
    Private AES256CSP_Temporal_Sender_Key(31) As Byte
    Private AES256CSP_Temporal_Sender_IV(15) As Byte

    Private AES256CSP_Temporal_BaseStream As Net.Sockets.NetworkStream
    Private AES256CSP_Temporal_Receiver As Security.Cryptography.AesCryptoServiceProvider
    Private AES256CSP_Temporal_Sender As Security.Cryptography.AesCryptoServiceProvider
    Private AES256CSP_Temporal_Receiver_Transform As Security.Cryptography.ICryptoTransform
    Private AES256CSP_Temporal_Sender_Transform As Security.Cryptography.ICryptoTransform
    Private AES256CSP_Temporal_Receiver_Stream As Security.Cryptography.CryptoStream
    Private AES256CSP_Temporal_Sender_Stream As Security.Cryptography.CryptoStream
    Private AES256CSP_Temporal_Receiver_Stream_Synchronized As IO.Stream
    Private AES256CSP_Temporal_Sender_Stream_Synchronized As IO.Stream
    Private AES256CSP_Temporal_BlockSize As Integer

    Private ReadOnly Channel_Receiver_SyncLock_Object As New Object
    Private ReadOnly Channel_Sender_SyncLock_Object As New Object
    Private ReadOnly Block_Receiver_SyncLock_Object As New Object
    Private ReadOnly Block_Sender_SyncLock_Object As New Object
    Private ReadOnly TCP_Connected_SyncLock_Object As New Object
    Public ReadOnly Property ReceiverCurrentDynamicSpeed As Int32
        Get
            Return TCP_Dynamic_Speed_Receiver_Current_Speed
        End Get
    End Property
    Public ReadOnly Property SenderCurrentDynamicSpeed As Int32
        Get
            Return TCP_Dynamic_Speed_Sender_Current_Speed
        End Get
    End Property
    Public ReadOnly Property AutoCurrentDynamicSpeed As Int32
        Get
            Return Math.Max(TCP_Dynamic_Speed_Receiver_Current_Speed, TCP_Dynamic_Speed_Sender_Current_Speed)
        End Get
    End Property
    Public Sub AddChannel(Channel As Int32, Optional BufferSize As Int32 = 16384)
        SyncLock Channel_Receiver_Synclock_Object
            SyncLock Channel_Sender_Synclock_Object
                If Channels.ContainsKey(Channel) = True Then
                    If Channels(Channel).Active = False Then Channels(Channel).Activate(BufferSize)
                Else
                    Channels.Add(Channel, New RWChannel(True, BufferSize))
                End If
            End SyncLock
        End SyncLock
    End Sub
    Public Sub RemoveChannel(Channel As Int32)
        SyncLock Channel_Receiver_Synclock_Object
            SyncLock Channel_Sender_Synclock_Object
                If Channels.ContainsKey(Channel) = True Then
                    If Channels(Channel).Active = True Then Channels(Channel).Deactivate()
                    Channels.Remove(Channel)
                End If
            End SyncLock
        End SyncLock
    End Sub

    Public Shadows ReadOnly Property Connected As Boolean
        Get
            SyncLock TCP_Connected_Synclock_Object
                SyncLock Block_Receiver_Synclock_Object
                    SyncLock Block_Sender_Synclock_Object
                        If Client.Connected = False Then TCP_Connected = False
                        If TCP_Connected = False Then Return False
                        TCP_Connected_Flag_A = Client.Poll(0, Net.Sockets.SelectMode.SelectRead)
                        TCP_Connected_Flag_B = Client.Available = 0
                        If Not TCP_Connected_Flag_A Then Return True
                        If Not TCP_Connected_Flag_B Then Return True

                        Return False
                    End SyncLock
                End SyncLock
            End SyncLock
        End Get
    End Property
    Public Shadows ReadOnly Property Available(ByVal Channel As Int32) As Int32
        Get
            SyncLock Channel_Receiver_Synclock_Object
                SyncLock Channel_Sender_Synclock_Object
                    If Channels.ContainsKey(Channel) = True Then
                        If Channels(Channel).Active = True Then
                            Return CInt(Channels(Channel).Readbuffer.Length)
                        Else
                            Return -2
                        End If
                    Else
                        Return -1
                    End If
                End SyncLock
            End SyncLock
        End Get
    End Property
    Friend Sub New(Socket As Net.Sockets.Socket)
        MyBase.New(Socket.AddressFamily)
        Client = Socket
        Initialize()
    End Sub
    Friend Sub New(Endpoint As Net.IPEndPoint)
        MyBase.New(Endpoint.AddressFamily)
        Connect(Endpoint)
        Initialize()
    End Sub
    Public Sub WriteJagged(ByRef Input As Byte()(), ByVal Channel As Int32)
        If Channels.ContainsKey(Channel) = True Then
            Dim SelectedChannel As RWChannel = Channels(Channel)
            If SelectedChannel.Active = True Then
                SyncLock Channel_Sender_Synclock_Object
                    Dim JaggedLength As Int32 = Input.Length
                    Dim JaggedLengthBytes As Byte() = BitConverter.GetBytes(JaggedLength)
                    SelectedChannel.Writebuffer.Write(JaggedLengthBytes, 0, 4)
                    For x = 0 To JaggedLength - 1
                        Dim ArrayLength As Int32 = Input(x).Length
                        Dim ArrayLengthBytes As Byte() = BitConverter.GetBytes(ArrayLength)
                        SelectedChannel.Writebuffer.Write(ArrayLengthBytes, 0, 4)
                        SelectedChannel.Writebuffer.Write(Input(x), 0, ArrayLength)
                    Next
                End SyncLock
            End If
        End If
    End Sub
    Public Sub ReadJagged(ByRef Output As Byte()(), ByVal Channel As Int32)
        If Channels.ContainsKey(Channel) = True Then
            Dim SelectedChannel As RWChannel = Channels(Channel)
            If SelectedChannel.Active = True Then
                SyncLock Channel_Receiver_Synclock_Object
                    If SelectedChannel.Readbuffer.Length > 0 Then
                        Dim JaggedLengthBytes(3) As Byte
                        Channels(Channel).Readbuffer.Read(JaggedLengthBytes, 0, 4)
                        Dim JaggedLength As Int32 = BitConverter.ToInt32(JaggedLengthBytes, 0)
                        ReDim Output(JaggedLength - 1)
                        For x = 0 To JaggedLength - 1
                            Dim ArrayLengthBytes(3) As Byte
                            SelectedChannel.Readbuffer.Read(ArrayLengthBytes, 0, 4)
                            Dim ArrayLength As Int32 = BitConverter.ToInt32(ArrayLengthBytes, 0)
                            ReDim Output(x)(ArrayLength - 1)
                            SelectedChannel.Readbuffer.Read(Output(x), 0, ArrayLength)
                        Next
                    End If
                End SyncLock
            End If
        End If
    End Sub
    Private Sub Initialize()
        TCP_RemoteEndpoint = CType(Client.RemoteEndPoint, Net.IPEndPoint)
        TCP_LocalEndpoint = CType(Client.LocalEndPoint, Net.IPEndPoint)
        Client.SendBufferSize = Int32.MaxValue
        Client.ReceiveBufferSize = Int32.MaxValue

        AES256CSP_Temporal_Sender_Timestamp = DateTime.utcNow
        
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Year, AES256CSP_Temporal_Sender_TimestampBuffer, 0)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Month, AES256CSP_Temporal_Sender_TimestampBuffer, 4)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Day, AES256CSP_Temporal_Sender_TimestampBuffer, 8)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Hour, AES256CSP_Temporal_Sender_TimestampBuffer, 12)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Minute, AES256CSP_Temporal_Sender_TimestampBuffer, 16)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Second, AES256CSP_Temporal_Sender_TimestampBuffer, 20)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Millisecond, AES256CSP_Temporal_Sender_TimestampBuffer, 24)
       
        AES256CSP_Temporal_BaseStream = GetStream
        
        SendBlock(AES256CSP_Temporal_Sender_TimestampBuffer,16, AES256CSP_Temporal_BaseStream)
        ReceiveBlock(AES256CSP_Temporal_Receiver_TimestampBuffer,AES256CSP_Temporal_BaseStream)
        
        AES256CSP_Temporal_Receiver_Timestamp = New DateTime(
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 0),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 4),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 8),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 12),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 16),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 20),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 24))
        
        AES256CSP_Temporal_Sender_EncryptionGenerator = New FastRNG(
            {AES256CSP_Temporal_Sender_Timestamp.Year,
             AES256CSP_Temporal_Sender_Timestamp.Month,
             AES256CSP_Temporal_Sender_Timestamp.Day,
             AES256CSP_Temporal_Sender_Timestamp.Hour,
             AES256CSP_Temporal_Sender_Timestamp.Minute,
             AES256CSP_Temporal_Sender_Timestamp.Second,
             AES256CSP_Temporal_Sender_Timestamp.Millisecond,
             TCP_LocalEndpoint.Port})
        AES256CSP_Temporal_Sender_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Sender_Key)
        AES256CSP_Temporal_Sender_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Sender_IV)
        AES256CSP_Temporal_Sender = New Security.Cryptography.AesCryptoServiceProvider With {
            .Key = AES256CSP_Temporal_Sender_Key, 
            .IV = AES256CSP_Temporal_Sender_IV,
            .Padding = Security.Cryptography.PaddingMode.None}
        AES256CSP_Temporal_Sender_Transform = AES256CSP_Temporal_Sender.CreateEncryptor
        AES256CSP_Temporal_Sender_Stream = New Security.Cryptography.CryptoStream(
            AES256CSP_Temporal_BaseStream,
            AES256CSP_Temporal_Sender_Transform, 
            Security.Cryptography.CryptoStreamMode.Write)
        AES256CSP_Temporal_Sender_Stream_Synchronized = IO.Stream.Synchronized(AES256CSP_Temporal_Sender_Stream)

        AES256CSP_Temporal_Receiver_EncryptionGenerator = New FastRNG(
            {AES256CSP_Temporal_Receiver_Timestamp.Year,
             AES256CSP_Temporal_Receiver_Timestamp.Month,
             AES256CSP_Temporal_Receiver_Timestamp.Day,
             AES256CSP_Temporal_Receiver_Timestamp.Hour,
             AES256CSP_Temporal_Receiver_Timestamp.Minute,
             AES256CSP_Temporal_Receiver_Timestamp.Second,
             AES256CSP_Temporal_Receiver_Timestamp.Millisecond,
             TCP_RemoteEndpoint.Port})
        AES256CSP_Temporal_Receiver_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Receiver_Key)
        AES256CSP_Temporal_Receiver_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Receiver_IV)
        AES256CSP_Temporal_Receiver = New Security.Cryptography.AesCryptoServiceProvider With {
            .Key = AES256CSP_Temporal_Receiver_Key, 
            .IV = AES256CSP_Temporal_Receiver_IV, 
            .Padding = Security.Cryptography.PaddingMode.None}
        AES256CSP_Temporal_Receiver_Transform = AES256CSP_Temporal_Receiver.CreateDecryptor
        AES256CSP_Temporal_Receiver_Stream = New Security.Cryptography.CryptoStream(
            AES256CSP_Temporal_BaseStream, 
            AES256CSP_Temporal_Receiver_Transform, 
            Security.Cryptography.CryptoStreamMode.Read)
        AES256CSP_Temporal_Receiver_Stream_Synchronized = IO.Stream.Synchronized(AES256CSP_Temporal_Receiver_Stream)

        AES256CSP_Temporal_Blocksize = CInt(AES256CSP_Temporal_Receiver.BlockSize / 8)

        
        
         AES256CSP_Temporal_Sender_Timestamp = DateTime.Now
        
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Year, AES256CSP_Temporal_Sender_TimestampBuffer, 0)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Month, AES256CSP_Temporal_Sender_TimestampBuffer, 4)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Day, AES256CSP_Temporal_Sender_TimestampBuffer, 8)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Hour, AES256CSP_Temporal_Sender_TimestampBuffer, 12)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Minute, AES256CSP_Temporal_Sender_TimestampBuffer, 16)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Second, AES256CSP_Temporal_Sender_TimestampBuffer, 20)
        Int32BlockCopyIn(AES256CSP_Temporal_Sender_Timestamp.Millisecond, AES256CSP_Temporal_Sender_TimestampBuffer, 24)
       
        AES256CSP_Temporal_BaseStream = GetStream
        
        SendBlock(AES256CSP_Temporal_Sender_TimestampBuffer)
        ReceiveBlock(AES256CSP_Temporal_Receiver_TimestampBuffer)
        
        AES256CSP_Temporal_Receiver_Timestamp = New DateTime(
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 0),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 4),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 8),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 12),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 16),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 20),
            Int32BlockCopyOut(AES256CSP_Temporal_Receiver_TimestampBuffer, 24))
        
        AES256CSP_Temporal_Sender_EncryptionGenerator = New FastRNG(
            {AES256CSP_Temporal_Sender_Timestamp.Year,
             AES256CSP_Temporal_Sender_Timestamp.Month,
             AES256CSP_Temporal_Sender_Timestamp.Day,
             AES256CSP_Temporal_Sender_Timestamp.Hour,
             AES256CSP_Temporal_Sender_Timestamp.Minute,
             AES256CSP_Temporal_Sender_Timestamp.Second,
             AES256CSP_Temporal_Sender_Timestamp.Millisecond,
             TCP_RemoteEndpoint.Port})
        AES256CSP_Temporal_Sender_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Sender_Key)
        AES256CSP_Temporal_Sender_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Sender_IV)
        AES256CSP_Temporal_Sender = New Security.Cryptography.AesCryptoServiceProvider With {
            .Key = AES256CSP_Temporal_Sender_Key, 
            .IV = AES256CSP_Temporal_Sender_IV,
            .Padding = Security.Cryptography.PaddingMode.None}
        AES256CSP_Temporal_Sender_Transform = AES256CSP_Temporal_Sender.CreateEncryptor
        AES256CSP_Temporal_Sender_Stream = New Security.Cryptography.CryptoStream(
            AES256CSP_Temporal_BaseStream,
            AES256CSP_Temporal_Sender_Transform, 
            Security.Cryptography.CryptoStreamMode.Write)
        AES256CSP_Temporal_Sender_Stream_Synchronized = IO.Stream.Synchronized(AES256CSP_Temporal_Sender_Stream)

        AES256CSP_Temporal_Receiver_EncryptionGenerator = New FastRNG(
            {AES256CSP_Temporal_Receiver_Timestamp.Year,
             AES256CSP_Temporal_Receiver_Timestamp.Month,
             AES256CSP_Temporal_Receiver_Timestamp.Day,
             AES256CSP_Temporal_Receiver_Timestamp.Hour,
             AES256CSP_Temporal_Receiver_Timestamp.Minute,
             AES256CSP_Temporal_Receiver_Timestamp.Second,
             AES256CSP_Temporal_Receiver_Timestamp.Millisecond,
             TCP_LocalEndpoint.Port})
        AES256CSP_Temporal_Receiver_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Receiver_Key)
        AES256CSP_Temporal_Receiver_EncryptionGenerator.GetRNGBytes(AES256CSP_Temporal_Receiver_IV)
        AES256CSP_Temporal_Receiver = New Security.Cryptography.AesCryptoServiceProvider With {
            .Key = AES256CSP_Temporal_Receiver_Key, 
            .IV = AES256CSP_Temporal_Receiver_IV, 
            .Padding = Security.Cryptography.PaddingMode.None}
        AES256CSP_Temporal_Receiver_Transform = AES256CSP_Temporal_Receiver.CreateDecryptor
        AES256CSP_Temporal_Receiver_Stream = New Security.Cryptography.CryptoStream(
            AES256CSP_Temporal_BaseStream, 
            AES256CSP_Temporal_Receiver_Transform, 
            Security.Cryptography.CryptoStreamMode.Read)
        AES256CSP_Temporal_Receiver_Stream_Synchronized = IO.Stream.Synchronized(AES256CSP_Temporal_Receiver_Stream)

        AES256CSP_Temporal_Blocksize = CInt(AES256CSP_Temporal_Receiver.BlockSize / 8)

        
        TCP_Connected = True
        Dim SenderThread As New Threading.Thread(AddressOf SenderThreadMethod)
        Dim ReceiverThread As New Threading.Thread(AddressOf ReceiverThreadMethod)
        SenderThread.Start()
        ReceiverThread.Start()
    End Sub
    Private Sub SenderThreadMethod()
        Dim ThreadGovernor As New Governors.LoopGovernor(3)
        Do While Connected = True
            ThreadGovernor.LoopsPerSecond = TCP_Dynamic_Speed_Sender_Current_Speed
            SyncLock Channel_Sender_Synclock_Object
                For Each Item In Channels
                    If Item.Value.Active = True Then
                        If Item.Value.Writebuffer.Length > 0 Then
                            Dim Data(CInt(Item.Value.Writebuffer.Length) - 1) As Byte
                            Dim StreamBuffer(Data.Length + 11) As Byte
                            Dim Code As Int32 = 0
                            Dim Channel As Int32 = Item.Key

                            Item.Value.Writebuffer.Read(Data, 0, Data.Length)

                            Int32BlockCopyIn(Code, StreamBuffer, 0)
                            Int32BlockCopyIn(Channel, StreamBuffer, 4)
                            Int32BlockCopyIn(Data.Length, StreamBuffer, 8)
                            Buffer.BlockCopy(Data, 0, StreamBuffer, 12, Data.Length)
                            SendBlock(StreamBuffer)
                        End If
                    End If
                    TCP_Dynamic_Speed_Sender_Do_Increase = True
                Next
            End SyncLock
            If TCP_Dynamic_Speed_Sender_Do_Increase = True Then
                If TCP_Dynamic_Speed_Sender_Current_Speed < TCP_Dynamic_Speed_Maximum Then TCP_Dynamic_Speed_Sender_Current_Speed += TCP_Dynamic_Speed_Increment
                If TCP_Dynamic_Speed_Sender_Current_Speed > TCP_Dynamic_Speed_Maximum Then TCP_Dynamic_Speed_Sender_Current_Speed = TCP_Dynamic_Speed_Maximum
            Else
                If TCP_Dynamic_Speed_Sender_Current_Speed > TCP_Dynamic_Speed_Minimum Then TCP_Dynamic_Speed_Sender_Current_Speed -= TCP_Dynamic_Speed_Decrement
                If TCP_Dynamic_Speed_Sender_Current_Speed < TCP_Dynamic_Speed_Minimum Then TCP_Dynamic_Speed_Sender_Current_Speed = TCP_Dynamic_Speed_Minimum
            End If
            TCP_Dynamic_Speed_Sender_Do_Increase = False
            ThreadGovernor.Limit()
        Loop
    End Sub
    Private Sub ReceiverThreadMethod()
        Dim ThreadGovernor As New Governors.LoopGovernor(3)
        Do While Connected = True
            ThreadGovernor.LoopsPerSecond = TCP_Dynamic_Speed_Receiver_Current_Speed
            If MyBase.Available > 0 Then
                SyncLock Channel_Receiver_Synclock_Object
                    Dim StreamBuffer As Byte() = Nothing
                    ReceiveBlock(StreamBuffer)
                    Dim Code As Int32 = Int32BlockCopyOut(StreamBuffer, 0)
                    
                    Select Case Code
                        Case 0
                            Dim Channel As Int32 = Int32BlockCopyOut(StreamBuffer, 4)
                            Dim Length As Int32 = Int32BlockCopyOut(StreamBuffer, 8)
                            Dim Data(Length - 1) As Byte
                            Buffer.BlockCopy(StreamBuffer, 12, Data, 0, Length)
                            If Channels.ContainsKey(Channel) = True Then
                                If Channels(Channel).Active = True Then
                                    Channels(Channel).Readbuffer.Write(Data, 0, Data.Length)
                                End If
                            End If
                    End Select

                End SyncLock
                TCP_Dynamic_Speed_Receiver_Do_Increase = True
            End If
            If TCP_Dynamic_Speed_Receiver_Do_Increase = True Then
                If TCP_Dynamic_Speed_Receiver_Current_Speed < TCP_Dynamic_Speed_Maximum Then TCP_Dynamic_Speed_Receiver_Current_Speed += TCP_Dynamic_Speed_Increment
                If TCP_Dynamic_Speed_Receiver_Current_Speed > TCP_Dynamic_Speed_Maximum Then TCP_Dynamic_Speed_Receiver_Current_Speed = TCP_Dynamic_Speed_Maximum
            Else
                If TCP_Dynamic_Speed_Receiver_Current_Speed > TCP_Dynamic_Speed_Minimum Then TCP_Dynamic_Speed_Receiver_Current_Speed -= TCP_Dynamic_Speed_Decrement
                If TCP_Dynamic_Speed_Receiver_Current_Speed < TCP_Dynamic_Speed_Minimum Then TCP_Dynamic_Speed_Receiver_Current_Speed = TCP_Dynamic_Speed_Minimum
            End If
            TCP_Dynamic_Speed_Receiver_Do_Increase = False
            ThreadGovernor.Limit()
        Loop
    End Sub
    Private Sub SendBlock(ByRef Data As Byte(), Optional ByVal BlockSize As Int32 = 0, Optional ByVal Stream As IO.Stream = Nothing)
        SyncLock Block_Sender_Synclock_Object
            If BlockSize = 0 Then BlockSize = AES256CSP_Temporal_Blocksize
            If Stream Is Nothing Then Stream = AES256CSP_Temporal_Sender_Stream_Synchronized
            If Data Is Nothing Then Return 

            Dim Header(15) As Byte
            Dim DataLength As Int32 = Data.Length
            Dim BlockLength As Int32 = CInt(Math.Ceiling(DataLength / BlockSize)) * BlockSize
            Int32BlockCopyIn(DataLength, Header, 0)
            Int32BlockCopyIn(BlockLength, Header, 4)
            Try
                Stream.Write(Header, 0, 16)

                Dim Padded(BlockLength - 1) As Byte
                Buffer.BlockCopy(Data, 0, Padded, 0, DataLength)

                Stream.Write(Padded, 0, BlockLength)
                Stream.Flush()
            Catch ex As Exception
                TCP_Connected = False
            End Try
        End SyncLock
    End Sub
    Private Sub ReceiveBlock(ByRef Data As Byte(), Optional ByVal Stream As IO.Stream = Nothing)
        SyncLock Block_Receiver_Synclock_Object
            If Stream Is Nothing Then Stream = AES256CSP_Temporal_Receiver_Stream_Synchronized
            Do Until Client.Available>=16
                Threading.Thread.Sleep(10)
            Loop
            Dim Header(15) As Byte
            Stream.Read(Header, 0, 16)
            Dim DataLength As Int32 = Int32BlockCopyOut(Header, 0)
            Dim BlockLength As Int32 = Int32BlockCopyOut(Header, 4)
            Do Until Client.Available>=BlockLength
                Threading.Thread.Sleep(10)
            Loop
            ReDim Data(DataLength - 1)
            Dim Padded(BlockLength - 1) As Byte
            Stream.Read(Padded, 0, BlockLength)
            Buffer.BlockCopy(Padded, 0, Data, 0, DataLength)
        End SyncLock
    End Sub
    Private Sub Int32BlockCopyIn(ByRef Source As Int32, ByRef Destination As Byte(), ByVal Offset As Int32)
        Dim SourceBytes As Byte() = BitConverter.GetBytes(Source)
        Buffer.BlockCopy(SourceBytes, 0, Destination, Offset, 4)
    End Sub
    Private Function Int32BlockCopyOut(ByRef Source As Byte(), ByVal Offset As Int32, Optional ByRef Destination As Int32 = Nothing) As Int32
        Dim DestinationBytes(3) As Byte
        Buffer.BlockCopy(Source, Offset, DestinationBytes, 0, 4)
        Destination = BitConverter.ToInt32(DestinationBytes, 0)
        Return Destination
    End Function
End Class