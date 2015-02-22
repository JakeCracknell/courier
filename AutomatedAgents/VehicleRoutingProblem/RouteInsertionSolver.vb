Public Class RouteInsertionSolver
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
                    GoTo FailedPermutation
                End If
            Next
            'End waiting time is ignored, as it counts as on time even if < 2 min to spare.

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
    ''So so so so unverified
    'Public Sub Reevaluate(ByVal NewStart As HopPosition)
    '    If WayPointList Is Nothing Then
    '        Exit Sub
    '    End If

    '    'Trim any completed waypoints.
    '    Do
    '        If WayPointList(0).Job.Status <> WayPointList(0).DefinedStatus Then
    '            WayPointList.RemoveAt(0)
    '            PointList.RemoveAt(0)
    '            JobList.RemoveAt(0)
    '            Select Case Minimiser
    '                Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC 'ooh dear not exact
    '                    RouteCost = RouteCost - AStarList(0).GetRoute.GetEstimatedHours
    '                Case RouteFindingMinimiser.DISTANCE
    '                    RouteCost = RouteCost - AStarList(0).GetRoute.GetKM
    '            End Select
    '            AStarList.RemoveAt(0)
    '        Else
    '            Exit Do
    '        End If
    '    Loop Until WayPointList.Count = 0

    '    Select Case Minimiser
    '        Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC 'ooh dear not exact
    '            RouteCost = RouteCost - AStarList(0).GetRoute.GetEstimatedHours + New AStarSearch(NewStart, WayPointList(0).Position, Map.NodesAdjacencyList, Minimiser).GetRoute.GetEstimatedHours
    '        Case RouteFindingMinimiser.DISTANCE
    '            RouteCost = RouteCost - AStarList(0).GetRoute.GetKM + New AStarSearch(NewStart, WayPointList(0).Position, Map.NodesAdjacencyList, Minimiser).GetRoute.GetKM
    '    End Select
    'End Sub

    'Private Function ExtractWayPointsFromOldSolver(ByVal OldSolver As RouteInsertionSolver)
    '    If OldSolver Is Nothing OrElse OldSolver.WayPointList Is Nothing Then
    '        Return New List(Of WayPoint)
    '    End If

    '    'Trim any completed waypoints.
    '    Dim OldWayPoints As List(Of WayPoint) = OldSolver.WayPointList
    '    Do
    '        If OldWayPoints(0).Job.Status <> OldWayPoints(0).DefinedStatus Then
    '            OldWayPoints.RemoveAt(0)
    '        Else
    '            Exit Do
    '        End If
    '    Loop Until OldWayPoints.Count = 0

    '    Return OldWayPoints
    'End Function


End Class
