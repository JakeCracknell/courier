Imports System.Collections.Concurrent

Public Class Route
    Private Hops As List(Of Hop)
    Private TotalKM As Double = -1
    Private TotalHours As Double = -1
    Private TotalHoursAtTime As New ConcurrentDictionary(Of Integer, Double)
    Public StartTime As TimeSpan 'Originally formulated to start at this time (may be shifted though)

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

    Public Function GetHoursWithoutTraffic() As Double
        'Lazily calculated, as I don't want a heavyweight constructor.
        If TotalHours < 0 Then
            TotalHours = 0
            For Each Hop As Hop In Hops
                TotalHours += Hop.GetMinimumTravelTime
            Next
        End If
        Return TotalHours
    End Function

    'GetEstimatedTravelTimeAtTime includes RoadDelays
    Public Function GetEstimatedHours(ByVal Time As TimeSpan) As Double
        Dim TimeIndex As Integer = GetTimeIndex(Time)
        If Not TotalHoursAtTime.ContainsKey(TimeIndex) Then
            Dim THours As Double = 0
            Dim WorkingTime As TimeSpan = Time
            For Each Hop As Hop In Hops
                THours += Hop.GetEstimatedTravelTimeAtTime(WorkingTime)
                WorkingTime = Time + TimeSpan.FromHours(THours)
            Next
            TotalHoursAtTime(TimeIndex) = THours
        End If
        Return TotalHoursAtTime(TimeIndex)
    End Function
    Public Function GetEstimatedHours() As Double
        Return GetEstimatedHours(StartTime)
    End Function
    Public Function GetEstimatedTime(ByVal Time As TimeSpan) As TimeSpan
        Return TimeSpan.FromHours(GetEstimatedHours(Time))
    End Function
    Public Function GetEstimatedTime() As TimeSpan
        Return TimeSpan.FromHours(GetEstimatedHours(StartTime))
    End Function
    Public Function GetTimeWithoutTraffic() As TimeSpan
        Return TimeSpan.FromHours(GetHoursWithoutTraffic())
    End Function


    Public Function GetOptimalFuelUsageWithoutTraffic(Optional ByVal VehicleSize As Vehicles.Type = Vehicles.Type.CAR) As Double
        Dim TotalLitres As Double = 0
        Dim WorkingTime As TimeSpan = StartTime
        For Each Hop As Hop In Hops
            TotalLitres += Hop.GetOptimalFuelUsage(VehicleSize)
            WorkingTime += TimeSpan.FromSeconds(Hop.GetEstimatedTravelTimeWithOptimalFuelUsage(WorkingTime, VehicleSize))
        Next
        Return TotalLitres
    End Function
    Public Function GetOptimalFuelUsageWithTraffic(Optional ByVal VehicleSize As Vehicles.Type = Vehicles.Type.CAR) As Double
        Dim TotalLitres As Double = 0
        Dim WorkingTime As TimeSpan = StartTime
        For Each Hop As Hop In Hops
            TotalLitres += Hop.GetOptimalFuelUsageAtTime(WorkingTime)
            WorkingTime += TimeSpan.FromSeconds(Hop.GetEstimatedTravelTimeWithOptimalFuelUsage(WorkingTime, VehicleSize))
        Next
        Return TotalLitres
    End Function

    Function GetCostForAgent(ByVal Agent As Agent) As Double
        Select Case Agent.RouteFindingMinimiser
            Case AutomatedAgents.RouteFindingMinimiser.DISTANCE
                Return GetKM()
            Case AutomatedAgents.RouteFindingMinimiser.TIME_NO_TRAFFIC
                Return GetHoursWithoutTraffic()
            Case AutomatedAgents.RouteFindingMinimiser.TIME_WITH_TRAFFIC
                Return GetEstimatedHours(StartTime)
            Case AutomatedAgents.RouteFindingMinimiser.FUEL_NO_TRAFFIC
                Return GetOptimalFuelUsageWithoutTraffic(Agent.VehicleType)
            Case AutomatedAgents.RouteFindingMinimiser.FUEL_WITH_TRAFFIC
                Return GetOptimalFuelUsageWithTraffic(Agent.VehicleType)
        End Select
        Return 0
    End Function


End Class
