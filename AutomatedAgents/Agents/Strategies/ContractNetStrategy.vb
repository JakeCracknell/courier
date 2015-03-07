
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
            Agent.Plan = Contractor.Solver.GetPlan
        End If

        If Agent.Plan.WayPoints.Count = 0 Then
            Exit Sub
        End If

        Agent.Plan.Update(Agent.Position, False)

        'If a route somewhere has just been completed...
        If Agent.Position.RouteCompleted Then
            If Agent.Plan.FirstWayPointReached(Agent.Position) Then
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
                            Agent.Plan.CapacityLeft += Job.CubicMetres
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            'Fail -> Depot
                            'PlannedRoute(0) = Job.DeliveryPosition
                            'Agent.Plan.Replan()
                        End If
                End Select

            End If

            If Agent.Plan.Routes.Count > 0 Then
                Agent.Position = New RoutePosition(Agent.Plan.Routes(0))
            End If
        End If
    End Sub
End Class
