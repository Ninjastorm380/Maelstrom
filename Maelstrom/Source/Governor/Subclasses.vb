Imports System.Threading

''' <summary>
''' High precision, high efficency spining wait class. Accurate to within 10 microseconds.
''' </summary>
''' <remarks></remarks>
Friend Class PrecisionSpinWait
    Private Readonly IdleWatcher as new Stopwatch
    Private Readonly SleepTargetOffset as new TimeSpan(0, 0, 0, 0, 0001)
    Private SleepTarget as Timespan
    Private PrecisionSpinTarget as TimeSpan

    ''' <summary>
    ''' Performs a hybrid spinning wait.
    ''' </summary>
    ''' <param name="Time">How long to wait for.</param>
    ''' <remarks></remarks>
    Friend Sub Wait(Time as TimeSpan)
        IdleWatcher.Restart()
            
        PrecisionSpinTarget = Time
        if PrecisionSpinTarget.Ticks <= 0 Then
            IdleWatcher.Reset()
            exit sub
        End If
        SleepTarget = Time.Subtract(SleepTargetOffset)
        if SleepTarget.Ticks > 0 Then Thread.Sleep(SleepTarget)
        While PrecisionSpinTarget.Ticks >= IdleWatcher.Elapsed.Ticks : End While
        IdleWatcher.Reset()
    End Sub
End Class