Public Class FreeForAllStrategy
    Implements IAgentStrategy

    Private Agent As Agent
    Private HopefulJob As CourierJob = Nothing

    Private Const AGENT_KNOWS_WHEN_JOB_HAS_BEEN_PICKED As Boolean = False

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        If Agent.Plan.IsIdle() Then
            HopefulJob = FindBestJob()
            If HopefulJob IsNot Nothing Then
                HopefulJob.Status = JobStatus.PENDING_PICKUP
                Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType, WayPoint.CreateWayPointList(HopefulJob))
                Agent.PickupPoints.Add(HopefulJob.PickupPosition)
                SimulationState.NewEvent(Agent.AgentID, LogMessages.JobAwarded(HopefulJob.JobID, Agent.Plan.Routes(0).GetEstimatedHours(NoticeBoard.Time)))
            Else
                Exit Sub
            End If
        ElseIf HopefulJob IsNot Nothing Then
            If AGENT_KNOWS_WHEN_JOB_HAS_BEEN_PICKED AndAlso HopefulJob.Status <> JobStatus.PENDING_PICKUP Then
                'Too late!
                HopefulJob = Nothing
                Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType)
                Exit Sub
            End If
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
                        HopefulJob = Nothing
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
                            'CNP policy invariant - replan or just skip waypoint, whatever is cheaper.
                            'Partial refund cost saving
                            Dim OldCost As Double = Agent.Plan.CostScore
                            Agent.Plan.ExtractCancelled()
                            Dim TriangleInequalityCost As Double = Agent.Plan.CostScore
                            Dim Replan As CourierPlan = New NNGAPlanner(Agent).GetPlan
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
                        If HopefulJob IsNot Nothing Then
                            'Agent arrives and waits the maximum time, only to realise the job has been taken.
                            If Not AGENT_KNOWS_WHEN_JOB_HAS_BEEN_PICKED Then
                                Agent.Delayer = New Delayer(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                            End If
                            HopefulJob = Nothing
                            Agent.Plan = New CourierPlan(Agent.Plan.RoutePosition.GetPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType)
                            Exit Sub
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
                            Dim ImmediateRouteToDepot As Route = RouteCache.GetRoute(Agent.Plan.StartPoint, DepotWaypoint.Position)

                            'Go to the depot right now.
                            Agent.Plan.WayPoints.Insert(0, DepotWaypoint)
                            Agent.Plan.Routes.Insert(0, ImmediateRouteToDepot)

                            SimulationState.NewEvent(Agent.AgentID, LogMessages.DeliveryFail(Job.JobID, ImmediateRouteToDepot.GetKM))
                        End If
                End Select

            End If

            If Agent.Plan.Routes.Count > 0 Then
                Agent.Plan.RoutePosition = New RoutePosition(Agent.Plan.Routes(0))
            End If
        End If
    End Sub

    Function FindBestJob() As CourierJob
        'Find best job based on the value of the job and whether the agent can perform it given its current resources.

        'TODO: maybe Take(5) by haversine distance?
        Dim BestJob As CourierJob = Nothing
        Dim BestValue As Double = Double.MinValue
        For Each JobToReview As CourierJob In NoticeBoard.UnallocatedJobs
            If JobToReview.CubicMetres > Agent.GetVehicleMaxCapacity Then
                Continue For
            End If

            Dim StartTime As TimeSpan = NoticeBoard.Time + Agent.Plan.GetDiversionTimeEstimate
            Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition, StartTime)
            Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(StartTime) + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG)
            Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition, StartTime + Route1Time)
            Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(StartTime + Route1Time)
            If StartTime + MinTime + SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB > JobToReview.Deadline Then
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
