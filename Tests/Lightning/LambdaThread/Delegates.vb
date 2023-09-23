Namespace Lightning
    Friend Partial Class LambdaThread
        Public Delegate Sub ThreadStart
    End Class

    Friend Partial Class LambdaThread(Of T1)
        Public Delegate Sub ThreadStart(Parameter1 As T1)
    End Class

    Friend Partial Class LambdaThread(Of T1, T2)
        Public Delegate Sub ThreadStart(Parameter1 As T1, Parameter2 As T2)
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3)
        Public Delegate Sub ThreadStart(Parameter1 As T1, Parameter2 As T2, Parameter3 As T3)
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3, T4)
        Public Delegate Sub ThreadStart(Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4)
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3, T4, T5)
        Public Delegate Sub ThreadStart(Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Parameter5 As T5)
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3, T4, T5, T6)
        Public Delegate Sub ThreadStart(Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Parameter5 As T5, Parameter6 As T6)
    End Class
End NameSpace