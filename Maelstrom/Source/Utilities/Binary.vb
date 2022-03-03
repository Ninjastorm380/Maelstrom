Friend Module BinaryPacking
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
End Module