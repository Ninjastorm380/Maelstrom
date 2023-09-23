Namespace Lightning
    Friend Partial Class Governor
        Public Property Frequency As Double
            Get
                Return BaseFrequency
            End Get
            Set
                BaseFrequency = Value
                TargetTimespan = FrequencyToTimespan(Value)
                Dim SleepValue As Double = Value - 1.0
                If SleepValue >= 2.0 Then
                    SleepTimespan = FrequencyToTimespan(SleepValue)
                    DoSleep = True 
                Else
                    DoSleep = False
                End If
            End Set
        End Property
    
        Public Property Delta As TimeSpan
    End Class
End NameSpace