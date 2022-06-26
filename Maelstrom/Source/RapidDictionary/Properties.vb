Partial Friend Class RapidDictionary(Of TKey, TValue)
    
    ''' <summary>
    ''' 32 bit unsigned integer. Tracks how many key-value paires are within this RapidDictonary.
    ''' </summary>
    ''' <remarks></remarks>
    Public Readonly Property Length as UInt32
        Get
            Return FreePointer - ReclaimedCount
        End Get
    End Property
    
    ''' <summary>
    ''' Default accessor. Gets or sets a specific value when provided a key.
    ''' </summary>
    ''' <param name="RKey">Key of the value to get or set.</param>
    ''' <remarks></remarks>
    Public Default Property Value(RKey As TKey) As TValue
        Get
            Dim ClaimedPointer as Int32 = Search(RKey)
            If ClaimedPointer = -1 Then Return Nothing
            Return BaseValuesArray(ClaimedPointer)
        End Get
        Set(Item As TValue)
            Dim ClaimedPointer as Int32 = Search(RKey)
            If ClaimedPointer <> -1 Then BaseValuesArray(ClaimedPointer) = Item
        End Set
    End Property
End Class