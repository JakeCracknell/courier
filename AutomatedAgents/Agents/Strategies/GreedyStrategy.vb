Public Class GreedyStrategy
    Implements IAgentStrategy

    Private Const MAX_JOBS As Integer = 10
    Private Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Private Jobs As List(Of CourierJob)
    Private Map As StreetMap

    Public Sub New(ByVal _Map As StreetMap, ByVal _Jobs As List(Of CourierJob))
        Map = _Map
        Jobs = _Jobs
    End Sub

    Public Sub UpdatePosition(ByRef Position As RoutePosition) Implements IAgentStrategy.UpdatePosition
        'Get a job if there is room and one is available.
        If Jobs.Count < MAX_JOBS Then
            Dim NewJob As CourierJob = NoticeBoard.GetJob
            If NewJob IsNot Nothing Then
                Jobs.Add(NewJob)
            End If
        End If

        'Do nothing if there are no jobs allocated.
        If Jobs.Count = 0 Then
            Exit Sub
        End If

        'If a route somewhere has just been completed...
        If Position.RouteCompleted Then
            If Jobs(0).PickupPosition.ApproximatelyEquals(Position.GetRoutingPoint) Then
                Position = New RoutePosition(New AStarSearch(Position.GetRoutingPoint, Jobs(0).DeliveryPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute)
                Jobs(0).Status = JobStatus.PENDING_DELIVERY
            ElseIf Jobs(0).DeliveryPosition.ApproximatelyEquals(Position.GetRoutingPoint) Then
                Jobs(0).Status = JobStatus.COMPLETED
                Jobs.RemoveAt(0)
            Else
                'Set a course for the next job.
                MoveUpClosestJob(Position)
                Position = New RoutePosition(New AStarSearch(Position.GetRoutingPoint, Jobs(0).PickupPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute)
            End If
        End If
    End Sub

    Private Sub MoveUpClosestJob(ByVal Position As RoutePosition)
        If Jobs.Count > 1 Then
            Dim Distances As New List(Of Integer)(Jobs.Count - 1)
            For i = 0 To Jobs.Count - 1
                Distances.Add(New AStarSearch(Position.GetRoutingPoint, Jobs(i).PickupPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute.GetKM)
            Next

            Dim IndexOfBest As Integer = Distances.IndexOf(Distances.Min)
            Dim BestJob As CourierJob = Jobs(IndexOfBest)
            Jobs.RemoveAt(IndexOfBest)
            Jobs.Insert(0, BestJob)
        End If
    End Sub
End Class
