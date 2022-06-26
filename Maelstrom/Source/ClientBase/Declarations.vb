    ''' <summary>
    ''' Base class for creating a maelstrom client.
    ''' </summary>
    ''' <remarks></remarks>
Public MustInherit Partial Class ClientBase
    
    ''' <summary>
    ''' Base Net.Sockets.Socket for MSocket.
    ''' </summary>
    ''' <remarks></remarks>
    Private BSocket as System.Net.Sockets.Socket 
    
    ''' <summary>
    ''' The client maelstrom socket.
    ''' </summary>
    ''' <remarks></remarks>
    Private MSocket as Socket
End Class