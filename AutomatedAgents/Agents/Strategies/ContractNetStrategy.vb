﻿Class ContractNetStrategy
    Implements IAgentStrategy

    Private Agent As Agent
    Private Contractor As ContractNetContractor
    Private Policy As ContractNetPolicy

    Public Sub New(ByVal Agent As Agent, ByVal Policy As ContractNetPolicy)
        Me.Agent = Agent
        Me.Policy = Policy
        Contractor = New ContractNetContractor(Agent, Policy)
        NoticeBoard.AvailableContractors.Add(Contractor)
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        Dim NewJob As CourierJob = Contractor.CollectJob
        If NewJob IsNot Nothing Then
            Select Case Policy
                Case ContractNetPolicy.CNP1 'TODO test 1-3 works with the idle strats
                    Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType, WayPoint.CreateWayPointList(NewJob))
                Case ContractNetPolicy.CNP2
                    Agent.Plan.WayPoints.AddRange(WayPoint.CreateWayPointList(NewJob))
                    Agent.Plan.RecreateRouteListFromWaypoints()
                Case ContractNetPolicy.CNP3, ContractNetPolicy.CNP4, ContractNetPolicy.CNP5
                    Agent.Plan = Contractor.Solver.GetPlan
                    'With CNP5, the agent may have been awarded a new, fresh job, but
                    'if it has been transferred a job from another agent, CollectJob
                    'would return null. Agent.Plan would have already been updated.
            End Select
            Agent.PickupPoints.Add(NewJob.PickupPosition)
        End If

        If Agent.Plan.IsIdle() Then
            Exit Sub
        End If

        'No need to recompute A* route to first waypoint.
        Agent.Plan.Update(False)

        If Agent.Plan.ReplanForTrafficConditions() AndAlso Policy = ContractNetPolicy.CNP5 Then
            Agent.Plan = CNP5Contingency()
        End If

        'If a route somewhere has just been completed...
        If Agent.Plan.RoutePosition.RouteCompleted Then
            If Agent.Plan.FirstWayPointReached() Then
                Dim Job As CourierJob = Agent.Plan.RemoveFirstWayPoint().Job
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
                            'CNP policy invariant
                            Agent.Plan.ExtractCancelled()
                            Agent.TotalCompletedJobs += 1 'Cancelled pick counts as completed job
                            SimulationState.NewEvent(Agent.AgentID, LogMessages.PickFail(Job.JobID))
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            'Successful pickup
                            Agent.Plan.CapacityLeft -= Job.CubicMetres
                            SimulationState.NewEvent(Agent.AgentID, LogMessages.PickSuccess(Job.JobID))
                        End If
                    Case JobStatus.PENDING_DELIVERY
                        Agent.Delayer = New Delayer(Job.Deliver())
                        If Job.Status = JobStatus.COMPLETED Then
                            'Successful dropoff (perhaps late)
                            Agent.TotalCompletedJobs += 1
                            Agent.Plan.CapacityLeft += Job.CubicMetres
                            Dim TimeLeft As TimeSpan = Job.Deadline - NoticeBoard.CurrentTime
                            If TimeLeft < TimeSpan.Zero Then
                                SimulationState.NewEvent(Agent.AgentID, LogMessages.DeliveryLate(Job.JobID, -TimeLeft, Job.OriginalCustomerFee))
                            Else
                                SimulationState.NewEvent(Agent.AgentID, LogMessages.DeliverySuccess(Job.JobID, TimeLeft))
                            End If
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            'Customer is not in -> deliver to nearest depot.
                            Job.DeliveryPosition = Agent.Map.GetNearestDepot(Agent.Plan.StartPoint)
                            Dim DepotWaypoint As New WayPoint() With {.DefinedStatus = JobStatus.PENDING_DELIVERY, _
                                          .Job = Job, .Position = Job.DeliveryPosition, _
                                          .VolumeDelta = -Job.CubicMetres}
                            Dim ImmediateRouteToDepot As Route = RouteCache.GetRoute(Agent.Plan.StartPoint, DepotWaypoint.Position)

                            Select Case Policy
                                Case ContractNetPolicy.CNP1, ContractNetPolicy.CNP2, ContractNetPolicy.CNP3
                                    'Go to the depot right now.
                                    Agent.Plan.WayPoints.Insert(0, DepotWaypoint)
                                    Agent.Plan.RecreateRouteListFromWaypoints() 'Routes(1) will have also changed!
                                Case ContractNetPolicy.CNP4
                                    'Go to the depot whenever it is optimal.
                                    Agent.Plan.WayPoints.Add(DepotWaypoint)
                                    Dim Solver As New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.MINIMISE_LATE_DELIVERIES), Agent.RouteFindingMinimiser, Agent.VehicleType)
                                    Agent.Plan = Solver.GetPlan
                                    Debug.Assert(Agent.Plan IsNot Nothing)
                                Case ContractNetPolicy.CNP5
                                    'Try to replan and avoid late deliveries.
                                    Agent.Plan.WayPoints.Add(DepotWaypoint)
                                    Dim Solver As New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.REDUNDANCY_TIME), Agent.RouteFindingMinimiser, Agent.VehicleType)
                                    Agent.Plan = If(Solver.GetPlan, CNP5Contingency())
                                    Debug.Assert(Agent.Plan IsNot Nothing)
                            End Select
                            SimulationState.NewEvent(Agent.AgentID, LogMessages.DeliveryFail(Job.JobID, ImmediateRouteToDepot.GetKM))
                        End If
                End Select
            End If

            If Agent.Plan.Routes.Count > 0 Then
                If Not PointsAreApproximatelyEqual(Agent.Plan.Routes(0).GetStartPoint, Agent.Plan.RoutePosition.GetPoint) Then
                    Agent.Plan.RecreateRouteListFromWaypoints()
                End If
                Agent.Plan.RoutePosition = New RoutePosition(Agent.Plan.Routes(0))
            End If
        End If
    End Sub

    Function CNP5Contingency() As CourierPlan
        Dim NecessaryJobs As New List(Of CourierJob)
        Dim RetractableJobs As New List(Of CourierJob)
        For Each Job As CourierJob In Agent.Plan.GetCurrentJobs
            If Job.Status = JobStatus.PENDING_PICKUP Then
                RetractableJobs.Add(Job)
            ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                NecessaryJobs.Add(Job)
            End If
        Next
        If RetractableJobs.Count > 0 Then
            'Order jobs by time left minus how long the route could take.
            RetractableJobs = RetractableJobs.OrderBy(Function(Job)
                                                          Return Job.Deadline - _
                                                              Job.GetDirectRoute.GetEstimatedTime(NoticeBoard.CurrentTime)
                                                      End Function).ToList
            SimulationState.NewEvent(Agent.AgentID, LogMessages.CNP5JobsSentForTransfer(RetractableJobs.Count))
            Dim JobsThatNoOtherAgentsCouldFulfil As List(Of CourierJob) = NoticeBoard.CNP5_ReallocateJobs(Contractor, RetractableJobs)
            SimulationState.NewEvent(Agent.AgentID, LogMessages.CNP5JobTransferResult( _
                  RetractableJobs.Count - JobsThatNoOtherAgentsCouldFulfil.Count, JobsThatNoOtherAgentsCouldFulfil.Count))
            Agent.Plan.WayPoints.Clear()
            Agent.Plan.WayPoints.AddRange(WayPoint.CreateWayPointList(NecessaryJobs))
            Agent.Plan.WayPoints.AddRange(WayPoint.CreateWayPointList(JobsThatNoOtherAgentsCouldFulfil))
        End If

        Dim Solver As New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.MINIMISE_LATE_DELIVERIES), Agent.RouteFindingMinimiser, Agent.VehicleType)
        Return Solver.GetPlan
    End Function

End Class
