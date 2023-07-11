Public Partial Class Governor
    Public Property Frequency As Double
        Get
            Return BaseFrequency
        End Get
        Set
            BaseFrequency = Value
            BaseFrequencyTicks = (TimeSpan.TicksPerMillisecond * (1000/BaseFrequency))
        End Set
    End Property
    
    Public ReadOnly Property Delta As Double
        Get
            Return (DeltaTicks/TimeSpan.TicksPerMillisecond)
        End Get
    End Property
End Class