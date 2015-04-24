Namespace SimulationParameters
    Module SimulationParameters
        '***** Global Variables/parameters
        Public SimulationSpeed As Integer = 30
        Public DisplayRefreshSpeed As Integer = 10
        Public DispatchRateCoefficient As Double = 1
        Public DeadlineAverage As Integer = 120 'TODO fix
        Public PackageSizeLambda As Double = 3
        Public ProbPickupFail As Double = 0.01
        Public ProbDeliveryFail As Double = 0.1
        Public FeeBasePrice As Double = 2
        Public FeeHourlyPrice As Double = 0.2
        Public AStarAccelerator As Double = 1.0
        Public TrafficDisplayAlpha As Double = 3.0 'TODO

        'Options selected in frmMain GUI
        Public CNPVersion As ContractNetPolicy = ContractNetPolicy.CNP4
        Public VehicleType As Vehicles.Type = Vehicles.Type.CAR
        Public RoutingStrategy As Integer = 0
        Public IdleStrategy As Integer = 2
        Public FailToDepot As Boolean = True

        '***** Global Constants
        Public SIMULATION_TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)
        Public Const REFUELLING_TIME_SECONDS As Integer = 60
        Public Const MAX_POSSIBLE_SPEED_KMH As Integer = 112
        Public Const MIN_POSSIBLE_SPEED_KMH As Integer = 1
        Public Const CubicMetresMin As Double = 0.0004
        Public Const CubicMetresMax As Double = 0.999
        Public Const StatisticsTickInterval As Integer = 20

        'These are supposed to take into account fluctuations in traffic,
        'waiting times at waypoints and emergency refuelling.
        Public DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB As TimeSpan = TimeSpan.FromMinutes(4)

        'TODO: In addition to above, this constraint should apply. 
        Public DEADLINE_PLANNING_REDUNDANCY_TIME_PER_ROUTE As TimeSpan = TimeSpan.FromMinutes(15)


        Public DisplayedDebugVariable As Object
    End Module
End Namespace
