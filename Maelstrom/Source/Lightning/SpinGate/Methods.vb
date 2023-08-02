Namespace Lightning
    Public Partial Class SpinGate(Of T)
        Public Sub New()
            Spinwatch = New Stopwatch()
        End Sub

        Public Function Lock(ByVal Optional TimeoutLimit As UInt32 = 0, Optional ByRef HasTimedOut As Boolean = False) As T
            If BaseReturnCalledEarlyFlag = False Then
                BaseReturnFlag = False
                Spinwatch.Restart()
                If TimeoutLimit = 0 Then
                    Do Until BaseReturnFlag = True : Loop 
                Else
                    Do Until BaseReturnFlag = True Or Spinwatch.ElapsedMilliseconds >= TimeoutLimit : Loop 
                End If
                Spinwatch.Stop()
                HasTimedOut = Not BaseReturnFlag
            End If

            BaseReturnCalledEarlyFlag = False
            BaseReturnFlag = False
            Return BaseReturnObject
        End Function

        Public Sub Unlock(ByVal ReturnValue As T)
            BaseReturnObject = ReturnValue
            BaseReturnCalledEarlyFlag = True
            BaseReturnFlag = True
        End Sub
    End Class
End Namespace