Public Class NearestNeighbourSolver
    Class WayPoint
        Public Predecessor As WayPoint
        Public Position As RoutingPoint
        Public Job As CourierJob
    End Class

    Property NNCost As Double = 0
    Property PointList As List(Of HopPosition)
    Property JobList As List(Of CourierJob)

    Sub New(ByVal Start As RoutingPoint, ByVal Jobs As List(Of CourierJob), ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser)
        Dim WayPoints As New List(Of WayPoint)
        For Each Job In Jobs
            Select Case Job.Status
                Case JobStatus.PENDING_PICKUP, JobStatus.UNALLOCATED
                    Dim WayPointColl As New WayPoint With {.Position = Job.PickupPosition, _
                                                                                          .Job = Job}
                    Dim WayPointDel As New WayPoint With {.Position = Job.DeliveryPosition, _
                                                                                        .Predecessor = WayPointColl, _
                                                                                        .Job = Job}
                    WayPoints.Add(WayPointColl)
                    WayPoints.Add(WayPointDel)
                Case JobStatus.PENDING_DELIVERY
                    Dim WayPointDel As New WayPoint With {.Position = Job.DeliveryPosition, _
                                                                                        .Job = Job}
                    WayPoints.Add(WayPointDel)
            End Select
        Next


        Dim ReorderedWayPoints As New List(Of WayPoint)
        Dim LastPoint As RoutingPoint = Start

        Do Until WayPoints.Count = 0
            Dim ClosestNeighbour As WayPoint = WayPoints(0)
            Dim ClosestCost As Double = Double.MaxValue
            For Each WP As WayPoint In WayPoints
                If WP.Predecessor Is Nothing OrElse ReorderedWayPoints.Contains(WP.Predecessor) Then
                    Dim Cost As Double = New AStarSearch(LastPoint, WP.Position, Map.NodesAdjacencyList, Minimiser).GetRoute.GetKM
                    If ClosestCost > Cost Then
                        ClosestCost = Cost
                        ClosestNeighbour = WP
                    End If
                End If
            Next
            WayPoints.Remove(ClosestNeighbour)
            ReorderedWayPoints.Add(ClosestNeighbour)
            LastPoint = ClosestNeighbour.Position
            NNCost += ClosestCost
        Loop

        PointList = New List(Of HopPosition)(ReorderedWayPoints.Count - 1)
        JobList = New List(Of CourierJob)(ReorderedWayPoints.Count - 1)
        For Each WP As WayPoint In ReorderedWayPoints
            PointList.Add(WP.Position)
            JobList.Add(WP.Job)
        Next


    End Sub

End Class
