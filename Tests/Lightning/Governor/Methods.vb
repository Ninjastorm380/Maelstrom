Public Partial Class Governor
    Public Sub New(Optional Frequency As Double = 60)
        BaseFrequency = Frequency
        BaseFrequencyTicks = (TimeSpan.TicksPerMillisecond * (1000/BaseFrequency))
        DeltaTicks = BaseFrequencyTicks
        GovernorWatch = Stopwatch.StartNew()
    End Sub
    
    Public Sub Limit()
        Do Until GovernorWatch.ElapsedTicks >= BaseFrequencyTicks
        Loop
        DeltaTicks = GovernorWatch.ElapsedTicks
        GovernorWatch.Restart()
    End Sub
End Class