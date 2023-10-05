Public Enum Result
    
    ''' <remarks>
    ''' Maelstrom sync was successful.
    ''' </remarks>
    Ok = 0
    
    ''' <remarks>
    ''' Some data could not be received from the underlying transport connection during maelstrom sync. Check your network.
    ''' </remarks>
    TransferFailure = 1
    
    ''' <remarks>
    '''  Basic data validation failed during maelstrom sync. Check that the remote endpoint supports maelstrom and try again. Both endpoints have disconnected.
    ''' </remarks>
    ValidationFailure = 2
    
    ''' <remarks>
    '''  <list type="table">
    '''   <item>
    '''    <term><c>BaseClient</c>:</term>
    '''    <description>
    '''     The identity of the selected server has not been added to the client trust list. The client has disconnected from the selected server.
    '''    </description>
    '''   </item>
    '''   <item>
    '''    <term><c>BaseServer</c>:</term>
    '''    <description>
    '''     The identity of the connecting client has been blocked on this server. The connecting client has been disconnected.
    '''    </description>
    '''   </item>
    '''  </list>
    ''' </remarks>
    IdentityFailure = 3
    
    ''' <remarks>
    ''' The socket disconnected before maelstrom could present to user code. Check your network.
    ''' </remarks>
    SocketDead = 4
    
    ''' <remarks>
    ''' If you are seeing this as an error, then something has gone very, very wrong!
    ''' </remarks>
    NotReady = 5
    
End Enum