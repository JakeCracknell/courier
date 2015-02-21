Module ListPermutator
    Private PermList As Object
    Function GetAllPermutations(Of T)(ByVal Values As List(Of T)) As List(Of List(Of T))
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

    'complexity = .5(n+2)(n+1). if insertion is optimal, note this sums to 1/6n(n^2+6n+11). TSP by insetion?
    Function GetAllListDoubleInsertions(Of T)(ByVal InitialList As List(Of T), ByVal A As T, ByVal B As T) As List(Of List(Of T))
        Dim Permutations As New List(Of List(Of T))
        For AInsertionPoint = 0 To InitialList.Count
            For BInsertionPoint = AInsertionPoint To InitialList.Count
                Dim List As New List(Of T)(InitialList.Count + 2)
                List.AddRange(InitialList)
                List.Insert(BInsertionPoint, B)
                List.Insert(AInsertionPoint, A)
                Permutations.Add(List)
            Next
        Next
        Return Permutations
    End Function
End Module
