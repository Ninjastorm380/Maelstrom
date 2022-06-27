
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography

''' <summary>
    ''' The maelstrom socket, a core part of any maelstrom client or server.
    ''' </summary>
    ''' <remarks></remarks>
Public Partial Class Socket : Implements IDisposable


    
    'Base variables
        
        ''' <summary>
        ''' Base socket. Used along with NetStream for the underlying TCP connection.
        ''' </summary>
        ''' <remarks></remarks>
    Private NetSocket as System.Net.Sockets.Socket
        
        ''' <summary>
        ''' Base network stream. Used along with NetSocket for the underlying TCP connection.
        ''' </summary>
        ''' <remarks></remarks>
    Private NetStream as NetworkStream
        
        ''' <summary>
        ''' Base variable for use with function IsConnected(). Stores the results of NetSocket.Poll().
        ''' </summary>
        ''' <remarks></remarks>
    Private DataIsAvailable as Boolean = False
        
        ''' <summary>
        ''' Base variable for use with function IsConnected(). Stores the results of NetSocket.Available = 0.
        ''' </summary>
        ''' <remarks></remarks>
    Private DataIsNotAvailable as Boolean = False
        
        ''' <summary>
        ''' Base variable for use with sub Dispose()
        ''' </summary>
        ''' <remarks></remarks>
    Private IsDisposed as Boolean = False
        
        ''' <summary>
        ''' Base variable for use with sub Close()
        ''' </summary>
        ''' <remarks></remarks>
    Private IsClosed As Boolean = False
        
        ''' <summary>
        ''' Base variable for use with InitBootstrap(). The maelstrom protocol inherently has a chance to fail doe to utc time misalignment, so we make up to 3 total attempts
        ''' </summary>
        ''' <remarks></remarks>
    Private BootstrapRetryLimit as UInt32 = 2
        
        ''' <summary>
        ''' Base variable for use with InitBootstrap(). The maelstrom protocol inherently has a chance to fail doe to utc time misalignment, so we make up to 3 total attempts
        ''' </summary>
        ''' <remarks></remarks>
    Private BootstrapRetryResult as UInt32 = 0
        
        ''' <summary>
        ''' Base variable for use with InitBootstrap(). The maelstrom protocol inherently has a chance to fail doe to utc time misalignment, so we make up to 3 total attempts
        ''' </summary>
        ''' <remarks></remarks>
    Private BootstrapRetryCounter as UInt32 = 0
        
        ''' <summary>
        ''' Base variable for use with Init. Tracks which port is the one the server uses to communicate with the client.
        ''' </summary>
        ''' <remarks></remarks>
    Private RemotePort as Int32
        
        ''' <summary>
        ''' Base variable for the subsockets feature. Maps each UInt32 subsocket to a QueueStream.
        ''' </summary>
        ''' <remarks></remarks>
    Private SubSocketBuffers as RapidDictionary(Of UInt32, QueueStream) = New RapidDictionary(Of UInteger,QueueStream)()
        
        ''' <summary>
        ''' Base variable for the subsockets feature. Maps each UInt32 subsocket to a SubSocketConfigFlag.
        ''' </summary>
        ''' <remarks></remarks>
    Private SubSocketConfigs as RapidDictionary(Of UInt32, SubSocketConfigFlag) = New RapidDictionary(Of UInteger,SubSocketConfigFlag)()

    'Read variables. Each of these variables are involved with reading from the network stream in one form or another, and must be guarded by a lock.
    Private RemoteCSP as AesCryptoServiceProvider
    Private RemoteTransform as ICryptoTransform
    Private RemoteKey(31) as Byte
    Private RemoteIV(15) as Byte
    Private RemoteDecompressor as ReadDecompressor
    Private RemoteEndpoint as IPEndPoint
    Private RemoteProtocolHandshake(63) as Byte 
    Private RemoteTimestamp as DateTime
    Private RemoteRNG as VariableRNG

        Private BufferHeader as New DataHeader
        Private BufferTransformBuffer(65535) as Byte
        Private HasDataHeader as New DataHeader
        Private HasDataTransformBuffer(65535) as Byte
        Private AsyncHeader as New DataHeader
        Private AsyncTransformBuffer(65535) as Byte
    
    'Write variables. Each of these variables are involved with writing to the network stream in one form or another, and must be guarded by a lock.
    Private LocalCSP as AesCryptoServiceProvider
    Private LocalTransform as ICryptoTransform
    Private LocalKey(31) as Byte
    Private LocalIV(15) as Byte
    Private LocalCompressor as WriteCompressor
    Private LocalEndpoint as IPEndPoint
    Private LocalProtocolHandshake(63) as Byte
    Private LocalTimestamp as DateTime
    Private LocalRNG as VariableRNG 
    Private LocalHeader as New DataHeader
    Private LocalTransformBuffer(65535) as Byte
        

    
    'Shared variables. These are used by both methods Read() and write().
        'Private ReadOnly Lock as Object = New Object
        Private ReadOnly BufferLock as Object = New Object
        Private ReadOnly BufferReadLock as Object = New Object
        Private ReadOnly ReadLock as Object = New Object
        Private ReadOnly WriteLock as Object = New Object
        Private ReadOnly Lock as Object = New Object
End Class