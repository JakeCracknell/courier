Public Class RouteInsertionSolver
    Private Map As StreetMap = Nothing
    Private Minimiser As RouteFindingMinimiser = Nothing
    Private WayPointList As List(Of WayPoint) = Nothing
    Private AStarList As List(Of AStarSearch) = Nothing

    Property RouteCost As Double = 0
    Property PointList As List(Of HopPosition)
    Property JobList As List(Of CourierJob)

    Sub New(ByVal Start As IPoint, ByVal OldSolver As RouteInsertionSolver, ByVal JobToInsert As CourierJob, ByVal VehicleCapacityLeft As Double, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser)
        Me.Map = Map
        Me.Minimiser = Minimiser
        RunAlgorithm(Start, ExtractWayPointsFromOldSolver(OldSolver), JobToInsert, VehicleCapacityLeft)
    End Sub

    Private Function ExtractWayPointsFromOldSolver(ByVal OldSolver As RouteInsertionSolver)
        If OldSolver Is Nothing OrElse OldSolver.WayPointList Is Nothing Then
            Return New List(Of WayPoint)
        End If

        'Trim any completed waypoints.
        Dim OldWayPoints As List(Of WayPoint) = OldSolver.WayPointList
        Do
            If OldWayPoints(0).Job.Status <> OldWayPoints(0).DefinedStatus Then
                OldWayPoints.RemoveAt(0)
            Else
                Exit Do
            End If
        Loop Until OldWayPoints.Count = 0

        Return OldWayPoints
    End Function

    Private Sub RunAlgorithm(ByVal Start As IPoint, ByVal CurrentRoute As List(Of WayPoint), ByVal JobToInsert As CourierJob, ByVal CapacityLeft As Double)
        Dim WayPoints As List(Of WayPoint) = WayPoint.CreateWayPointList(JobToInsert)
        Dim WayPointPermutations As List(Of List(Of WayPoint)) = GetAllListDoubleInsertions(CurrentRoute, WayPoints(0), WayPoints(1))

        Dim BestRoute As List(Of WayPoint) = Nothing
        Dim BestRouteCost As Double = Double.MaxValue

        For Each Route As List(Of WayPoint) In WayPointPermutations
            'Ensure never overfull
            Dim CapacityRemaining As Double = CapacityLeft
            For Each WP As WayPoint In Route
                CapacityRemaining -= WP.VolumeDelta
                If CapacityRemaining < 0 Then
                    GoTo FailedPermutation
                End If
            Next

            'Calculate routes now
            Dim AStarRoutes(Route.Count - 2) As AStarSearch
            Dim LastPoint As IPoint = Start
            For i = 0 To Route.Count - 2
                Dim IntermDict As Dictionary(Of IPoint, AStarSearch) = Nothing
                If Not RouteCache.TryGetValue(LastPoint, IntermDict) OrElse _
                    Not IntermDict.TryGetValue(Route(i).Position, AStarRoutes(i)) Then
                    AStarRoutes(i) = New AStarSearch(LastPoint, Route(i).Position, Map.NodesAdjacencyList, Minimiser)
                    If Not RouteCache.ContainsKey(LastPoint) Then
                        RouteCache.Add(LastPoint, New Dictionary(Of IPoint, AStarSearch))
                    End If
                    RouteCache(LastPoint).Add(Route(i).Position, AStarRoutes(i))
                End If
            Next

            'Ensure it meets all the deadlines
            Dim TimeAdded As TimeSpan = NoticeBoard.CurrentTime 'Safe/cloned
            For i = 0 To Route.Count - 2
                TimeAdded += AStarRoutes(i).GetRoute().GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
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
                    For Each AStar As AStarSearch In AStarRoutes
                        Cost += AStar.GetRoute.GetKM
                    Next
            End Select
            If Cost < BestRouteCost Then
                BestRouteCost = Cost
                BestRoute = Route
                AStarList = AStarRoutes.ToList
            End If

FailedPermutation:
        Next

        If BestRoute IsNot Nothing Then
            PointList = New List(Of HopPosition)(BestRoute.Count)
            JobList = New List(Of CourierJob)(BestRoute.Count)
            WayPointList = BestRoute
            RouteCost = BestRouteCost
            For Each WayPoint As WayPoint In BestRoute
                PointList.Add(WayPoint.Position)
                JobList.Add(WayPoint.Job)
            Next
        End If
    End Sub

    'So so so so unverified
    Public Sub Reevaluate(ByVal NewStart As HopPosition)
        If WayPointList Is Nothing Then
            Exit Sub
        End If

        'Trim any completed waypoints.
        Do
            If WayPointList(0).Job.Status <> WayPointList(0).DefinedStatus Then
                WayPointList.RemoveAt(0)
                PointList.RemoveAt(0)
                JobList.RemoveAt(0)
                Select Case Minimiser
                    Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC 'ooh dear not exact
                        RouteCost = RouteCost - AStarList(0).GetRoute.GetEstimatedHours
                    Case RouteFindingMinimiser.DISTANCE
                        RouteCost = RouteCost - AStarList(0).GetRoute.GetKM
                End Select
                AStarList.RemoveAt(0)
            Else
                Exit Do
            End If
        Loop Until WayPointList.Count = 0

        Select Minimiser
            Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC 'ooh dear not exact
                RouteCost = RouteCost - AStarList(0).GetRoute.GetEstimatedHours + New AStarSearch(NewStart, WayPointList(0).Position, Map.NodesAdjacencyList, Minimiser).GetRoute.GetEstimatedHours
            Case RouteFindingMinimiser.DISTANCE
                RouteCost = RouteCost - AStarList(0).GetRoute.GetKM + New AStarSearch(NewStart, WayPointList(0).Position, Map.NodesAdjacencyList, Minimiser).GetRoute.GetKM
        End Select
    End Sub

End Class
