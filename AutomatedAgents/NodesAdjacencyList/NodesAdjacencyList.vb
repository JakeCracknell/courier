Public Class NodesAdjacencyList
    Public Rows As New Dictionary(Of Long, NodesAdjacencyListRow)
    Public Sub AddWay(ByVal Way As Way)
        If Way.Nodes.Length >= 2 Then
            Dim LastNode As Node = Way.Nodes(0)

            For i = 1 To Way.Nodes.Length - 1
                Dim CurrentNode As Node = Way.Nodes(i)
                AddNodesWay(LastNode, CurrentNode, Way)
                If Not Way.OneWay Then
                    AddNodesWay(CurrentNode, LastNode, Way)
                Else
                    AddNodeEmpty(CurrentNode)
                End If
                LastNode = CurrentNode
            Next
        End If
    End Sub

    Public Sub AddNodesWay(ByVal Node1 As Node, ByVal Node2 As Node, ByVal Way As Way)
        Dim Row As NodesAdjacencyListRow = AddNodeEmpty(Node1)

        Dim Cell As New NodesAdjacencyListCell(Node2, Way)
        Row.AddCell(Cell)
    End Sub

    Public Function AddNodeEmpty(ByVal Node As Node) As NodesAdjacencyListRow
        Dim Row As NodesAdjacencyListRow = Nothing
        Rows.TryGetValue(Node.ID, Row)
        If Row Is Nothing Then
            Row = New NodesAdjacencyListRow(Node)
            Rows.Add(Node.ID, Row)
        End If
        Return Row
    End Function

    Function GetRandomNode() As Node
        Return Rows.Values(Int(Rnd() * Rows.Count)).NodeKey
    End Function

    Function GetRandomNodePosition() As RoutePosition
        Return Nothing 'TODO
    End Function

    Function GetNearestNode(ByVal Latitude As Double, ByVal Longitude As Double)
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue
        For Each Row As NodesAdjacencyListRow In Rows.Values
            Dim Distance As Double = GetDistance(Row.NodeKey.Latitude, Row.NodeKey.Longitude, Latitude, Longitude)
            If Distance < BestDistance Then
                BestNode = Row.NodeKey
                BestDistance = Distance
            End If
        Next
        Return BestNode
    End Function

    Sub RemoveDisconnectedComponents()
        'Surprisingly fast function. 300ms for crete!
        Dim t As New Stopwatch
        t.Start()
        'Dim GraphComponents As New List(Of List(Of Node))
        Dim FullyExploredNodeIDs As New HashSet(Of Long)

        'Run DFS from a random node. Traversing any one way roads: call RouteExists() which uses AStar or other to check it is not a dead end
        Dim Stack As New Stack(Of NodesAdjacencyListRow)
        Dim StackIDs As New HashSet(Of Long)

        'Dim CurrentNode As Node = GetRandomNode()
        Stack.Push(Rows.Values(Int(Rnd() * Rows.Count)))
        Do
            Dim CurrentRow As NodesAdjacencyListRow = Stack.Peek
            For Each AdjacentCell As NodesAdjacencyListCell In CurrentRow.Cells
                If Not StackIDs.Contains(AdjacentCell.Node.ID) AndAlso _
                        Not FullyExploredNodeIDs.Contains(AdjacentCell.Node.ID) Then
                    Stack.Push(Rows(AdjacentCell.Node.ID))
                    StackIDs.Add(AdjacentCell.Node.ID)
                    Continue Do
                End If
            Next
            'Need to check that this is a valid move. If the road is one-way, we need to verify there is an alternate route
            'inefficient to run Astar every time from these two points?
            'Maybe make a list of one-way pops and do astar from the nearest node in FullyExploredNodeIDs
            Stack.Pop()
            StackIDs.Remove(CurrentRow.NodeKey.ID)
            FullyExploredNodeIDs.Add(CurrentRow.NodeKey.ID)
        Loop Until Stack.Count = 0

        Dim str As String = ""
        Dim count As Integer = 0
        For Each Row As NodesAdjacencyListRow In Rows.Values
            If Not FullyExploredNodeIDs.Contains(Row.NodeKey.ID) Then
                'TODO: start the same search from here!!!
                count += 1
            End If
        Next
        'MsgBox(t.ElapsedMilliseconds & " ms", MsgBoxStyle.OkOnly, count)
    End Sub

End Class
