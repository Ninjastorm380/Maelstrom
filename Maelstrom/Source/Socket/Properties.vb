Public Partial Class Socket : Implements IDisposable
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
    
    Public Readonly Property Connected as Boolean
        get
            SyncLock WriteLock
                SyncLock ReadLock
                    Return IsConnected()
                End SyncLock
            End SyncLock
        End get
    End Property
    
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
                        ReliableRead(AsyncHeader.HeaderRaw, 0, 32)
                        If AsyncTransformBuffer.Length < AsyncHeader.Length Then _
                            ReDim AsyncTransformBuffer(AsyncHeader.Length - 1)
                        ReliableRead(AsyncTransformBuffer, 0, AsyncHeader.Length)
            
                        If (AsyncHeader.SubSocketConfig And SubSocketConfigFlag.Encrypted) <> 0 Then
                            RemoteTransform.TransformBlock(AsyncTransformBuffer,
                                                           0,
                                                           AsyncHeader.EncryptedLength,
                                                           AsyncTransformBuffer,
                                                           0)
                        End If

                        If (AsyncHeader.SubSocketConfig And SubSocketConfigFlag.Compressed) <> 0 Then
                            RemoteDecompressor.Transform(AsyncTransformBuffer,
                                                         AsyncTransformBuffer,
                                                         AsyncHeader.CompressedLength)
                        End If

                        SyncLock BufferLock
                            SubSocketBuffers(AsyncHeader.SubSocket).Write(AsyncHeader.HeaderRaw, 32)
                            SubSocketBuffers(AsyncHeader.SubSocket).Write(AsyncTransformBuffer, AsyncHeader.RawLength)
                        End SyncLock

                        Return (AsyncHeader.SubSocket = SubSocket)
                    End If
                End SyncLock
            End If


            Return False
        End get
    End Property
End Class