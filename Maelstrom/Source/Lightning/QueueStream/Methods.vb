Namespace Lightning
        Public Partial Class QueueStream(Of T) : Implements IDisposable
        Public Sub New(ByVal Optional Capacity As Int32 = 65536)
            BaseBuffer = New T(Capacity - 1) {}
            Array.Resize(BaseBuffer, Capacity)
            IsDisposed = False
        End Sub

        Public Sub Read(ByRef Buffer As T(), ByVal Offset As Int32, ByVal Seek As Int32, ByVal Count As Int32)
            BlockCopy(BaseBuffer, Seek, Buffer, Offset, Count)
        End Sub
        
        Public Sub ReadOne(ByRef Item As T, ByVal Seek As Int32)
            Item = BaseBuffer(Seek)
        End Sub

        Public Sub Write(ByRef Buffer As T(), ByVal Offset As Int32, ByVal Count As Int32)
            If BaseBuffer.Length < WritePointer + Count Then Array.Resize(BaseBuffer, WritePointer + Count)
            BlockCopy(Buffer, Offset, BaseBuffer, WritePointer, Count)
            WritePointer = WritePointer + Count
        End Sub
        
        Public Sub WriteOne(ByRef Item As T)
            If BaseBuffer.Length < WritePointer + 1 Then Array.Resize(BaseBuffer, WritePointer + 1)
            BaseBuffer(WritePointer) = Item
            WritePointer = WritePointer + 1
        End Sub
        
        Public Sub Shift(ByVal Amount As Int32)
            If WritePointer - Amount > 0 Then BlockCopy(BaseBuffer, Amount, BaseBuffer, 0, WritePointer - Amount)
            WritePointer = WritePointer - Amount
        End Sub
        Public Sub ShiftOne()
            If WritePointer - 1 > 0 Then BlockCopy(BaseBuffer, 1, BaseBuffer, 0, WritePointer - 1)
            WritePointer = WritePointer - 1
        End Sub
        
        Public Sub Seek(ByVal Amount As Int32)
            WritePointer = Amount
        End Sub
        
        Public Sub Clear()
            Shift(Length)
        End Sub
        
        Public Function GetBuffer(ByRef Reference As T()) As Int32
            Reference = BaseBuffer
            Return Reference.Length
        End Function

        Private Sub BlockCopy(ByRef InputBuffer As T(), ByVal InputBufferOffset As Int32, ByRef OutputBuffer As T(), ByVal OutputBufferOffset As Int32, ByVal Count As Int32)
            For Index As Int32 = InputBufferOffset To InputBufferOffset + Count - 1
                OutputBuffer((Index - InputBufferOffset) + OutputBufferOffset) = InputBuffer(Index)
            Next
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If IsDisposed = False Then
                BaseBuffer = Nothing
                IsDisposed = True
            End If
        End Sub
    End Class
End Namespace
