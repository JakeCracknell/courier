Public Class LazyStrategy
    Implements IAgentStrategy
    Private Agent As Agent

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run

        'Do nothing if there are no jobs allocated.
        If Agent.AssignedJobs.Count = 0 Then
            GetBestJob(Agent.Position)
            Exit Sub
        End If

        'If a route somewhere has just been completed...
        If Agent.Position.RouteCompleted Then
            If Agent.AssignedJobs(0).DeliveryPosition.ApproximatelyEquals(Agent.Position.GetRoutingPoint) Then
                Agent.AssignedJobs(0).Status = JobStatus.COMPLETED
                Agent.AssignedJobs.RemoveAt(0)
            ElseIf Agent.AssignedJobs(0).PickupPosition.ApproximatelyEquals(Agent.Position.GetRoutingPoint) Then
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetRoutingPoint, Agent.AssignedJobs(0).DeliveryPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
                Agent.AssignedJobs(0).Status = JobStatus.PENDING_DELIVERY
            Else
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetRoutingPoint, Agent.AssignedJobs(0).PickupPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
            End If
        End If
    End Sub

    Sub GetBestJob(ByVal Position As RoutePosition)
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count > 0 Then
            Dim BestJob As CourierJob = Nothing
            Dim BestCost As Double = Double.MaxValue
            For Each Job As CourierJob In UnallocatedJobs
                Dim TotalCost As Double = New AStarSearch(Position.GetRoutingPoint, Job.PickupPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute.GetKM '+ GetDistance(Job.PickupPosition, Job.DeliveryPosition)

                If BestCost > TotalCost Then
                    BestCost = TotalCost
                    BestJob = Job
                End If
            Next

            NoticeBoard.AllocateJob(BestJob)
            Agent.AssignedJobs.Add(BestJob)
        End If
    End Sub
End Class
