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

    'Public RoutePlan As New List(Of Route)

    Public AssignedJobs As New List(Of CourierJob)

    Protected VehicleSize As VehicleSize = AutomatedAgents.VehicleSize.CAR
    Protected RoutePosition As Integer = 0
    Protected Strategy As AgentStrategy

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.New(Map, Color, AutomatedAgents.VehicleSize.CAR)
    End Sub
    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color, ByVal VehicleSize As VehicleSize)
        Me.Map = Map
        Me.Color = Color
        Me.AgentName = AgentNameAssigner.AssignAgentName()
        Me.VehicleSize = VehicleSize
        Strategy = New GreedyStrategy(Map, AssignedJobs)
        Refuel()

        Dim NullRoute As Route = New Route(Map.NodesAdjacencyList.GetRandomPoint)
        Position = New RoutePosition(NullRoute)
        'RoutePlan.Add(NullRoute)
        Position.Move(VehicleSize)
    End Sub

    Public Overridable Sub Move()
        'Reroutes if needed. In the simple case, when a waypoint is reached
        Strategy.UpdatePosition(Position)

        If Not Position.RouteCompleted Then
            Dim DistanceTravelled As Double = Position.Move(VehicleSize)
            CurrentSpeedKMH = Position.GetCurrentSpeed(VehicleSize)
            TotalKMTravelled += DistanceTravelled
            DepleteFuel(DistanceTravelled)
        End If

        'If Position.RouteCompleted Then
        '    Dim CurrentJob As CourierJob = If(AssignedJobs.Count > 0, AssignedJobs(0), Nothing)
        '    If CurrentJob IsNot Nothing Then
        '        If CurrentJob.Status = JobStatus.PENDING_PICKUP Then
        '            CurrentJob.Status = JobStatus.PENDING_DELIVERY
        '        ElseIf CurrentJob.Status = JobStatus.PENDING_DELIVERY Then
        '            CurrentJob.Status = JobStatus.COMPLETED
        '        End If
        '    End If
        'End If


    End Sub


    Public Overridable Sub SetRouteTo(ByVal DestinationPoint As RoutingPoint)
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

    'Private Sub FetchJob()
    '    If NoticeBoard.WaitingJobs.Count > 0 Then
    '        SetRouteTo(NoticeBoard.WaitingJobs(0).PickupPosition)
    '        NoticeBoard.WaitingJobs.RemoveAt(0)
    '    End If
    'End Sub

End Class
