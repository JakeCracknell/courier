Module SimulationParameters
    'Variables
    Public SimulationSpeed As Integer = 1
    Public DisplayRefreshSpeed As Integer = 10
    Public DispatchRatePerHour As Integer = 60

    'Global Constants
    Public DEADLINE_PLANNING_REDUNDANCY_TIME As TimeSpan = TimeSpan.FromMinutes(3)

End Module
