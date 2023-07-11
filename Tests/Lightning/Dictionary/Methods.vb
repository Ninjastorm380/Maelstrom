    Public Partial Class Dictionary(Of TKey, TValue)
        Private Function Search(ByVal Key As TKey) As Int32
            If FreePointer = 0 Then Return -1

            For Index = 0 To FreePointer - 1

                If BaseClaimedBitmap(Index) = True Then
                    If BaseKeysArray(Index)?.Equals(Key) = True Then Return Index
                End If
            Next

            Return -1
        End Function

        Public Sub Add(ByVal Key As TKey, ByVal Value As TValue)
            If Search(Key) = -1 Then

                If ReclaimedCount = 0 Then

                    If FreePointer >= BaseKeysArray.Length Then
                        Array.Resize(BaseKeysArray, FreePointer * 2)
                        Array.Resize(BaseValuesArray, FreePointer * 2)
                        Array.Resize(BaseClaimedBitmap, FreePointer * 2)
                    End If

                    BaseKeysArray(FreePointer) = Key
                    BaseValuesArray(FreePointer) = Value
                    BaseClaimedBitmap(FreePointer) = True
                    FreePointer += 1
                Else
                    Dim ClaimedPointer = ReclaimedPointers.Dequeue()
                    BaseKeysArray(ClaimedPointer) = Key
                    BaseValuesArray(ClaimedPointer) = Value
                    BaseClaimedBitmap(ClaimedPointer) = True
                    ReclaimedCount -= 1
                End If
            End If
        End Sub

        Public Sub Remove(ByVal Key As TKey)
            Dim ClaimedPointer As Int32 = Search(Key)

            If ClaimedPointer <> -1 Then
                ReclaimedPointers.Enqueue(ClaimedPointer)
                BaseClaimedBitmap(ClaimedPointer) = False
                ReclaimedCount += 1
            End If
        End Sub

        Public Function Contains(ByVal Key As TKey) As Boolean
            Return Search(Key) <> -1
        End Function
        
        Private Sub BlockCopyKeys(ByRef InputBuffer As TKey(), ByVal InputBufferOffset As Int32, ByRef OutputBuffer As TKey(), ByVal OutputBufferOffset As UInt32, ByVal Count As UInt32)
            For Index As Int32 = InputBufferOffset To InputBufferOffset + Count - 1
                OutputBuffer((Index - InputBufferOffset) + OutputBufferOffset) = InputBuffer(Index)
            Next
        End Sub
        Private Sub BlockCopyValues(ByRef InputBuffer As TValue(), ByVal InputBufferOffset As Int32, ByRef OutputBuffer As TValue(), ByVal OutputBufferOffset As UInt32, ByVal Count As UInt32)
            For Index As Int32 = InputBufferOffset To InputBufferOffset + Count - 1
                OutputBuffer((Index - InputBufferOffset) + OutputBufferOffset) = InputBuffer(Index)
            Next
        End Sub
    End Class

