Public Partial Class Governor
    ''' <summary>
    ''' Double. Gets or sets how fast the governor should run in iterations per second.
    ''' </summary>
    ''' <remarks></remarks>
    Public Property Rate as Double
        get
            Return BaseRate
        End get
        Set
            BaseRate = Value
            BaseRateTimeConstant = New TimeSpan(TimeSpan.TicksPerMillisecond*(1000.0/Value))
        End Set
    End Property
    
    ''' <summary>
    ''' Double. Gets How long the code has run in milliseconds since the last call to method Limit().
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property Delta as Double
        get
            Return LoopWatch.Elapsed.TotalMilliseconds
        End get
    End Property
End Class