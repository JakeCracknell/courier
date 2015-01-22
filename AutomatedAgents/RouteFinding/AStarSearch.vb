Public Class AStarSearch
    Implements IRouteFinder

    Private StartPoint As IPoint
    Private EndPoint As IPoint
    Private AdjacencyList As NodesAdjacencyList
    Private Route As Route
    Private NodesSearched As New List(Of Node)
    Private Minimiser As RouteFindingMinimiser
    Private Const EPSILON As Double = 0.0000001

    Sub New(ByVal StartPoint As IPoint, ByVal EndPoint As IPoint, ByVal AdjacencyList As NodesAdjacencyList, ByVal Minimiser As RouteFindingMinimiser)
        Me.StartPoint = StartPoint
        Me.EndPoint = EndPoint
        Me.AdjacencyList = AdjacencyList
        Me.Minimiser = Minimiser
        If Not StartPoint.Equals(EndPoint) Then
            If TypeOf StartPoint Is HopPosition AndAlso TypeOf EndPoint Is HopPosition Then
                'Handle case where destination on same hop. o/w would have to route to end of hop and come back.
                Dim HP1 As HopPosition = StartPoint
                Dim HP2 As HopPosition = EndPoint

                'Note DO NOT compare on Way, as they are long.
                If HP1.IsOnSameHop(HP2) Then
                    Dim OneHopList As New List(Of Hop)(1)
                    OneHopList.Add(New Hop(HP1, HP2, HP1.Hop.Way))
                    Route = New Route(OneHopList)
                    Return
                End If
            End If
            DoAStar()
        Else
            'Routing from A to A.
            Console.Beep()
        End If

        'WHAT HAPPENS IF I ALLOW 0 LENGTH ROUTES? IN THE SAME WYA I ALLOW HP1->HP2?
    End Sub

    Private Sub DoAStar()
        Dim PriorityQueue As New SortedList(Of Double, AStarTreeNode)
        Dim AlreadyVisitedNodes As New HashSet(Of Long)
        Dim BestDistancesToNodes As New Dictionary(Of Long, Double)
        PriorityQueue.Add(0, New AStarTreeNode(StartPoint))
        Do
            Dim AStarTreeNode As AStarTreeNode = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If AStarTreeNode.GetCurrentPoint.Equals(EndPoint) Then
                Route = AStarTreeNode.GetRoute
                Exit Sub
            End If

            Dim CurrentPoint As IPoint = AStarTreeNode.GetCurrentPoint
            If TypeOf CurrentPoint Is Node Then
                NodesSearched.Add(CurrentPoint)
            End If



            Dim Row As NodesAdjacencyListRow = AdjacencyList.GetRow(CurrentPoint)
            For Each Cell As NodesAdjacencyListCell In Row.Cells
                If Not AlreadyVisitedNodes.Contains(Cell.Node.ID) Then
                    Dim NextAStarTreeNode As New AStarTreeNode(AStarTreeNode, Cell)
                    NextAStarTreeNode.CalculateCost(Minimiser)

                    'Heuristic cost must not overestimate, must be admissible.
                    Dim HeuristicCost As Double
                    Select Case Minimiser
                        Case RouteFindingMinimiser.DISTANCE
                            HeuristicCost = GetDistance(Cell.Node, EndPoint)
                        Case RouteFindingMinimiser.TIME_NO_TRAFFIC
                            HeuristicCost = GetDistance(Cell.Node, EndPoint) / MAX_POSSIBLE_SPEED_KMH
                        Case RouteFindingMinimiser.TIME_WITH_TRAFFIC 'TODO
                            HeuristicCost = GetDistance(Cell.Node, EndPoint) / MAX_POSSIBLE_SPEED_KMH
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

            If TypeOf EndPoint Is HopPosition Then
                Dim EndHopPosition As HopPosition = CType(EndPoint, HopPosition)
                If EndHopPosition.Hop.FromPoint.Equals(Row.NodeKey) OrElse (EndHopPosition.Hop.ToPoint.Equals(Row.NodeKey) AndAlso Not EndHopPosition.Hop.Way.OneWay) Then
                    Dim NextAStarTreeNode As New AStarTreeNode(AStarTreeNode, New Hop(Row.NodeKey, EndHopPosition, EndHopPosition.Hop.Way))
                    NextAStarTreeNode.CalculateCost(Minimiser)

                    '0 heuristic cost.
                    Dim F_Cost As Double = 0 + NextAStarTreeNode.TotalCost
                    Do Until Not PriorityQueue.ContainsKey(F_Cost) 'Exception can occur otherwise
                        F_Cost += EPSILON
                    Loop
                    PriorityQueue.Add(F_Cost, NextAStarTreeNode)
                End If
            End If

        Loop Until PriorityQueue.Count = 0

        Debug.WriteLine("No route found - disconnected graph?")

    End Sub


    Public Function GetRoute() As Route Implements IRouteFinder.GetRoute
        Return Route
    End Function

    Public Function GetNodesSearched() As List(Of Node) Implements IRouteFinder.GetNodesSearched
        Return NodesSearched
    End Function


    Private Class AStarTreeNode
        Public Parent As AStarTreeNode
        Public Hop As Hop
        Public TotalCost As Double

        Public Sub New(ByVal StartNode As IPoint)
            Hop = New Hop(StartNode, StartNode, Nothing)
            TotalCost = 0
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastHop As Hop)
            Parent = OldTree
            Hop = LastHop
            TotalCost = OldTree.TotalCost
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastNodeWay As NodesAdjacencyListCell)
            Me.New(OldTree, New Hop(OldTree.Hop.ToPoint, LastNodeWay))
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

        Public Function GetCurrentPoint() As IPoint
            Return Hop.ToPoint
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