    ''' <summary>
    ''' Representation of maelstrom header data.
    ''' </summary>
    ''' <remarks></remarks>
Friend Partial Class DataHeader
        
        ''' <summary>
        ''' Base variable for HeaderRaw
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseHeaderRaw(31) as Byte
        
        ''' <summary>
        ''' Base variable for RawLength.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseRawLength As UInt32 = 0
        
        ''' <summary>
        ''' Base variable for CompressedLength.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseCompressedLength As UInt32 = 0
        
        ''' <summary>
        ''' Base variable for SubSocketConfig.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseSubSocketConfig As Byte = 0
        
        ''' <summary>
        ''' Base variable for SubSocket.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseSubSocket As UInt32 = 0
End Class