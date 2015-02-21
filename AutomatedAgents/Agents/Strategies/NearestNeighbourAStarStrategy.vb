Public Class NearestNeighbourAStarStrategy
    Implements IAgentStrategy
    Private Const MARGINAL_COST_ACCEPTANCE_COEFFICIENT As Double = 10
    Private Const MAX_JOB_ALLOCATION As Integer = 20

    Private LastJobConsidered As CourierJob = Nothing

    Private PlannedRoute As New List(Of HopPosition)
    Private PlannedJobRoute As New List(Of CourierJob)

    Private Agent As Agent

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run

        'Do nothing if there are no jobs allocated.
        If Agent.AssignedJobs.Count = 0 Then
            GetOneJob()
            Exit Sub
        End If

        If Agent.AssignedJobs.Count < MAX_JOB_ALLOCATION Then
            GetGoodJobs()
        End If

        'If a route somewhere has just been completed...
        If Agent.Position.RouteCompleted Then
            If Agent.Position.GetPoint.ApproximatelyEquals(PlannedRoute(0)) Then
                Dim Job As CourierJob = PlannedJobRoute(0)
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
                            PlannedRoute.RemoveAt(0)
                            PlannedJobRoute.RemoveAt(0)
                            Dim DeliveryIndex As Integer = PlannedJobRoute.IndexOf(Job)
                            PlannedJobRoute.RemoveAt(DeliveryIndex)
                            PlannedRoute.RemoveAt(DeliveryIndex)
                            Agent.AssignedJobs.Remove(Job)
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            Agent.VehicleCapacityUsed += Job.CubicMetres
                            PlannedJobRoute.RemoveAt(0)
                            PlannedRoute.RemoveAt(0)
                        End If
                    Case JobStatus.PENDING_DELIVERY
                        Agent.Delayer = New Delayer(Job.Deliver())
                        If Job.Status = JobStatus.COMPLETED Then
                            Agent.VehicleCapacityUsed -= Job.CubicMetres
                            PlannedJobRoute.RemoveAt(0)
                            PlannedRoute.RemoveAt(0)
                            Agent.AssignedJobs.Remove(Job)
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            'Fail -> Depot
                            PlannedRoute(0) = Job.DeliveryPosition
                            RecalculateRoute(Agent.Position)
                        End If
                End Select

            End If

            If PlannedRoute.Count > 0 Then
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetPoint, PlannedRoute(0), Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
            End If
        End If


    End Sub

    Sub GetOneJob()
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count > 0 Then
            Dim BestJob As CourierJob = Nothing
            Dim BestCost As Double = Double.MaxValue
            For Each Job As CourierJob In UnallocatedJobs
                'Cost of getting to pickup position determines pick
                'Ensure it can be delivered on time.
                Dim RouteToPickup As Route = New AStarSearch(Agent.Position.GetPoint, Job.PickupPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute
                Dim RouteToDropoff As Route = New AStarSearch(Job.PickupPosition, Job.DeliveryPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute

                Dim GetToPickupCost As Double = RouteToPickup.GetKM
                Dim Time As TimeSpan = TimeSpan.FromHours(RouteToPickup.GetEstimatedHours + RouteToDropoff.GetEstimatedHours) + _
                                        TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG)

                If BestCost > GetToPickupCost AndAlso Job.Deadline - (NoticeBoard.CurrentTime + Time) > DEADLINE_PLANNING_REDUNDANCY_TIME Then
                    BestCost = GetToPickupCost
                    BestJob = Job
                End If
            Next

            'Maybe there were unallocated jobs, but they are all too
            'far away to fulfil in time!
            If BestJob IsNot Nothing Then
                NoticeBoard.AllocateJob(BestJob)

                Agent.AssignedJobs.Add(BestJob)
                PlannedJobRoute.Add(BestJob)
                PlannedJobRoute.Add(BestJob)
                PlannedRoute.Add(BestJob.PickupPosition)
                PlannedRoute.Add(BestJob.DeliveryPosition)
                LastJobConsidered = Nothing
            End If
        End If
    End Sub

    Sub GetGoodJobs()
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count = 0 OrElse UnallocatedJobs.Last.Equals(LastJobConsidered) Then
            'Exit if no new jobs added to noticeboard
            Exit Sub
        End If

        Dim CurrentRouteDistance As Double = New NearestNeighbourSolver(Agent.Position.GetPoint, Agent.AssignedJobs, Agent.GetVehicleCapacityLeft, Agent.Map, Agent.RouteFindingMinimiser).NNCost
        For i = UnallocatedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnallocatedJobs(i)
            Dim JobsToPlan As New List(Of CourierJob)(Agent.AssignedJobs.Count)
            JobsToPlan.AddRange(Agent.AssignedJobs)
            JobsToPlan.Add(Job)

            Dim NNS As New NearestNeighbourSolver(Agent.Position.GetPoint, JobsToPlan, Agent.GetVehicleCapacityLeft, Agent.Map, Agent.RouteFindingMinimiser)
            If NNS.PointList IsNot Nothing Then
                'A route exists that conforms to deadlines and vehicle capacity
                Dim MarginalCost As Double = NNS.NNCost - CurrentRouteDistance
                If MarginalCost * MARGINAL_COST_ACCEPTANCE_COEFFICIENT < CurrentRouteDistance Then
                    'The route only marginally inconveniences the agent/current customers.
                    NoticeBoard.AllocateJob(Job)
                    Agent.AssignedJobs.Add(Job)
                    PlannedRoute = NNS.PointList
                    PlannedJobRoute = NNS.JobList
                End If
            End If
        Next

        LastJobConsidered = If(UnallocatedJobs.Count > 0, UnallocatedJobs.Last, Nothing)

    End Sub

    Private Sub RecalculateRoute(ByVal Position As RoutePosition)
        Dim NNS As New NearestNeighbourSolver(Position.GetPoint, Agent.AssignedJobs, Agent.GetVehicleCapacityLeft)
        If NNS.PointList IsNot Nothing Then
            PlannedRoute = NNS.PointList
            PlannedJobRoute = NNS.JobList
        Else
            'Shows that NNS is not very versatile. Happens when vehicle is behind schedule.
            'Only called when depot waypoint is added so just bump this back to the end.
            PlannedRoute.Add(PlannedRoute(0))
            PlannedRoute.RemoveAt(0)
            PlannedJobRoute.Add(PlannedJobRoute(0))
            PlannedJobRoute.RemoveAt(0)
        End If
    End Sub
End Class
