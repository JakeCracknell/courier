﻿Public Class Route
    Private Hops As List(Of Hop)
    Private TotalKM As Double = -1
    Private TotalHours As Double = -1

    Public Sub New(ByVal Hops As List(Of Hop))
        Debug.Assert(Hops IsNot Nothing AndAlso Hops.Count >= 1)

        'Remove header hop A->A.
        If Hops(0).Way Is Nothing Then
            Hops.RemoveAt(0)
        End If
        Me.Hops = Hops

        Debug.Assert(Hops.Count >= 1)
    End Sub

    'A route of 0
    Public Sub New(ByVal OnlyPoint As IPoint)
        Me.Hops = New List(Of Hop)
        Me.Hops.Add(New Hop(OnlyPoint, OnlyPoint, Nothing))
    End Sub

    Public Function At(ByVal Index As Integer) As Hop
        Return Hops(Index)
    End Function

    Public Function GetStartPoint() As IPoint
        Return Hops.First.FromPoint
    End Function

    Public Function GetEndPoint() As IPoint
        Return Hops.Last.ToPoint
    End Function

    Public Function GetHopList() As List(Of Hop)
        Return Hops
    End Function

    Public Function HopCount() As Integer
        Return Hops.Count
    End Function

    Public Function GetKM() As Double
        'Lazily calculated, as I don't want a heavyweight constructor.
        If TotalKM < 0 Then
            TotalKM = 0
            For Each Hop As Hop In Hops
                TotalKM += Hop.GetDistance
            Next
        End If
        Return TotalKM
    End Function

    Public Function GetEstimatedHours() As Double
        'Lazily calculated, as I don't want a heavyweight constructor.
        If TotalHours < 0 Then
            TotalHours = 0
            For Each Hop As Hop In Hops
                TotalHours += Hop.GetEstimatedTravelTime
            Next
        End If
        Return TotalHours
    End Function

    Public Function GetEstimatedTime() As TimeSpan
        Return TimeSpan.FromHours(GetEstimatedHours())
    End Function

    Public Function GetEstimatedFuelUsage(ByVal VehicleSize As Vehicles.Type) As Double
        'TODO
        Return GetKM() * 0.1
    End Function

    Function GetCostForAgent(ByVal Agent As Agent) As Double
        Select Case Agent.RouteFindingMinimiser
            Case AutomatedAgents.RouteFindingMinimiser.DISTANCE
                Return GetKM()
            Case AutomatedAgents.RouteFindingMinimiser.TIME_NO_TRAFFIC, AutomatedAgents.RouteFindingMinimiser.TIME_WITH_TRAFFIC
                Return GetEstimatedHours()
            Case AutomatedAgents.RouteFindingMinimiser.FUEL
                Return GetEstimatedFuelUsage(Agent.VehicleType)
        End Select
        Return 0
    End Function


End Class
