Public Class CourierPlan
    Property StartPoint As HopPosition
    Property WayPoints As New List(Of WayPoint)
    Property Routes As New List(Of Route)

    Property Map As StreetMap
    Property Minimiser As RouteFindingMinimiser
    Property CapacityLeft As Double = 0
    Property RoutePosition As RoutePosition
    Property VehicleType As Vehicles.Type

    Private LastTimeIndexOfTrafficReplan As Integer = GetTimeIndex(NoticeBoard.Time)

    Public Sub New(ByVal StartPoint As HopPosition, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser, ByVal CapacityLeft As Double, ByVal VehicleType As Vehicles.Type)
        Me.StartPoint = StartPoint
        Me.Map = Map
        Me.Minimiser = Minimiser
        Me.CapacityLeft = CapacityLeft
        Me.VehicleType = VehicleType
        RoutePosition = New RoutePosition(New Route(StartPoint))
    End Sub
    Public Sub New(ByVal StartPoint As HopPosition, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser, ByVal CapacityLeft As Double, ByVal VehicleType As Vehicles.Type, ByVal WayPoints As List(Of WayPoint))
        Me.New(StartPoint, Map, Minimiser, CapacityLeft, VehicleType)
        Me.WayPoints = WayPoints
        RecreateRouteListFromWaypoints()
    End Sub
    Public Sub New(ByVal StartPoint As HopPosition, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser, ByVal CapacityLeft As Double, ByVal VehicleType As Vehicles.Type, ByVal WayPoints As List(Of WayPoint), ByVal RouteList As List(Of Route))
        Me.New(StartPoint, Map, Minimiser, CapacityLeft, VehicleType)
        Me.WayPoints = WayPoints
        Me.Routes = RouteList
    End Sub

    Public Sub RecreateRouteListFromWaypoints()
        Routes = New List(Of Route)
        Dim LastPoint As IPoint = RoutePosition.GetPoint
        For Each W As WayPoint In WayPoints
            Routes.Add(RouteCache.GetRoute(LastPoint, W.Position))
            LastPoint = W.Position
        Next
    End Sub

    'Agent has made progress on its plan, perhaps completing waypoints.
    'Sets StartPoint and Routes(0)
    Public Sub Update(ByVal RecreateRoute As Boolean)
        StartPoint = RoutePosition.GetPoint
        If WayPoints.Count > 0 Then
            If RecreateRoute AndAlso Not Routes(0).GetStartPoint.Equals(WayPoints(0).Position) Then
                If RoutePosition.Route.Equals(Routes(0)) Then
                    Routes(0) = RoutePosition.GetSubRoute
                End If
            End If
        End If
    End Sub

    Public Function UpdateAndGetCost() As Double
        Update(True)

        Dim TotalCost As Double = 0
        Select Case Minimiser
            Case RouteFindingMinimiser.DISTANCE
                For Each R As Route In Routes
                    TotalCost += R.GetKM()
                Next
            Case RouteFindingMinimiser.TIME_NO_TRAFFIC
                For Each R As Route In Routes
                    TotalCost += R.GetHoursWithoutTraffic() + CourierJob.CUSTOMER_WAIT_TIME_MAX / 3600
                Next
            Case RouteFindingMinimiser.TIME_WITH_TRAFFIC
                'I am making the decision here to not reevaluate traffic WRT time. Route.StartTime will suffice.
                For Each R As Route In Routes
                    TotalCost += R.GetEstimatedHours() + CourierJob.CUSTOMER_WAIT_TIME_MAX / 3600
                Next
            Case RouteFindingMinimiser.FUEL_NO_TRAFFIC
                For Each R As Route In Routes
                    TotalCost += R.GetOptimalFuelUsageWithoutTraffic(VehicleType)
                Next
            Case RouteFindingMinimiser.FUEL_WITH_TRAFFIC
                For Each R As Route In Routes
                    TotalCost += R.GetOptimalFuelUsageWithTraffic(VehicleType)
                Next
        End Select
        Return TotalCost
    End Function

    Public Function FirstWayPointReached() As Boolean
        If ForcedDiversion Is Nothing Then
            Return RoutePosition.GetPoint.ApproximatelyEquals(WayPoints(0).Position)
        Else
            Return False 'Edge case where waypoint is on the way to the fuel point.
        End If
    End Function

    Public Function RemoveFirstWayPoint() As WayPoint
        Dim WP As WayPoint = WayPoints(0)
        StartPoint = WP.Position
        WayPoints.RemoveAt(0)
        Routes.RemoveAt(0)

        Return WP
    End Function

    'A->CCC->B->CCC->D goes to A->B->D. Triangle inequality should bring some efficiency gains. TODO: compare with a new TSP solve.
    Sub ExtractCancelled()
        Dim LastPoint As HopPosition = StartPoint
        Dim Index As Integer = 0
        Do While Index < WayPoints.Count
            If WayPoints(Index).Job.Status = JobStatus.CANCELLED Then
                WayPoints.RemoveAt(Index)
                Routes.RemoveAt(Index)
                If Index >= Routes.Count Then
                    Exit Sub
                End If
                Routes(Index) = New AStarSearch(LastPoint, WayPoints(Index).Position, Map.NodesAdjacencyList, Minimiser).GetRoute
            End If
            LastPoint = WayPoints(Index).Position
            Index += 1
        Loop
    End Sub

    'If traffic conditions have changed (check TimeIndex), replan and retrun true iff late.
    Function ReplanForTrafficConditions() As Boolean
        Dim TimeIndex As Integer = GetTimeIndex(NoticeBoard.Time)
        If TimeIndex <> LastTimeIndexOfTrafficReplan Then
            LastTimeIndexOfTrafficReplan = TimeIndex
            'TODO: REPLAN!!!!!!!!!!
            'modify waypoints order and routes accordingly
            '
            Return IsBehindSchedule()
        End If
        Return False
    End Function

    Function GetCurrentJobs() As List(Of CourierJob)
        Return (From W In WayPoints
               Where W.DefinedStatus = JobStatus.PENDING_DELIVERY
               Select W.Job).ToList
    End Function

    'Might be idle and moving. Idle = not doing any jobs, might be moving according to an idle strategy
    Function IsIdle() As Boolean
        Return WayPoints.Count = 0 And Not IsOnDiversion()
    End Function

    Function IsStationary() As Boolean
        Return WayPoints.Count = 0 AndAlso RoutePosition.RouteCompleted
    End Function

    Function IsBehindSchedule() As Boolean
        Update(True)

        Dim WorkingTime As TimeSpan = NoticeBoard.Time
        For i = 0 To WayPoints.Count - 1
            WorkingTime += Routes(i).GetTimeWithoutTraffic
            If WayPoints(i).Job.Deadline < WorkingTime Then
                Return True
            End If
            WorkingTime += TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
        Next
        Return False
    End Function

    Function LateWaypointsCount() As Integer
        Update(True)
        Dim LateWaypoints As Integer = 0
        Dim WorkingTime As TimeSpan = NoticeBoard.Time
        For i = 0 To WayPoints.Count - 1
            WorkingTime += Routes(i).GetTimeWithoutTraffic
            If WayPoints(i).Job.Deadline < WorkingTime Then
                LateWaypoints += 1
            End If
            WorkingTime += TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
        Next
        Return LateWaypoints
    End Function

    Function CostScore() As Double
        Return UpdateAndGetCost() + LateWaypointsCount() * 1000
    End Function

    '----------------------------Diversions-------------------------------------
    Private ForcedDiversion As Route = Nothing
    Sub SetNewRoute(ByVal Route As Route) 'Only for use whilst idle - will be overridden if jobs come in.
        Debug.Assert(WayPoints.Count = 0)
        RoutePosition = New RoutePosition(Route)
    End Sub
    Sub SetDiversion(ByVal Route As Route) 'For emergency refueling that cannot be interrupted.
        Debug.Assert(WayPoints.Count <> 0)
        RoutePosition = New RoutePosition(Route)
        ForcedDiversion = Route
    End Sub
    Sub EndDiversion()
        Debug.Assert(ForcedDiversion IsNot Nothing)
        ForcedDiversion = Nothing
        StartPoint = RoutePosition.GetPoint
        Routes(0) = RouteCache.GetRoute(StartPoint, WayPoints(0).Position)
    End Sub
    Function GetDiversionTimeEstimate() As TimeSpan
        If ForcedDiversion Is Nothing Then
            Return TimeSpan.Zero
        Else
            Return RoutePosition.GetSubRoute.GetEstimatedTime() + TimeSpan.FromSeconds(SimulationParameters.REFUELLING_TIME_SECONDS)
        End If
    End Function
    Function IsOnDiversion() As Boolean
        Return ForcedDiversion IsNot Nothing
    End Function

End Class
