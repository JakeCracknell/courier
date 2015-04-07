
Class ContractNetStrategy
    Implements IAgentStrategy

    Private Agent As Agent
    Private Contractor As ContractNetContractor

    Private LastJobConsidered As CourierJob = Nothing

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
        Contractor = New ContractNetContractor(Agent)
        NoticeBoard.AvailableContractors.Add(Contractor)
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        Dim NewJob As CourierJob = Contractor.CollectJob
        If NewJob IsNot Nothing Then
            Agent.Plan = Contractor.Solver.Solution
        End If

        If Agent.Plan.WayPoints.Count = 0 Then
            Exit Sub
        End If

        Agent.Plan.Update(False)

        'If a route somewhere has just been completed...
        If Agent.Plan.RoutePosition.RouteCompleted Then
            If Agent.Plan.FirstWayPointReached() Then
                Dim Job As CourierJob = Agent.Plan.RemoveFirstWayPoint().Job
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
                            Agent.Plan.ExtractCancelled()
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            Agent.Plan.CapacityLeft -= Job.CubicMetres
                        End If
                    Case JobStatus.PENDING_DELIVERY
                        Agent.Delayer = New Delayer(Job.Deliver())
                        If Job.Status = JobStatus.COMPLETED Then
                            Agent.TotalCompletedJobs += 1
                            Agent.Plan.CapacityLeft += Job.CubicMetres
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            'Fail -> Depot
                            Dim DepotWaypoint As New WayPoint() With {.DefinedStatus = JobStatus.PENDING_DELIVERY, _
                                                                      .Job = Job, .Position = Job.DeliveryPosition, _
                                                                      .VolumeDelta = -Job.CubicMetres}
                            Agent.Plan.WayPoints.Add(DepotWaypoint)
                            Dim Solver As New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.MINIMISE_LATE_DELIVERIES), Agent.RouteFindingMinimiser)
                            Agent.Plan = Solver.Solution 'can't do until contingency done
                            If Agent.Plan Is Nothing Then
                                Dim sss As New NNSearchSolver(Solver.OldPlan, New SolverPunctualityStrategy(SolverPunctualityStrategy.PStrategy.MINIMISE_LATE_DELIVERIES), Agent.RouteFindingMinimiser)

                            End If
                            Debug.Assert(Agent.Plan IsNot Nothing)
                        End If
                End Select

            End If

            If Agent.Plan.Routes.Count > 0 Then
                Agent.Plan.RoutePosition = New RoutePosition(Agent.Plan.Routes(0))
            End If
        End If
    End Sub
End Class
