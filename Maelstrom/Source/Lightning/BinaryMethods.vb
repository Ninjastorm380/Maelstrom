Namespace Lightning
    Friend Module BinaryMethods
        Public Sub PackUInt32(ByRef Buffer as Byte(), ByVal Offset as Int32, ByVal Data as UInt32)
            System.Buffer.BlockCopy(BitConverter.GetBytes(Data), 0, Buffer, Offset, 4)
        End Sub

        Public Sub UnpackUInt32(ByVal Buffer as Byte(), ByVal Offset as Int32, ByRef Data as UInt32)
            Data = BitConverter.ToUInt32(Buffer, Offset)
        End Sub
        
        Public Sub PackInt32(ByRef Buffer as Byte(), ByVal Offset as Int32, ByVal Data as Int32)
            System.Buffer.BlockCopy(BitConverter.GetBytes(Data), 0, Buffer, Offset, 4)
        End Sub

        Public Sub UnpackInt32(ByVal Buffer as Byte(), ByVal Offset as Int32, ByRef Data as Int32)
            Data = BitConverter.ToInt32(Buffer, Offset)
        End Sub

        Public Function BinaryCompare(A as Byte(), B As Byte(), Offset As Int32, Length As Int32) As Boolean
            If A.Length - (Offset + Length) < 0 Then Return False
            If B.Length - (Offset + Length) < 0 Then Return False
            
            For Index = Offset to Length - 1
                If A(Index) <> B(Index) Then Return False
            Next
            Return True
        End Function
    End Module
End Namespace
