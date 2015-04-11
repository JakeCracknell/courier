Namespace SimulationParameters
    Module SimulationParameters
        '***** Global Variables/parameters
        Public SimulationSpeed As Integer = 1
        Public DisplayRefreshSpeed As Integer = 1
        Public DispatchRatePerHour As Integer = 18
        Public DeadlineAverage As Integer = 120
        Public CubicMetresAverage As Double = 0.5
        Public ProbPickupFail As Double = 0.01
        Public ProbDeliveryFail As Double = 0.1
        Public FeeBasePrice As Double = 2 'TODO currently unused
        Public FeeHourlyPrice As Double = 0.2 'TODO currently unused
        Public CNPVersion As ContractNetPolicy = ContractNetPolicy.CNP4


        '***** Global Constants
        Public Const REFUELLING_TIME_SECONDS As Integer = 60
        Public Const MAX_POSSIBLE_SPEED_KMH As Integer = 112
        Public Const CubicMetresMin As Double = 0.0004

        'These are supposed to take into account flucutations in traffic and also waiting times at waypoints.
        Public DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB As TimeSpan = TimeSpan.FromMinutes(3)
        Public DEADLINE_PLANNING_REDUNDANCY_TIME_PER_ROUTE As TimeSpan = TimeSpan.FromMinutes(15)



        Public DisplayedDebugVariable As Object
    End Module
End Namespace
