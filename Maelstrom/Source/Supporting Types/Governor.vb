Public NotInheritable Class Governor
    Private LocalRate as Double
    Private LocalRateTimeConstant as TimeSpan

    Public Property Rate as Double
        get
            Return LocalRate
        End get
        Set
            LocalRate = Value
            LocalRateTimeConstant = New TimeSpan(TimeSpan.TicksPerMillisecond*(1000.0/Value))
        End Set
    End Property

    Private Readonly LoopWatch as new Stopwatch
    Private Readonly PrecisionSpinner as new PrecisionSpinWait

    Public Sub New(Optional Rate as Double = 60)
        Me.Rate = Rate
    End Sub

    Public Sub Limit
        PrecisionSpinner.Wait(LocalRateTimeConstant.Subtract(LoopWatch.Elapsed))
        LoopWatch.Restart()
    End Sub

    Private Class PrecisionSpinWait
        Private Readonly IdleWatcher as new Stopwatch
        Private Readonly SleepTargetOffset as new TimeSpan(0, 0, 0, 0, 0001)
        Private SleepTarget as Timespan
        Private PrecisionSpinTarget as TimeSpan

        Friend Sub Wait(Time as TimeSpan)
            IdleWatcher.Restart()
            
            PrecisionSpinTarget = Time
            if PrecisionSpinTarget.TotalMilliseconds <= 0 Then
                IdleWatcher.Reset()
                exit sub
            End If
            SleepTarget = Time.Subtract(SleepTargetOffset)
            if SleepTarget.TotalMilliseconds > 0 Then Threading.Thread.Sleep(SleepTarget)
            While PrecisionSpinTarget.TotalMilliseconds >= IdleWatcher.Elapsed.TotalMilliseconds : End While
            IdleWatcher.Reset()
        End Sub
    End Class
End Class