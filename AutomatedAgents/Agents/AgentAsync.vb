﻿Public Class AgentAsync
    Inherits Agent

    Private RouteFinder As AsyncRouteFinder
    Private AwaitingRoute As Boolean = False
    Private TicksWaited As Integer = 0

    Public Sub New(ByVal ID As Integer, ByVal Map As StreetMap, ByVal Color As Color)
        MyBase.New(ID, Map, Color)
    End Sub

    Public Overrides Sub Move()
        If AwaitingRoute Then
            If RouteFinder IsNot Nothing AndAlso RouteFinder.RoutingComplete Then
                If RouteFinder.PlannedRoute IsNot Nothing Then
                    Plan.RoutePosition = New RoutePosition(RouteFinder.PlannedRoute)
                    AwaitingRoute = False
                Else
                    'This handles the case where the agent tried to route to a disconnected subgraph
                    'What about when it was placed on a small disconnected subgraph - stuck!
                    SetRouteTo(Map.NodesAdjacencyList.GetRandomPoint)
                    Exit Sub
                End If

                TicksWaited = 0
            Else
                'Skip a turn if still waiting on route
                TicksWaited += 1
                Exit Sub
            End If
        End If

        If Plan.RoutePosition Is Nothing OrElse Plan.RoutePosition.RouteCompleted() Then
            TotalCompletedJobs += 1
            SetRouteTo(Map.NodesAdjacencyList.GetRandomPoint)
        Else
            Dim DistanceTravelled As Double = Plan.RoutePosition.Move()
            TotalKMTravelled += DistanceTravelled
            DepleteFuel(DistanceTravelled)
            CurrentSpeedKMH = Plan.RoutePosition.GetCurrentWay.GetSpeedAtTime(NoticeBoard.Time)
        End If
    End Sub

    Public Overrides Sub SetRouteTo(ByVal DestinationPoint As IPoint)
        Dim StartingPoint As IPoint = If(Plan.RoutePosition IsNot Nothing, Plan.RoutePosition.GetPoint, Map.NodesAdjacencyList.GetRandomPoint)
        RouteFinder = New AsyncRouteFinder(StartingPoint, DestinationPoint, Map.NodesAdjacencyList)
        AwaitingRoute = True
    End Sub

    Private Class AsyncRouteFinder
        Private FromPoint As IPoint
        Private ToPoint As IPoint
        Private AdjacencyList As NodesAdjacencyList
        Public PlannedRoute As Route
        Public RoutingComplete As Boolean = False 'Might have failed

        Public Sub New(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint, ByVal AdjacencyList As NodesAdjacencyList)
            Me.FromPoint = FromPoint
            Me.ToPoint = ToPoint
            Me.AdjacencyList = AdjacencyList
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf Run)
        End Sub

        Protected Sub Run()
            Dim RouteFinder As IRouteFinder = New AStarSearch(FromPoint, ToPoint, AdjacencyList, RouteFindingMinimiser)
            PlannedRoute = RouteFinder.GetRoute
            RoutingComplete = True
        End Sub
    End Class

End Class
