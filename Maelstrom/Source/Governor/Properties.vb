Public Partial Class Governor
    Public Property Frequency As UInt16
        Get
            Return BaseFrequency
        End Get
        Set
            BaseFrequency = Value
            WaitTimespan = New TimeSpan(10000000/BaseFrequency)
        End Set
    End Property
    
    Public ReadOnly Property Delta As Double
        Get
            Return BaseDelta
        End Get
    End Property
End Class