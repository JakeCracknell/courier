Public Class AStarSearch2Way
    Implements RouteFinder

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Private HopList As List(Of Hop)
    Private NodesSearched As New List(Of Node)
    Private Cost As Integer
    Private Const EPSILON As Double = 0.0000001

    Sub New(ByVal SourceNode As Node, ByVal DestinationNode As Node, ByVal AdjacencyList As NodesAdjacencyList)
        Me.SourceNode = SourceNode
        Me.DestinationNode = DestinationNode
        Me.AdjacencyList = AdjacencyList
        HopList = Nothing
        DoAStar2()
    End Sub

    'I can imagine some extreme examples where this would be a lot quicker.
    'But mostly it should be about twice as long.
    'Need to test along with regular to assess real world impact.
    Public Sub DoAStar2()
        Dim PriorityQueueFw, PriorityQueueBw As New SortedList(Of Double, AStarTreeNode)
        Dim AlreadyVisitedFw, AlreadyVisitedBw As New HashSet(Of Long)
        PriorityQueueFw.Add(0, New AStarTreeNode(SourceNode))
        PriorityQueueBw.Add(0, New AStarTreeNode(DestinationNode))
        Do
            DoAStarTreeIteration(PriorityQueueFw, AlreadyVisitedFw, DestinationNode)
            DoAStarTreeIteration(PriorityQueueBw, AlreadyVisitedBw, SourceNode)

            If PriorityQueueFw.Count = 0 Or PriorityQueueBw.Count = 0 Then
                Debug.WriteLine("No route found - disconnected graph?")
                Exit Sub
            End If

            ''Not useful results, even if I could join somehow
            'Dim BothVisited As New HashSet(Of Long)
            'If AlreadyVisitedFw.Overlaps(AlreadyVisitedBw) Then
            '    HopList = PriorityQueueBw.Values(0).Route
            '    Cost = PriorityQueueBw.Values(0).TotalCost

            'End If

        Loop Until HopList IsNot Nothing


    End Sub

    Private Sub DoAStarTreeIteration(ByRef PriorityQueue As SortedList(Of Double, AStarTreeNode), _
                                     ByRef AlreadyVisited As HashSet(Of Long),
                                     ByVal DestNode As Node)
        Dim AStarTreeNode As AStarTreeNode = PriorityQueue.Values(0)
        PriorityQueue.RemoveAt(0)
        If AStarTreeNode.GetCurrentNode = DestNode Then
            HopList = AStarTreeNode.Route
            Cost = AStarTreeNode.TotalCost
            Exit Sub
        End If

        NodesSearched.Add(AStarTreeNode.GetCurrentNode)
        AlreadyVisited.Add(AStarTreeNode.GetCurrentNode.ID)

        Dim Row As NodesAdjacencyListRow = AdjacencyList.Rows(AStarTreeNode.GetCurrentNode.ID)
        For Each Cell As NodesAdjacencyListCell In Row.Cells
            If Not AlreadyVisited.Contains(Cell.Node.ID) Then
                Dim NextAStarTreeNode As New AStarTreeNode(AStarTreeNode, Cell)

                Dim HeuristicCost As Double = GetDistance(Cell.Node, DestNode)
                Dim F_Cost = HeuristicCost + NextAStarTreeNode.TotalCost
                Do Until Not PriorityQueue.ContainsKey(F_Cost) 'Exception can occur otherwise
                    F_Cost += EPSILON
                Loop
                PriorityQueue.Add(F_Cost, NextAStarTreeNode)
            End If
        Next
    End Sub

    Public Function GetCost() As Double Implements RouteFinder.GetCost
        Return Cost
    End Function

    Public Function GetRoute() As Route Implements RouteFinder.GetRoute
        Return New Route(HopList)
    End Function

    Public Function GetNodesSearched() As List(Of Node) Implements RouteFinder.GetNodesSearched
        Return NodesSearched
    End Function

    Private Class AStarTreeNode
        Public Route As List(Of Hop)
        Public ReadOnly TotalCost As Double

        Public Sub New(ByVal StartNode As Node)
            Route = New List(Of Hop)
            Route.Add(New Hop(StartNode, StartNode, Nothing))
            TotalCost = 0
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastHop As Hop)
            Route = OldTree.Route.ToList 'Cloned
            Route.Add(LastHop)
            TotalCost = OldTree.TotalCost + LastHop.GetCost
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastNodeWay As NodesAdjacencyListCell)
            Me.New(OldTree, New Hop(OldTree.Route(OldTree.Route.Count - 1).ToNode, LastNodeWay))
        End Sub

        Public Function GetCurrentNode() As Node
            Return Me.Route(Me.Route.Count - 1).ToNode
        End Function
    End Class
End Class
