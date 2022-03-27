
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
    
    Public Property ReadTimeout As Int32 = 5000
End Class