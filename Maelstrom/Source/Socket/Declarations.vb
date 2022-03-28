Imports System.Net.Sockets
Imports System.Security.Cryptography
Public Partial Class Socket
    Implements IDisposable
    
    Private WriteDataHeader(15) as Byte
    Private WriteDataLength as Int32 = 0
    Private WritePaddedLength as Int32 = 0
    Private WritePaddedData(8198144) as Byte
    
    Private ReadDataHeader(15) as Byte
    Private ReadDataLength as Int32 = 0
    Private ReadPaddedLength as Int32 = 0
    Private ReadPaddedData(8198144) as Byte
    Private ReadIsJagged as Int32
    Private ReadIsMuxed as Int32
    
    Private SeekDataHeader(15) as Byte
    Private SeekPaddedLength as Int32 = 0
    Private SeekPaddedData(8198144) as Byte
    Private SeekIsMuxed as Int32

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
    Private RemoteStream as CryptoStream
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

    'write variables
    Private LocalCSP as AesCryptoServiceProvider
    Private LocalEncryptor as ICryptoTransform
    Private LocalStream as CryptoStream
    Private LocalKey(31) as Byte
    Private LocalIV(15) as Byte
    Private LocalProtocolHandshake(63) as Byte
    Private LocalTimestamp as DateTime
    Private LocalEndpoint as Net.IPEndPoint
    Private LocalRNGMan as RNG
    Private ReadOnly WriteLock as Object = New Object
    Private WriteJaggedIndexer as Int32 = 0
    Private WriteJaggedCounter as Int32 = 0
End Class