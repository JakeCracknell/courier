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
        Dim Node As Node
        Do
            Node = Rows.Values(Int(Rnd() * Rows.Count)).NodeKey
        Loop Until Node.Connected
        Return Node
    End Function

    Function GetRandomNodePosition() As RoutePosition
        Return Nothing 'TODO
    End Function

    Function GetNearestNode(ByVal Latitude As Double, ByVal Longitude As Double)
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue
        For Each Row As NodesAdjacencyListRow In Rows.Values
            Dim Node As Node = Row.NodeKey
            If Node.Connected Then
                Dim Distance As Double = GetDistance(Node.Latitude, Node.Longitude, Latitude, Longitude)
                If Distance < BestDistance Then
                    BestNode = Node
                    BestDistance = Distance
                End If
            End If
        Next
        Return BestNode
    End Function

    'Tests for a single hop link from Node1 to Node2. Cannot be one-way in opposite direction.
    Function AreNodesLinked(ByVal Node1 As Node, ByVal Node2 As Node)
        For Each Cell As NodesAdjacencyListCell In Rows(Node1.ID).Cells
            If Cell.Node = Node2 Then
                Return True
            End If
        Next
        Return False
    End Function

    Sub RemoveDisconnectedComponents()
        Dim t As New Stopwatch
        t.Start()
        Dim FullyExploredNodes As New HashSet(Of Node)

        'Run DFS from a random node.
        Dim Stack As New Stack(Of NodesAdjacencyListRow)
        Dim StackIDs As New HashSet(Of Long)
        Dim OneWayPops As New List(Of Tuple(Of Node, Node))
        Dim VerifiedNodes As New HashSet(Of Node)
        Dim NodesToVerify As New HashSet(Of Node)
        Dim NodesToPrune As New List(Of Node)

        'Starts from random Node
        Dim RandomStartRow As NodesAdjacencyListRow = Rows.Values(Int(Rnd() * Rows.Count))
        Stack.Push(RandomStartRow)
        VerifiedNodes.Add(RandomStartRow.NodeKey)
        Do
            Dim CurrentRow As NodesAdjacencyListRow = Stack.Peek
            For Each AdjacentCell As NodesAdjacencyListCell In CurrentRow.Cells
                If Not StackIDs.Contains(AdjacentCell.Node.ID) AndAlso _
                        Not FullyExploredNodes.Contains(AdjacentCell.Node) Then

                    Stack.Push(Rows(AdjacentCell.Node.ID))
                    StackIDs.Add(AdjacentCell.Node.ID)
                    Continue Do
                End If
            Next

            Stack.Pop()
            If Stack.Count <> 0 Then
                'If the road is one-way, we need to verify there is an alternate route. Often there is not.
                If Not AreNodesLinked(CurrentRow.NodeKey, Stack.Peek().NodeKey) Then
                    OneWayPops.Add(New Tuple(Of Node, Node)(CurrentRow.NodeKey, Stack.Peek().NodeKey))
                    NodesToVerify.Add(CurrentRow.NodeKey)
                    'ElseIf VerifiedNodes.Contains(Stack.Peek.NodeKey) Then
                    '    VerifiedNodes.Add(CurrentRow.NodeKey)
                End If
            End If


            StackIDs.Remove(CurrentRow.NodeKey.ID)
            FullyExploredNodes.Add(CurrentRow.NodeKey)
        Loop Until Stack.Count = 0

        NodesToVerify = New HashSet(Of Node)(FullyExploredNodes)
        NodesToVerify.ExceptWith(VerifiedNodes)

        For Each Node As Node In NodesToVerify

            'Dim AStarSearch As New AStarSearch(Nodes.Item1, Nodes.Item2, Me)
            'Debug.WriteLine(Nodes.Item1.ID & " " & Nodes.Item2.ID & " " & AStarSearch.GetCost)
            'If AStarSearch.GetRoute Is Nothing Then
            '    NodesToPrune.Add(Nodes.Item1)
            'End If

            'If Node.ID = 1947370028 Then
            '    Console.Beep()
            'End If
            If Not DFSToAny(Node, VerifiedNodes, Me) Then '<> (AStarSearch.GetRoute Is Nothing) Then
                Dim AStarSearch As New AStarSearch(Node, RandomStartRow.NodeKey, Me)
                If AStarSearch.GetRoute IsNot Nothing Then
                    Debug.WriteLine(Node.ID)
                End If
                'Debug.WriteLine(Nodes.Item1.ID & " " & Nodes.Item2.ID & " " & AStarSearch.GetCost)
                NodesToPrune.Add(Node)
            End If
        Next

        'For Each Nodes As Tuple(Of Node, Node) In OneWayPops

        '    'Dim AStarSearch As New AStarSearch(Nodes.Item1, Nodes.Item2, Me)
        '    'Debug.WriteLine(Nodes.Item1.ID & " " & Nodes.Item2.ID & " " & AStarSearch.GetCost)
        '    'If AStarSearch.GetRoute Is Nothing Then
        '    '    NodesToPrune.Add(Nodes.Item1)
        '    'End If

        '    If Nodes.Item1.ID = 1947370028 Then
        '        Console.Beep()
        '    End If
        '    If Not DFSToAny(Nodes.Item1, VerifiedNodes, Me) Then '<> (AStarSearch.GetRoute Is Nothing) Then
        '        'Debug.WriteLine(Nodes.Item1.ID & " " & Nodes.Item2.ID & " " & AStarSearch.GetCost)
        '        NodesToPrune.Add(Nodes.Item1)
        '    End If
        'Next
        Debug.WriteLine(NodesToPrune.Count)

        For Each Row As NodesAdjacencyListRow In Rows.Values
            If Not FullyExploredNodes.Contains(Row.NodeKey) Then
                NodesToPrune.Add(Row.NodeKey)
            End If
        Next

        For Each NodeToPrune In NodesToPrune
            'All I am going to do for now. Complex to remove from Adj list and not necessary.
            NodeToPrune.Connected = False
        Next

        Debug.WriteLine(t.ElapsedMilliseconds)
    End Sub

End Class
