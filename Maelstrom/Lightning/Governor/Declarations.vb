Namespace Lightning
    Friend Partial Class Governor
        Private ReadOnly GovernorWatch As Stopwatch
        Private SleepTimespan As TimeSpan
        Private TargetTimespan As TimeSpan
        Private BaseFrequency As Double
        Private DoSleep As Boolean
    End Class
End NameSpace