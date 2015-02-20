Module ListPermutator
    Private PermList As Object
    Function GetAllPermutations(Of T)(ByVal Values As List(Of T))
        PermList = New List(Of List(Of T))
        GetAllPermutations(New List(Of T), Values)
        Return PermList
    End Function
    Private Function GetAllPermutations(Of T)(ByVal PrefixList As List(Of T), ByVal Values As List(Of T))
        Dim Length As Integer = Values.Count
        If Length = 0 Then
            PermList.Add(PrefixList)
        Else
            For i = 0 To Length - 1
                If True Then
                    Dim BiggerPrefixList As New List(Of T)(PrefixList.Count)
                    Dim SmallerValuesList As List(Of T)
                    If Length = 1 Then
                        SmallerValuesList = New List(Of T)
                    Else
                        SmallerValuesList = New List(Of T)(Values.Count - 2)
                    End If
                    Dim PickedNode As T = Values(i)
                    BiggerPrefixList.AddRange(PrefixList)
                    BiggerPrefixList.Add(PickedNode)
                    SmallerValuesList.AddRange(Values.Where(Function(x) Not x.Equals(PickedNode)))
                    GetAllPermutations(BiggerPrefixList, SmallerValuesList)
                End If

            Next
        End If
        Return Nothing
    End Function
End Module
