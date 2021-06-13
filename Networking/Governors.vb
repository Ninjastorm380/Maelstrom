Namespace Governors
    Public Class LoopGovernor
        Private FPSStartTime As Long = System.Diagnostics.Stopwatch.GetTimestamp()
        Private FPSFrameCount As Long = 0
        Public Property LoopsPerSecond As Integer
        Sub New(Optional LoopsPerSecond As Integer = 60)
            Me.LoopsPerSecond = LoopsPerSecond
        End Sub
        Public Sub Limit()
            Dim freq As Long
            Dim frame As Long
            freq = System.Diagnostics.Stopwatch.Frequency
            frame = System.Diagnostics.Stopwatch.GetTimestamp()
            While (frame - FPSStartTime) * LoopsPerSecond < freq * FPSFrameCount
                Dim sleepTime As Integer = CInt(((FPSStartTime * LoopsPerSecond + freq * FPSFrameCount - frame * LoopsPerSecond) * 1000 / (freq * LoopsPerSecond)))
                If sleepTime > 0 Then System.Threading.Thread.Sleep(sleepTime)
                frame = System.Diagnostics.Stopwatch.GetTimestamp()
            End While
            If System.Threading.Interlocked.Increment(FPSFrameCount) > LoopsPerSecond Then
                FPSFrameCount = 0
                FPSStartTime = frame
            End If
        End Sub
    End Class
    Public Class LockGovernor
        Private ResetLock As Boolean
        Private LockGovernor As LoopGovernor
        Public Sub New()
            ResetLock = False
            LockGovernor = New LoopGovernor(100)
        End Sub
        Public Sub Lock()
            Do While ResetLock = False
                LockGovernor.Limit()
            Loop
            ResetLock = False
        End Sub
        Public Sub Unlock()
            ResetLock = True
        End Sub
    End Class
End Namespace
