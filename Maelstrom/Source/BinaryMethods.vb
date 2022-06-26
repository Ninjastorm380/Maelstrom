Friend Module BinaryMethods
    
    ''' <summary>
    ''' Serializes a basic primitive to raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to write bytes to.</param>
    ''' <param name="Offset">Where to start writing in the backing buffer.</param>
    ''' <param name="Data">Variable holding the primitive to serialize.</param>
    ''' <remarks></remarks>
    Friend Sub PackTimestamp(ByRef Buffer as Byte(), Byval Offset as Int32, Byref Data as DateTime)
        Dim YearBytes As Byte() = BitConverter.GetBytes(Data.Year)
        Dim MonthBytes As Byte() = BitConverter.GetBytes(Data.Month)
        Dim DayBytes As Byte() = BitConverter.GetBytes(Data.Day)
        Dim HourBytes As Byte() = BitConverter.GetBytes(Data.Hour)
        Dim MinuteBytes As Byte() = BitConverter.GetBytes(Data.Minute)
        Dim SecondBytes As Byte() = BitConverter.GetBytes(Data.Second)
        Dim MillisecondBytes As Byte() = BitConverter.GetBytes(Data.Millisecond)
        System.Buffer.BlockCopy(YearBytes, 0, Buffer, Offset, 4)
        System.Buffer.BlockCopy(MonthBytes, 0, Buffer, Offset + 4, 4)
        System.Buffer.BlockCopy(DayBytes, 0, Buffer, Offset + 8, 4)
        System.Buffer.BlockCopy(HourBytes, 0, Buffer, Offset + 12, 4)
        System.Buffer.BlockCopy(MinuteBytes, 0, Buffer, Offset + 16, 4)
        System.Buffer.BlockCopy(SecondBytes, 0, Buffer, Offset + 20, 4)
        System.Buffer.BlockCopy(MillisecondBytes, 0, Buffer, Offset + 24, 4)
    End Sub

    ''' <summary>
    ''' Deserializes a basic primitive from raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to read bytes from.</param>
    ''' <param name="Offset">Where to start reading in the backing buffer.</param>
    ''' <param name="Data">Variable to store the resulting primitive in.</param>
    ''' <remarks></remarks>
    Friend Sub UnpackTimestamp(Byref Buffer as Byte(), Byval Offset as Int32, ByRef Data as DateTime)
        Dim Year As Int32 = BitConverter.ToInt32(Buffer, Offset + 0)
        Dim Month As Int32 = BitConverter.ToInt32(Buffer, Offset + 4)
        Dim Day As Int32 = BitConverter.ToInt32(Buffer, Offset + 8) 
        Dim Hour As Int32 = BitConverter.ToInt32(Buffer, Offset + 12)
        Dim Minute As Int32 = BitConverter.ToInt32(Buffer, Offset + 16)
        Dim Second As Int32 = BitConverter.ToInt32(Buffer, Offset + 20)
        Dim Millisecond As Int32 = BitConverter.ToInt32(Buffer, Offset + 24)
        Data = New DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond)
    End Sub
    
    
    
    
    
    ''' <summary>
    ''' Serializes a basic primitive to raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to write bytes to.</param>
    ''' <param name="Offset">Where to start writing in the backing buffer.</param>
    ''' <param name="Data">Variable holding the primitive to serialize.</param>
    ''' <remarks></remarks>
    Friend Sub PackInt32(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as Int32)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Data), 0, Buffer, Offset, 4)
    End Sub

    ''' <summary>
    ''' Deserializes a basic primitive from raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to read bytes from.</param>
    ''' <param name="Offset">Where to start reading in the backing buffer.</param>
    ''' <param name="Data">Variable to store the resulting primitive in.</param>
    ''' <remarks></remarks>
    Friend Sub UnpackInt32(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as Int32)
        Data = BitConverter.ToInt32(Buffer, Offset)
    End Sub
     
    ''' <summary>
    ''' Serializes a basic primitive to raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to write bytes to.</param>
    ''' <param name="Offset">Where to start writing in the backing buffer.</param>
    ''' <param name="Data">Variable holding the primitive to serialize.</param>
    ''' <remarks></remarks>
    Friend Sub PackUInt32(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as UInt32)
         System.Buffer.BlockCopy(BitConverter.GetBytes(Data), 0, Buffer, Offset, 4)
    End Sub

    ''' <summary>
    ''' Deserializes a basic primitive from raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to read bytes from.</param>
    ''' <param name="Offset">Where to start reading in the backing buffer.</param>
    ''' <param name="Data">Variable to store the resulting primitive in.</param>
    ''' <remarks></remarks>
    Friend Sub UnpackUInt32(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as UInt32)
        Data = BitConverter.ToUInt32(Buffer, Offset)
    End Sub
    
    ''' <summary>
    ''' Serializes a basic primitive to raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to write bytes to.</param>
    ''' <param name="Offset">Where to start writing in the backing buffer.</param>
    ''' <param name="Data">Variable holding the primitive to serialize.</param>
    ''' <remarks></remarks>
    Friend Sub PackInt16(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as Int16)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Data), 0, Buffer, Offset, 2)
    End Sub

    ''' <summary>
    ''' Deserializes a basic primitive from raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to read bytes from.</param>
    ''' <param name="Offset">Where to start reading in the backing buffer.</param>
    ''' <param name="Data">Variable to store the resulting primitive in.</param>
    ''' <remarks></remarks>
    Friend Sub UnpackInt16(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as Int16)
        Data = BitConverter.ToInt16(Buffer, Offset)
    End Sub
     
    ''' <summary>
    ''' Serializes a basic primitive to raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to write bytes to.</param>
    ''' <param name="Offset">Where to start writing in the backing buffer.</param>
    ''' <param name="Data">Variable holding the primitive to serialize.</param>
    ''' <remarks></remarks>
    Friend Sub PackUInt16(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as UInt16)
        System.Buffer.BlockCopy(BitConverter.GetBytes(Data), 0, Buffer, Offset, 2)
    End Sub

    ''' <summary>
    ''' Deserializes a basic primitive from raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to read bytes from.</param>
    ''' <param name="Offset">Where to start reading in the backing buffer.</param>
    ''' <param name="Data">Variable to store the resulting primitive in.</param>
    ''' <remarks></remarks>
    Friend Sub UnpackUInt16(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as UInt16)
        Data = BitConverter.ToUInt16(Buffer, Offset)
    End Sub
     
    ''' <summary>
    ''' Serializes a DateTime instance to raw bytes.
    ''' </summary>
    ''' <param name="A">Backing buffer to store bytes in.</param>
    ''' <param name="B">Where to start storing bytes in the backing buffer.</param>
    ''' <param name="Offset">The data to serialize.</param>
    ''' <remarks></remarks>
    Friend Function BinaryCompare(A as Byte(), B As Byte(), Offset As UInt32, Length As UInt32) As Boolean
         If A.Length <> B.Length Then Return False
         If Length - Offset < 0 Then Return False
         For Index = Offset to Length - 1
             if A(Index) <> B(Index) Then
                 Return False
             End If
         Next
         Return True
     End Function
    
    ''' <summary>
    ''' Serializes a basic primitive to raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to write bytes to.</param>
    ''' <param name="Offset">Where to start writing in the backing buffer.</param>
    ''' <param name="Data">Variable holding the primitive to serialize.</param>
    ''' <remarks></remarks>
    Friend Sub PackJagged(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as Byte()())
        Dim ByteCounter As UInt32 = 0
        PackUInt32(Buffer, ByteCounter + Offset, Data.Length) : ByteCounter += 4
        For Index = 0 To Data.Length - 1
            PackUInt32(Buffer, ByteCounter + Offset, Data(Index).Length) : ByteCounter += 4
            System.Buffer.BlockCopy(Data(Index), 0, Buffer, ByteCounter + Offset, Data(Index).Length) : ByteCounter += Data(Index).Length 
        Next
     End Sub
    
    ''' <summary>
    ''' Deserializes a basic primitive from raw bytes.
    ''' </summary>
    ''' <param name="Buffer">Backing buffer to read bytes from.</param>
    ''' <param name="Offset">Where to start reading in the backing buffer.</param>
    ''' <param name="Data">Variable to store the resulting primitive in.</param>
    ''' <remarks></remarks>
    Friend Sub UnpackJagged(Byref Buffer as Byte(), Byval Offset as Int32, Byref Data as Byte()())
        Dim ByteCounter As UInt32 = 0
        Dim Count As UInt32 = 0
        Dim Length As UInt32 = 0
        UnpackUInt32(Buffer, ByteCounter + Offset, Count) : ByteCounter += 4
        Redim Data(Count - 1)
        For Index = 0 To Count - 1
            UnpackUInt32(Buffer, ByteCounter + Offset, Length) : ByteCounter += 4
            ReDim Data(Index)(Length - 1)
            System.Buffer.BlockCopy(Buffer,ByteCounter + Offset,Data(Index),0,Length) : ByteCounter += Length 
        Next
     End Sub
End Module