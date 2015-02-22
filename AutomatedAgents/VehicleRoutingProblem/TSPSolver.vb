Public Class TSPSolver

    Private Map As StreetMap = Nothing
    Private Minimiser As RouteFindingMinimiser = Nothing

    Property RouteCost As Double = 0
    Property PointList As List(Of HopPosition)
    Property JobList As List(Of CourierJob)

    Sub New(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal VehicleCapacityLeft As Double, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser)
        Me.Map = Map
        Me.Minimiser = Minimiser
        RunAlgorithm(Start, Jobs, VehicleCapacityLeft)
    End Sub

    Private Sub RunAlgorithm(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal CapacityLeft As Double)
        Dim WayPoints As List(Of WayPoint) = WayPoint.CreateWayPointList(Jobs)
        Dim WayPointPermutations As List(Of List(Of WayPoint)) = GetAllPermutations(WayPoints)

        Dim BestRoute As List(Of WayPoint) = Nothing
        Dim BestRouteCost As Double = Double.MaxValue

        For Each Route As List(Of WayPoint) In WayPointPermutations
            'Ensure pickups -> dropoffs
            For i = 0 To Route.Count - 1
                Dim Pred As WayPoint = Route(i).Predecessor
                If Pred IsNot Nothing AndAlso Route.IndexOf(Pred) > i Then
                    Continue For
                End If
            Next

            'Ensure never overfull
            Dim CapacityRemaining As Double = CapacityLeft
            For Each WP As WayPoint In Route
                CapacityRemaining -= WP.VolumeDelta
                If CapacityRemaining < 0 Then
                    Continue For
                End If
            Next

            'Calculate routes now
            Dim AStarRoutes(Route.Count - 1) As Route
            Dim LastPoint As IPoint = Start
            For i = 0 To Route.Count - 1
                Dim IntermDict As Dictionary(Of IPoint, Route) = Nothing
                If Not RouteCache.TryGetValue(LastPoint, IntermDict) OrElse _
                    Not IntermDict.TryGetValue(Route(i).Position, AStarRoutes(i)) Then
                    AStarRoutes(i) = New AStarSearch(LastPoint, Route(i).Position, Map.NodesAdjacencyList, Minimiser).GetRoute
                    If Not RouteCache.ContainsKey(LastPoint) Then
                        RouteCache.Add(LastPoint, New Dictionary(Of IPoint, Route))
                    End If
                    RouteCache(LastPoint).Add(Route(i).Position, AStarRoutes(i))
                End If
                LastPoint = Route(i).Position
            Next

            'Ensure it meets all the deadlines
            Dim TimeAdded As TimeSpan = NoticeBoard.CurrentTime 'Safe/cloned
            For i = 0 To Route.Count - 2
                TimeAdded += AStarRoutes(i).GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                If Route(i).Job.Deadline < TimeAdded Then
                    Continue For
                End If
            Next
            'End waiting time is ignored, as it counts as on time even if < 2 min to spare.

            'Fuel TODO

            'Is it any better?
            Dim Cost As Double = 0
            Select Case Minimiser
                Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC
                    Cost = TimeAdded.TotalMilliseconds
                Case RouteFindingMinimiser.DISTANCE
                    For Each R As Route In AStarRoutes
                        Cost += R.GetKM
                    Next
            End Select
            If Cost < BestRouteCost Then
                BestRouteCost = Cost
                BestRoute = Route
            End If
        Next

        If BestRoute IsNot Nothing Then
            PointList = New List(Of HopPosition)(BestRoute.Count)
            RouteCost = BestRouteCost
            For Each WayPoint As WayPoint In BestRoute
                PointList.Add(WayPoint.Position)
            Next
        End If
    End Sub
End Class
