Imports Maelstrom.Lightning

Public Partial MustInherit Class ClientBase
    Private WithEvents NetSocket As Lightning.Socket
    Private SocketLookup As Dictionary(Of Lightning.Socket, Socket)
End Class