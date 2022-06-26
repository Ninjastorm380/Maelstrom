Friend Module Constants
    
    ''' <summary>
    ''' Block size in bytes for AES256 cryptographic transformations.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Const AESBlockSize as UInt32 = 16
    
    ''' <summary>
    ''' Block size in bytes for large data send operations.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Const LargeBlockSize as UInt32 = 20000
    
    ''' <summary>
    ''' Internal flag constant that is used in conjunction with enum SubSocketConfigFlags. When present in DataHeader.SubSocketConfig, it indicates that a large send operation is occuring.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Const SubSocketConfigFlagLarge = 4
    
    ''' <summary>
    ''' Binary representation of the maelstrom protocol version. Exchanged between a client and server as an compatibility check during init.
    ''' </summary>
    ''' <remarks></remarks>
    Friend ReadOnly MaelstromHandshakeVersion as Byte() = {0,0,0,2, 0,0,0,0, 0,0,0,0, 0,0,0,0}
    
    ''' <summary>
    ''' Binary representation of the word "MAELSTROMSECURED". Exchanged between a client and server as an integrity check during init.
    ''' </summary>
    ''' <remarks></remarks>
    Friend ReadOnly MaelstromHandshakeHeader as Byte() = {109, 97, 101, 108, 115, 116, 114, 111, 109, 115, 101, 99, 117, 114, 101, 100}
    
    ''' <summary>
    ''' Binary representation a zeroed maelstrom handshake header.
    ''' </summary>
    ''' <remarks></remarks>
    Friend ReadOnly MaelstromHeaderZeroed as Byte() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0}
End Module