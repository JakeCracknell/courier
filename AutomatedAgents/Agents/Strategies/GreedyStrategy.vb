Public Class GreedyStrategy
    Implements IAgentStrategy

    Private Const MAX_JOBS As Integer = 10
    Private Agent As Agent

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        'Get a job if there is room and one is available.
        If Agent.AssignedJobs.Count < MAX_JOBS Then
            Dim NewJob As CourierJob = NoticeBoard.GetJob
            If NewJob IsNot Nothing Then
                Agent.AssignedJobs.Add(NewJob)
            End If
        End If

        'Do nothing if there are no jobs allocated.
        If Agent.AssignedJobs.Count = 0 Then
            Exit Sub
        End If

        'If a route somewhere has just been completed...
        If Agent.Position.RouteCompleted Then
            If Agent.AssignedJobs(0).PickupPosition.ApproximatelyEquals(Agent.Position.GetRoutingPoint) Then
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetRoutingPoint, Agent.AssignedJobs(0).DeliveryPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
                Agent.AssignedJobs(0).Status = JobStatus.PENDING_DELIVERY
            ElseIf Agent.AssignedJobs(0).DeliveryPosition.ApproximatelyEquals(Agent.Position.GetRoutingPoint) Then
                Agent.AssignedJobs(0).Status = JobStatus.COMPLETED
                Agent.AssignedJobs.RemoveAt(0)
            Else
                'Set a course for the next job.
                MoveUpClosestJob(Agent.Position)
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetRoutingPoint, Agent.AssignedJobs(0).PickupPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
            End If
        End If
    End Sub

    Private Sub MoveUpClosestJob(ByVal Position As RoutePosition)
        If Agent.AssignedJobs.Count > 1 Then
            Dim Distances As New List(Of Integer)(Agent.AssignedJobs.Count - 1)
            For i = 0 To Agent.AssignedJobs.Count - 1
                Distances.Add(New AStarSearch(Position.GetRoutingPoint, Agent.AssignedJobs(i).PickupPosition, Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute.GetKM)
            Next

            Dim IndexOfBest As Integer = Distances.IndexOf(Distances.Min)
            Dim BestJob As CourierJob = Agent.AssignedJobs(IndexOfBest)
            Agent.AssignedJobs.RemoveAt(IndexOfBest)
            Agent.AssignedJobs.Insert(0, BestJob)
        End If
    End Sub
End Class
