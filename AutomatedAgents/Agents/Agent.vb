Public Class Agent
    Public Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Public AgentName As String
    Public PetroleumLitres As Double
    Public FuelCosts As Double = 0
    Public CurrentSpeedKMH As Double = 0
    Public TotalKMTravelled As Double = 0
    Public Position As RoutePosition
    Protected Map As StreetMap
    Public Color As Color

    Public AssignedJobs As New List(Of CourierJob)
    Public Delayer As New Delayer(0)

    Protected VehicleSize As VehicleSize = AutomatedAgents.VehicleSize.CAR
    Protected RoutePosition As Integer = 0
    Protected Strategy As IAgentStrategy

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.New(Map, Color, AutomatedAgents.VehicleSize.CAR)
    End Sub
    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color, ByVal VehicleSize As VehicleSize)
        Me.Map = Map
        Me.Color = Color
        Me.AgentName = AgentNameAssigner.AssignAgentName()
        Me.VehicleSize = VehicleSize
        Strategy = New NearestNeighbourEuclidianStrategy(Map, AssignedJobs)
        Refuel()

        Dim NullRoute As Route = New Route(Map.NodesAdjacencyList.GetRandomPoint)
        Position = New RoutePosition(NullRoute)
        'RoutePlan.Add(NullRoute)
        Position.Move(VehicleSize)
    End Sub

    Public Overridable Sub Move()
        'Reroutes if needed. In the simple case, when a waypoint is reached
        Strategy.Run(Position, Delayer)

        If Not Position.RouteCompleted And Delayer.Tick() Then
            Dim DistanceTravelled As Double = Position.Move(VehicleSize)
            CurrentSpeedKMH = Position.GetCurrentSpeed(VehicleSize)
            TotalKMTravelled += DistanceTravelled
            DepleteFuel(DistanceTravelled)
        End If
    End Sub


    Public Overridable Sub SetRouteTo(ByVal DestinationPoint As IPoint)
        'Dim StartingPoint As RoutingPoint = RoutePlan.Last.GetEndPoint

        'Dim RouteFinder As RouteFinder = New AStarSearch(StartingPoint, DestinationPoint, Map.NodesAdjacencyList, RouteFindingMinimiser)
        'Debug.Assert(RouteFinder IsNot Nothing)

        'RoutePlan.Add(RouteFinder.GetRoute)
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

End Class
