Module SimulationParameters
    'Variables
    Public SimulationSpeed As Integer = 1
    Public DisplayRefreshSpeed As Integer = 10
    Public DispatchRatePerHour As Integer = 18
    Public DeadlineAverage As Integer = 120
    Public CubicMetresAverage As Double = 0.5
    Public ProbPickupFail As Double = 0.01
    Public ProbDeliveryFail As Double = 0.1

    Public Const CubicMetresMin As Double = 0.0004

    Public dEbUgVaRiAbLe As Object

    'Global Constants
    Public DEADLINE_PLANNING_REDUNDANCY_TIME As TimeSpan = TimeSpan.FromMinutes(3)
    Public Const MAX_POSSIBLE_SPEED_KMH As Integer = 112


End Module
