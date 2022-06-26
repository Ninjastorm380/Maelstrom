Public Module BinaryMethods
     Friend Sub PackTimestamp(Byref Input as DateTime, ByRef Output as Byte(), Optional Byval OutputOffset as Int32 = 0)
        Dim YearBytes As Byte() = BitConverter.GetBytes(Input.Year)
        Dim MonthBytes As Byte() = BitConverter.GetBytes(Input.Month)
        Dim DayBytes As Byte() = BitConverter.GetBytes(Input.Day)
        Dim HourBytes As Byte() = BitConverter.GetBytes(Input.Hour)
        Dim MinuteBytes As Byte() = BitConverter.GetBytes(Input.Minute)
        Dim SecondBytes As Byte() = BitConverter.GetBytes(Input.Second)
        Dim MillisecondBytes As Byte() = BitConverter.GetBytes(Input.Millisecond)
        Buffer.BlockCopy(YearBytes, 0, Output, OutputOffset, 4)
        Buffer.BlockCopy(MonthBytes, 0, Output, OutputOffset + 4, 4)
        Buffer.BlockCopy(DayBytes, 0, Output, OutputOffset + 8, 4)
        Buffer.BlockCopy(HourBytes, 0, Output, OutputOffset + 12, 4)
        Buffer.BlockCopy(MinuteBytes, 0, Output, OutputOffset + 16, 4)
        Buffer.BlockCopy(SecondBytes, 0, Output, OutputOffset + 20, 4)
        Buffer.BlockCopy(MillisecondBytes, 0, Output, OutputOffset + 24, 4)
    End Sub

    Friend Sub UnpackTimestamp(Byref Input as Byte(), ByRef Output as DateTime, Optional Byval InputOffset as Int32 = 0)
        Dim Year As Int32 = BitConverter.ToInt32(Input, InputOffset + 0)
        Dim Month As Int32 = BitConverter.ToInt32(Input, InputOffset + 4)
        Dim Day As Int32 = BitConverter.ToInt32(Input, InputOffset + 8)
        Dim Hour As Int32 = BitConverter.ToInt32(Input, InputOffset + 12)
        Dim Minute As Int32 = BitConverter.ToInt32(Input, InputOffset + 16)
        Dim Second As Int32 = BitConverter.ToInt32(Input, InputOffset + 20)
        Dim Millisecond As Int32 = BitConverter.ToInt32(Input, InputOffset + 24)
        Output = New DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond)
    End Sub

    Friend Sub PackInt32(Byref Input as Int32, Byref Output as Byte(), Optional Byval OutputOffset as Int32 = 0)
        Buffer.BlockCopy(BitConverter.GetBytes(Input), 0, Output, OutputOffset, 4)
    End Sub

    Friend Sub UnpackInt32(Byref Input as Byte(), Byref Output as Int32, Optional Byval InputOffset as Int32 = 0)
        Output = BitConverter.ToInt32(Input, InputOffset)
    End Sub
     
     Friend Sub PackUInt32(Byref Input as UInt32, Byref Output as Byte(), Optional Byval OutputOffset as Int32 = 0)
         Buffer.BlockCopy(BitConverter.GetBytes(Input), 0, Output, OutputOffset, 4)
     End Sub

     Friend Sub UnpackUInt32(Byref Input as Byte(), Byref Output as UInt32, Optional Byval InputOffset as Int32 = 0)
         Output = BitConverter.ToUInt32(Input, InputOffset)
     End Sub
     
     Friend Sub PackUInt16(Byref Input as UInt16, Byref Output as Byte(), Optional Byval OutputOffset as Int32 = 0)
         Buffer.BlockCopy(BitConverter.GetBytes(Input), 0, Output, OutputOffset, 2)
     End Sub

     Friend Sub UnpackUInt16(Byref Input as Byte(), Byref Output as UInt16, Optional Byval InputOffset as Int32 = 0)
         Output = BitConverter.ToUInt16(Input, InputOffset)
     End Sub
     
     Public Function BinaryCompare(A as Byte(), B As Byte(), Offset As UInt32, Count As UInt32) As Boolean
         If A.Length <> B.Length Then Return False
         For Index = Offset to Count - 1
             if A(Index) <> B(Index) Then
                 Return False
             End If
         Next
         Return True
     End Function
     Friend Function PackJagged(Byref Input as Byte()(), Byref Output as Byte(), Optional Byval OutputOffset as Int32 = 0) As UInt32
         Dim ByteCounter As UInt32 = 0
         Dim ByteLength As UInt32 = 0
         ByteLength = Input.Length * 4
         For Index = 0 To Input.Length - 1 : ByteLength += Input(Index).Length : Next
         If Output Is Nothing OrElse Output.Length < ByteLength + OutputOffset Then ReDim Output(ByteLength + OutputOffset - 1)
         
         For Index = 0 To Input.Length - 1
             PackUInt32(Input(Index).Length,Output,ByteCounter + OutputOffset) : ByteCounter += 4
             Buffer.BlockCopy(Input(Index), 0, Output, ByteCounter + OutputOffset, Input(Index).Length) : ByteCounter += Input(Index).Length
         Next
         Return ByteLength
     End Function
     Friend Sub UnpackJagged(Byref Input as Byte(), Byref Output as Byte()(), Optional Byval InputOffset as Int32 = 0)
         Dim ByteCounter As UInt32 = 0
         Dim D0Length As UInt32 = Output.Length
         Dim DNLength As UInt32 = 0
         
         For Index = 0 To D0Length - 1
             UnpackUInt32(Input, DNLength, ByteCounter + InputOffset) : ByteCounter += 4
             ReDim Output(Index)(DNLength - 1)
             Buffer.BlockCopy(Input,ByteCounter + InputOffset,Output(Index),0,DNLength) : ByteCounter += DNLength
         Next
     End Sub
End Module