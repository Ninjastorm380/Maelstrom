    Imports System.Net.Sockets
    Imports System.Threading

''' <summary>
    ''' Base class for creating a maelstrom server.
    ''' </summary>
    ''' <remarks></remarks>
Public MustInherit Partial Class ServerBase
        
        ''' <summary>
        ''' Listens for connecting sockets, which we convert to maelstrom sockets.
        ''' </summary>
        ''' <remarks></remarks>
    Private Listener As TcpListener 'Listens for connecting sockets, which we convert to maelstrom sockets
        
        ''' <summary>
        ''' Seperate thread for variable Listener to operate within.
        ''' </summary>
        ''' <remarks></remarks>
    Private ListenerThread As Thread 'Seperate thread for variable Listener to operate within.
        
        ''' <summary>
        ''' Base variable for read only property online.
        ''' </summary>
        ''' <remarks></remarks>
    Private IsOnline As Boolean = False 'Base variable for property online
End Class