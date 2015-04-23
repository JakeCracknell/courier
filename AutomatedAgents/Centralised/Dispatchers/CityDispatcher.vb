Public Class CityDispatcher
    Implements IDispatcher

    'Probabilities of dispatch by hour of day, for weekdays and weekends
    Private ReadOnly P_WD_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 10, 15, 20, 20, 20, 20, 20, 20, 20, 18, 14, 10, 6, 4, 3, 2, 1}
    Private ReadOnly P_WE_Dispatch As Double() = {1, 1, 1, 1, 1, 1, 1, 5, 7, 10, 10, 10, 10, 10, 10, 10, 9, 7, 5, 3, 2, 1, 1, 1}

    'Business deliveries are more commonly BOOKED between 7am and 5pm.
    Private Const START_OF_BUSINESS_HOUR As Byte = 7
    Private Const END_OF_BUSINESS_HOUR As Byte = 17

    Private Const ALL_X2B_DELIVERIES_HAVE_EOB_DEADLINE_WHERE_POSSIBLE As Boolean = False
    Private ReadOnly END_OF_BUSINESS_DAY_MIN As TimeSpan = TimeSpan.Parse("17:15:00")
    Private ReadOnly END_OF_BUSINESS_DAY_MAX As TimeSpan = TimeSpan.Parse("17:45:00")

    'Probabilities of different types of order, by period of day: B2B, B2C, C2B, C2C:
    Private ReadOnly P_WD_B_Destinations As Double() = {0.8, 0.05, 0.05, 0.1}
    Private ReadOnly P_WD_NonB_Destinations As Double() = {0.2, 0.1, 0, 0.7}
    Private ReadOnly P_WE_B_Destinations As Double() = {0.5, 0.2, 0.1, 0.2}
    Private ReadOnly P_WE_NonB_Destinations As Double() = {0.2, 0.1, 0, 0.7}

    Private Const SECONDS_IN_HOUR As Integer = 3600
    Private RandomNumberGenerator As New Random(44)
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    'Simulation time starts on a MONDAY at midnight.
    Public Sub Tick() Implements IDispatcher.Tick
        Dim DayOfWeek As DayOfWeek = (Int(NoticeBoard.CurrentTime.TotalDays) + 1) Mod 7
        Dim TimeOfDay As TimeSpan = TimeSpan.FromSeconds(NoticeBoard.CurrentTime.TotalSeconds Mod TimeSpan.FromDays(1).TotalSeconds)

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


        'Probabilistically determine whether the job is B2B, B2C, C2B or C2C:
        Dim RandomSample As Double = RandomNumberGenerator.NextDouble
        Dim Distribution As Double()
        Select Case DayOfWeek
            Case DayOfWeek.Saturday, DayOfWeek.Sunday
                Select Case TimeOfDay.Hours
                    Case Is < START_OF_BUSINESS_HOUR, Is > END_OF_BUSINESS_HOUR
                        Distribution = P_WE_NonB_Destinations
                    Case Else
                        Distribution = P_WE_B_Destinations
                End Select
            Case Else
                Select Case TimeOfDay.Hours
                    Case Is < START_OF_BUSINESS_HOUR, Is > END_OF_BUSINESS_HOUR
                        Distribution = P_WD_NonB_Destinations
                    Case Else
                        Distribution = P_WD_B_Destinations
                End Select
        End Select
        Dim JobType As Integer = 0
        Dim Sum As Double = 0
        For JobType = 0 To Distribution.Length - 1
            Sum += Distribution(JobType)
            If RandomSample < Sum Then
                Exit For
            End If
        Next

        'Given the type of job, randomly pick the pickup and destination points.
        Dim PickupName As String = ""
        Dim DeliveryName As String = ""
        Dim PickupLocation As HopPosition = Nothing
        Dim DeliveryLocation As HopPosition = Nothing
        Select Case JobType
            Case 0 'B2B
                Dim RandomBusinessFrom As Node = Map.Businesses(RandomNumberGenerator.Next(0, Map.Businesses.Count))
                PickupLocation = Map.GetNearestPoint(RandomBusinessFrom)
                Dim RandomBusinessTo As Node = Map.Businesses(RandomNumberGenerator.Next(0, Map.Businesses.Count))
                DeliveryLocation = Map.GetNearestPoint(RandomBusinessTo)
                PickupName = GenerateWaypointName(PickupLocation, RandomBusinessFrom)
                DeliveryName = GenerateWaypointName(DeliveryLocation, RandomBusinessTo)
            Case 1 'B2C
                Dim RandomBusinessFrom As Node = Map.Businesses(RandomNumberGenerator.Next(0, Map.Businesses.Count))
                PickupLocation = Map.GetNearestPoint(RandomBusinessFrom)
                DeliveryLocation = Map.NodesAdjacencyList.GetRandomPoint
                PickupName = GenerateWaypointName(PickupLocation, RandomBusinessFrom)
                DeliveryName = GenerateWaypointName(DeliveryLocation)
            Case 2 'C2B
                PickupLocation = Map.NodesAdjacencyList.GetRandomPoint
                Dim RandomBusinessTo As Node = Map.Businesses(RandomNumberGenerator.Next(0, Map.Businesses.Count))
                DeliveryLocation = Map.GetNearestPoint(RandomBusinessTo)
                PickupName = GenerateWaypointName(PickupLocation)
                DeliveryName = GenerateWaypointName(DeliveryLocation, RandomBusinessTo)
            Case 3 'C2C
                PickupLocation = Map.NodesAdjacencyList.GetRandomPoint
                DeliveryLocation = Map.NodesAdjacencyList.GetRandomPoint
                PickupName = GenerateWaypointName(PickupLocation)
                DeliveryName = GenerateWaypointName(DeliveryLocation)
        End Select

        'Assign a deadline, based on the direct route time (and the extra time the client can wait)
        'Optionally, _2B deliveries require a deadline uniformly distributed around the end of the business day
        Dim Deadline As TimeSpan
        Dim DirectRoute As Route = RouteCache.GetRoute(PickupLocation, DeliveryLocation)
        If (JobType = 0 Or JobType = 2) AndAlso _
                ALL_X2B_DELIVERIES_HAVE_EOB_DEADLINE_WHERE_POSSIBLE AndAlso _
                TimeOfDay.Hours >= START_OF_BUSINESS_HOUR AndAlso _
                TimeOfDay.Hours <= END_OF_BUSINESS_HOUR AndAlso _
                TimeOfDay + DirectRoute.GetEstimatedTime < END_OF_BUSINESS_DAY_MIN Then
            Deadline = NoticeBoard.CurrentTime - TimeOfDay + _
                TimeSpan.FromSeconds(RandomNumberGenerator.Next( _
                                     END_OF_BUSINESS_DAY_MIN.TotalSeconds, END_OF_BUSINESS_DAY_MAX.TotalSeconds))
        Else
            'A gamma distribution, giving a range from 0 to infinity. Mode of (alpha-1)*theta = +1h
            'Spread mostly between 0.5 and 4
            Deadline = NoticeBoard.CurrentTime + DirectRoute.GetEstimatedTime() + TimeSpan.FromHours(ProbabilityDistributions.Gamma(2, 1))
        End If

        'Generate a random package size from an exponential distribution (as many packages will be documents).
        Dim Size As Double = ProbabilityDistributions.Exponential(SimulationParameters.PackageSizeLambda, RandomNumberGenerator.NextDouble)
        Size = Math.Max(Size, SimulationParameters.CubicMetresMin)

        Dim CourierJob As New CourierJob(PickupLocation, DeliveryLocation, PickupName, DeliveryName, Size, Deadline)
        NoticeBoard.AddJob(CourierJob)
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
