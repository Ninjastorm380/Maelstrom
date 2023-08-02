    Namespace Lightning
        Public Partial Class Dictionary(Of TKey, TValue)
        Private Function Search(ByVal Key As TKey) As Int32
            If FreePointer = 0 Then Return -1
            For Index = 0 To FreePointer - 1
                If BaseClaimedBitmap(Index) = True AndAlso BaseKeysArray(Index).Equals(Key) = True Then Return Index
            Next
            Return -1
        End Function

        Public Sub Add(ByVal Key As TKey, ByVal Value As TValue)
            If Search(Key) <> -1 Then Return
            
            If ReclaimedCount <> 0 Then
                Dim ClaimedPointer = ReclaimedPointers.Dequeue()
                BaseKeysArray(ClaimedPointer) = Key
                BaseValuesArray(ClaimedPointer) = Value
                BaseClaimedBitmap(ClaimedPointer) = True
                ReclaimedCount -= 1
                Return
            End If
            
            If FreePointer >= BaseKeysArray.Length Then
                Array.Resize(BaseKeysArray, FreePointer * 2)
                Array.Resize(BaseValuesArray, FreePointer * 2)
                Array.Resize(BaseClaimedBitmap, FreePointer * 2)
            End If
            BaseKeysArray(FreePointer) = Key
            BaseValuesArray(FreePointer) = Value
            BaseClaimedBitmap(FreePointer) = True
            FreePointer += 1
        End Sub

        Public Sub Remove(ByVal Key As TKey)
            Dim ClaimedPointer As Int32 = Search(Key)
            If ClaimedPointer = -1 Then Return
            
            ReclaimedPointers.Enqueue(ClaimedPointer)
            BaseClaimedBitmap(ClaimedPointer) = False
            ReclaimedCount += 1
        End Sub

        Public Function Contains(ByVal Key As TKey) As Boolean
            Return Search(Key) <> -1
        End Function
    End Class
    End Namespace

