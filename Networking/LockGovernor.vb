Namespace Governors
    Public Class LockGovernor(of T)
        Private ResetLock As Boolean
        Private ReadOnly LockGovernor As LoopGovernor
        Private LoopMax As Integer = 500
        Private LoopCurrent As Integer = 0
        Private ReadOnly LoopFrequency As Integer = 100
        Private Obj As T = Nothing
        Public Structure LockResult
            Friend Property TimedOut As Boolean
            Public Property ReturnedObject As T
        End Structure
        Public Sub New(Optional LoopFrequency As Integer = 100)
            ResetLock = False
            LockGovernor = New LoopGovernor(LoopFrequency)
            Me.LoopFrequency = LoopFrequency
        End Sub
        Public Function Lock(Optional TimeoutMS As Integer = 0) As LockResult
            If TimeoutMS <> 0 Then
                LoopMax = CInt(Math.Ceiling((TimeoutMS / 1000) * LoopFrequency))
            Else
                LoopMax = 0
            End If
            LoopCurrent = 0
            Do While ResetLock = False
                If LoopMax <> 0 Then
                    LoopCurrent += 1
                    If LoopCurrent >= LoopMax Then
                        Return New LockResult With {.TimedOut = True, .ReturnedObject = Nothing}
                    End If
                End If
                LockGovernor.Limit()
            Loop
            ResetLock = False
            Return New LockResult With {.TimedOut = False, .ReturnedObject = Obj}
        End Function
        Public Sub Unlock(Optional ObjPassthrough As T = Nothing)
            Obj = ObjPassthrough
            ResetLock = True
        End Sub
    End Class
End Namespace
