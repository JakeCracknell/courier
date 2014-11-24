Public Class AStarSearch
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
        DoAStar()
    End Sub

    Public Sub DoAStar()
        Dim PriorityQueue As New SortedList(Of Double, AStarTreeNode)
        Dim AlreadyVisited As New HashSet(Of Long)
        PriorityQueue.Add(0, New AStarTreeNode(SourceNode))
        Do
            Dim AStarTreeNode As AStarTreeNode = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If AStarTreeNode.GetCurrentNode = DestinationNode Then
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

                    Dim HeuristicCost As Double = GetDistance(Cell.Node, DestinationNode)
                    Dim F_Cost = HeuristicCost + NextAStarTreeNode.TotalCost
                    Do Until Not PriorityQueue.ContainsKey(F_Cost) 'Exception can occur otherwise
                        F_Cost += EPSILON
                    Loop
                    PriorityQueue.Add(F_Cost, NextAStarTreeNode)
                End If
            Next
        Loop Until PriorityQueue.Count = 0

        Debug.WriteLine("No route found - disconnected graph?")

    End Sub


    Public Function GetCost() As Double Implements RouteFinder.GetCost
        Return Cost
    End Function

    Public Function GetRoute() As List(Of Hop) Implements RouteFinder.GetRoute
        Return HopList
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
            Route = Hop.CloneList(OldTree.Route)
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
