Imports System.Net.Sockets

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
            SyncLock ReadLock
                SyncLock WriteLock
                    Return IsConnected()
                End SyncLock
            End SyncLock
        End get
    End Property
    
    Public ReadOnly Property SubSocketHasData(Byval SubSocket as UInt32) as Boolean
        get
                If SubSocketBuffers.Contains(SubSocket) = False Then
                    Return False
                End If
                If SubSocketBuffers(SubSocket).Length >= 32 Then
                    Return True
                End If
            SyncLock ReadLock
                If NetSocket.Available >= 32 Then 
                    Dim HasDataHeader as New DataHeader 
                    Dim HasDataTransformBuffer(65535) as Byte
                        ReliableRead(HasDataHeader.HeaderRaw, 0, 32)
            
                        If HasDataTransformBuffer.Length < HasDataHeader.Length Then ReDim HasDataTransformBuffer(HasDataHeader.Length - 1)
                    
                    If (HasDataHeader.SubSocketConfig And SubSocketConfigFlagLarge) <> 0 Then
                        For Offset = 0 To HasDataHeader.Length - 1 Step LargeBlockSize
                            ReliableRead(HasDataTransformBuffer, Offset, LargeBlockSize)
                        Next
                        'ReliableRead(HasDataTransformBuffer, 0, HasDataHeader.Length)
                    Else
                        ReliableRead(HasDataTransformBuffer, 0, HasDataHeader.Length)
                    End If
                    
                    

                    
                        If (HasDataHeader.SubSocketConfig And SubSocketConfigFlag.Encrypted) <> 0 Then
                            RemoteTransform.TransformBlock(HasDataTransformBuffer,
                                                           0,
                                                           HasDataHeader.EncryptedLength,
                                                           HasDataTransformBuffer,
                                                           0)
                        End If
                
                        If (HasDataHeader.SubSocketConfig And SubSocketConfigFlag.Compressed) <> 0 Then
                            RemoteDecompressor.Transform(HasDataTransformBuffer,
                                                         HasDataTransformBuffer,
                                                         HasDataHeader.CompressedLength)
                        End If
                        SyncLock BufferLock
                            SubSocketBuffers(HasDataHeader.SubSocket).Write(HasDataHeader.HeaderRaw, 32)
                            SubSocketBuffers(HasDataHeader.SubSocket).Write(HasDataTransformBuffer, HasDataHeader.RawLength)
                        End SyncLock
                        Return (HasDataHeader.SubSocket = SubSocket)
                End If
            End SyncLock
            Return False
        End get
    End Property
End Class