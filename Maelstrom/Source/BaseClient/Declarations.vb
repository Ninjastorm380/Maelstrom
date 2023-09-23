Public Partial MustInherit Class BaseClient
    Private WithEvents BaseSocket As New Lightning.Socket
    Private SocketLookup As New Dictionary(Of Lightning.Socket, Socket)
End Class