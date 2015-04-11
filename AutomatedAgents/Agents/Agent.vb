Public Class Agent
    Private Const DEFAULT_KMH As Double = 48
    Private Const FUEL_TANK_FULL_THRESHOLD As Double = 0.95

    Public Const RouteFindingMinimiser As RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
    Public AgentName As String
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

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.New(Map, Color, Vehicles.Type.CAR)
    End Sub
    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color, ByVal VehicleType As Vehicles.Type)
        Me.Map = Map
        Me.Color = Color
        Me.AgentName = AgentNameAssigner.AssignAgentName()
        Me.VehicleType = VehicleType
        Strategy = New ContractNetStrategy(Me, SimulationParameters.CNPVersion)
        IdleStrategy = New SleepingIdleStrategy(Me)
        Refuel()

        'TODO: start at depot, which is also a refuelling station
        Plan = New CourierPlan(Map.GetStartingPoint, Map, RouteFindingMinimiser, GetVehicleMaxCapacity)
        Plan.RoutePosition.Move(VehicleType)
    End Sub

    Public Overridable Sub Move()
        Plan.CapacityLeft = Math.Round(Plan.CapacityLeft, 5)
        If Plan.CapacityLeft > GetVehicleMaxCapacity() Then
            'Throw New OverflowException
            Debug.WriteLine("Vehicle is too full by: " & GetVehicleCapacityPercentage() & "%")
        ElseIf Plan.IsIdle() AndAlso Plan.CapacityLeft <> GetVehicleMaxCapacity() Then
            Debug.WriteLine("Capacity left is non-empty, but vehicle is empty: " & Plan.CapacityLeft)
            'Debug.WriteLine(AgentName & " [" & "".PadRight((1 - Plan.CapacityLeft) * 60, "#") & "".PadRight(Plan.CapacityLeft * 60, " ") & "]")
        End If

        'Reroutes if needed. In the simple case, when a waypoint is reached
        Strategy.Run()

        If Not Plan.RoutePosition.RouteCompleted And Delayer.Tick() Then
            Dim DistanceTravelled As Double = Plan.RoutePosition.Move(VehicleType)
            CurrentSpeedKMH = Plan.RoutePosition.GetCurrentSpeed(VehicleType)
            TotalKMTravelled += DistanceTravelled
            TotalDrivingTime += 1
            DepleteFuel(DistanceTravelled)
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

End Class
