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

    Protected VehicleSize As VehicleSize = AutomatedAgents.VehicleSize.CAR
    Protected RoutePosition As Integer = 0
    Protected NodeToRouteTo As Node

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.New(Map, Color, AutomatedAgents.VehicleSize.CAR)
    End Sub
    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color, ByVal VehicleSize As VehicleSize)
        Me.Map = Map
        Me.Color = Color
        Me.AgentName = AgentNameAssigner.AssignAgentName()
        Me.VehicleSize = VehicleSize
        Refuel()

        WarpToRandomNode()
    End Sub

    Protected Sub WarpToRandomNode()
        NodeToRouteTo = Map.NodesAdjacencyList.GetRandomNode
    End Sub

    Protected Sub MoveRandomly()
        Dim AdjacentNodes As List(Of NodesAdjacencyListCell) = _
            Map.NodesAdjacencyList.Rows(Position.GetOldNode.ID).Cells
        Dim Cell As NodesAdjacencyListCell = _
            AdjacentNodes(Int(Rnd() * AdjacentNodes.Count))
        NodeToRouteTo = Cell.Node
    End Sub

    Public Overridable Sub Move()
        Do Until Not Position.RouteCompleted
            SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
        Loop

        Dim DistanceTravelled As Double = Position.Move(VehicleSize)
        TotalKMTravelled += DistanceTravelled
        DepleteFuel(DistanceTravelled)
        CurrentSpeedKMH = Position.GetCurrentWay.GetMaxSpeedKMH(VehicleSize)
    End Sub


    Public Overridable Sub SetRouteTo(ByVal DestinationNode As Node)
        Dim RouteFinder As RouteFinder = New AStarSearch(GetCurrentNode, DestinationNode, Map.NodesAdjacencyList, RouteFindingMinimiser)
        If RouteFinder.GetRoute IsNot Nothing Then
            Position = New RoutePosition(RouteFinder.GetRoute)
        End If
    End Sub

    Protected Function GetCurrentNode() As Node
        If Position IsNot Nothing AndAlso Position.GetOldNode IsNot Nothing Then
            Return Position.GetOldNode
        Else
            Return Map.NodesAdjacencyList.GetRandomNode
        End If
    End Function

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
    End Function
End Class
