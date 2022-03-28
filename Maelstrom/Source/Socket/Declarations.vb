Imports System.Net.Sockets
Imports System.Security.Cryptography
Public Partial Class Socket
    Implements IDisposable
    
    Private WriteDataHeader(31) as Byte
    Private WriteDataLength as Int32 = 0
    Private WritePaddedLength as Int32 = 0
    Private WritePaddedData(65535) as Byte
    Private WriteBlockCount as Int32 = 0
    Private WriteBlockSize as Int32 = 20000
    Private WriteLoopIndexer as Int32 = 0
    
    Private ReadDataHeader(31) as Byte
    Private ReadDataLength as Int32 = 0
    Private ReadPaddedLength as Int32 = 0
    Private ReadPaddedData(65535) as Byte
    Private ReadIsJagged as Int32
    Private ReadIsMuxed as Int32
    Private ReadBlockCount as Int32 = 0
    Private ReadBlockSize as Int32 = 20000
    Private ReadLoopIndexer as Int32 = 0
    
    Private SeekDataHeader(31) as Byte
    Private SeekDataLength as Int32 = 0
    Private SeekPaddedLength as Int32 = 0
    Private SeekPaddedData(65535) as Byte
    Private SeekIsJagged as Int32
    Private SeekIsMuxed as Int32
    Private SeekBlockCount as Int32 = 0
    Private SeekBlockSize as Int32 = 20000
    Private SeekLoopIndexer as Int32 = 0
    
    'miscellanious variables
    Private RemotePort as Int32
    Private ReadOnly Manager as new RapidDictionary
    Private Closing as Boolean = False
    Private ReadOnly DictLock as Object = New Object
    
    'base network stream
    Private NetStream as NetworkStream
    Private NetSocket as Net.Sockets.Socket
    Private DataIsAvailable as Boolean = False
    Private DataIsNotAvailable as Boolean = False
    

    'read variables
    Private RemoteCSP as AesCryptoServiceProvider
    Private RemoteDecryptor as ICryptoTransform
    Private RemoteKey(31) as Byte
    Private RemoteIV(15) as Byte
    Private RemoteProtocolHandshake(63) as Byte
    Private RemoteTimestamp as DateTime
    Private RemoteEndpoint as Net.IPEndPoint
    Private RemoteRNGMan as RNG
    Private ReadOnly ReadLock as Object = New Object
    Private ReadJaggedIndexer as Int32 = 0
    Private ReadJaggedIndexLength as Int32 = 0
    Private ReadJaggedCounter as Int32 = 0
    Private RemoteTransformBuffer(65535) as Byte
    
    'write variables
    Private LocalCSP as AesCryptoServiceProvider
    Private LocalEncryptor as ICryptoTransform
    Private LocalKey(31) as Byte
    Private LocalIV(15) as Byte
    Private LocalProtocolHandshake(63) as Byte
    Private LocalTimestamp as DateTime
    Private LocalEndpoint as Net.IPEndPoint
    Private LocalRNGMan as RNG
    Private ReadOnly WriteLock as Object = New Object
    Private WriteJaggedIndexer as Int32 = 0
    Private WriteJaggedCounter as Int32 = 0
    Private LocalTransformBuffer(65536) as Byte
End Class