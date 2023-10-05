Public Class TestServer : Inherits Maelstrom.BaseServer
    Private Payload as Byte() =
                {1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1,
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1}
    
    Public Property WorkerCount As UInt32 = 1
    Public Property WorkerSpeed As Double = 1.0

    Protected Overrides Sub OnConnect(Socket As Maelstrom.Socket)
        Console.WriteLine("  Server: is connecting a client")
        Lightning.LambdaThread(Of Maelstrom.Socket).Start(AddressOf Kickoff, Socket, "TestServer Networked Worker Kickoff Thread")
    End Sub

    Protected Overrides Sub OnDisconnect(Socket As Maelstrom.Socket)
        Console.WriteLine("  Server: is disconnecting a client")
    End Sub
    
    Protected Overrides Sub OnDrop(Socket As Maelstrom.Socket)
        Console.WriteLine("  Server: has disconnected an invalid client. reason: " & Socket.SyncResult.ToString() & ", remote identification token: " & Convert.ToBase64String(Socket.RemoteIdentificationToken))
    End Sub
    Protected Overrides Sub OnListen()
        Console.WriteLine("  Server: is now listening")
    End Sub

    Protected Overrides Sub OnDeafen()
        Console.WriteLine("  Server: is no longer listening")
    End Sub
    
    Private Sub Kickoff(Socket As Maelstrom.Socket)
        If Math.Abs(WorkerSpeed - 0.0) <= 0.0 Then Return
        If WorkerCount = 0 Then Return
        Console.WriteLine("  Server: is now spawning " & WorkerCount & " networked workers"  )
        For Subsocket As UInt32 = 1 To WorkerCount
            Console.WriteLine("  Server:   spawning networked worker " & Subsocket & " operating on subsocket " & Subsocket & " at a frequency of " & WorkerSpeed & "hz"  )
            Lightning.LambdaThread(Of Maelstrom.Socket, UInt32).Start(AddressOf Worker, Socket, Subsocket, "TestServer Networked Worker Thread")
        Next
    End Sub
    
    Private Sub Worker(Socket As Maelstrom.Socket, Subsocket As UInt32)
        Dim Buffer(Payload.Length - 1) as Byte
        Socket.Create(Subsocket)
        Socket.Write(Subsocket, Payload)
        Dim Governor As New Lightning.Governor(WorkerSpeed)
        Do While Socket.Connected = True
            If Socket.Available(Subsocket) = True Then
                Socket.Read(Subsocket, Buffer)
                If Validate(Buffer, Payload, Payload.Length) = False Then Socket.Disconnect() : Array.Clear(Buffer, 0, Buffer.Length)
                If Socket.Connected = True Then Socket.Write(Subsocket, Payload)
            End If
            Governor.Limit()
        Loop
        Console.WriteLine("  Server:   networked worker " & Subsocket & " operating on subsocket " & Subsocket & " at a frequency of " & WorkerSpeed & "hz is now exiting")
    End Sub
    
    Private Function Validate(A As Byte(), B As Byte(), Length As Int32) As Boolean
        Dim Validated = True
        For Index = 0 To Length - 1
            If A(Index) <> B(Index) Then Validated = False
        Next
        Return Validated
    End Function
End Class