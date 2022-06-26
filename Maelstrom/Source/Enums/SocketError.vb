    ''' <summary>
    ''' Errors that can be returned by a maelstrom socket during init.
    ''' </summary>
    ''' <remarks></remarks>
Public Enum SocketError
    
    ''' <summary>
    ''' Maelstrom header mismatch during init. The remote end is likely not using a maelstrom socket, or corruption occured.
    ''' </summary>
    ''' <remarks></remarks>
    HeaderMismatch = 1 
    
    ''' <summary>
    ''' Maelstrom version mismatch during init. The remote end is likely not using a current maelstrom library version.
    ''' </summary>
    ''' <remarks></remarks>
    VersionMismatch = 2 
End Enum