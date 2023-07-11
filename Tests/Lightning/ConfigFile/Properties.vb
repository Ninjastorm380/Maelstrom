Public Partial Class ConfigFile
    Public Property Name(Optional Index As Int32 = 0) As String
        Get
            Return BaseData.Keys(Index)
        End Get
        Set
            If Index <= BaseData.Length - 1 AndAlso BaseData.Keys(Index) <> Value Then
                If BaseData.Contains(Value) = True
                    Dim Temp As Dictionary(Of String, String) = BaseData(BaseData.Keys(Index))
                    BaseData.Remove(BaseData.Keys(Index))
                    
                    For Counter = 0 To Temp.Length - 1
                        If BaseData(Value).Contains(Temp.Keys(Counter)) = True
                            BaseData(Value)(Temp.Keys(Counter)) = Temp.Values(Counter)
                        Else
                            BaseData(Value).Add(Temp.Keys(Counter), Temp.Values(Counter))
                        End If
                    Next
                Else
                    Dim Temp As Dictionary(Of String, String) = BaseData(BaseData.Keys.ToArray(Index))
                    BaseData.Remove(BaseData.Keys(Index))
                    BaseData.Add(Value, Temp)
                End If
            Else
                BaseData.Add(Value, New Dictionary(Of String,String))
            End If
        End Set
    End Property

    
    Public Default Property Value(Key As String, Optional Name As String = "") As String
        Get
            If Name = "" Then Name = BaseData.Keys(0)
            If BaseData(Name).Contains(Key) = False Then Return ""
            Return BaseData(Name)(Key)
        End Get
        Set
            If Name = "" Then Name = BaseData.Keys(0)
            If BaseData.Contains(Name) = False Then BaseData.Add(Name, New Dictionary(Of String,String))
            If BaseData(Name).Contains(Key) = False Then
                BaseData(Name).Add(Key, Value)
            Else
                BaseData(Name)(Key) = Value
            End If
        End Set
    End Property
End Class