Imports System.IO
Imports System.IO.Compression

''' <summary>
''' GZip stream wrapper for block based compression.
''' </summary>
''' <remarks></remarks>
Public Partial Class WriteCompressor
    
    ''' <summary>
    ''' Decompresses data.
    ''' </summary>
    ''' <param name="Input">Byte array to compress.</param>
    ''' <param name="Output">Byte array to store the result of compression</param>
    ''' <param name="Count">Length of the data to compress.</param>
    ''' <remarks></remarks>
    Public Function Transform(Byref Input as Byte(), Byref Output as Byte(), Byval Count As UInt32) As UInt32
        Dim CompressedMemStream = New MemoryStream()
        Dim Compressor = New GZipStream(CompressedMemStream,CompressionMode.Compress)
        
        Compressor.Write(Input,0, Count)
        Compressor.Close()
        Dim TempBuffer = CompressedMemStream.ToArray()
        
        If Output.Length < TempBuffer.Length Then Redim Output(TempBuffer.Length - 1)
        Buffer.BlockCopy(TempBuffer,0,Output,0,TempBuffer.Length )
        
        Compressor.Dispose()
        CompressedMemStream.Dispose()
        Return TempBuffer.Length
    End Function
End Class