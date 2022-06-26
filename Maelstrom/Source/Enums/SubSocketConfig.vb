    ''' <summary>
    ''' User configurable maelstrom subsocket flags.
    ''' </summary>
    ''' <remarks></remarks>
<Flags> Public Enum SubSocketConfigFlag As Byte
    ''' <summary>
    ''' Default. Do not perform any encryption or compression on this Subsocket.
    ''' </summary>
    ''' <remarks></remarks>
    [Nothing] = 0
    
    ''' <summary>
    ''' Apply a compression transformation to outgoing data.
    ''' </summary>
    ''' <remarks></remarks>
    Compressed = 1
    
    ''' <summary>
    ''' Apply an encryption transformation to outgoing data.
    ''' </summary>
    ''' <remarks></remarks>
    Encrypted = 2
End Enum