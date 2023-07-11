Imports System.Threading
Imports Maelstrom

Friend Class TestServer : Inherits ServerBase
    Private Payload as Byte() =
                {1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1}
    
    Private ReadOnly WaitLock As Object = New Object()
    Private ReadOnly Console As AsyncConsole = New AsyncConsole()
    Dim Speed As Double = 1
    Friend Sub SetSpeed(Rate as Double)
        Speed = Rate
    End Sub
    
    Dim Instances As Double = 1
    Friend Sub SetInstances(Amount as UInt32)
        Instances = Amount
    End Sub
    
    Private Sub CreateAsyncInstance(Socket as Socket, Subsocket as UInt32)
        Dim AsyncThread as new Thread(
            Sub()
                Dim Governor as new Governor(Speed)
                Dim Buffer(Payload.Length - 1) as Byte
                For x = 0 To Buffer.Length - 1
                    Buffer(x) = 0
                Next
                Dim Compared as Boolean

                Console.WriteLine("DEBUG - SERVER - ASYNC INSTANCE " + Subsocket.ToString() + " - awaiting unlock...")
                SyncLock WaitLock : End SyncLock
                Console.WriteLine("DEBUG - SERVER - ASYNC INSTANCE " + Subsocket.ToString() + " - unlocked!") 
                
                Socket.Write(Subsocket, Payload)
                Do While Socket.Connected = True And Listening = True
                    If Socket.Pending(Subsocket) = True Then : Console.WriteLine("DEBUG - SERVER - ASYNC INSTANCE " + Subsocket.ToString() + " - data is available to read. reading data...") 
                        Socket.Read(Subsocket, Buffer) : Console.WriteLine("DEBUG - SERVER - ASYNC INSTANCE " + Subsocket.ToString() + " - data read. validating...") 
                        Compared = BinaryCompare(Buffer, Payload, 0, Buffer.Length)
                        For x = 0 To Buffer.Length - 1
                            Buffer(x) = 0
                        Next
                        If Compared = False Then
                            Console.WriteLine("DEBUG - SERVER - ASYNC INSTANCE " + Subsocket.ToString() + " - data failed to validate! disconnecting...") 
                            Socket.Disconnect()
                        Else
                            Console.WriteLine("DEBUG - SERVER - ASYNC INSTANCE " + Subsocket.ToString() + " - data validated. sending response data...")
                            Socket.Write(Subsocket, Payload)  
                        End If
                        
                    End If
                    Governor.Limit()
                Loop
            End Sub)
        AsyncThread.Start()
    End Sub

    Protected Overrides Sub OnLoading(Socket As Socket)

    End Sub
    
    Protected Overrides Sub OnLoaded(Socket As Socket)
        For Subsocket = 0 to Instances - 1
            Socket.Add(Subsocket)
            Socket.Compressed(Subsocket) = False
            Socket.Encrypted(Subsocket) = False
        Next
        
        Console.WriteLine("DEBUG - SERVER: client has connected")
        Console.WriteLine("DEBUG - SERVER: creating async instances....") 
        SyncLock WaitLock
            For x = 0 to Instances - 1
                CreateAsyncInstance(Socket,x)
                Console.WriteLine("DEBUG - SERVER: async instance " + x.ToString() + " created.") 
            Next
        End SyncLock
        Console.WriteLine("DEBUG - SERVER: all async instances created.") 
    End Sub
    
    Protected Overrides Sub OnUnloading(Socket As Socket)
        For Subsocket = 0 to Instances - 1
            Socket.Remove(Subsocket)
        Next
    End Sub

    Protected Overrides Sub OnUnloaded(Socket As Socket)
        
    End Sub

    Protected Overrides Sub OnListenerBinding()
        
    End Sub

    Protected Overrides Sub OnListenerBound()
        
    End Sub

    Protected Overrides Sub OnListenerUnbinding()
        
    End Sub

    Protected Overrides Sub OnListenerUnbound()
        
    End Sub
End Class