    Friend Partial Class Governor
        Public Sub New(ByVal Optional Rate As System.Double = 100.0)
            BaseRate = Rate
            BaseSleepOffsetConstant = TimeSpan.TicksPerMillisecond / 10
            BaseTimeConstant = TimeSpan.TicksPerMillisecond * CLng((1000.0 / BaseRate))
            BaseSleepTarget = New TimeSpan(BaseTimeConstant - BaseSleepOffsetConstant)
            BaseGovernorWatch = Stopwatch.StartNew()
        End Sub
        
        Public Sub Pause()
            BaseGovernorWatch.Stop()
            IsPaused = True
        End Sub
        
        Public Sub [Resume]()
            BaseGovernorWatch.Start()
            IsPaused = False
        End Sub

        Public Sub Limit()
            If BaseSleepTarget.Ticks > 1000 and UseExtremePrecision = False Then Threading.Thread.Sleep(BaseSleepTarget)
            Do : Loop While BaseGovernorWatch.ElapsedTicks < BaseTimeConstant
            BaseDelta = BaseGovernorWatch.ElapsedTicks
            BaseGovernorWatch.Restart()
        End Sub
    End Class