Imports System.Net.Sockets
Imports System.Net

Namespace Lightning
    Friend Partial Class Socket
        Implements IDisposable

        Public Sub New()
            IsBaseDisposed = False
            NetSocket.DualMode = True
            NetSocket.LingerState = New LingerOption(False, 0)
            NetSocket.Blocking = True
        End Sub

        Public Delegate Sub SocketHandler(ByVal NewSocket As Socket)

        Public Event SocketConnecting As SocketHandler
        Public Event SocketConnected As SocketHandler
        Public Event SocketDisconnecting As SocketHandler
        Public Event SocketDisconnected As SocketHandler
        Public Event SocketListenerBinding As SocketHandler
        Public Event SocketListenerBound As SocketHandler
        Public Event SocketListenerUnbinding As SocketHandler
        Public Event SocketListenerUnbound As SocketHandler
        

        Private Sub HookupSocket(ByVal BaseSocket As Sockets.Socket)
            NetSocket = BaseSocket
            NetSocket.LingerState = New LingerOption(False, 0)
            LocalEndPoint = CType(NetSocket.LocalEndPoint, IPEndPoint)
            RemoteEndPoint = CType(NetSocket.RemoteEndPoint, IPEndPoint)
            IsConnected = True
            BaseClientBound = False
            NetSocket.Blocking = True
        End Sub

        Private Sub AsyncSocketConnectedMethod(Socket As Object)
            RaiseEvent SocketConnected(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketDisconnectedMethod(Socket As Object)
            RaiseEvent SocketDisconnected(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketConnectingMethod(Socket As Object)
            RaiseEvent SocketConnecting(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketDisconnectingMethod(Socket As Object)
            RaiseEvent SocketDisconnecting(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketListenerBindingMethod(Socket As Object)
            RaiseEvent SocketListenerBinding(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketListenerBoundMethod(Socket As Object)
            RaiseEvent SocketListenerBound(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketListenerUnbindingMethod(Socket As Object)
            RaiseEvent SocketListenerUnbinding(CType(Socket, Socket))
        End Sub
        Private Sub AsyncSocketListenerUnboundMethod(Socket As Object)
            RaiseEvent SocketListenerUnbound(CType(Socket, Socket))
        End Sub

        Public Sub Connect(ByVal Endpoint As IPEndPoint)
            If IsBaseDisposed = True Then NetSocket = New Sockets.Socket(SocketType.Stream, ProtocolType.Tcp)
            AsyncSocketConnectingMethod(Me)
            BaseClientBound = True
            NetSocket.Connect(Endpoint)
            NetSocket.Blocking = True
            IsConnected = True
            LocalEndPoint = CType(NetSocket.LocalEndPoint, IPEndPoint)
            RemoteEndPoint = CType(NetSocket.RemoteEndPoint, IPEndPoint)
            AsyncSocketConnectedMethod(Me)
        End Sub

        Public Sub Disconnect()
            IsConnected = False
            AsyncSocketDisconnectingMethod(Me)
            NetSocket.Shutdown(SocketShutdown.Both)
            NetSocket.Disconnect(True)
            AsyncSocketDisconnectedMethod(Me)
        End Sub

        Public Sub Listen(ByVal Endpoint As IPEndPoint)
            AsyncSocketListenerBindingMethod(Me)
            NetSocket.Bind(Endpoint)
            NetSocket.Listen(100)
            IsListening = True
            BaseClientBound = False
            Dim AsyncListener = New Threading.Thread(AddressOf AsyncListenerMethod)
            AsyncListener.Start()
            AsyncSocketListenerBoundMethod(Me)
        End Sub

        Public Sub Deafen()
            AsyncSocketListenerUnbindingMethod(Me)
            IsListening = False
            NetSocket.Disconnect(True)
            AsyncSocketListenerUnboundMethod(Me)
        End Sub
        
        Public Function Read(ByRef Buffer As Byte(), ByVal Offset As Int32, ByVal Length As Int32, ByVal Flags As SocketFlags) As Int32
            If NetSocket Is Nothing Then Return 0
            If IsConnected = False Then Return 0
            
            Dim BytesRead As Int32
            Dim TotalBytesRead As Int32
            Dim TimeoutMS As Long = 100
            ReadTimeoutStopwatch.Restart()
            
            Do : SyncLock ReadLock : Try
                If NetSocket.Available > 0 Then 
                    BytesRead = NetSocket.Receive(Buffer, TotalBytesRead + Offset, Length - TotalBytesRead, Flags)
                    ReadTimeoutStopwatch.Restart()
                Else
                    BytesRead = 0
                End If
                
            Catch NetException As Exception
                BytesRead = 0
            End Try
                TotalBytesRead += BytesRead : End SyncLock
            Loop Until ReadTimeoutStopwatch.ElapsedMilliseconds >= TimeoutMS Or TotalBytesRead >= Length Or IsConnected = False
            
            ReadTimeoutStopwatch.Reset()
            Return TotalBytesRead
        End Function

        Public Function Write(ByRef Buffer As Byte(), ByVal Offset As Int32, ByVal Length As Int32, ByVal Flags As SocketFlags) As Int32
            If NetSocket Is Nothing Then Return 0
            If IsConnected = False Then Return 0
            
            Dim TotalBytesWritten As Int32
            Dim BytesWritten As Int32
            Dim TimeoutMS As Long = 100
            WriteTimeoutStopwatch.Restart()
            
            Do : SyncLock WriteLock : Try 
                If NetSocket.Poll(0, SelectMode.SelectWrite) = True Then
                    BytesWritten = NetSocket.Send(Buffer, Offset + TotalBytesWritten, Length - TotalBytesWritten, Flags)
                    WriteTimeoutStopwatch.Restart() : Exit Try
                End If
                BytesWritten = 0
            Catch NetException As Exception 
                BytesWritten = 0 
            End Try
                TotalBytesWritten += BytesWritten : End SyncLock
            Loop Until WriteTimeoutStopwatch.ElapsedMilliseconds >= TimeoutMS Or TotalBytesWritten >= Length Or IsConnected = False

            WriteTimeoutStopwatch.Reset()
            Return TotalBytesWritten
        End Function

        Private Sub AsyncListenerMethod()
            Do : If NetSocket.Poll(0, SelectMode.SelectRead) And IsListening = True Then
                Dim NewSocketBase = NetSocket.Accept()
                Dim NewSocket = New Socket()
                AsyncSocketConnectingMethod(NewSocket)
                NewSocket.HookupSocket(NewSocketBase)
                AsyncSocketConnectedMethod(NewSocket) : End If
                Threading.Thread.Yield()
            Loop While IsListening
        End Sub
        
        Public Sub Dispose() Implements IDisposable.Dispose
            If IsBaseDisposed = False Then NetSocket.Dispose()
            ReadTimeoutStopwatch.Reset()
            ReadTimeoutStopwatch = Nothing
            WriteTimeoutStopwatch.Reset()
            WriteTimeoutStopwatch = Nothing
            IsBaseDisposed = True
        End Sub
    End Class
End Namespace


