Public Partial Class Socket : Implements IDisposable

    ''' <summary>
    ''' SubSocketConfigFlag. Gets or sets the configuration for a particular subsocket.
    ''' </summary>
    ''' <param name="SubSocket">Subsocket to get or set configuration on.</param>
    ''' <remarks></remarks>
    Public Property ConfigureSubSocket(SubSocket As UInt32) as SubsocketConfigFlag
        get
            SyncLock BufferLock
                Return SubSocketConfigs(SubSocket) 
            End SyncLock
        End get
        Set
            SyncLock BufferLock
                SubSocketConfigs(SubSocket) = Value
            End SyncLock
        End Set
    End Property
    
    ''' <summary>
    ''' Boolean. Returns true if this maelstrom socket is still connected, otherwise false.
    ''' </summary>
    ''' <param name="SubSocket">Subsocket to remove.</param>
    ''' <remarks></remarks>
    Public Readonly Property Connected as Boolean
        get
            SyncLock WriteLock
                SyncLock ReadLock
                    Return IsConnected()
                End SyncLock
            End SyncLock
        End get
    End Property
    
    ''' <summary>
    ''' Checks if a subsocket buffer has data available to read. This method will read data into the appropriate subsocket buffer if there is any to read on the underlying net.sockets.socket.
    ''' </summary>
    ''' <param name="SubSocket">Subsocket buffer to check.</param>
    ''' <remarks></remarks>
    Public ReadOnly Property SubSocketHasData(Byval SubSocket as UInt32) as Boolean
        get
            SyncLock BufferLock
                If SubSocketBuffers.Contains(SubSocket) = False Then 
                    Return False
                End If
                If SubSocketBuffers(SubSocket).Length >= 32 Then
                    Return True
                End If
            End SyncLock


            If NetSocket.Available >= 32 Then
                SyncLock ReadLock
                    If NetSocket.Available >= 32 Then

                        If NetSocket.Available < 32 Then Return False
                        ReliableRead(RemoteHeader.HeaderRaw, 0, 32)
                        If RemoteTransformBuffer.Length < RemoteHeader.Length Then _
                            ReDim RemoteTransformBuffer(RemoteHeader.Length - 1)
                        ReliableRead(RemoteTransformBuffer, 0, RemoteHeader.Length)
            
                        If (RemoteHeader.SubSocketConfig And SubSocketConfigFlag.Encrypted) <> 0 Then
                            RemoteTransform.TransformBlock(RemoteTransformBuffer,
                                                           0,
                                                           RemoteHeader.EncryptedLength,
                                                           RemoteTransformBuffer,
                                                           0)
                        End If

                        If (RemoteHeader.SubSocketConfig And SubSocketConfigFlag.Compressed) <> 0 Then
                            RemoteDecompressor.Transform(RemoteTransformBuffer,
                                                         RemoteTransformBuffer,
                                                         RemoteHeader.CompressedLength)
                        End If

                        SyncLock BufferLock
                            SubSocketBuffers(RemoteHeader.SubSocket).Write(RemoteHeader.HeaderRaw, 32)
                            SubSocketBuffers(RemoteHeader.SubSocket).Write(RemoteTransformBuffer, RemoteHeader.RawLength)
                        End SyncLock

                        Return (RemoteHeader.SubSocket = SubSocket)
                    End If
                End SyncLock
            End If


            Return False
        End get
    End Property
End Class