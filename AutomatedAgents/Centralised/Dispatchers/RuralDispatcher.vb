Public Class RuralDispatcher
    Implements IDispatcher

    'Probabilities of dispatch by hour of day, for weekdays and weekends
    Private ReadOnly P_WD_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 10, 15, 20, 20, 20, 20, 20, 20, 20, 18, 14, 10, 6, 4, 3, 2, 1}
    Private ReadOnly P_WE_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 5, 7, 10, 10, 10, 10, 10, 10, 10, 9, 7, 5, 3, 2, 1, 1, 1}

    Private Const SECONDS_IN_HOUR As Integer = 3600
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    'Simulation time starts on a MONDAY at midnight.
    Public Function Tick() As Boolean Implements IDispatcher.Tick
        Dim DayOfWeek As DayOfWeek = (Int(NoticeBoard.Time.TotalDays) + 1) Mod 7
        Dim TimeOfDay As TimeSpan = TimeSpan.FromSeconds(NoticeBoard.Time.TotalSeconds Mod TimeSpan.FromDays(1).TotalSeconds)

        Dim ProbabilityOfDispatch As Double
        Select Case DayOfWeek
            Case DayOfWeek.Saturday, DayOfWeek.Sunday
                ProbabilityOfDispatch = P_WE_Dispatch(TimeOfDay.Hours)
            Case Else
                ProbabilityOfDispatch = P_WD_Dispatch(TimeOfDay.Hours)
        End Select
        ProbabilityOfDispatch = ProbabilityOfDispatch * SimulationParameters.DispatchRateCoefficient / SECONDS_IN_HOUR

        'A Bernoulli(P) distribution, where P varies by day and hour.
        If RNG.R("dispatcher").NextDouble > ProbabilityOfDispatch Then
            Return False 'No job is dispatched.
        End If

        'Given the type of job, randomly pick the pickup and destination points.
        Dim PickupName As String = ""
        Dim DeliveryName As String = ""
        Dim PickupLocation As HopPosition = Nothing
        Dim DeliveryLocation As HopPosition = Nothing
        PickupLocation = Map.NodesAdjacencyList.GetRandomPoint
        DeliveryLocation = Map.NodesAdjacencyList.GetRandomPoint
        PickupName = GenerateWaypointName(PickupLocation)
        DeliveryName = GenerateWaypointName(DeliveryLocation)
        Dim RouteTime As TimeSpan = RouteCache.GetRoute(PickupLocation, DeliveryLocation).GetEstimatedTime
        Dim Deadline As TimeSpan = NoticeBoard.Time + RouteTime + _
            TimeSpan.FromHours(ProbabilityDistributions.Gamma(RNG.R("deadline"), _
                       SimulationParameters.DEADLINE_GAMMA_K, SimulationParameters.DeadlineGammaTheta))

        'Generate a random package size from an exponential distribution (as many packages will be documents).
        Dim Size As Double = ProbabilityDistributions.Exponential(SimulationParameters.PackageSizeLambda, RNG.R("dispatcher").NextDouble)
        Size = Math.Min(Math.Max(Size, SimulationParameters.CubicMetresMin), SimulationParameters.CubicMetresMax)

        Dim CourierJob As New CourierJob(PickupLocation, DeliveryLocation, PickupName, DeliveryName, Size, Deadline)
        NoticeBoard.PostJob(CourierJob)
        Return True
    End Function


    Private Function GenerateWaypointName(ByVal Position As HopPosition) As String
        Dim Name As String = FirstNameAssigner.AssignName()
        Name = Name(0) & Name.Substring(1).ToLower
        Dim Age As Integer = Int(18 + RNG.R("age").NextDouble * 82)
        Dim WayName As String = Position.Hop.Way.Name
        If WayName = "" Then
            WayName = "Unnamed Road"
        End If
        Dim HouseNo As Integer = 1 + Int(ProbabilityDistributions.Exponential(0.03))
        Return String.Format("{0} ({1}), {2} {3}", Name, Age, HouseNo, WayName)
    End Function
End Class
