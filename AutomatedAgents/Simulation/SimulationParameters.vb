Module SimulationParameters
    'Variables
    Public SimulationSpeed As Integer = 1
    Public DisplayRefreshSpeed As Integer = 1
    Public DispatchRatePerHour As Integer = 18
    Public DeadlineAverage As Integer = 120
    Public CubicMetresAverage As Double = 0.5
    Public ProbPickupFail As Double = 0.01
    Public ProbDeliveryFail As Double = 0.1
    Public FeeBasePrice As Double = 2
    Public FeeHourlyPrice As Double = 0.2


    Public Const CubicMetresMin As Double = 0.0004

    Public dEbUgVaRiAbLe As Object

    'Global Constants
    Public DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB As TimeSpan = TimeSpan.FromMinutes(3)
    Public DEADLINE_PLANNING_REDUNDANCY_TIME_PER_ROUTE As TimeSpan = TimeSpan.FromMinutes(15)

    Public Const MAX_POSSIBLE_SPEED_KMH As Integer = 112


End Module
