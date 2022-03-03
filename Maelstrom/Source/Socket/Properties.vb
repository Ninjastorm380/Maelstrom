
Public Partial Class Socket
    Implements IDisposable
    
    Public Readonly Property Connected as Boolean
        Get
            Synclock ReadLock
                Synclock WriteLock
                    Return IsConnected
                End SyncLock
            End SyncLock
        End Get
    End Property

    Private Property Disposed as boolean = False
    
End Class