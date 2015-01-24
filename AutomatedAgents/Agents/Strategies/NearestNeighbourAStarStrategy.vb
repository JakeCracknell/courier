Public Class NearestNeighbourAStarStrategy
    Implements IAgentStrategy
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

    Public Sub Run(ByRef Position As RoutePosition, ByRef Delayer As Delayer) Implements IAgentStrategy.Run

        'Do nothing if there are no jobs allocated.
        If Jobs.Count = 0 Then
            GetOneJob(Position)
            Exit Sub
        End If

        GetGoodJobs(Position)

        'If a route somewhere has just been completed...
        If Position.RouteCompleted Then
            If Position.GetRoutingPoint.ApproximatelyEquals(PlannedRoute(0)) Then
                Select Case PlannedJobRoute(0).Status
                    Case JobStatus.PENDING_PICKUP
                        PlannedJobRoute(0).Status = JobStatus.PENDING_DELIVERY
                    Case JobStatus.PENDING_DELIVERY
                        PlannedJobRoute(0).Status = JobStatus.COMPLETED
                        Jobs.Remove(PlannedJobRoute(0))
                End Select

                PlannedJobRoute.RemoveAt(0)
                PlannedRoute.RemoveAt(0)
            End If

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
                'Cost of getting to pickup position. no longer looks at dropoff
                Dim Cost As Double = New AStarSearch(Position.GetRoutingPoint, Job.PickupPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute.GetKM

                If BestCost > Cost Then
                    BestCost = Cost
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

    Sub GetGoodJobs(ByVal Position As RoutePosition)
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count = 0 OrElse UnallocatedJobs.Last.Equals(LastJobConsidered) Then
            'Exit if no new jobs added to noticeboard
            Exit Sub
        End If

        Dim CurrentRouteDistance As Double = New NearestNeighbourSolver(Position.GetRoutingPoint, Jobs, Map, RouteFindingMinimiser).NNCost
        For i = UnallocatedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnallocatedJobs(i)
            Dim JobsToPlan As New List(Of CourierJob)(Jobs.Count)
            JobsToPlan.AddRange(Jobs)
            JobsToPlan.Add(Job)
            Dim NNS As New NearestNeighbourSolver(Position.GetRoutingPoint, JobsToPlan, Map, RouteFindingMinimiser)
            Dim MarginalCost As Double = NNS.NNCost - CurrentRouteDistance

            If MarginalCost * MARGINAL_COST_ACCEPTANCE_COEFFICIENT < CurrentRouteDistance Then
                NoticeBoard.AllocateJob(Job)
                Jobs.Add(Job)
                PlannedRoute = NNS.PointList
                PlannedJobRoute = NNS.JobList
            End If

        Next

        LastJobConsidered = If(UnallocatedJobs.Count > 0, UnallocatedJobs.Last, Nothing)

    End Sub
End Class
