
    Friend Partial Class Governor
        Private BaseRate As System.Double
        Private BaseTimeConstant As Int64
        Private BaseDelta As Int64
        Private ReadOnly BaseGovernorWatch As Stopwatch
        Private ReadOnly BaseSleepOffsetConstant As Int64
        Private BaseSleepTarget As TimeSpan
        Private IsPaused As Boolean = False
    End Class

