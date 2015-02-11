
Class WayPoint
    Public Predecessor As WayPoint
    Public Position As IPoint
    Public Job As CourierJob
    Public VolumeDelta As Double

    Public Shared Function CreateWayPointList(ByVal CourierJobs As List(Of CourierJob)) As List(Of WayPoint)
        Dim WayPoints As New List(Of WayPoint)
        For Each Job In CourierJobs
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
        Return WayPoints
    End Function
End Class
