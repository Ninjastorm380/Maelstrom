    Friend Partial Class Dictionary(Of TKey, TValue)
        Private BaseKeysArray As TKey() = New TKey(32767) {}
        Private BaseValuesArray As TValue() = New TValue(32767) {}
        Private BaseClaimedBitmap As Boolean() = New Boolean(32767) {}
        Private FreePointer As Int32 = 0
        Private ReadOnly ReclaimedPointers As Queue(Of Int32) = New Queue(Of Int32)()
        Private ReclaimedCount As Int32 = 0
    End Class