Public Class NearestNeighbourEuclidianStrategy
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

    Public Sub UpdatePosition(ByRef Position As RoutePosition) Implements IAgentStrategy.UpdatePosition

        'Do nothing if there are no jobs allocated.
        If Jobs.Count = 0 Then
            GetOneJob(Position)
            Exit Sub
        End If

        GetGoodJobs(Position)

        'If a route somewhere has just been completed...
        If Position.RouteCompleted Then
            If Position.GetRoutingPoint.ApproximatelyEquals(PlannedRoute(0)) Then
                Dim Job As CourierJob = PlannedJobRoute(0)
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Dim TimeToWait As Integer = Job.Collect()
                        If Job.Status = JobStatus.CANCELLED Then
                            PlannedRoute.RemoveAt(0)
                            PlannedJobRoute.RemoveAt(0)
                            Dim DeliveryIndex As Integer = PlannedJobRoute.IndexOf(Job)
                            PlannedJobRoute.RemoveAt(DeliveryIndex)
                            PlannedRoute.RemoveAt(DeliveryIndex)
                            Jobs.Remove(Job)
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            PlannedJobRoute.RemoveAt(0)
                            PlannedRoute.RemoveAt(0)
                        End If
                    Case JobStatus.PENDING_DELIVERY
                        Dim TimeToWait As Integer = Job.Deliver()
                        If Job.Status = JobStatus.COMPLETED Then
                            PlannedJobRoute.RemoveAt(0)
                            PlannedRoute.RemoveAt(0)
                            Jobs.Remove(Job)
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then 'Depot
                            PlannedRoute(0) = Job.DeliveryPosition
                            RecalculateRoute()
                        End If
                End Select

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
                Dim Cost As Double = GetDistance(Position.GetRoutingPoint, Job.PickupPosition)

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

        Dim CurrentRouteDistance As Double = New NearestNeighbourSolver(Position.GetRoutingPoint, Jobs).NNCost
        For i = UnallocatedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnallocatedJobs(i)
            Dim JobsToPlan As New List(Of CourierJob)(Jobs.Count)
            JobsToPlan.AddRange(Jobs)
            JobsToPlan.Add(Job)
            Dim NNS As New NearestNeighbourSolver(Position.GetRoutingPoint, JobsToPlan)
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

    Private Sub RecalculateRoute()
        'TODO
    End Sub
End Class
