
Class ContractNetStrategy
    Implements IAgentStrategy

    Private Agent As Agent
    Private Contractor As ContractNetContractor

    Private LastJobConsidered As CourierJob = Nothing

    Private PlannedRoute As New List(Of HopPosition)
    Private PlannedJobRoute As New List(Of CourierJob)

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
        Contractor = New ContractNetContractor(Agent)
        NoticeBoard.AvailableContractors.Add(Contractor)
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        Dim NewJob As CourierJob = Contractor.CollectJob
        If NewJob IsNot Nothing Then
            Agent.AssignedJobs.Add(NewJob)
            RecalculateRoute()
        End If

        If Agent.AssignedJobs.Count = 0 Then
            Exit Sub
        End If

        'If a route somewhere has just been completed...
        If Agent.Position.RouteCompleted Then
            If Agent.Position.GetRoutingPoint.ApproximatelyEquals(PlannedRoute(0)) Then
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
                            RecalculateRoute()
                        End If
                End Select

            End If

            If PlannedRoute.Count > 0 Then
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetRoutingPoint, PlannedRoute(0), Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
            End If
        End If
    End Sub

    Private Sub RecalculateRoute()
        Dim NNS As New NearestNeighbourSolver(Agent.Position.GetRoutingPoint, Agent.AssignedJobs, Agent.GetVehicleCapacityLeft)
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
