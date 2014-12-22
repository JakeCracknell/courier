Module RoutingUtils
    Function DFSToAny(ByVal StartNode As Node, ByRef Destinations As HashSet(Of Node), ByVal AdjList As NodesAdjacencyList) As Boolean
        Dim Stack As New Stack(Of NodesAdjacencyListRow)
        Dim StackIDs As New HashSet(Of Long)
        Dim FullyExploredNodes As New HashSet(Of Node)

        Stack.Push(AdjList.Rows(StartNode.ID))
        Do
            Dim CurrentRow As NodesAdjacencyListRow = Stack.Peek
            If Destinations.Contains(CurrentRow.NodeKey) Then
                For Each Row As NodesAdjacencyListRow In Stack
                    Destinations.Add(Row.NodeKey)
                Next
                Return True
            End If

            For Each AdjacentCell As NodesAdjacencyListCell In CurrentRow.Cells
                If Not StackIDs.Contains(AdjacentCell.Node.ID) AndAlso _
                        Not FullyExploredNodes.Contains(AdjacentCell.Node) Then

                    Stack.Push(AdjList.Rows(AdjacentCell.Node.ID))
                    StackIDs.Add(AdjacentCell.Node.ID)
                    Continue Do
                End If
            Next

            Stack.Pop()
            StackIDs.Remove(CurrentRow.NodeKey.ID)
            FullyExploredNodes.Add(CurrentRow.NodeKey)
        Loop Until Stack.Count = 0

        'No route found
        Return False
    End Function
End Module
