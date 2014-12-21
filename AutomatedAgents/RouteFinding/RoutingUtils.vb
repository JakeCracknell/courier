Module RoutingUtils
    Function GetNearestNodeFromSet(ByVal TargetNode As Node, ByVal Candidates As HashSet(Of Node)) As Node
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue
        For Each Node As Node In Candidates
            Dim Distance As Double = GetDistance(Node.Latitude, Node.Longitude, TargetNode.Latitude, TargetNode.Longitude)
            If Distance < BestDistance Then
                BestNode = Node
                BestDistance = Distance
            End If
        Next
        Return BestNode
    End Function

    Function DFSToAny(ByVal StartNode As Node, ByRef Destinations As HashSet(Of Node), ByVal AdjList As NodesAdjacencyList) As Boolean
        'StartNode = AdjList.Rows(1592509830).NodeKey

        Dim Stack As New Stack(Of NodesAdjacencyListRow)
        Dim StackIDs As New HashSet(Of Long)
        Dim FullyExploredNodes As New HashSet(Of Node)

        'Starts from random Node
        Stack.Push(AdjList.Rows(StartNode.ID))
        Do
            Dim CurrentRow As NodesAdjacencyListRow = Stack.Peek
            If Destinations.Contains(CurrentRow.NodeKey) Then
                For Each Row As NodesAdjacencyListRow In Stack
                    Destinations.Add(Row.NodeKey)
                Next
                'Debug.WriteLine(Destinations.Count.ToString.PadLeft(30))
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

        'Debug.WriteLine("DFS failed")
        'No route found
        Return False
    End Function
End Module
