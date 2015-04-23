Public Class RoundRobinStrategy
    Implements IAgentStrategy

    Private Agent As Agent
    Private Contractor As BasicContractor

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
        Contractor = New BasicContractor(Agent)
        NoticeBoard.Broadcaster.RegisterContractor(Contractor)
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        Dim NewJob As CourierJob = Contractor.CollectJob
        If NewJob IsNot Nothing Then
            Agent.Plan.Update(True)
            Dim Solver As New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.MINIMISE_LATE_DELIVERIES), Agent.RouteFindingMinimiser, Agent.VehicleType, NewJob)
            Agent.Plan = Solver.GetPlan
            Agent.PickupPoints.Add(NewJob.PickupPosition)
        End If

        If Agent.Plan.IsIdle() Then
            Exit Sub
        End If

        'No need to recompute A* route to first waypoint.
        Agent.Plan.Update(False)

        Agent.Plan.ReplanForTrafficConditions()

        'If a route somewhere has just been completed...
        If Agent.Plan.RoutePosition.RouteCompleted Then
            If Agent.Plan.FirstWayPointReached() Then
                Dim Job As CourierJob = Agent.Plan.RemoveFirstWayPoint().Job
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
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
                            Dim TimeLeft As TimeSpan = Job.Deadline - NoticeBoard.Time
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

                            'Go to the depot whenever it is optimal.
                            Agent.Plan.WayPoints.Add(DepotWaypoint)
                            Dim Solver As New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.MINIMISE_LATE_DELIVERIES), Agent.RouteFindingMinimiser, Agent.VehicleType)
                            Agent.Plan = Solver.GetPlan
                            Debug.Assert(Agent.Plan IsNot Nothing)
                                
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
                                                              Job.GetDirectRoute.GetEstimatedTime(NoticeBoard.Time)
                                                      End Function).ToList
            SimulationState.NewEvent(Agent.AgentID, LogMessages.CNP5JobsSentForTransfer(RetractableJobs.Count))
            Dim JobsThatNoOtherAgentsCouldFulfil As List(Of CourierJob) = NoticeBoard.Broadcaster.ReallocateJobs(Contractor, RetractableJobs)
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

