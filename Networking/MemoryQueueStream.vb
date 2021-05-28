Public Class MemoryQueueStream : Inherits IO.MemoryStream
    Public Shadows Sub Write(ByRef buffer() As Byte, offset As Integer, count As Integer)
        SyncLock Me
            MyBase.Position = MyBase.Length
            MyBase.Write(buffer, offset, count)
        End SyncLock
    End Sub
    Public Shadows Sub Read(ByRef buffer() As Byte, offset As Integer, count As Integer, Optional peek As Boolean = False)
        SyncLock Me
            MyBase.Position = 0
            MyBase.Read(buffer, offset, count)
            If peek = False Then
                Dim MSBuffer As Byte() = MyBase.GetBuffer
                System.Buffer.BlockCopy(MSBuffer, count, MSBuffer, 0, CInt(MyBase.Length) - count)
                MyBase.SetLength(MyBase.Length - count)
            End If
        End SyncLock
    End Sub
End Class
