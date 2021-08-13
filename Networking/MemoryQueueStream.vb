Friend Class MemoryQueueStream : Inherits IO.MemoryStream
    Private InternalLength As Long = 0
    Private MSBuffer As Byte()
    Sub New(Optional DefaultSize As Integer = 0)
        MyBase.New(DefaultSize)
    End Sub
    Public Shadows Sub Write(ByRef buffer() As Byte, offset As Integer, count As Integer)
        SyncLock Me
            Position = InternalLength
            MyBase.Write(buffer, offset, count)
            InternalLength += count
        End SyncLock
    End Sub
    Public Shadows Sub Read(ByRef buffer() As Byte, offset As Integer, count As Integer, Optional peek As Boolean = False)
        SyncLock Me
            Position = 0
            MyBase.Read(buffer, offset, count)
            If peek = False Then
                MSBuffer = GetBuffer
                System.Buffer.BlockCopy(MSBuffer, count, MSBuffer, 0, CInt(InternalLength) - count)
                SetLength(InternalLength - count)
                InternalLength -= count
            End If
        End SyncLock
    End Sub
End Class