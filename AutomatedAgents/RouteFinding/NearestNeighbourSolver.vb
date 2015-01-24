Public Class NearestNeighbourSolver
    Class WayPoint
        Public Predecessor As WayPoint
        Public Position As IPoint
        Public Job As CourierJob
        Public VolumeDelta As Double
    End Class

    Private Map As StreetMap = Nothing
    Private Minimiser As RouteFindingMinimiser = Nothing

    Property NNCost As Double = 0
    Property PointList As List(Of HopPosition)
    Property JobList As List(Of CourierJob)

    'Use this constructor to use straight line distance. Up to 100x faster than AStar.
    Sub New(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal VehicleCapacityLeft As Double)
        RunAlgorithm(Start, Jobs, VehicleCapacityLeft)
    End Sub

    'Use this constructor to run AStar on each pair. More optimal.
    Sub New(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal VehicleCapacityLeft As Double, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser)
        Me.Map = Map
        Me.Minimiser = Minimiser
        RunAlgorithm(Start, Jobs, VehicleCapacityLeft)
    End Sub

    Sub RunAlgorithm(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal CapacityLeft As Double)
        Dim WayPoints As New List(Of WayPoint)
        For Each Job In Jobs
            Select Case Job.Status
                Case JobStatus.PENDING_PICKUP, JobStatus.UNALLOCATED
                    Dim WayPointColl As New WayPoint With {.Position = Job.PickupPosition, _
                                                          .Job = Job, _
                                                          .VolumeDelta = Job.CubicMetres}
                    Dim WayPointDel As New WayPoint With {.Position = Job.DeliveryPosition, _
                                                          .Predecessor = WayPointColl, _
                                                          .Job = Job, _
                                                          .VolumeDelta = -Job.CubicMetres}
                    WayPoints.Add(WayPointColl)
                    WayPoints.Add(WayPointDel)
                Case JobStatus.PENDING_DELIVERY
                    Dim WayPointDel As New WayPoint With {.Position = Job.DeliveryPosition, _
                                                          .Job = Job, _
                                                          .VolumeDelta = -Job.CubicMetres}
                    WayPoints.Add(WayPointDel)
            End Select
        Next

        Dim Time As TimeSpan = NoticeBoard.CurrentTime
        Dim ReorderedWayPoints As New List(Of WayPoint)
        Dim LastPoint As IPoint = Start

        Do Until WayPoints.Count = 0
            Dim ClosestNeighbour As WayPoint = Nothing
            Dim ClosestCost As Double = Double.MaxValue
            Dim ClosestExtraTime As TimeSpan = Nothing
            For Each WP As WayPoint In WayPoints
                If (WP.Predecessor Is Nothing OrElse _
                    ReorderedWayPoints.Contains(WP.Predecessor)) AndAlso _
                    CapacityLeft - WP.VolumeDelta > 0 Then

                    Dim Cost As Double
                    Dim ExtraTime As New TimeSpan
                    If Map IsNot Nothing Then
                        Dim AStar As New AStarSearch(LastPoint, WP.Position, Map.NodesAdjacencyList, Minimiser)
                        Cost += AStar.GetRoute.GetKM
                        ExtraTime += TimeSpan.FromHours(AStar.GetRoute.GetEstimatedHours()) 'TODO: vehicle type
                    Else
                        Cost = GetDistance(LastPoint, WP.Position)
                        ExtraTime += TimeSpan.FromHours(Cost / 48) ' Agent.GetAverageKMH)
                    End If

                    If Time > WP.Job.Deadline Then 'TODO: or run out of fuel!
                        'If any job cannot be delivered in time (e.g. this one)
                        'and assuming optimal route up to this point
                        'This route has failed, so short circuit out of here
                        Exit Sub
                    End If

                    'The expected wait time should not make THIS job late
                    'but it should be factored in to the route in general
                    ExtraTime += TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG)


                    If ClosestCost > Cost Then
                        ClosestCost = Cost
                        ClosestNeighbour = WP
                        ClosestExtraTime = ExtraTime
                    End If
                End If
            Next
            If ClosestNeighbour IsNot Nothing Then
                WayPoints.Remove(ClosestNeighbour)
                ReorderedWayPoints.Add(ClosestNeighbour)
                LastPoint = ClosestNeighbour.Position
                NNCost += ClosestCost
                CapacityLeft -= ClosestNeighbour.VolumeDelta
            Else
                'Ran out of vehicle space - cannot route.
                Exit Sub
            End If

        Loop

        PointList = New List(Of HopPosition)(ReorderedWayPoints.Count - 1)
        JobList = New List(Of CourierJob)(ReorderedWayPoints.Count - 1)
        For Each WP As WayPoint In ReorderedWayPoints
            PointList.Add(WP.Position)
            JobList.Add(WP.Job)
        Next
    End Sub
End Class
