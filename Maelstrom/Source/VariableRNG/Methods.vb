Public Partial Class VariableRNG
    Public Sub New(Seeds As Int32())
        Dim MaxPart = CInt(Math.Floor(Int32.MaxValue/Seeds.Length))
        Dim FinalSeed = 0
        For x = 0 To Seeds.Length - 1
            Dim RNGStepper As New Random(Seeds(x))
            FinalSeed += RNGStepper.Next(-MaxPart, MaxPart)
        Next
        RngInternal = New Random(FinalSeed)
    End Sub

    Public Sub [Next](ByRef Buffer As Byte())
        RngInternal.NextBytes(Buffer)
    End Sub
    
    Public Function [Next](Length As Int32) As Byte()
        Dim Buffer(Length - 1) As Byte
        RngInternal.NextBytes(Buffer)
        Return Buffer
    End Function
End Class