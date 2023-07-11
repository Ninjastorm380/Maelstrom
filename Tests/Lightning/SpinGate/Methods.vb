    Public Partial Class SpinGate(Of T)
        Public Sub New()
            Spinwatch = New Stopwatch()
        End Sub

        Public Function Lock(ByVal Optional TimeoutLimit As UInt32 = 0) As T
            If BaseReturnCalledEarlyFlag = False Then
                BaseReturnFlag = False
                Spinwatch.Restart()
                Do Until BaseReturnFlag = True Or Spinwatch.ElapsedMilliseconds >= TimeoutLimit : Loop 
                Spinwatch.Stop()
                If BaseReturnFlag = False Then Throw New TimeoutException("SpinGate timed out!")
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
