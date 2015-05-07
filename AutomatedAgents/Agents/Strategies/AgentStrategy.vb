Public MustInherit Class AgentStrategy
    Protected ReadOnly Agent As Agent
    Protected Const AGENT_KNOWS_WHEN_JOB_HAS_BEEN_PICKED As Boolean = False

    Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    MustOverride Sub Run()

    MustOverride Sub CollectJobs()

    Function RouteCompletion(ByVal CNPPolicy As ContractNetPolicy)
        If Agent.Plan.RoutePosition.RouteCompleted Then
            If Agent.Plan.FirstWayPointReached() Then
                Dim Waypoint As WayPoint = Agent.Plan.RemoveFirstWayPoint()
                Dim Job As CourierJob = Waypoint.Job
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
                        If Waypoint.DefinedStatus = JobStatus.PENDING_PICKUP Then
                            'Agent arrives at a pickup waypoint, but the job is pending delivery!
                            'It waits the maximum time, only to realise the job has been taken. (FFA)
                            If Not AGENT_KNOWS_WHEN_JOB_HAS_BEEN_PICKED Then
                                Agent.Delayer = New Delayer(Customers.WaitTimeMaxSeconds)
                            End If
                            Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType)
                            Return True
                        End If

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

                            Select Case CNPPolicy
                                Case ContractNetPolicy.CNP1, ContractNetPolicy.CNP2, ContractNetPolicy.CNP3
                                    'Go to the depot right now.
                                    Agent.Plan.WayPoints.Insert(0, DepotWaypoint)
                                    Agent.Plan.RecreateRouteListFromWaypoints() 'Routes(1) will have also changed!
                                Case ContractNetPolicy.CNP4
                                    'Go to the depot whenever it is optimal.
                                    Agent.Plan.WayPoints.Add(DepotWaypoint)
                                    Dim Planner As New NNGAPlanner(Agent, True)
                                    Agent.Plan = Planner.GetPlan
                                Case ContractNetPolicy.CNP5
                                    'Try to replan and avoid late deliveries.
                                    Agent.Plan.WayPoints.Add(DepotWaypoint)

                                    Dim Planner As New NNGAPlanner(Agent, True)
                                    Agent.Plan = If(Planner.IsSuccessful, Planner.GetPlan, ContractNetStrategy.CNP5Contingency(Agent))
                            End Select
                            SimulationState.NewEvent(Agent.AgentID, LogMessages.DeliveryFail(Job.JobID, _
                                           HaversineDistance(Agent.Plan.StartPoint, DepotWaypoint.Position)))
                        End If
                End Select
            End If

            If Agent.Plan.Routes.Count > 0 Then
                If Not PointsAreApproximatelyEqual(Agent.Plan.Routes(0).GetStartPoint, Agent.Plan.RoutePosition.GetPoint) Then
                    Agent.Plan.RecreateRouteListFromWaypoints()
                End If
                Agent.Plan.RoutePosition = New RoutePosition(Agent.Plan.Routes(0))
            End If
            Return True
        End If
        Return False
    End Function

    Sub PeriodicReplan(ByVal Policy As ContractNetPolicy)
        If SimulationParameters.PERIODIC_REPLAN AndAlso Agent.Plan.NeedToReplan() Then
            Dim NewPlan As CourierPlan = Nothing
            Select Case Policy
                Case ContractNetPolicy.CNP3
                    NewPlan = New CNP3Planner(Agent).GetPlan
                Case ContractNetPolicy.CNP4, ContractNetPolicy.CNP5
                    NewPlan = New NNGAPlanner(Agent, True).GetPlan
            End Select
            If NewPlan IsNot Nothing AndAlso NewPlan.CostScore < Agent.Plan.CostScore Then
                Agent.Plan = NewPlan
            End If
            If Policy = ContractNetPolicy.CNP5 AndAlso Agent.Plan.IsBehindSchedule Then
                Agent.Plan = ContractNetStrategy.CNP5Contingency(Agent)
            End If
        End If
    End Sub
End Class
