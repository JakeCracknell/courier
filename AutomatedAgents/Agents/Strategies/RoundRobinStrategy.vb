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
            Dim Planner As New NNGAPlanner(Agent, True, NewJob)
            Agent.Plan = Planner.GetPlan
            Agent.PickupPoints.Add(NewJob.PickupPosition)
        End If

        If Agent.Plan.IsIdle() Then
            Exit Sub
        End If

        'No need to recompute A* route to first waypoint.
        Agent.Plan.Update(False)

        'Periodically, check for changed traffic conditions that might warrant a replan.
        If SimulationParameters.PERIODIC_REPLAN AndAlso Agent.Plan.NeedToReplan() Then
            Dim NewPlan As CourierPlan = New NNGAPlanner(Agent, True).GetPlan
            If NewPlan.CostScore < Agent.Plan.CostScore Then
                Agent.Plan = NewPlan
            End If
        End If

        'If a route somewhere has just been completed...
        If Agent.Plan.RoutePosition.RouteCompleted Then
            If Agent.Plan.FirstWayPointReached() Then
                Dim Job As CourierJob = Agent.Plan.RemoveFirstWayPoint().Job
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
                            'CNP policy invariant - replan or just skip waypoint, whatever is cheaper.
                            'Partial refund cost saving
                            Dim OldCost As Double = Agent.Plan.CostScore
                            Agent.Plan.ExtractCancelled()
                            Dim TriangleInequalityCost As Double = Agent.Plan.CostScore
                            Dim Replan As CourierPlan = New NNGAPlanner(Agent, True).GetPlan
                            Dim ReplanCost As Double = Replan.CostScore
                            If ReplanCost < TriangleInequalityCost Then
                                Agent.Plan = Replan
                            End If
                            Dim CostSaving As Double = OldCost - Math.Min(ReplanCost, OldCost)
                            Job.PartialRefund(CostSaving)
                            Agent.TotalCompletedJobs += 1 'Cancelled pick counts as completed job
                            SimulationState.NewEvent(Agent.AgentID, LogMessages.PickFail(Job))
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
                            Dim DepotWaypoint As WayPoint = WayPoint.MakeFailedDeliveryWaypoint(Agent, Job)
                            Dim ImmediateRouteToDepot As Route = RouteCache.GetRoute(Agent.Plan.StartPoint, DepotWaypoint.Position)

                            'Go to the depot whenever it is optimal.
                            Agent.Plan.WayPoints.Add(DepotWaypoint)
                            Dim Planner As New NNGAPlanner(Agent, True)
                            Agent.Plan = Planner.GetPlan
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

End Class

