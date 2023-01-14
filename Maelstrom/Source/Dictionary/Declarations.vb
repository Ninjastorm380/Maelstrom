    Friend Partial Class Dictionary(Of TKey, TValue)
        Private BaseKeysArray As TKey() = New TKey(63) {}
        Private BaseValuesArray As TValue() = New TValue(63) {}
        Private BaseClaimedBitmap As Boolean() = New Boolean(63) {}
        Private FreePointer As Int32 = 0
        Private ReadOnly ReclaimedPointers As Queue(Of Int32) = New Queue(Of Int32)()
        Private ReclaimedCount As Int32 = 0
    End Class