Public Class FreeForAllStrategy
    Inherits AgentStrategy

    Private HopefulJob As CourierJob = Nothing
    Private JobsToIgnore As New HashSet(Of CourierJob)

    Public Sub New(ByVal Agent As Agent)
        MyBase.New(Agent)
    End Sub

    Overrides Sub Run()
        CollectJobs()

        If Agent.Plan.IsIdle() Then
            Exit Sub
        End If

        Agent.Plan.Update(False)

        'If a route somewhere has just been completed...
        If RouteCompletion(ContractNetPolicy.CNP1) Then
            HopefulJob = Nothing
        End If
    End Sub

    Public Overrides Sub CollectJobs()
        If Agent.Plan.IsIdle() Then
            HopefulJob = FindBestJob()
            If HopefulJob IsNot Nothing Then
                HopefulJob.Status = JobStatus.PENDING_PICKUP
                Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, _
                                             Agent.GetVehicleCapacityLeft, Agent.VehicleType, WayPoint.CreateWayPointList(HopefulJob))
                Agent.PickupPoints.Add(HopefulJob.PickupPosition)
                SimulationState.NewEvent(Agent.AgentID, LogMessages.JobAwarded(HopefulJob.JobID, Agent.Plan.Routes(0).GetEstimatedHours(NoticeBoard.Time)))
            End If
        ElseIf HopefulJob IsNot Nothing Then
            If AGENT_KNOWS_WHEN_JOB_HAS_BEEN_PICKED AndAlso HopefulJob.Status <> JobStatus.PENDING_PICKUP Then
                'Too late!
                HopefulJob = Nothing
                Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, _
                                             Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType)
                Exit Sub
            End If
        End If
    End Sub

    Function FindBestJob() As CourierJob
        'Find best job based on the value of the job and whether the agent can perform it given its current resources.
        Dim BestJob As CourierJob = Nothing
        Dim BestValue As Double = Double.MinValue
        For Each JobToReview As CourierJob In NoticeBoard.UnallocatedJobs
            If JobToReview.CubicMetres > Agent.GetVehicleMaxCapacity OrElse _
                JobsToIgnore.Contains(JobToReview) Then
                Continue For
            End If

            Dim StartTime As TimeSpan = NoticeBoard.Time + Agent.Plan.GetDiversionTimeEstimate
            'Too much garbage otherwise
            Dim Route1 As Route = New AStarSearch(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition, _
                                                  Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser, StartTime).GetRoute
            Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(StartTime) + Customers.WaitTimeAvg
            Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition, StartTime + Route1Time)
            Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(StartTime + Route1Time)
            If StartTime + MinTime + SimulationParameters.DEADLINE_REDUNDANCY > JobToReview.Deadline Then
                JobsToIgnore.Add(JobToReview)
                Continue For
            End If
            Dim JobValue As Double = Route2.GetCostForAgent(Agent) / Route1.GetCostForAgent(Agent)
            If JobValue > BestValue Then
                BestJob = JobToReview
                BestValue = Route1.GetCostForAgent(Agent)
            End If
        Next
        Return BestJob
    End Function
End Class
