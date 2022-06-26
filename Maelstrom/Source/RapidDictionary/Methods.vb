Partial Friend Class RapidDictionary(Of TKey, TValue)
    
    ''' <summary>
    ''' Searches for the specfied key and returns it's index, or -1 if nothing was found.
    ''' </summary>
    ''' <param name="RKey">Key to get the index of.</param>
    ''' <remarks></remarks>
    Private Function Search(RKey as TKey) As Int32
        If FreePointer = 0 Then Return -1
        
        For Index = 0 To FreePointer - 1
            If BaseClaimedBitmap(Index) = True 
                If BaseKeysArray(Index).Equals(RKey) = True Then 
                    Return Index
                End If
            End If
        Next
        Return -1
    End Function
    
    ''' <summary>
    ''' Adds a key-value pair to the dictionary if it doesn't already exist.
    ''' </summary>
    ''' <param name="RKey">Key of the key-value pair to add.</param>
    ''' <param name="RValue">Value of the key-value pair to add.</param>
    ''' <remarks></remarks>
    Public Sub Add(RKey As TKey, RValue As TValue)
        If Search(RKey) = -1
            If ReclaimedCount = 0
                
                if FreePointer >= BaseKeysArray.Length
                    Redim Preserve BaseKeysArray(Freepointer * 2)
                    Redim Preserve BaseValuesArray(Freepointer * 2)
                    Redim Preserve BaseClaimedBitmap(Freepointer * 2)
                End If
                
                BaseKeysArray(FreePointer) = RKey
                BaseValuesArray(FreePointer) = RValue
                BaseClaimedBitmap(FreePointer) = True
                Freepointer += 1
            Else
                Dim ClaimedPointer = ReclaimedPointers.Dequeue()
                BaseKeysArray(ClaimedPointer) = RKey
                BaseValuesArray(ClaimedPointer) = RValue
                BaseClaimedBitmap(ClaimedPointer) = True
                ReclaimedCount -= 1
            End If
        End If
    End Sub
    
    ''' <summary>
    ''' marks an existing key-value pair as reclaimed, allowing reuse of the reclaimed index.
    ''' </summary>
    ''' <param name="RKey">Key of the key-value pair to remove.</param>
    ''' <remarks></remarks>
    Public Sub Remove(RKey As TKey)
        Dim ClaimedPointer as Int32 = Search(RKey)
        If ClaimedPointer <> -1 Then
            ReclaimedPointers.Enqueue(ClaimedPointer)
            BaseClaimedBitmap(ClaimedPointer) = False
            ReclaimedCount += 1
        End If
    End Sub
    
    ''' <summary>
    ''' Checks if a key-value pair exists.
    ''' </summary>
    ''' <param name="RKey">Key of the key-value pair to search for.</param>
    ''' <remarks></remarks>
    Public Function Contains(RKey As TKey) As Boolean
        Return Search(RKey) <> -1
    End Function
End Class