    ''' <summary>
    ''' High precision spinning governor.
    ''' </summary>
    ''' <remarks></remarks>
Public Partial Class Governor
    'Base variables for property Rate.
    Private BaseRate as Double
    Private BaseRateTimeConstant as TimeSpan
    
    'Stopwatch for tracking and reporting Delta
    Private Readonly LoopWatch as new Stopwatch
    
    'Spinning wait for governor
    Private Readonly PrecisionSpinner as new PrecisionSpinWait
End Class