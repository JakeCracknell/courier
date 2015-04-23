Public Class RuralDispatcher
    Implements IDispatcher

    'Probabilities of dispatch by hour of day, for weekdays and weekends
    Private ReadOnly P_WD_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 10, 15, 20, 20, 20, 20, 20, 20, 20, 18, 14, 10, 6, 4, 3, 2, 1}
    Private ReadOnly P_WE_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 5, 7, 10, 10, 10, 10, 10, 10, 10, 9, 7, 5, 3, 2, 1, 1, 1}

    Private Const SECONDS_IN_HOUR As Integer = 3600
    Private RandomNumberGenerator As New Random(44)
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    'Simulation time starts on a MONDAY at midnight.
    Public Sub Tick() Implements IDispatcher.Tick
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
        If RandomNumberGenerator.NextDouble > ProbabilityOfDispatch Then
            Exit Sub 'No job is dispatched.
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

        Dim Deadline As TimeSpan = NoticeBoard.Time + _
            RouteCache.GetRoute(PickupLocation, DeliveryLocation).GetEstimatedTime + _
            TimeSpan.FromHours(ProbabilityDistributions.Gamma(2, 1))

        'Generate a random package size from an exponential distribution (as many packages will be documents).
        Dim Size As Double = ProbabilityDistributions.Exponential(SimulationParameters.PackageSizeLambda, RandomNumberGenerator.NextDouble)
        Size = Math.Max(Size, SimulationParameters.CubicMetresMin)

        Dim CourierJob As New CourierJob(PickupLocation, DeliveryLocation, PickupName, DeliveryName, Size, Deadline)
        NoticeBoard.PostJob(CourierJob)
    End Sub


    Function GenerateWaypointName(ByVal Position As HopPosition) As String
        Dim Name As String = FirstNameAssigner.AssignName()
        Name = Name(0) & Name.Substring(1).ToLower
        Dim Age As Integer = Int(18 + Rnd() * 82)
        Dim WayName As String = Position.Hop.Way.Name
        If WayName <> "" Then
            WayName = "Unnamed Road"
        End If
        Dim HouseNo As Integer = 1 + Int(ProbabilityDistributions.Exponential(0.03))
        Return String.Format("{0} ({1}), {2} {3}", Name, Age, HouseNo, WayName)
    End Function
    Function GenerateWaypointName(ByVal Position As HopPosition, ByVal Business As Node) As String
        Dim Name As String = Char.ToUpper(Business.Description(0)) & Business.Description.Substring(1)
        Dim WayName As String = Position.Hop.Way.Name
        If WayName = "" Then
            WayName = "Unnamed Road"
        End If
        Dim HouseNo As Integer = 1 + Int(ProbabilityDistributions.Exponential(0.03))
        Return String.Format("{0}, {1} {2}", Name, HouseNo, WayName)
    End Function
End Class
