Friend NotInheritable Class RapidDictionary
    Private ReadOnly KeysArray(32767) as Int32
    Private ReadOnly ValuesArray(32767) as QueueStream
    Private FreeSlotPointer as Int32 = 0
    Private ReadOnly ReclaimedIndices as New Queue(Of Int32)


    Private Function Search(Key as Int32) As Int32
        If FreeSlotPointer = 0 Then Return - 1
        For x = 0 to FreeSlotPointer
            if KeysArray(x).Equals(Key) and ReclaimedIndices.Contains(x) = False
                return x
            End If
        Next
        Return - 1
    End Function
    Public Readonly Property Length as Int32
        Get
            Return FreeSlotPointer - ReclaimedIndices.Count
        End Get
    End Property



#Disable Warning ParameterHidesMember
    Public Sub Add(Key As Int32, Value As QueueStream)
#Enable Warning ParameterHidesMember
        Dim LKey as Int32 = Key + 1
            If Search(LKey) = - 1
                if ReclaimedIndices.Count > 0
                    Dim ClaimedIndex as Int32 = ReclaimedIndices.Dequeue()
                    KeysArray(ClaimedIndex) = LKey
                    ValuesArray(ClaimedIndex) = Value
                Else
                    KeysArray(FreeSlotPointer) = LKey
                    ValuesArray(FreeSlotPointer) = Value
                    FreeSlotPointer += 1
                End If
            End If
    End Sub

    Public Sub Remove(Key as Int32)
        Dim LKey as Int32 = Key + 1
            Dim KeyIndex as Int32 = Search(LKey)
            If KeyIndex <> - 1
                ReclaimedIndices.Enqueue(KeyIndex)
            End If
    End Sub

    Public Function Contains(Key as Int32) as Boolean
        Dim LKey as Int32 = Key + 1
            Return (Search(LKey) <> - 1)
    End Function

    Public Property Value(Key as Int32) as QueueStream
        get
            Dim LKey as Int32 = Key + 1
            Dim Searched as Int32 = Search(LKey)
            If Searched = -1 Then Return Nothing
            Return ValuesArray(Searched)
        End get
        Set(Item as QueueStream)
                Dim LKey as Int32 = Key + 1
                ValuesArray(Search(LKey)) = Item
        End Set
    End Property
End Class