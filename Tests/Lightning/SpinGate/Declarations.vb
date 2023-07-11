
    Public Partial Class SpinGate(Of T)
        Private BaseReturnObject As T = Nothing
        Private BaseReturnFlag As System.Boolean = False
        Private BaseReturnCalledEarlyFlag As System.Boolean = False
        Private BaseRate As System.Double = 20.0
        Private Spinwatch As Stopwatch
    End Class
