    ''' <summary>
    ''' Custom exception for a lack of subsockets.
    ''' </summary>
    ''' <remarks></remarks>
Friend Class SubSocketsExhaustedException : Inherits Exception
    
    ''' <summary>
    ''' Creates a new SubSocketsExhaustedException.
    ''' </summary>
    ''' <param name="Message">Error message.</param>
    ''' <remarks></remarks>
    Public Sub New(Message as String)
        MyBase.New(Message)
    End Sub
End Class