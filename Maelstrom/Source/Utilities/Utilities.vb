Friend Module Utilities
    Friend Function GetNearestBlockSize(Size as Int32) as Int32
        Return CInt(math.Ceiling(Size/AESBlockSizeBytes))*AESBlockSizeBytes
    End Function
End Module