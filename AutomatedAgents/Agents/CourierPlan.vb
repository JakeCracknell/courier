﻿Public Class CourierPlan
    Property StartPoint As HopPosition
    Property WayPoints As New List(Of WayPoint)
    Property Routes As New List(Of Route)

    Property Map As StreetMap
    Property Minimiser As RouteFindingMinimiser
    Property CapacityLeft As Double = 0

    Public Sub New(ByVal StartPoint As HopPosition, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser, ByVal CapacityLeft As Double, ByVal WayPoints As List(Of WayPoint), ByVal RouteList As List(Of Route))
        Me.StartPoint = StartPoint
        Me.Map = Map
        Me.Minimiser = Minimiser
        Me.CapacityLeft = CapacityLeft
        Me.WayPoints = WayPoints
        Me.Routes = RouteList
    End Sub

    Public Sub New(ByVal StartPoint As HopPosition, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser, ByVal CapacityLeft As Double)
        Me.StartPoint = StartPoint
        Me.Map = Map
        Me.Minimiser = Minimiser
        Me.CapacityLeft = CapacityLeft
    End Sub

    'Agent has made progress on its plan, perhaps completing waypoints.
    Public Sub Update(ByVal RoutePosition As RoutePosition)
        If WayPoints.Count > 0 Then
            'If RoutePosition.GetPoint.ApproximatelyEquals(WayPoints(0).Position) Then
            '    Routes.RemoveAt(0)
            '    WayPoints.RemoveAt(0)
            'End If

            If WayPoints.Count > 0 Then
                StartPoint = RoutePosition.GetPoint
                Dim AStar As New AStarSearch(StartPoint, WayPoints(0).Position, Map.NodesAdjacencyList, Minimiser)
                Routes(0) = AStar.GetRoute
            End If
        End If
    End Sub

    Public Function UpdateAndGetCost(ByVal RoutePosition As RoutePosition)
        Update(RoutePosition)
        Dim TotalCost As Double = 0
        Select Case Minimiser
            Case RouteFindingMinimiser.DISTANCE
                For Each R As Route In Routes
                    TotalCost += R.GetKM
                Next
            Case RouteFindingMinimiser.TIME_NO_TRAFFIC, RouteFindingMinimiser.TIME_WITH_TRAFFIC
                For Each R As Route In Routes
                    TotalCost += R.GetEstimatedHours + CourierJob.CUSTOMER_WAIT_TIME_MAX / 3600
                Next
            Case RouteFindingMinimiser.FUEL
                Throw New NotImplementedException
        End Select
        Return TotalCost
    End Function

    'Forces a complete replan
    'TODO: perhaps this could try to connect two disjoint route lists in different ways.
    'DO NOT USE THIS FOR TESTING AND PLACING BIDS.
    Public Sub Replan(Optional AdditionalJob As CourierJob = Nothing)
        Throw New NotImplementedException
        'For i = WayPoints.Count - 1 To 0 Step -1
        '    If WayPoints(i).Job.Status = JobStatus.CANCELLED Then
        '        WayPoints.RemoveAt(i)
        '        Routes.RemoveAt(i)
        '    End If
        'Next
    End Sub

    Public Sub InsertWayPointOptimally(ByVal WayPoint As WayPoint)
        Dim RIS As New RouteInsertionSolver(Me, WayPoint)
        Dim NewPlan As CourierPlan = RIS.GetPlan()
        If NewPlan IsNot Nothing Then
            WayPoints = NewPlan.WayPoints
            Routes = NewPlan.Routes
        Else
            Debug.WriteLine("Could not insert a waypoint optimally")
            Routes.Add(New AStarSearch(WayPoints.Last.Position, WayPoint.Position, Map.NodesAdjacencyList, Minimiser).GetRoute)
            WayPoints.Add(WayPoint)
        End If

    End Sub

    Public Function FirstWayPointReached(ByVal RoutePosition As RoutePosition) As Boolean
        Return RoutePosition.GetPoint.ApproximatelyEquals(WayPoints(0).Position)
    End Function

    Public Function RemoveFirstWayPoint() As WayPoint
        Dim WP As WayPoint = WayPoints(0)
        StartPoint = WP.Position
        WayPoints.RemoveAt(0)
        Routes.RemoveAt(0)

        Return WP
    End Function

End Class