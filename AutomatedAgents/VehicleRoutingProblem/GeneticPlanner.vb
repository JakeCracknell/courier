Public Class GeneticPlanner
    Implements ISolver

    Private _OldPlan As CourierPlan
    Private _ExtraJob As CourierJob
    Private _AllWaypoints As List(Of WayPoint)
    Private _Agent As Agent
    Private _WaypointsToLock As Integer

    Private _NewPlan As CourierPlan

    Sub New(ByVal Agent As Agent, Optional ByVal ExtraJob As CourierJob = Nothing)
        _Agent = Agent
        _OldPlan = Agent.Plan
        _AllWaypoints = New List(Of WayPoint)(_OldPlan.WayPoints)
        If ExtraJob IsNot Nothing Then
            _ExtraJob = ExtraJob
            _AllWaypoints.AddRange(WayPoint.CreateWayPointList(ExtraJob))
        End If
        _AllWaypoints = _AllWaypoints.OrderBy(Function(W As WayPoint) W.Job.Deadline.Ticks).ToList
        Solve()
    End Sub

    Sub Solve()
        _WaypointsToLock = GetWaypointsToLock()

        Dim NNSolution As List(Of WayPoint) = GetNearestNeighbourSolution()

        _NewPlan = New CourierPlan(_OldPlan.RoutePosition.GetPoint, _OldPlan.Map, _OldPlan.Minimiser, _OldPlan.CapacityLeft, _OldPlan.VehicleType, NNSolution)
    End Sub

    Private Function GetWaypointsToLock() As Integer
        Dim GuarenteedTime As Double
        Dim WaypointsToLock As Integer = 0
        For i = 0 To _OldPlan.Routes.Count - 1
            GuarenteedTime += _OldPlan.Routes(i).GetEstimatedHours
            If GuarenteedTime <= SimulationParameters.COURTESY_CALL_LOCK_TIME_HOURS Then
                WaypointsToLock += 1
            Else
                Exit For
            End If
        Next
        Return WaypointsToLock
    End Function

    Private Function GetNearestNeighbourSolution() As List(Of WayPoint)
        Dim WaypointListSolution As New List(Of WayPoint)(_AllWaypoints.Count)
        Dim WaypointsLeft As New List(Of WayPoint)(_AllWaypoints)
        Dim CapacityLeft As Double = _Agent.GetVehicleCapacityLeft
        Dim Position As HopPosition = _OldPlan.RoutePosition.GetPoint
        Dim Time As TimeSpan = NoticeBoard.Time

        For i = 0 To _WaypointsToLock - 1
            Dim NextWaypoint As WayPoint = _OldPlan.WayPoints(i)
            WaypointListSolution.Add(NextWaypoint)
            WaypointsLeft.Remove(NextWaypoint)
            Position = NextWaypoint.Position
            CapacityLeft -= NextWaypoint.VolumeDelta
            Time += _OldPlan.Routes(i).GetEstimatedTime(Time)
        Next


        Do Until WaypointsLeft.Count = 0
            Dim OrderedWaypoints As IOrderedEnumerable(Of WayPoint)
            OrderedWaypoints = WaypointsLeft.Where( _
                Function(W) CapacityLeft - W.VolumeDelta >= 0 AndAlso _
                    Not WaypointsLeft.Contains(W.Predecessor)). _
                        OrderBy(Function(W) GetFuzzyDistance(Position, W.Position)). _
                            Take(3).OrderBy(Function(W) RouteCache.GetRoute(Position, W.Position).GetCostForAgent(_Agent, Time))

            Dim NextWaypoint As WayPoint = OrderedWaypoints(0)
            WaypointsLeft.Remove(NextWaypoint)
            WaypointListSolution.Add(NextWaypoint)
            Position = NextWaypoint.Position
            CapacityLeft -= NextWaypoint.VolumeDelta
            Time += RouteCache.GetRoute(Position, NextWaypoint.Position).GetEstimatedTime(Time)
        Loop
        Return WaypointListSolution
    End Function










    Public Function GetPlan() As CourierPlan Implements ISolver.GetPlan
        Return _NewPlan
    End Function

    Public Function GetTotalCost() As Double Implements ISolver.GetTotalCost
        Return _NewPlan.UpdateAndGetCost
    End Function

    Public Function IsSuccessful() As Boolean Implements ISolver.IsSuccessful
        Return True
    End Function
End Class
