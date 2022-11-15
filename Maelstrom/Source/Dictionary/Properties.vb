    Friend Partial Class Dictionary(Of TKey, TValue)
        Public ReadOnly Property Length As Int32
            Get
                Return FreePointer - ReclaimedCount
            End Get
        End Property

        Default Public Property Item(ByVal Key As TKey) As TValue
            Get
                Dim ClaimedPointer As Int32 = Search(Key)

                If ClaimedPointer = -1 Then
                    Throw New KeyNotFoundException($"The key was not found in the dictionary!")
                End If

                Return BaseValuesArray(ClaimedPointer)
            End Get
            Set(ByVal value As TValue)
                Dim ClaimedPointer As Int32 = Search(Key)

                If ClaimedPointer = -1 Then
                    Throw New KeyNotFoundException($"The key was not found in the dictionary!")
                End If

                BaseValuesArray(ClaimedPointer) = value
            End Set
        End Property
    End Class

