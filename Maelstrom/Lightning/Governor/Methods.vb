Imports System.Threading

Namespace Lightning

    Friend Partial Class Governor
        Public Sub New(Optional Frequency As Double = 60)
            Me.Frequency = Frequency
            GovernorWatch = Stopwatch.StartNew()
        End Sub
    
        Public Sub Limit()
            If DoSleep = True Then Thread.Sleep(SleepTimespan)
            Do Until GovernorWatch.Elapsed >= TargetTimespan : Loop
            Delta = GovernorWatch.Elapsed
            GovernorWatch.Restart()
        End Sub
    
        Private Shared Function FrequencyToTimespan(InputFrequency As Double) As TimeSpan
            Return TimeSpan.FromMilliseconds(1000/InputFrequency)
        End Function
    End Class
End NameSpace