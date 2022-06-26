    ''' <summary>
    ''' Extremly fast generic dictionary.
    ''' </summary>
    ''' <remarks></remarks>
Partial Friend Class RapidDictionary(Of TKey, TValue)
        
        ''' <summary>
        ''' Key array. Contains unique values, and is used along with BaseClaimedBitmap to retrieve values from BaseValuesArray.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseKeysArray(32767) as TKey
        
        ''' <summary>
        ''' Value array. Contains non-unique values.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseValuesArray(32767) as TValue
        
        ''' <summary>
        ''' Boolean array. determines if a key-value pair index is claimed or free.
        ''' </summary>
        ''' <remarks></remarks>
    Private BaseClaimedBitmap(32767) as Boolean
    
        ''' <summary>
        ''' 32 bit signed integer. Tracks the next free key-value pair index.
        ''' </summary>
        ''' <remarks></remarks>
    Private FreePointer as Int32 = 0
    
        ''' <summary>
        ''' Generic queue. Tracks all reclaimed pairs, and is used to avoid fragmentation.
        ''' </summary>
        ''' <remarks></remarks>
    Private ReadOnly ReclaimedPointers as New Queue(Of UInt32)
        
        ''' <summary>
        ''' 32 bit signed integer. Tracks how many pairs are currently reclaimed.
        ''' </summary>
        ''' <remarks></remarks>
    Private ReclaimedCount as Int32 = 0

End Class