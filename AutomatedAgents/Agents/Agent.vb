Public Class Agent
    Private Const DEFAULT_KMH As Double = 48
    Private Const FUEL_TANK_FULL_THRESHOLD As Double = 0.95
    Private Const FUEL_TANK_LOW_THRESHOLD As Double = 0.05
    Private Const START_IN_RANDOM_POSITION As Boolean = False
    Private SpeedCap As Double = SimulationParameters.MAX_POSSIBLE_SPEED_KMH

    Public Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Public ReadOnly AgentID As Integer
    Public ReadOnly PickupPoints As New List(Of HopPosition)
    Public FuelLitres As Double
    Public FuelCosts As Decimal = 0
    Public CurrentSpeedKMH As Double = 0
    Public TotalKMTravelled As Double = 0
    Public TotalDrivingTime As Integer = 0
    Public TotalCompletedJobs As Integer = 0
    Public Map As StreetMap
    Public Color As Color
    Public Delayer As New Delayer

    Public VehicleType As Vehicles.Type
    Public Plan As CourierPlan
    Protected Strategy As IAgentStrategy
    Protected IdleStrategy As IIdleStrategy

    Public Sub New(ByVal ID As Integer, ByVal Map As StreetMap, ByVal Color As Color)
        Me.New(ID, Map, Color, SimulationParameters.VehicleType)
    End Sub
    Public Sub New(ByVal ID As Integer, ByVal Map As StreetMap, ByVal Color As Color, ByVal VehicleType As Vehicles.Type)
        Me.AgentID = ID
        Me.Map = Map
        Me.Color = Color
        Me.VehicleType = VehicleType
        Select Case SimulationParameters.RoutingStrategy
            Case 0
                Strategy = New ContractNetStrategy(Me, SimulationParameters.CNPVersion)
            Case 1
                Strategy = New FreeForAllStrategy(Me)
            Case 2
                Strategy = New RoundRobinStrategy(Me)
        End Select
        Select Case SimulationParameters.IdleStrategy
            Case 0
                IdleStrategy = New NoIdleStrategy()
            Case 1
                IdleStrategy = New SleepingIdleStrategy(Me)
            Case 2
                IdleStrategy = New ConvergeToPickupIdleStrategy(Me)
            Case 3
                IdleStrategy = New ScatterIdleStrategy(Me)
        End Select

        Select Case RouteFindingMinimiser
            Case AutomatedAgents.RouteFindingMinimiser.FUEL_NO_TRAFFIC, AutomatedAgents.RouteFindingMinimiser.FUEL_WITH_TRAFFIC
                SpeedCap = Vehicles.OPTIMAL_KMH
            Case Else
                SpeedCap = SimulationParameters.MAX_POSSIBLE_SPEED_KMH
        End Select

        Refuel()

        'Agents start at a randomly chosen depot, or any spot?
        Dim StartingPoint As HopPosition = If(START_IN_RANDOM_POSITION, Map.NodesAdjacencyList.GetRandomPoint, Map.GetStartingPoint)
        Plan = New CourierPlan(StartingPoint, Map, RouteFindingMinimiser, GetVehicleMaxCapacity, VehicleType)
        Plan.RoutePosition.Move(VehicleType)
    End Sub

    Dim tripped = False
    Public Overridable Sub Move()
        'TODO remove
        'If NoticeBoard.Time > TimeSpan.FromHours(65) And Not tripped Then
        '    tripped = True
        '    SimulationParameters.SimulationSpeed = 1
        '    Console.Beep()
        'ElseIf tripped Then
        '    Debug.Write(GetVehicleCapacityLeft)
        '    For Each W In Plan.WayPoints
        '        Debug.Write(", " & W.VolumeDelta)
        '    Next
        '    Debug.WriteLine("")
        'End If



        Plan.CapacityLeft = Math.Round(Plan.CapacityLeft, 5)
        If Plan.CapacityLeft > GetVehicleMaxCapacity() Then
            'Throw New OverflowException
            Debug.WriteLine("ERROR: Vehicle is too full by: " & GetVehicleCapacityPercentage() & "%")
        ElseIf Plan.IsIdle() AndAlso Plan.CapacityLeft <> GetVehicleMaxCapacity() Then
            Debug.WriteLine("ERROR: Capacity left is non-empty, but vehicle is empty: " & Plan.CapacityLeft)
        ElseIf FuelLitres <= 0 Then
            Debug.WriteLine("ERROR: Out of fuel! Fuel level is: " & FuelLitres)
        End If

        'Reroutes if needed. In the simple case, when a waypoint is reached
        Strategy.Run()

        If Not Plan.RoutePosition.RouteCompleted And Delayer.Tick() Then
            Dim DistanceTravelled As Double = Plan.RoutePosition.Move(SpeedCap)
            CurrentSpeedKMH = Plan.RoutePosition.GetCurrentSpeed(VehicleType)
            TotalKMTravelled += DistanceTravelled
            TotalDrivingTime += 1
            DepleteFuel(DistanceTravelled)

            If Not Plan.IsIdle AndAlso EmergencyRefuelRequired() Then
                If Not Plan.IsOnDiversion Then
                    Dim RouteToFuel As Route = GetRouteForRefuel()
                    Plan.SetDiversion(RouteToFuel)
                    SimulationState.NewEvent(AgentID, LogMessages.EmergencyRefuel(FuelLitres, RouteToFuel.GetKM))
                ElseIf Plan.RoutePosition.RouteCompleted Then
                    Delayer = New Delayer(SimulationParameters.REFUELLING_TIME_SECONDS)
                    Plan.EndDiversion()
                    Refuel()
                    'Else : Agent is on its way to the fuel point now
                End If
            End If
        Else
            CurrentSpeedKMH = 0
            If Plan.IsIdle Then
                IdleStrategy.Run()
            End If
        End If
    End Sub


    Public Overridable Sub SetRouteTo(ByVal DestinationPoint As IPoint)
        If Plan.IsIdle Then
            Plan.SetNewRoute(RouteCache.GetRoute(Plan.RoutePosition.GetPoint, DestinationPoint))
        End If
    End Sub

    Protected Sub DepleteFuel(ByVal DistanceTravelled As Double)
        FuelLitres -= Vehicles.FuelEconomy(VehicleType, DistanceTravelled)
    End Sub

    Public Sub Refuel()
        Dim FuelLitresPurchased As Double = Vehicles.FuelTankSize(VehicleType) - FuelLitres
        Dim FuelPurchasePrice As Decimal = Vehicles.FuelCost(VehicleType, FuelLitresPurchased)
        NoticeBoard.FuelBill += FuelPurchasePrice
        FuelCosts += FuelPurchasePrice
        FuelLitres = Vehicles.FuelTankSize(VehicleType)
        SimulationState.NewEvent(AgentID, LogMessages.Refuel(FuelLitresPurchased, FuelPurchasePrice))
    End Sub

    Public Function GetVehicleString() As String
        Return Vehicles.Name(VehicleType)
    End Function

    Public Function GetVehicleMaxCapacity() As Double
        Return Vehicles.Capacity(VehicleType)
    End Function

    Public Function GetVehicleCapacityPercentage() As Double
        Return 100 * (1 - Plan.CapacityLeft / Vehicles.Capacity(VehicleType))
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

    Function FuelTankIsFull() As Boolean
        Return FuelLitres / Vehicles.FuelTankSize(VehicleType) > FUEL_TANK_FULL_THRESHOLD
    End Function

    Function EmergencyRefuelRequired() As Boolean
        Return FuelLitres / Vehicles.FuelTankSize(VehicleType) < FUEL_TANK_LOW_THRESHOLD
    End Function

    Function GetRouteForRefuel() As Route
        Dim CurrentPoint As HopPosition = Plan.RoutePosition.GetPoint
        Dim NearestFuel As HopPosition = Map.GetNearestFuelPoint(CurrentPoint)
        Dim RouteToFuel As Route = RouteCache.GetRoute(CurrentPoint, NearestFuel)
        Return RouteToFuel
    End Function
End Class
