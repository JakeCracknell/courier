Public Class RouteInsertionSolver
    Private Const SECONDS_PER_KM_STRAIGHTLINE As Double = 55.987558 '40 mph

    Private Map As StreetMap = Nothing
    Private Minimiser As RouteFindingMinimiser = Nothing
    Private WayPointList As List(Of WayPoint) = Nothing
    Private RouteList As List(Of Route) = Nothing

    Private Start As HopPosition
    Private InitialCapacityLeft As Double
    Private InitialWayPoints As List(Of WayPoint)

    Property RouteCost As Double = 0

    Sub New(ByVal CourierPlan As CourierPlan, ByVal JobToInsert As CourierJob)
        Me.Map = CourierPlan.Map
        Me.Minimiser = CourierPlan.Minimiser
        Me.Start = CourierPlan.StartPoint
        Me.InitialCapacityLeft = CourierPlan.CapacityLeft
        Me.InitialWayPoints = CourierPlan.WayPoints
        InsertJob(JobToInsert)
    End Sub
    Sub New(ByVal CourierPlan As CourierPlan, ByVal WayPointToInsert As WayPoint)
        Me.Map = CourierPlan.Map
        Me.Minimiser = CourierPlan.Minimiser
        Me.Start = CourierPlan.StartPoint
        Me.InitialCapacityLeft = CourierPlan.CapacityLeft
        Me.InitialWayPoints = CourierPlan.WayPoints
        InsertWayPoint(WayPointToInsert)
    End Sub

    Private Sub InsertJob(ByVal JobToInsert As CourierJob)
        Dim WayPoints As List(Of WayPoint) = WayPoint.CreateWayPointList(JobToInsert)
        Dim WayPointPermutations As List(Of List(Of WayPoint)) = GetAllListDoubleInsertions(InitialWayPoints, WayPoints(0), WayPoints(1))
        TestPermutations(WayPointPermutations)
    End Sub
    Private Sub InsertWayPoint(ByVal WayPointToInsert As WayPoint)
        Dim WayPointPermutations As List(Of List(Of WayPoint)) = GetAllListSingleInsertions(InitialWayPoints, WayPointToInsert)
        TestPermutations(WayPointPermutations)
    End Sub

    Private Sub TestPermutations(ByVal WayPointPermutations As List(Of List(Of WayPoint)))
        Dim BestRoute As List(Of WayPoint) = Nothing
        Dim BestRouteCost As Double = Double.MaxValue

        For Each Route As List(Of WayPoint) In WayPointPermutations
            'Ensure never overfull
            Dim CapacityRemaining As Double = InitialCapacityLeft
            For Each WP As WayPoint In Route
                CapacityRemaining -= WP.VolumeDelta
                If CapacityRemaining < 0 Then
                    GoTo FailedPermutation
                End If
            Next

            'Retrieve pre-computed routes
            Dim AStarRoutes(Route.Count - 1) As Route
            Dim LastPoint As IPoint = Start
            For i = 0 To Route.Count - 1
                AStarRoutes(i) = RouteCache.GetRouteIfCached(LastPoint, Route(i).Position)
                LastPoint = Route(i).Position
            Next

            'Ensure it meets all the deadlines - first estimate
            Dim TimeAdded As TimeSpan = NoticeBoard.CurrentTime 'Safe/cloned
            For i = 0 To Route.Count - 1
                If AStarRoutes(i) IsNot Nothing Then
                    TimeAdded += AStarRoutes(i).GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                Else
                    LastPoint = If(i = 0, Start, Route(i - 1).Position)
                    TimeAdded += TimeSpan.FromSeconds(HaversineDistance(LastPoint, Route(i).Position) * SECONDS_PER_KM_STRAIGHTLINE + CourierJob.CUSTOMER_WAIT_TIME_MAX)
                End If
                If Route(i).Job.Deadline < TimeAdded Then
                    GoTo FailedPermutation
                End If
            Next
            'End waiting time is ignored, as it counts as on time even if < 2 min to spare.

            'Actually calculate new A* route
            For i = 0 To AStarRoutes.Count - 1
                If AStarRoutes(i) Is Nothing Then
                    LastPoint = If(i = 0, Start, Route(i - 1).Position)
                    AStarRoutes(i) = RouteCache.GetRoute(LastPoint, Route(i).Position)
                    If AStarRoutes(i) Is Nothing Then
                        GoTo FailedPermutation
                    End If
                End If
            Next

            'Recompute time - properly this time!
            TimeAdded = NoticeBoard.CurrentTime 'Safe/cloned
            For i = 0 To Route.Count - 1
                TimeAdded += AStarRoutes(i).GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                If Route(i).Job.Deadline < TimeAdded Then
                    GoTo FailedPermutation
                End If
            Next

            'Fuel TODO

            'Is it any better?
            Dim Cost As Double = 0
            Select Case Minimiser
                Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC
                    Cost = TimeAdded.TotalHours
                Case RouteFindingMinimiser.DISTANCE
                    For Each R As Route In AStarRoutes
                        Cost += R.GetKM
                    Next
            End Select
            If Cost < BestRouteCost Then
                BestRouteCost = Cost
                BestRoute = Route
                RouteList = AStarRoutes.ToList
            End If

FailedPermutation:
        Next

        WayPointList = BestRoute
        RouteCost = BestRouteCost
    End Sub

    Function GetPlan() As CourierPlan
        If WayPointList IsNot Nothing Then 'Impossible to solve
            Return New CourierPlan(Start, Map, Minimiser, InitialCapacityLeft, WayPointList, RouteList)
        Else
            Return Nothing
        End If
    End Function

End Class
