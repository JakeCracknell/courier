﻿Public Class Agent
    Private Const DEFAULT_KMH As Double = 48
    Public Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Public AgentName As String
    Public PetroleumLitres As Double
    Public FuelCosts As Double = 0
    Public CurrentSpeedKMH As Double = 0
    Public TotalKMTravelled As Double = 0
    Public TotalDrivingTime As Integer = 0
    Public TotalCompletedJobs As Integer = 0
    Public Map As StreetMap
    Public Color As Color
    Public Delayer As New Delayer

    Public VehicleSize As VehicleSize = AutomatedAgents.VehicleSize.CAR
    Public Plan As CourierPlan
    Protected Strategy As IAgentStrategy

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.New(Map, Color, AutomatedAgents.VehicleSize.CAR)
    End Sub
    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color, ByVal VehicleSize As VehicleSize)
        Me.Map = Map
        Me.Color = Color
        Me.AgentName = AgentNameAssigner.AssignAgentName()
        Me.VehicleSize = VehicleSize
        Strategy = New ContractNetStrategy(Me, ContractNetPolicy.CNP4)
        Refuel()

        'TODO: start at depot, which is also a refuelling station
        Plan = New CourierPlan(Map.NodesAdjacencyList.GetRandomPoint, Map, RouteFindingMinimiser, GetVehicleMaxCapacity)
        Plan.RoutePosition.Move(VehicleSize)
    End Sub

    Public Overridable Sub Move()
        Plan.CapacityLeft = Math.Round(Plan.CapacityLeft, 5)
        If Plan.CapacityLeft > GetVehicleMaxCapacity() Then
            'Throw New OverflowException
            Debug.WriteLine("Vehicle is too full by: " & GetVehicleCapacityPercentage() * 100 & "%")
        ElseIf Plan.WayPoints.Count = 0 AndAlso Plan.CapacityLeft <> GetVehicleMaxCapacity() Then
            Debug.WriteLine("Capacity left is non-empty, but vehicle is empty: " & Plan.CapacityLeft)
            'Debug.WriteLine(AgentName & " [" & "".PadRight((1 - Plan.CapacityLeft) * 60, "#") & "".PadRight(Plan.CapacityLeft * 60, " ") & "]")
        End If

        'Reroutes if needed. In the simple case, when a waypoint is reached
        Strategy.Run()

        If Not Plan.RoutePosition.RouteCompleted And Delayer.Tick() Then
            Dim DistanceTravelled As Double = Plan.RoutePosition.Move(VehicleSize)
            CurrentSpeedKMH = Plan.RoutePosition.GetCurrentSpeed(VehicleSize)
            TotalKMTravelled += DistanceTravelled
            TotalDrivingTime += 1
            DepleteFuel(DistanceTravelled)
        Else
            CurrentSpeedKMH = 0
            'Idle strategy?
        End If
    End Sub


    Public Overridable Sub SetRouteTo(ByVal DestinationPoint As IPoint)
        Debug.WriteLine("This type fo agent does not support direct routing commands")
    End Sub

    Protected Sub DepleteFuel(ByVal DistanceTravelled As Double)
        'TODO: make this a major point in the project, fuel economy
        'Select Case VehicleSize
        'Case
        PetroleumLitres -= DistanceTravelled / 17.7
        'End Select
        'For now just say fixed KPL. Note 100 mpg = 35.4 kpl
    End Sub

    Protected Sub Refuel()
        Select Case VehicleSize
            Case AutomatedAgents.VehicleSize.CAR
                PetroleumLitres = 50
            Case AutomatedAgents.VehicleSize.VAN
                PetroleumLitres = 75
            Case AutomatedAgents.VehicleSize.TRUCK_7_5_TONNE
                PetroleumLitres = 100
        End Select
    End Sub

    Public Function GetVehicleString() As String
        Select Case VehicleSize
            Case AutomatedAgents.VehicleSize.CAR
                Return "Car"
            Case AutomatedAgents.VehicleSize.VAN
                Return "Van"
            Case AutomatedAgents.VehicleSize.TRUCK_7_5_TONNE
                Return "Truck"
        End Select
        Return ""
    End Function

    Public Function GetVehicleMaxCapacity() As Double
        Select Case VehicleSize
            Case AutomatedAgents.VehicleSize.CAR
                Return 1
            Case AutomatedAgents.VehicleSize.VAN
                Return 2
            Case AutomatedAgents.VehicleSize.TRUCK_7_5_TONNE
                Return 8
        End Select
        Return Double.MaxValue
    End Function

    Public Function GetVehicleCapacityPercentage() As Double
        Return 1 - Plan.CapacityLeft / GetVehicleMaxCapacity()
    End Function

    Public Function GetVehicleCapacityLeft() As Double
        Return Plan.CapacityLeft
    End Function

    Public Function GetAverageKMH() As Double
        'Returns average driving speed (not influenced by "waiting")
        'If the agent has not driven at all, returns 48 kmh.
        Return If(TotalDrivingTime > 0, _
                  TotalKMTravelled * 3600 / TotalDrivingTime,
                    DEFAULT_KMH)
    End Function
End Class
