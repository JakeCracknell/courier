Namespace SimulationParameters
    Module SimulationParameters
        '***** Global Variables/parameters
        Public SimulationSpeed As Integer = 30
        Public DisplayRefreshSpeed As Integer = 10
        Public DispatchRateCoefficient As Double = 1
        Public DeadlineGammaTheta As Double = 1
        Public PackageSizeLambda As Double = 3
        Public ProbPickupFail As Double = 0.01
        Public ProbDeliveryFail As Double = 0.1
        Public FeeBasePrice As Double = 2
        Public FeeHourlyPrice As Double = 5
        Public AStarAccelerator As Double = 8
        Public TrafficDisplayAlpha As Double = 3.0


        'Options selected in frmMain GUI
        Public CNPVersion As ContractNetPolicy = ContractNetPolicy.CNP5
        Public VehicleType As Vehicles.Type = Vehicles.Type.CAR
        Public RoutingStrategy As Integer = 0
        Public IdleStrategy As Integer = 2
        Public FailToDepot As Boolean = True
        Public Dispatcher As Integer = 0
        Public RouteTestingMinimiser As RouteFindingMinimiser = AutomatedAgents.RouteFindingMinimiser.TIME_WITH_TRAFFIC

        '***** Global Constants
        Public SIMULATION_TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)
        Public Const REFUELLING_TIME_SECONDS As Integer = 60
        Public Const MAX_POSSIBLE_SPEED_KMH As Integer = 112
        Public Const MIN_POSSIBLE_SPEED_KMH As Integer = 1
        Public Const CubicMetresMin As Double = 0.0004
        Public Const CubicMetresMax As Double = 0.999
        Public Const SimpleDeadlineAverage As Integer = 120
        Public Const StatisticsTickInterval As Integer = 20
        Public Const COURTESY_CALL_LOCK_TIME_HOURS As Double = 5 / 60
        Public Const PERIODIC_REPLAN As Boolean = True
        Public Const ENABLE_DEPOT_DISPATCHER As Boolean = True

        Public Const DEADLINE_GAMMA_K As Double = 2 'shape

        'These are supposed to take into account fluctuations in traffic,
        'waiting times at waypoints and emergency refuelling.
        Public DEADLINE_REDUNDANCY As TimeSpan = TimeSpan.FromMinutes(4)

        Public DisplayedDebugVariable As Object
    End Module
End Namespace
