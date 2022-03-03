Friend Module StaticGlobalVariables
    Friend Const AESBlockSizeBytes as Int32 = 16
    Friend ReadOnly MaelstromHandshakeVersion as Int32() = {1,0,0,0}
    Friend ReadOnly MaelstromHandshakeHeader as Byte() = {109, 97, 101, 108, 115, 116, 114, 111, 109, 115, 101, 99, 117, 114, 101, 100}
End Module