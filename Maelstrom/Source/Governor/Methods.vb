Public Partial Class Governor
    ''' <summary>
    ''' Creates a new Governor instance.
    ''' </summary>
    ''' <param name="Rate">Speed of the governor, in iterations per second.</param>
    ''' <remarks></remarks>
    Public Sub New(Optional Rate as Double = 60)
        Me.Rate = Rate
    End Sub

    ''' <summary>
    ''' Performs a blocking wait. Used to keep a loop at the specified iterations per second.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Limit
        PrecisionSpinner.Wait(BaseRateTimeConstant.Subtract(LoopWatch.Elapsed))
        LoopWatch.Restart()
    End Sub
End Class