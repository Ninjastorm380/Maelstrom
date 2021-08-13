Namespace Governors
    Public Class LoopGovernor
        Private LoopStartTime As Long = Stopwatch.GetTimestamp()
        Private LoopCount As Long = 0
        Private ReadOnly Frequency As Long = Stopwatch.Frequency
        Private Timestamp As Long

        Private ReadOnly DeltaTimeArray(4) As Double
        Private DeltaTimeIndex As Integer = 0
        Private DeltaTimeIndex2 As Integer = 0
        Private ReadOnly DeltaStopwatch As New Stopwatch
        Private DeltaTime As Double = 0
        Public Property LoopsPerSecond As Integer
        Public ReadOnly Property LoopDeltaTime As Double
            Get
                Return DeltaTime
            End Get
        End Property
        Sub New(Optional LoopsPerSecond As Integer = 60)
            Me.LoopsPerSecond = LoopsPerSecond
            DeltaStopwatch.Start()
        End Sub
        Public Sub Limit()
            Timestamp = Stopwatch.GetTimestamp()
            While (Timestamp - LoopStartTime) * LoopsPerSecond < Frequency * LoopCount
                Dim WaitTime As Integer = CInt((LoopStartTime * LoopsPerSecond + Frequency * LoopCount - Timestamp * LoopsPerSecond) * 1000 / (Frequency * LoopsPerSecond))
                If WaitTime > 0 Then Threading.Thread.Sleep(WaitTime)
                Timestamp = Stopwatch.GetTimestamp()
            End While
            If Threading.Interlocked.Increment(LoopCount) > LoopsPerSecond Then
                LoopCount = 0
                LoopStartTime = Timestamp
            End If
            DeltaTimeIndex2 = DeltaTimeIndex
            DeltaTimeIndex2 += 1
            If DeltaTimeIndex2 > 4 Then DeltaTimeIndex2 = 0
            DeltaTimeArray(DeltaTimeIndex) = DeltaStopwatch.Elapsed.TotalMilliseconds
            DeltaTime = (DeltaTimeArray(0) + DeltaTimeArray(1) + DeltaTimeArray(2) + DeltaTimeArray(3) + DeltaTimeArray(4)) / 5
            DeltaTimeIndex = DeltaTimeIndex2
            DeltaStopwatch.Restart()
        End Sub
    End Class
End Namespace