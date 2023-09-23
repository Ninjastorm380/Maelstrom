Namespace Lightning
    Friend Partial Class Random
        Public Sub New(Seeds As Int32())
            Me.Seeds = Nothing
            Array.Resize(Me.Seeds, Seeds.Length)
            Array.Copy(Seeds, Me.Seeds, Seeds.Length)
            
            Dim MaxPart = CInt(Math.Floor(Int32.MaxValue/Seeds.Length))
            Dim NegMaxPart = -1 * MaxPart
            Dim EndSeed = 0
            For x = 0 To Seeds.Length - 1
                Dim RNGStepper As New System.Random(Seeds(x))
                EndSeed += RNGStepper.Next(NegMaxPart, MaxPart)
            Next
            RngInternal = New System.Random(EndSeed)
            FinalSeed = EndSeed
        End Sub

        Public Sub New(Seeds As Byte())
            Me.Seeds = Nothing
            Array.Resize(Me.Seeds, Seeds.Length)
            Array.Copy(Seeds, Me.Seeds, Seeds.Length)
            Dim MaxPart = CInt(Math.Floor(Int32.MaxValue/Seeds.Length))
            Dim NegMaxPart = -1 * MaxPart
            Dim EndSeed = 0
            For x = 0 To Seeds.Length - 1
                Dim RNGStepper As New System.Random(Seeds(x))
                EndSeed += RNGStepper.Next(NegMaxPart, MaxPart)
            Next
            RngInternal = New System.Random(EndSeed)
            FinalSeed = EndSeed
        End Sub

        Public Sub ByteNext(ByRef Buffer As Byte())
            RngInternal.NextBytes(Buffer)
        End Sub

        Public Function ByteNext(Length As Int32) As Byte()
            Dim Buffer(Length - 1) As Byte
            ByteNext(Buffer)
            Return Buffer
        End Function
    
        Public Sub Int32Next(ByRef Buffer As Int32())
            Dim ByteBuffer((Buffer.Length * 4) - 1) As Byte : RngInternal.NextBytes(ByteBuffer)
            For Index = 0 To Buffer.Length - 1
                Dim ByteBufferIndex As Int32 = Index * 4
                Buffer(Index) = BitConverter.ToInt32(ByteBuffer, ByteBufferIndex)
            Next
        End Sub

        Public Function Int32Next(Length As Int32) As Int32()
            Dim Buffer(Length - 1) As Int32
            Int32Next(Buffer)
            Return Buffer
        End Function
        
        Public Shadows Function ToString() As String
            Dim ResultBuilder As New Text.StringBuilder
            ResultBuilder.Append("[ ")
            For Each Item In Seeds
                ResultBuilder.Append(Item)
                ResultBuilder.Append(" ")
            Next
            ResultBuilder.Append("{ ")
            ResultBuilder.Append(FinalSeed)
            ResultBuilder.Append(" } ]")
            Return ResultBuilder.ToString()
        End Function
    End Class
End NameSpace