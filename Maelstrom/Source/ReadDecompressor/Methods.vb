Imports System.IO
Imports System.IO.Compression

''' <summary>
''' GZip stream wrapper for block based decompression.
''' </summary>
''' <remarks></remarks>
Partial Friend Class ReadDecompressor
    
    ''' <summary>
    ''' Decompresses data.
    ''' </summary>
    ''' <param name="Input">Byte array to decompress.</param>
    ''' <param name="Output">Byte array to store the result of decompression in.</param>
    ''' <param name="Count">Length of the compressed data.</param>
    ''' <remarks></remarks>
    Friend Function Transform(Byval Input As Byte(), Byref Output as Byte(), Byval Count As UInt32) As UInt32
        Dim InternalBuffer(Count - 1) As Byte
        Buffer.BlockCopy(Input,0,InternalBuffer,0,Count)
        Dim CompressedMemStream = New MemoryStream(InternalBuffer)
        Dim Decompressor = New GZipStream(CompressedMemStream,CompressionMode.Decompress)
        Dim DecompressedMemStream = New MemoryStream()
        
        Decompressor.CopyTo(DecompressedMemStream)
        Dim TempBuffer = DecompressedMemStream.ToArray()
        If Output.Length < TempBuffer.Length then redim Output(TempBuffer.Length - 1)
        Buffer.BlockCopy(TempBuffer,0,Output,0,TempBuffer.Length)
        
        CompressedMemStream.Dispose()
        Decompressor.Dispose()
        DecompressedMemStream.Dispose()
        Return TempBuffer.Length
    End Function
End Class