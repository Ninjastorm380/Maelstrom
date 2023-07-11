Public Partial Class ConfigFile
    Public Sub New()
        BaseData = New Dictionary(Of String, Dictionary(Of String, String))
    End Sub
    
    Public Shared Function Load(Path as String) As ConfigFile
        Dim FS As New IO.FileStream(Path, IO.FileMode.Open)
        Dim RawBytes(FS.Length - 1) As Byte
        FS.Read(RawBytes, 0, FS.Length)
        FS.Close() : FS.Dispose()
        
        Dim Raw As String = Text.Encoding.ASCII.GetString(RawBytes)
        Dim Result As New ConfigFile
        Unwrap(Raw, Result.BaseData)
        
        Return Result
    End Function
    
    Public Sub Save(Path as String)
        Dim Raw As String = ""
        Wrap(Raw, BaseData)
        Dim FS As New IO.FileStream(Path, IO.FileMode.Create)
        FS.Write(Text.Encoding.ASCII.GetBytes(Raw), 0, Raw.Length)
        FS.Flush()
        FS.Close()
        FS.Dispose()
    End Sub
    
    Public Sub Remove(Name As String, Key As String)
        If BaseData.Contains(Name) = True AndAlso BaseData(Name).Contains(Key) = True Then
            BaseData(Name).Remove(Key)
        End If
    End Sub
    
    Public Sub Add(Name As String, Key As String, Optional Value As String = "")
        If BaseData.Contains(Name) = False Then BaseData.Add(Name, New Dictionary(Of String,String))
        If BaseData(Name).Contains(Key) = True Then
            BaseData(Name)(Key) = Value
        Else 
            BaseData(Name).Add(Key, Value)
        End If
    End Sub
    
    Private Shared Sub Wrap(ByRef Raw As String, ByRef Data As Dictionary(Of String, Dictionary(Of String, String)))
        Raw = ""
        For Index = 0 To Data.Length - 1
            If Data.Keys(Index) <> "" Then
                Raw += "[" + Data.Keys(Index) + "]" + Environment.NewLine
                For ChildIndex = 0 To Data(Data.Keys(Index)).Length - 1
                    Raw += Data(Data.Keys(Index)).Keys(ChildIndex) + "=" + Data(Data.Keys(Index)).Values(ChildIndex) + Environment.NewLine
                Next
                Raw += Environment.NewLine
            End If
        Next
    End Sub
    
    Private Shared Sub Unwrap(ByRef Raw As String, ByRef Data As Dictionary(Of String, Dictionary(Of String, String)))
        Dim Items As String() = Raw.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
        Dim Name As String = ""
        Dim Item As String
        
        For Counter = 0 To Items.Length - 1
            Item = Items(Counter).Trim()
            If Item(0) = "["
                Name = Item
                Name = Name.Remove(0, 1)
                Name = Name.Remove(Name.Length - 1, 1)
                If Data.Contains(Name) = False  And Name <> "" Then Data.Add(Name, New Dictionary(Of String,String))
            Else
                If Name <> "" Then
                    Dim ItemPair As String() = Item.Split("=", 2, StringSplitOptions.None)
                    ItemPair(0) = ItemPair(0).Trim()
                    ItemPair(1) = ItemPair(1).Trim()
                    
                    If Data(Name).Contains(ItemPair(0)) = True
                        Data(Name)(ItemPair(0)) = ItemPair(1)
                    Else 
                        Data(Name).Add(ItemPair(0), ItemPair(1))
                    End If
                End If
            End If
        Next
    End Sub
    
    Public Overrides Function ToString As String
        Dim Raw As String = ""
        Wrap(Raw, BaseData)
        Return Raw.TrimEnd(vbLf).TrimEnd(vbCr)
    End Function
End Class