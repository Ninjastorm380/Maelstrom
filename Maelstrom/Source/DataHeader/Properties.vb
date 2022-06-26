Friend Partial Class DataHeader
    
    ''' <summary>
    ''' Byte array. Underlying data for this DataHeader.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Property HeaderRaw As Byte()
        Get
            Return BaseHeaderRaw
        End Get
        Set
            BaseHeaderRaw = Value
        End Set
    End Property
    
    ''' <summary>
    ''' UInt32. Subsocket this DataHeader reports for.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Property SubSocket As UInt32
        Get
            UnpackUInt32(BaseHeaderRaw, 0, BaseSubSocket)
            Return BaseSubSocket
        End Get
        Set
            BaseSubSocket = Value
            PackUInt32(BaseHeaderRaw, 0, BaseSubSocket)
        End Set
    End Property
    
    ''' <summary>
    ''' UInt32. Length of data before all required transformations are applied.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Property RawLength As UInt32
        Get
            UnpackUInt32(BaseHeaderRaw, 4, BaseRawLength)
            Return BaseRawLength
        End Get
        Set
            BaseRawLength = Value
            PackUInt32(BaseHeaderRaw, 4, BaseRawLength)
        End Set
    End Property
    
    ''' <summary>
    ''' UInt32. Length of data after the compression transformation. Set to 0 if no compression transformation was applied.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Property CompressedLength As UInt32
        Get
            UnpackUInt32(BaseHeaderRaw, 8, BaseCompressedLength)
            Return BaseCompressedLength
        End Get
        Set
            BaseCompressedLength = Value
            PackUInt32(BaseHeaderRaw, 8, BaseCompressedLength)
        End Set
    End Property
    
    ''' <summary>
    ''' SubSocketConfigFlag. Configuration of the overall send/receive.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Property SubSocketConfig As SubSocketConfigFlag
        Get
            BaseSubSocketConfig = BaseHeaderRaw(16)
            Return BaseSubSocketConfig
        End Get
        Set
            BaseSubSocketConfig = Value
            BaseHeaderRaw(16) = Value
        End Set
    End Property
    
    ''' <summary>
    ''' UInt32. Length of data after resizing for encryption. This is always a multiple of the constant AESBlockSize.
    ''' </summary>
    ''' <remarks></remarks>
    Friend ReadOnly Property EncryptedLength As UInt32
        Get
            Select Case SubSocketConfig
                Case = SubSocketConfigFlag.Encrypted
                    Return CInt(math.Ceiling(RawLength/AESBlockSize))*AESBlockSize
                Case = SubSocketConfigFlag.Compressed + SubSocketConfigFlag.Encrypted
                    Return CInt(math.Ceiling(CompressedLength/AESBlockSize))*AESBlockSize
                Case = SubSocketConfigFlagLarge + SubSocketConfigFlag.Encrypted
                    Return CInt(math.Ceiling(RawLength/AESBlockSize))*AESBlockSize
                Case = SubSocketConfigFlagLarge + SubSocketConfigFlag.Compressed + SubSocketConfigFlag.Encrypted
                    Return CInt(math.Ceiling(CompressedLength/AESBlockSize))*AESBlockSize
            End Select
            Return 0
        End Get
    End Property
    
    ''' <summary>
    ''' UInt32. Length of data after resizing for a large send operation. This is always both a multiple of the constant AESBlockSize and the constant LargeBlockSize.
    ''' </summary>
    ''' <remarks></remarks>
    Friend ReadOnly Property LargeLength As UInt32
        Get
            Select Case SubSocketConfig
                Case = SubSocketConfigFlagLarge
                    Return CInt(math.Ceiling(RawLength/LargeBlockSize))*LargeBlockSize
                    
                Case = SubSocketConfigFlagLarge + SubSocketConfigFlag.Compressed
                    Return CInt(math.Ceiling(CompressedLength/LargeBlockSize))*LargeBlockSize
                    
                Case = SubSocketConfigFlagLarge + SubSocketConfigFlag.Encrypted
                    Return CInt(math.Ceiling(EncryptedLength/LargeBlockSize))*LargeBlockSize
                    
                Case = SubSocketConfigFlagLarge + SubSocketConfigFlag.Compressed + SubSocketConfigFlag.Encrypted
                    Return CInt(math.Ceiling(EncryptedLength/LargeBlockSize))*LargeBlockSize
                    
            End Select
            Return 0
        End Get
    End Property
    
    
    ''' <summary>
    ''' UInt32. Length of data after all necessary transformations have been applied.
    ''' </summary>
    ''' <remarks></remarks>
    Friend ReadOnly Property Length As UInt32
        Get
            Select Case SubSocketConfig
                Case = SubSocketConfigFlag.Nothing
                    Return RawLength
                Case = SubSocketConfigFlag.Compressed
                    Return CompressedLength
                Case = SubSocketConfigFlag.Encrypted
                    Return EncryptedLength
                Case = SubSocketConfigFlagLarge
                    Return LargeLength
                Case = SubSocketConfigFlag.Compressed + SubSocketConfigFlag.Encrypted
                    Return EncryptedLength
                Case = SubSocketConfigFlag.Compressed + SubSocketConfigFlagLarge
                    Return LargeLength
                Case = SubSocketConfigFlag.Encrypted + SubSocketConfigFlagLarge
                    Return LargeLength
                Case = SubSocketConfigFlag.Compressed + SubSocketConfigFlag.Encrypted + SubSocketConfigFlagLarge
                    Return LargeLength
            End Select
            Return 0
        End Get
    End Property
End Class