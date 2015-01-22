Public Class LazyStrategy
    Implements IAgentStrategy
    Private Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Private Jobs As List(Of CourierJob)
    Private Map As StreetMap

    Public Sub New(ByVal _Map As StreetMap, ByVal _Jobs As List(Of CourierJob))
        Map = _Map
        Jobs = _Jobs
    End Sub

    Public Sub UpdatePosition(ByRef Position As RoutePosition) Implements IAgentStrategy.UpdatePosition

        'Do nothing if there are no jobs allocated.
        If Jobs.Count = 0 Then
            GetBestJob(Position)
            Exit Sub
        End If

        'If a route somewhere has just been completed...
        If Position.RouteCompleted Then
            If Jobs(0).DeliveryPosition.ApproximatelyEquals(Position.GetRoutingPoint) Then
                Jobs(0).Status = JobStatus.COMPLETED
                Jobs.RemoveAt(0)
            ElseIf Jobs(0).PickupPosition.ApproximatelyEquals(Position.GetRoutingPoint) Then
                Position = New RoutePosition(New AStarSearch(Position.GetRoutingPoint, Jobs(0).DeliveryPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute)
                Jobs(0).Status = JobStatus.PENDING_DELIVERY
            Else
                Position = New RoutePosition(New AStarSearch(Position.GetRoutingPoint, Jobs(0).PickupPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute)
            End If
        End If
    End Sub

    Sub GetBestJob(ByVal Position As RoutePosition)
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count > 0 Then
            Dim BestJob As CourierJob = Nothing
            Dim BestCost As Double = Double.MaxValue
            For Each Job As CourierJob In UnallocatedJobs
                Dim TotalCost As Double = New AStarSearch(Position.GetRoutingPoint, Job.PickupPosition, Map.NodesAdjacencyList, RouteFindingMinimiser).GetRoute.GetKM '+ GetDistance(Job.PickupPosition, Job.DeliveryPosition)

                If BestCost > TotalCost Then
                    BestCost = TotalCost
                    BestJob = Job
                End If
            Next

            NoticeBoard.AllocateJob(BestJob)
            Jobs.Add(BestJob)
        End If
    End Sub
End Class
