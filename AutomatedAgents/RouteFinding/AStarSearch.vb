Public Class AStarSearch
    Implements IRouteFinder

    Private StartPoint As IPoint
    Private EndPoint As IPoint
    Private AdjacencyList As NodesAdjacencyList
    Private Route As Route
    Private NodesSearched As New List(Of Node)
    Private Minimiser As RouteFindingMinimiser
    Private StartTime As TimeSpan
    Private Const EPSILON As Double = 0.0000001

    Sub New(ByVal StartPoint As IPoint, ByVal EndPoint As IPoint, ByVal AdjacencyList As NodesAdjacencyList, _
            ByVal Minimiser As RouteFindingMinimiser)
        Me.New(StartPoint, EndPoint, AdjacencyList, Minimiser, NoticeBoard.CurrentTime)
    End Sub
    Sub New(ByVal StartPoint As IPoint, ByVal EndPoint As IPoint, ByVal AdjacencyList As NodesAdjacencyList, _
            ByVal Minimiser As RouteFindingMinimiser, ByVal StartTime As TimeSpan)
        Me.StartPoint = StartPoint
        Me.EndPoint = EndPoint
        Me.AdjacencyList = AdjacencyList
        Me.Minimiser = Minimiser
        Me.StartTime = StartTime
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
                    Exit Sub
                End If
            End If
            Run()
        Else
            'Routing from A to A. This is acceptable 
            Route = New Route(StartPoint)
        End If
    End Sub

    Private Sub Run()
        Dim PriorityQueue As New SortedList(Of Double, AStarTreeNode)
        Dim AlreadyVisitedNodes As New HashSet(Of Long)
        Dim BestDistancesToNodes As New Dictionary(Of Long, Double)
        PriorityQueue.Add(0, New AStarTreeNode(StartPoint, StartTime))
        Do
            Dim AStarTreeNode As AStarTreeNode = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If AStarTreeNode.GetCurrentPoint.Equals(EndPoint) Then
                Route = AStarTreeNode.GetRoute
                Route.StartTime = StartTime
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
                    NextAStarTreeNode.CalculateCostAndTime(Cell.Distance, Minimiser)

                    'Heuristic cost must not overestimate, must be admissible.
                    Dim HeuristicCost As Double
                    Dim StraightLineDistanceKM As Double = HaversineDistance(Cell.Node, EndPoint)
                    Select Case Minimiser
                        Case RouteFindingMinimiser.DISTANCE
                            HeuristicCost = StraightLineDistanceKM
                        Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC
                            HeuristicCost = StraightLineDistanceKM / SimulationParameters.MAX_POSSIBLE_SPEED_KMH
                        Case RouteFindingMinimiser.FUEL_WITH_TRAFFIC, RouteFindingMinimiser.FUEL_NO_TRAFFIC
                            HeuristicCost = Vehicles.OptimalFuelUsageAndTime(StraightLineDistanceKM, SimulationParameters.MAX_POSSIBLE_SPEED_KMH).Item1
                    End Select

                    HeuristicCost *= SimulationParameters.AStarAccelerator 'Not admissible if >1, but makes search faster

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

                End If
            Next

            If TypeOf EndPoint Is HopPosition Then
                Dim EndHopPosition As HopPosition = CType(EndPoint, HopPosition)
                If EndHopPosition.Hop.FromPoint.Equals(Row.NodeKey) OrElse (EndHopPosition.Hop.ToPoint.Equals(Row.NodeKey) AndAlso Not EndHopPosition.Hop.Way.OneWay) Then
                    Dim NextAStarTreeNode As New AStarTreeNode(AStarTreeNode, New Hop(Row.NodeKey, EndHopPosition, EndHopPosition.Hop.Way))
                    NextAStarTreeNode.CalculateCostAndTime(HaversineDistance(Row.NodeKey, EndHopPosition), Minimiser)

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
        Public WallTime As TimeSpan 'Only incremented for searches incorporating traffic

        Public Sub New(ByVal StartNode As IPoint, ByVal StartTime As TimeSpan)
            Hop = New Hop(StartNode, StartNode, Nothing)
            TotalCost = 0
            WallTime = StartTime
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastHop As Hop)
            Parent = OldTree
            Hop = LastHop
            TotalCost = OldTree.TotalCost
            WallTime = OldTree.WallTime
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastNodeWay As NodesAdjacencyListCell)
            Me.New(OldTree, New Hop(OldTree.Hop.ToPoint, LastNodeWay))
        End Sub

        Public Sub CalculateCostAndTime(ByVal Distance As Double, ByVal Minimiser As RouteFindingMinimiser)
            Select Case Minimiser
                Case RouteFindingMinimiser.DISTANCE
                    TotalCost += Distance
                Case RouteFindingMinimiser.TIME_NO_TRAFFIC
                    TotalCost += Hop.GetMinimumTravelTime
                Case RouteFindingMinimiser.TIME_WITH_TRAFFIC
                    TotalCost += Hop.GetEstimatedTravelTimeAtTime(WallTime)
                    WallTime += TimeSpan.FromHours(Hop.GetEstimatedTravelTimeAtTime(WallTime)) 'Includes delay
                Case RouteFindingMinimiser.FUEL_NO_TRAFFIC
                    TotalCost += Hop.GetOptimalFuelUsage()
                Case RouteFindingMinimiser.FUEL_WITH_TRAFFIC
                    TotalCost += Hop.GetOptimalFuelUsageAtTime(WallTime)
                    WallTime += TimeSpan.FromSeconds(Hop.GetEstimatedTravelTimeWithOptimalFuelUsage(WallTime) + GetAverageDelayLength(Hop, WallTime))
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