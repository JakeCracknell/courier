Public Class WayPoint
    Public Predecessor As WayPoint
    Public Position As IPoint
    Public Job As CourierJob
    Public VolumeDelta As Double
    Public DefinedStatus As JobStatus
    Public Deadline As TimeSpan

    Public Shared Function CreateWayPointList(ByVal CourierJobs As List(Of CourierJob)) As List(Of WayPoint)
        Dim WayPoints As New List(Of WayPoint)
        For Each J In CourierJobs
            Select Case J.Status
                Case JobStatus.PENDING_PICKUP, JobStatus.UNALLOCATED
                    Dim WayPointColl As WayPoint = CreatePickupWaypoint(J)
                    Dim WayPointDel As WayPoint = CreateDropoffWaypoint(J, WayPointColl)

                    WayPoints.Add(WayPointColl)
                    WayPoints.Add(WayPointDel)
                Case JobStatus.PENDING_DELIVERY
                    WayPoints.Add(CreateDropoffWaypoint(J))
            End Select
        Next
        Return WayPoints
    End Function

    Public Shared Function CreateWayPointList(ByVal J As CourierJob) As List(Of WayPoint)
        Dim WayPoints As New List(Of WayPoint)(2)
        Select Case J.Status
            Case JobStatus.PENDING_PICKUP, JobStatus.UNALLOCATED
                Dim WayPointColl As WayPoint = CreatePickupWaypoint(J)
                Dim WayPointDel As WayPoint = CreateDropoffWaypoint(J, WayPointColl)

                WayPoints.Add(WayPointColl)
                WayPoints.Add(WayPointDel)
            Case JobStatus.PENDING_DELIVERY
                WayPoints.Add(CreateDropoffWaypoint(J))
        End Select

        Return WayPoints
    End Function

    Private Shared Function CreatePickupWaypoint(ByVal J As CourierJob) As WayPoint
        Return New WayPoint With {.Position = J.PickupPosition, _
                                    .Job = J, _
                                    .Deadline = J.Deadline - J.GetDirectRoute.GetTimeWithoutTraffic - Customers.WaitTimeMax, _
                                    .VolumeDelta = J.CubicMetres, _
                                    .DefinedStatus = JobStatus.PENDING_PICKUP}
    End Function
    Private Shared Function CreateDropoffWaypoint(ByVal J As CourierJob, Optional ByVal PickupWaypoint As WayPoint = Nothing) As WayPoint
        Return New WayPoint With {.Position = J.DeliveryPosition, _
                                    .Predecessor = PickupWaypoint, _
                                    .Job = J, _
                                    .Deadline = J.Deadline, _
                                    .VolumeDelta = -J.CubicMetres, _
                                    .DefinedStatus = JobStatus.PENDING_DELIVERY}
    End Function



    Public Shared Function MakeFailedDeliveryWaypoint(ByVal Agent As Agent, ByVal Job As CourierJob) As WayPoint
        Job.DeliveryPosition = Agent.Map.GetNearestDepot(Agent.Plan.StartPoint)
        Job.DeliveryName = If(Job.IsFailedDelivery(), "[" & If(SimulationParameters.FailToDepot, CType(Job.DeliveryPosition.Hop.FromPoint, Node).ToString, "↺") & "] ← ", "") & Job.DeliveryName
        Return CreateDropoffWaypoint(Job)
    End Function

    Public Overrides Function ToString() As String
        Select Case DefinedStatus
            Case JobStatus.PENDING_PICKUP
                Return "↑ " & Job.PickupName
            Case JobStatus.PENDING_DELIVERY
                Return "↓ " & Job.DeliveryName
            Case Else
                Return ""
        End Select
    End Function

    Public Overrides Function GetHashCode() As Integer
        Const Prime As Integer = 31
        Return Job.JobID * Prime + DefinedStatus
    End Function
End Class
