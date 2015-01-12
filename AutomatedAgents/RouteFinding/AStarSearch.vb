﻿Public Class AStarSearch
    Implements RouteFinder

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Private Route As Route
    Private NodesSearched As New List(Of Node)
    Private Minimiser As RouteFindingMinimiser
    Private Const EPSILON As Double = 0.0000001

    Sub New(ByVal SourceNode As Node, ByVal DestinationNode As Node, ByVal AdjacencyList As NodesAdjacencyList, ByVal Minimiser As RouteFindingMinimiser)
        Me.SourceNode = SourceNode
        Me.DestinationNode = DestinationNode
        Me.AdjacencyList = AdjacencyList
        Me.Minimiser = Minimiser
        If SourceNode <> DestinationNode Then
            DoAStar()
        End If
    End Sub

    Private Sub DoAStar()
        Dim PriorityQueue As New SortedList(Of Double, AStarTreeNode)
        Dim AlreadyVisitedNodes As New HashSet(Of Long)
        Dim BestDistancesToNodes As New Dictionary(Of Long, Double)
        PriorityQueue.Add(0, New AStarTreeNode(SourceNode))
        Do
            Dim AStarTreeNode As AStarTreeNode = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If AStarTreeNode.GetCurrentNode = DestinationNode Then
                Route = AStarTreeNode.GetRoute
                Exit Sub
            End If

            NodesSearched.Add(AStarTreeNode.GetCurrentNode)

            Dim Row As NodesAdjacencyListRow = AdjacencyList.Rows(AStarTreeNode.GetCurrentNode.ID)
            For Each Cell As NodesAdjacencyListCell In Row.Cells
                If Not AlreadyVisitedNodes.Contains(Cell.Node.ID) Then
                    Dim NextAStarTreeNode As New AStarTreeNode(AStarTreeNode, Cell)
                    NextAStarTreeNode.CalculateCost(Minimiser)

                    'Heuristic cost must not overestimate, must be admissible.
                    Dim HeuristicCost As Double
                    Select Case Minimiser
                        Case RouteFindingMinimiser.DISTANCE
                            HeuristicCost = GetDistance(Cell.Node, DestinationNode)
                        Case RouteFindingMinimiser.TIME_NO_TRAFFIC
                            HeuristicCost = GetDistance(Cell.Node, DestinationNode) / MAX_POSSIBLE_SPEED_KMH
                        Case RouteFindingMinimiser.TIME_WITH_TRAFFIC 'TODO
                            HeuristicCost = GetDistance(Cell.Node, DestinationNode) / MAX_POSSIBLE_SPEED_KMH
                    End Select

                    Dim F_Cost As Double = HeuristicCost + NextAStarTreeNode.TotalCost
                    Do Until Not PriorityQueue.ContainsKey(F_Cost) 'Exception can occur otherwise
                        F_Cost += EPSILON
                    Loop

                    If BestDistancesToNodes.ContainsKey(Cell.Node.ID) Then
                        Dim CompetingDistance As Double = BestDistancesToNodes(Cell.Node.ID)
                        If NextAStarTreeNode.TotalCost < CompetingDistance Then
                            BestDistancesToNodes(Cell.Node.ID) = NextAStarTreeNode.TotalCost
                            PriorityQueue.Add(F_Cost, NextAStarTreeNode)
                        End If
                    Else
                        BestDistancesToNodes.Add(Cell.Node.ID, F_Cost)
                        PriorityQueue.Add(F_Cost, NextAStarTreeNode)
                    End If

                    'TODO investigate if I am insane or if it is looking too far
                    'to the side of the map with a*. crete for example!


                End If
            Next
        Loop Until PriorityQueue.Count = 0

        Debug.WriteLine("No route found - disconnected graph?")

    End Sub


    Public Function GetRoute() As Route Implements RouteFinder.GetRoute
        Return Route
    End Function

    Public Function GetNodesSearched() As List(Of Node) Implements RouteFinder.GetNodesSearched
        Return NodesSearched
    End Function


    Private Class AStarTreeNode
        Public Parent As AStarTreeNode
        Public Hop As Hop
        Public TotalCost As Double

        Public Sub New(ByVal StartNode As Node)
            Hop = New Hop(StartNode, StartNode, Nothing)
            TotalCost = 0
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastHop As Hop)
            Parent = OldTree
            Hop = LastHop
            TotalCost = OldTree.TotalCost
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastNodeWay As NodesAdjacencyListCell)
            Me.New(OldTree, New Hop(OldTree.Hop.ToNode, LastNodeWay))
        End Sub

        Public Sub CalculateCost(ByVal Minimiser As RouteFindingMinimiser)
            Select Case Minimiser
                Case RouteFindingMinimiser.DISTANCE
                    TotalCost += Hop.GetCost
                Case RouteFindingMinimiser.TIME_NO_TRAFFIC
                    TotalCost += Hop.GetEstimatedTravelTime
                Case RouteFindingMinimiser.TIME_WITH_TRAFFIC 'TODO
                    TotalCost += Hop.GetEstimatedTravelTime
            End Select
        End Sub

        Public Function GetCurrentNode() As Node
            Return Hop.ToNode
        End Function

        Public Function GetRoute() As Route
            Dim Hops As New List(Of Hop)
            Dim CurrentAStarNode As AStarTreeNode = Me
            Do
                Hops.Insert(0, CurrentAStarNode.Hop)
                CurrentAStarNode = CurrentAStarNode.Parent
            Loop Until CurrentAStarNode Is Nothing
            Return New Route(Hops)
        End Function
    End Class
End Class