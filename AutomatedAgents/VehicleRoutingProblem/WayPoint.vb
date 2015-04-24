
Public Class WayPoint
    Public Predecessor As WayPoint
    Public Position As IPoint
    Public Job As CourierJob
    Public VolumeDelta As Double
    Public DefinedStatus As JobStatus

    Public Shared Function CreateWayPointList(ByVal CourierJobs As List(Of CourierJob)) As List(Of WayPoint)
        Dim WayPoints As New List(Of WayPoint)
        For Each J In CourierJobs
            Select Case J.Status
                Case JobStatus.PENDING_PICKUP, JobStatus.UNALLOCATED
                    Dim WayPointColl As New WayPoint With {.Position = J.PickupPosition, _
                                                          .Job = J, _
                                                          .VolumeDelta = J.CubicMetres, _
                                                          .DefinedStatus = JobStatus.PENDING_PICKUP}
                    Dim WayPointDel As New WayPoint With {.Position = J.DeliveryPosition, _
                                                          .Predecessor = WayPointColl, _
                                                          .Job = J, _
                                                          .VolumeDelta = -J.CubicMetres, _
                                                          .DefinedStatus = JobStatus.PENDING_DELIVERY}
                    WayPoints.Add(WayPointColl)
                    WayPoints.Add(WayPointDel)
                Case JobStatus.PENDING_DELIVERY
                    Dim WayPointDel As New WayPoint With {.Position = J.DeliveryPosition, _
                                                          .Job = J, _
                                                          .VolumeDelta = -J.CubicMetres, _
                                                          .DefinedStatus = JobStatus.PENDING_DELIVERY}

                    WayPoints.Add(WayPointDel)
            End Select
        Next
        Return WayPoints
    End Function

    Public Shared Function CreateWayPointList(ByVal J As CourierJob) As List(Of WayPoint)
        Dim WayPoints As New List(Of WayPoint)(2)
        Select Case J.Status
            Case JobStatus.PENDING_PICKUP, JobStatus.UNALLOCATED
                Dim WayPointColl As New WayPoint With {.Position = J.PickupPosition, _
                                                      .Job = J, _
                                                      .VolumeDelta = J.CubicMetres, _
                                                      .DefinedStatus = JobStatus.PENDING_PICKUP}
                Dim WayPointDel As New WayPoint With {.Position = J.DeliveryPosition, _
                                                      .Predecessor = WayPointColl, _
                                                      .Job = J, _
                                                      .VolumeDelta = -J.CubicMetres, _
                                                      .DefinedStatus = JobStatus.PENDING_DELIVERY}
                WayPoints.Add(WayPointColl)
                WayPoints.Add(WayPointDel)
            Case JobStatus.PENDING_DELIVERY
                Dim WayPointDel As New WayPoint With {.Position = J.DeliveryPosition, _
                                                      .Job = J, _
                                                      .VolumeDelta = -J.CubicMetres, _
                                                      .DefinedStatus = JobStatus.PENDING_DELIVERY}

                WayPoints.Add(WayPointDel)
        End Select

        Return WayPoints
    End Function

    Public Overrides Function ToString() As String
        Select Case DefinedStatus
            Case JobStatus.PENDING_PICKUP
                Return "↑ " & Job.PickupName
            Case JobStatus.PENDING_DELIVERY
                If Job.IsFailedDelivery Then
                    Return "↓ " & CType(CType(Position, HopPosition).Hop.FromPoint, Node).ToString
                Else
                    Return "↓ " & Job.DeliveryName
                End If
            Case Else
                Return ""
        End Select
    End Function
End Class
