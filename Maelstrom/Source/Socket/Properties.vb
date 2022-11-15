
    Public Partial Class Socket
        Public Readonly Property Connected as Boolean
            Get
                Return NetSocket.Connected
            End Get
        End Property
        
        Public Readonly Property Pending(Subsocket As UInt32) as Boolean
            Get
                'Dim AsyncRead As New Threading.Thread(AddressOf AsyncReadMethod)
                'AsyncRead.Start()
                AsyncReadMethod()
                If SubSocketBuffers.Contains(Subsocket) = False Then Return False
                If SubSocketBuffers(SubSocket).Length >= 32 Then Return True Else Return False
            End Get
        End Property
        Public Property Encrypt(Subsocket As UInt32) As Boolean
            Get
                If Exists(Subsocket) = False Then Throw New ArgumentException("The requested subsocket does not exist!", "Subsocket")
                If SubsocketEncryptionFlags(Subsocket) = 0 Then
                    Return False
                ElseIf SubsocketEncryptionFlags(Subsocket) <> 0
                    Return true
                End If
                Return Nothing
            End Get
            Set
                If Exists(Subsocket) = False Then Throw New ArgumentException("The requested subsocket does not exist!", "Subsocket")
                If Value = True Then
                    SubsocketEncryptionFlags(Subsocket) = 1
                Else
                    SubsocketEncryptionFlags(Subsocket) = 0
                End If
            End Set
        End Property
        Public Property Compress(Subsocket As UInt32) As Boolean
            Get
                If Exists(Subsocket) = False Then Throw New ArgumentException("The requested subsocket does not exist!", "Subsocket")
                If SubsocketCompressionFlags(Subsocket) = 0 Then
                    Return False
                ElseIf SubsocketCompressionFlags(Subsocket) <> 0
                    Return true
                End If
                Return Nothing
            End Get
            Set
                If Exists(Subsocket) = False Then Throw New ArgumentException("The requested subsocket does not exist!", "Subsocket")
                If Value = True Then
                    SubsocketCompressionFlags(Subsocket) = 1
                Else
                    SubsocketCompressionFlags(Subsocket) = 0
                End If
            End Set
        End Property

    End Class
