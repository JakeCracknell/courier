Class ContractNetStrategy
    Inherits AgentStrategy

    Private Contractor As ContractNetContractor
    Private Policy As ContractNetPolicy

    Public Sub New(ByVal Agent As Agent, ByVal Policy As ContractNetPolicy)
        MyBase.New(Agent)
        Me.Policy = Policy
        Contractor = New ContractNetContractor(Agent, Policy)
        If NoticeBoard.Broadcaster IsNot Nothing Then
            NoticeBoard.Broadcaster.RegisterContractor(Contractor)
        End If
    End Sub

    Overrides Sub Run()
        CollectJobs()

        If Agent.Plan.IsIdle() Then
            Exit Sub
        End If

        Agent.Plan.Update(False)

        'Periodically, check for changed traffic conditions that might warrant a replan.
        PeriodicReplan(Policy)

        'If a route somewhere has just been completed...
        RouteCompletion(Policy)
    End Sub

    Public Overrides Sub CollectJobs()
        Dim NewJob As CourierJob = Contractor.CollectJob
        If NewJob IsNot Nothing Then
            Select Case Policy
                Case ContractNetPolicy.CNP1
                    Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType, WayPoint.CreateWayPointList(NewJob))
                Case ContractNetPolicy.CNP2
                    Agent.Plan.WayPoints.AddRange(WayPoint.CreateWayPointList(NewJob))
                    Agent.Plan.RecreateRouteListFromWaypoints()
                Case ContractNetPolicy.CNP3, ContractNetPolicy.CNP4, ContractNetPolicy.CNP5
                    Agent.Plan = Contractor.Planner.GetPlan
                    'With CNP5, the agent may have been awarded a new, fresh job, but
                    'if it has been transferred a job from another agent, CollectJob
                    'would return null. Agent.Plan would have already been updated.
            End Select
            Agent.PickupPoints.Add(NewJob.PickupPosition)
        End If
    End Sub

    Public Shared Function CNP5Contingency(ByVal Agent As Agent) As CourierPlan
        Dim NecessaryJobs As New List(Of CourierJob)
        Dim ReallocatableJobs As New List(Of CourierJob)
        For Each Job As CourierJob In Agent.Plan.GetCurrentJobs
            If Job.Status = JobStatus.PENDING_PICKUP Then
                ReallocatableJobs.Add(Job)
            ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                NecessaryJobs.Add(Job)
            End If
        Next
        If ReallocatableJobs.Count > 0 Then
            'Order jobs by time left minus how long the original planned route could take (given current traffic).
            ReallocatableJobs = ReallocatableJobs.OrderBy(Function(Job)
                                                              Return Job.Deadline - _
                                                                  Job.GetDirectRoute.GetEstimatedTime(NoticeBoard.Time)
                                                          End Function).ToList
            SimulationState.NewEvent(Agent.AgentID, LogMessages.CNP5JobsSentForTransfer(ReallocatableJobs.Count))
            Dim JobsThatNoOtherAgentsCouldFulfil As List(Of CourierJob) = NoticeBoard.Broadcaster.ReallocateJobs(Agent.AgentID, ReallocatableJobs)
            SimulationState.NewEvent(Agent.AgentID, LogMessages.CNP5JobTransferResult( _
                  ReallocatableJobs.Count - JobsThatNoOtherAgentsCouldFulfil.Count, JobsThatNoOtherAgentsCouldFulfil.Count))
            Agent.Plan.WayPoints.Clear()
            Agent.Plan.WayPoints.AddRange(WayPoint.CreateWayPointList(NecessaryJobs))
            Agent.Plan.WayPoints.AddRange(WayPoint.CreateWayPointList(JobsThatNoOtherAgentsCouldFulfil))
            Agent.Plan.RecreateRouteListFromWaypoints()
        End If

        Dim Planner As New NNGAPlanner(Agent, True)
        Return Planner.GetPlan
    End Function

End Class
