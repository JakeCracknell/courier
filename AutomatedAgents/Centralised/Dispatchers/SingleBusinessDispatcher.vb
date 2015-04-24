Public Class SingleBusinessDispatcher
    Implements IDispatcher
    'Probabilities of dispatch by hour of day, for weekdays and weekends
    Private ReadOnly P_WD_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 10, 15, 20, 20, 20, 20, 20, 20, 20, 18, 14, 10, 6, 4, 3, 2, 1}
    Private ReadOnly P_WE_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 5, 7, 10, 10, 10, 10, 10, 10, 10, 9, 7, 5, 3, 2, 1, 1, 1}

    Private B2C_PERCENTAGE As Double = 0.5 'versus C2B

    Private Const SECONDS_IN_HOUR As Integer = 3600
    Private Map As StreetMap
    Private DepotPosition As HopPosition

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        DepotPosition = Map.NodesAdjacencyList.GetHopPositionFromNode(Map.Depots(0))
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

        Dim B2C As Boolean = RNG.R("sbd_b2c").NextDouble < B2C_PERCENTAGE

        'Given the type of job, randomly pick the pickup and destination points.
        Dim PickupLocation As HopPosition = If(B2C, DepotPosition, Map.NodesAdjacencyList.GetRandomPoint)
        Dim PickupName As String = If(B2C, "DEPOT A", GenerateWaypointName(PickupLocation))
        Dim DeliveryLocation As HopPosition = If(Not B2C, DepotPosition, Map.NodesAdjacencyList.GetRandomPoint)
        Dim DeliveryName As String = If(Not B2C, "DEPOT A", GenerateWaypointName(PickupLocation))

        Dim Deadline As TimeSpan = NoticeBoard.Time + _
            RouteCache.GetRoute(PickupLocation, DeliveryLocation).GetEstimatedTime + _
            TimeSpan.FromHours(ProbabilityDistributions.Gamma(RNG.R("deadline"), 2, 1))

        'Generate a random package size from an exponential distribution (as many packages will be documents).
        Dim Size As Double = ProbabilityDistributions.Exponential(SimulationParameters.PackageSizeLambda, RNG.R("dispatcher").NextDouble)
        Size = Math.Min(Math.Max(Size, SimulationParameters.CubicMetresMin), SimulationParameters.CubicMetresMax)

        Dim CourierJob As New CourierJob(PickupLocation, DeliveryLocation, PickupName, DeliveryName, Size, Deadline)
        NoticeBoard.PostJob(CourierJob)
        Return True
    End Function


    Function GenerateWaypointName(ByVal Position As HopPosition) As String
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
