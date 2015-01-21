Public Class NearestNeighbourStrategy
    Implements AgentStrategy
    Private Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Private Const MARGINAL_COST_ACCEPTANCE_COEFFICIENT As Double = 10

    Private LastJobConsidered As CourierJob = Nothing

    Private Jobs As List(Of CourierJob)
    Private PlannedRoute As New List(Of HopPosition)
    Private PlannedJobRoute As New List(Of CourierJob)
    Private Map As StreetMap

    Public Sub New(ByVal _Map As StreetMap, ByVal _Jobs As List(Of CourierJob))
        Map = _Map
        Jobs = _Jobs
    End Sub

    Public Sub UpdatePosition(ByRef Position As RoutePosition) Implements AgentStrategy.UpdatePosition

        'Do nothing if there are no jobs allocated.
        If Jobs.Count = 0 Then
            GetOneJob(Position)
            Exit Sub
        End If

        GetGoodJobs(Position)

        'If a route somewhere has just been completed...
        If Position.RouteCompleted Then
            Select Case PlannedJobRoute(0).Status
                Case JobStatus.PENDING_PICKUP
                    PlannedJobRoute(0).Status = JobStatus.PENDING_DELIVERY
                Case JobStatus.PENDING_DELIVERY
                    PlannedJobRoute(0).Status = JobStatus.COMPLETED
                    Jobs.Remove(PlannedJobRoute(0))
            End Select

            PlannedJobRoute.RemoveAt(0)
            PlannedRoute.RemoveAt(0)

            If PlannedRoute.Count > 0 Then
                Position = New RoutePosition(New AStarSearch(Position.GetRoutingPoint, PlannedRoute(0), Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute)
            End If
        End If

      
    End Sub

    Sub GetOneJob(ByVal Position As RoutePosition)
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count > 0 Then
            Dim BestJob As CourierJob = Nothing
            Dim BestCost As Double = Double.MaxValue
            For Each Job As CourierJob In UnallocatedJobs
                Dim TotalCost As Double = New AStarSearch(Position.GetRoutingPoint, Job.PickupPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute.GetKM

                If BestCost > TotalCost Then
                    BestCost = TotalCost
                    BestJob = Job
                End If
            Next

            NoticeBoard.AllocateJob(BestJob)

            Jobs.Add(BestJob)
            PlannedJobRoute.Add(BestJob)
            PlannedJobRoute.Add(BestJob)
            PlannedRoute.Add(BestJob.PickupPosition)
            PlannedRoute.Add(BestJob.DeliveryPosition)
            LastJobConsidered = Nothing
        End If
    End Sub

    'Private Function CalculatePlannedRouteDistances(ByVal Route As List(Of HopPosition))
    '    If Route.Count >= 2 Then
    '        Dim RouteCosts As New List(Of Double)(Route.Count - 2)
    '        For i = 0 To Route.Count - 2
    '            RouteCosts(i) = New AStarSearch(Route(i), Route(i + 1), Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute.GetKM
    '        Next
    '        Return RouteCosts
    '    End If
    '    Return New List(Of Double)
    'End Function

    Sub GetGoodJobs(ByVal Position As RoutePosition)
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count = 0 OrElse UnallocatedJobs.Last.Equals(LastJobConsidered) Then
            'Exit if no new jobs added to noticeboard
            Exit Sub
        End If

        Dim CurrentRouteDistance As Double = New NearestNeighbourSolver(Position.GetRoutingPoint, Jobs, Map, RouteFindingMinimiser).NNCost
        Dim LowestMarginalCost As Double = Double.MaxValue
        Dim BestNewPlannedRoute As List(Of HopPosition) = Nothing
        Dim BestNewPlannedJobRoute As List(Of CourierJob) = Nothing
        Dim BestJobToAllocate As CourierJob = Nothing
        For Each Job As CourierJob In UnallocatedJobs
            Dim JobsToPlan As New List(Of CourierJob)(Jobs.Count)
            JobsToPlan.AddRange(Jobs)
            JobsToPlan.Add(Job)
            Dim NNS As New NearestNeighbourSolver(Position.GetRoutingPoint, JobsToPlan, Map, RouteFindingMinimiser)
            Dim MarginalCost As Double = NNS.NNCost - CurrentRouteDistance
            If LowestMarginalCost > MarginalCost Then
                LowestMarginalCost = MarginalCost
                BestNewPlannedRoute = NNS.PointList
                BestNewPlannedJobRoute = NNS.JobList
                BestJobToAllocate = Job
            End If

        Next

        If LowestMarginalCost * MARGINAL_COST_ACCEPTANCE_COEFFICIENT < CurrentRouteDistance Then
            NoticeBoard.AllocateJob(BestJobToAllocate)
            Jobs.Add(BestJobToAllocate)
            PlannedRoute = BestNewPlannedRoute
            'PlannedRouteCost = CurrentRouteDistance + LowestMarginalCost
            PlannedJobRoute = BestNewPlannedJobRoute
        Else
            'Used to prevent rechecking unless new jobs appear
            LastJobConsidered = UnallocatedJobs.Last
        End If
    End Sub
End Class
