Public Partial Class Governor
    Public Sub New(Optional Frequency As UInt16 = 60)
        BaseFrequency = Frequency
        WaitTimespan = New TimeSpan(10000000/BaseFrequency)
        DeltaTicks = 0
        BaseDelta = 0
        GovernorWatch = Stopwatch.StartNew()
    End Sub
    
    Public Sub Limit()
        BaseDelta = DeltaTicks / TimeSpan.TicksPerMillisecond
        Do Until GovernorWatch.ElapsedTicks >= WaitTimespan.Ticks
        Loop 
        DeltaTicks = GovernorWatch.ElapsedTicks
        GovernorWatch.Restart()
    End Sub
End Class