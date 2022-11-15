    Friend Partial Class Governor
        Public Property Rate As Double
            Get
                Return BaseRate
            End Get
            Set(ByVal value As System.Double)
                BaseRate = value
                BaseTimeConstant = TimeSpan.TicksPerMillisecond * CLng((1000.0 / BaseRate))
                BaseSleepTarget = New TimeSpan(BaseTimeConstant - BaseSleepOffsetConstant)
            End Set
        End Property
        Public ReadOnly Property Paused As Boolean
            Get
                Return IsPaused
            End Get
        End Property

        
        Public Property UseExtremePrecision As Boolean = False

        Public ReadOnly Property Delta As Double
            Get
                Return BaseDelta / CDbl(BaseTimeConstant)
            End Get
        End Property

        Public ReadOnly Property DeltaInverse As Double
            Get
                Return CDbl(BaseTimeConstant) / BaseDelta
            End Get
        End Property

        Public ReadOnly Property IterationElapsed As Double
            Get
                Return Delta / 1000.0
            End Get
        End Property
    End Class