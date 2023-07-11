Public Partial MustInherit Class ServerBase
    Private WithEvents NetSocket As Lightning.Socket
    Private SocketLookup As Dictionary(Of Lightning.Socket, Socket)
End Class