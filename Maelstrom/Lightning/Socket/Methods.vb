
Namespace Lightning
    Friend Partial Class Socket : Implements IDisposable
    
        Public Sub New()
            BaseClientSide = True
        End Sub
    
        Private Sub New(Socket As Net.Sockets.Socket)
            BaseClientSide = False
            NetSocket = Socket
            RemoteEndpoint = CType(NetSocket.LocalEndPoint, Net.IPEndPoint)
            LocalEndpoint = CType(NetSocket.RemoteEndPoint, Net.IPEndPoint)
            IsConnected = True
        End Sub
    
        Private Sub AsyncOnConnect(Socket As Socket)
            RaiseEvent OnConnect(Socket)
        End Sub
    
        Private Sub AsyncOnDisconnect(Socket As Socket)
            RaiseEvent OnDisconnect(Socket)
        End Sub
    
        Private Sub AsyncOnListen(Socket As Socket)
            RaiseEvent OnListen(Socket)
        End Sub
    
        Private Sub AsyncOnDeafen(Socket As Socket)
            RaiseEvent OnDeafen(Socket)
        End Sub
    
        Public Sub Connect(Endpoint As Net.IPEndPoint, SocketType As Net.Sockets.SocketType, ProtocolType As Net.Sockets.ProtocolType)
            NetSocket = CreateSaneNetSocket(SocketType, ProtocolType)
            NetSocket.Connect(Endpoint)
            RemoteEndpoint = CType(NetSocket.RemoteEndPoint, Net.IPEndPoint)
            LocalEndpoint = CType(NetSocket.LocalEndPoint, Net.IPEndPoint)
            IsConnected = True
            AsyncOnConnect(Me)
        End Sub
    
        Public Sub Disconnect()
            SyncLock ReadLock : SyncLock WriteLock
                IsConnected = False
                NetSocket.Shutdown(Net.Sockets.SocketShutdown.Both)
                NetSocket.Disconnect(False)
                NetSocket.Dispose()
            End SyncLock : End SyncLock
            AsyncOnDisconnect(Me)
        End Sub
    
        Public Sub Listen(Endpoint As Net.IPEndPoint, SocketType As Net.Sockets.SocketType, ProtocolType As Net.Sockets.ProtocolType)
            NetSocket = CreateSaneNetSocket(SocketType, ProtocolType)
            NetSocket.Bind(Endpoint)
            NetSocket.Listen(100)
            IsListening = True
        
            LambdaThread(Of Net.Sockets.Socket).Start(AddressOf Listener, NetSocket, "Server socket listener thread")
            AsyncOnListen(Me)
        End Sub
    
        Public Sub Deafen()
            SyncLock ReadLock : SyncLock WriteLock
                IsListening = False
                NetSocket.Shutdown(Net.Sockets.SocketShutdown.Both)
                NetSocket.Dispose()
            End SyncLock : End SyncLock
            AsyncOnDeafen(Me)
        End Sub
    
        Public Function Read(ByRef Buffer As Byte(), Offset As Int32, Length As Int32, Flags As Net.Sockets.SocketFlags, ByRef Result As Net.Sockets.SocketError) As Int32
            SyncLock ReadLock
                Dim TotalRead, InstanceRead As Int32 : Do
                    If Connected = True AndAlso NetSocket.Poll(1, Net.Sockets.SelectMode.SelectRead) = True
                        InstanceRead = NetSocket.Receive(Buffer, Offset + TotalRead, Length - TotalRead, Flags, Result)
                    End If
                    If InstanceRead > 0 Then
                        TotalRead += InstanceRead
                        InstanceRead = 0
                        ReadTimeoutStopwatch.Restart()
                    End If
                Loop Until TotalRead = Length Or IsConnected = False Or ReadTimeoutStopwatch.Elapsed >= ReadTimeout : ReadTimeoutStopwatch.Reset()
                Return TotalRead
            End SyncLock
        End Function
    
        Public Function Write(ByRef Buffer As Byte(), Offset As Int32, Length As Int32, Flags As Net.Sockets.SocketFlags, ByRef Result As Net.Sockets.SocketError) As Int32
            SyncLock WriteLock
                Dim TotalWritten, InstanceWritten As Int32
                Do
                    If Connected = True AndAlso NetSocket.Poll(1, Net.Sockets.SelectMode.SelectWrite) = True
                        InstanceWritten = NetSocket.Send(Buffer, Offset + TotalWritten, Length - TotalWritten, Flags, Result)
                    End If
                    If InstanceWritten > 0 Then
                        TotalWritten += InstanceWritten
                        InstanceWritten = 0
                        WriteTimeoutStopwatch.Restart()
                    End If
                Loop Until TotalWritten = Length Or IsConnected = False Or WriteTimeoutStopwatch.Elapsed >= WriteTimeout
                Return TotalWritten
            End SyncLock
        End Function
    
        Private Sub Listener(ListeningSocket As Net.Sockets.Socket)
            Dim ListenerGovernor As new Lightning.Governor(10)
            Do While IsListening = True
                If ListeningSocket.Poll(1, Net.Sockets.SelectMode.SelectRead) = True Then
                    Dim NewNetSocket As Net.Sockets.Socket = ListeningSocket.Accept()
                    Dim NewSocket As New Socket(NewNetSocket)
                    NewSocket.RemoteEndpoint = CType(NewNetSocket.RemoteEndPoint, Net.IPEndPoint)
                    NewSocket.LocalEndpoint = CType(NewNetSocket.LocalEndPoint, Net.IPEndPoint)
                    LambdaThread(Of Socket).Start(AddressOf Monitor, NewSocket, "Server socket monitor thread")
                End If
                ListenerGovernor.Limit()
            Loop
        End Sub
    
        Private Sub Monitor(Socket As Socket)
            AsyncOnConnect(Socket)
            Dim MonitorGovernor As New Governor(100)
            Do While Socket.Connected = True : MonitorGovernor.Limit() : Loop
            AsyncOnDisconnect(Socket)
            If Socket.Connected = True Then Socket.Disconnect()
            Socket.Dispose()
        End Sub
    
        Private Function CreateSaneNetSocket(SocketType As Net.Sockets.SocketType, ProtocolType As Net.Sockets.ProtocolType) As Net.Sockets.Socket
            Dim NewNetSocket As New Net.Sockets.Socket(SocketType, ProtocolType)
            NewNetSocket.NoDelay = True
            NewNetSocket.LingerState = New Net.Sockets.LingerOption(True, 5)
            NewNetSocket.ReceiveBufferSize = 524288
            NewNetSocket.SendBufferSize = 524288
            NewNetSocket.Blocking = True
            NewNetSocket.DualMode = True
        
            ReadTimeout = TimeSpan.FromMilliseconds(100)
            WriteTimeout = TimeSpan.FromMilliseconds(100)
            Return NewNetSocket
        End Function
    
        Public Sub Dispose() Implements IDisposable.Dispose
            If BaseDisposed = False Then
                If IsConnected = True Then
                    Disconnect()
                End If
            
                If IsListening = True Then
                    Deafen()
                End If
            
                BaseDisposed = True
            End If
        End Sub
    End Class
End NameSpace