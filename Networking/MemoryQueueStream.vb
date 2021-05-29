Public Class MemoryQueueStream : Inherits IO.MemoryStream
    Dim InternalLength As Long = 0
    Dim MaxLength As Long = 0
    Sub New(Optional DefaultSize As Integer = 0)
        MyBase.New(DefaultSize)
        MaxLength = DefaultSize
    End Sub
    Public Shadows Sub Write(ByRef buffer() As Byte, offset As Integer, count As Integer)
        SyncLock Me
            MyBase.Position = InternalLength
            MyBase.Write(buffer, offset, count)
            InternalLength += count
        End SyncLock
    End Sub
    Public Shadows Sub Read(ByRef buffer() As Byte, offset As Integer, count As Integer, Optional peek As Boolean = False)
        SyncLock Me
            MyBase.Position = 0
            MyBase.Read(buffer, offset, count)
            If peek = False Then
                Dim MSBuffer As Byte() = MyBase.GetBuffer
                System.Buffer.BlockCopy(MSBuffer, count, MSBuffer, 0, CInt(InternalLength) - count)
                MyBase.SetLength(InternalLength - count)
                InternalLength -= count
            End If
        End SyncLock
    End Sub
End Class
