Imports System.Threading
Imports Maelstrom

Friend Class TestClient : Inherits ClientBase
    Private Subsockets As Dictionary(Of UInt32, Maelstrom.Socket) = New Dictionary(Of UInt32, Maelstrom.Socket)

    Private Payload as Byte() =
                {1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 
                 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1, 1,0,1,0,1,0,1,0, 1,0,1,0,1,0,1,1}
    Private ReadOnly WaitLock As Object = New Object()
    Private ReadOnly Console As AsyncConsole = New AsyncConsole()
    Dim Speed As Double = 1
    Friend Sub SetSpeed(Rate as Double)
        Speed = Rate
    End Sub
    
    Dim Instances As Int32 = 1
    Friend Sub SetInstances(Amount as Int32)
        Instances = Amount
    End Sub
    
    
    
    Private Sub CreateAsyncInstance(Socket as Maelstrom.Socket, Subsocket as Int32)
        Dim AsyncThread as new Thread(
            Sub()
                Dim Governor as new Governor(Speed)
                Dim Buffer(Payload.Length - 1) as Byte
                Dim Compared as Boolean
                If Socket.Add(Subsocket) = False Then Debug.Print("DEBUG - CLIENT - ASYNC INSTANCE " & Subsocket & ": encryption sync failed!")
                Socket.Compressed(Subsocket) = False
                Socket.Encrypted(Subsocket) = False
                
                Console.WriteLine("DEBUG - CLIENT - ASYNC INSTANCE " + Subsocket.ToString() + " - awaiting unlock...")
                SyncLock WaitLock : End SyncLock
                Console.WriteLine("DEBUG - CLIENT - ASYNC INSTANCE " + Subsocket.ToString() + " - unlocked!") 
                
                Socket.Write(Subsocket, Payload)
                Do While Socket.Connected = True
                    If Socket.Pending(SubSocket) = True Then : Console.WriteLine("DEBUG - CLIENT - ASYNC INSTANCE " + SubSocket.ToString() + " - data is available to read. reading data...") 
                        Socket.Read(Subsocket, Buffer) : Console.WriteLine("DEBUG - CLIENT - ASYNC INSTANCE " + SubSocket.ToString() + " - data read. validating...") 
                        Compared = BinaryCompare(Buffer, Payload, 0, Buffer.Length)
                        For x = 0 To Buffer.Length - 1
                            Buffer(x) = 0
                        Next
                        If Compared = False Then
                            Console.WriteLine("DEBUG - CLIENT - ASYNC INSTANCE " + Subsocket.ToString() + " - data failed to validate! disconnecting...") 
                            Socket.Disconnect()
                        Else
                            Console.WriteLine("DEBUG - CLIENT - ASYNC INSTANCE " + Subsocket.ToString() + " - data validated. sending response data...")
                            Socket.Write(Subsocket, Payload)  
                        End If
                    End If
                    Governor.Limit()
                Loop
                Socket.Remove(Subsocket)
            End Sub)
        AsyncThread.Start()
    End Sub
    
    Protected Overrides Sub OnLoading(Socket As Maelstrom.Socket)
        Console.WriteLine("DEBUG - CLIENT: connecting...") 
    End Sub
    
    Protected Overrides Sub OnLoaded(Socket As Maelstrom.Socket)
        Console.WriteLine("DEBUG - CLIENT: connected.") 
        Console.WriteLine("DEBUG - CLIENT: creating async instances....") 
        SyncLock WaitLock
            For x = 0 to Instances - 1
                Threading.Thread.Sleep(10)
                CreateAsyncInstance(Socket,x)
                Console.WriteLine("DEBUG - CLIENT: async instance " + x.ToString() + " created.") 
                
            Next
        End SyncLock
        Console.WriteLine("DEBUG - CLIENT: all async instances created.") 
        
    End Sub

    Protected Overrides Sub OnUnloading(Socket As Maelstrom.Socket)
        Console.WriteLine("DEBUG - CLIENT: disconnecting...") 
    End Sub

    Protected Overrides Sub OnUnloaded(Socket As Maelstrom.Socket)
        Console.WriteLine("DEBUG - CLIENT: disconnected.") 
    End Sub

End Class