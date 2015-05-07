Public Class CourierJob
    Private Const DEADLINE_TO_DEPOT As Integer = 60 * 60 * 12 ' 12 hours TODO poo
    Private Const DEPOT_STRING_IDENTIFIER As String = "DEPOT"

    Public ReadOnly JobID As Integer = UIDAssigner.NewID("job")
    Public ReadOnly PickupPosition As HopPosition
    Public DeliveryPosition As HopPosition
    Public ReadOnly OriginalDeliveryPosition As HopPosition
    Public Deadline As TimeSpan
    Public ReadOnly CubicMetres As Double
    Public CustomerFee As Decimal 'Potentially refunded
    Public OriginalCustomerFee As Decimal
    Public Status As JobStatus = JobStatus.UNALLOCATED
    Public PickupName As String
    Public DeliveryName As String
    Public OriginalDeliveryName As String

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal PickupName As String, ByVal DeliveryName As String, ByVal Size As Double, ByVal Deadline As TimeSpan)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
        Me.OriginalDeliveryPosition = DeliveryPosition
        Me.PickupName = PickupName
        Me.DeliveryName = DeliveryName
        Me.OriginalDeliveryName = DeliveryName
        Me.CubicMetres = Size
        Me.Deadline = Deadline
    End Sub
    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition)
        Me.New(PickupPosition, DeliveryPosition, PickupPosition.ToString, DeliveryPosition.ToString, _
               Math.Max(SimulationParameters.CubicMetresMin, ProbabilityDistributions.Exponential(SimulationParameters.PackageSizeLambda, RNG.R("simple_size").NextDouble)), _
                    NoticeBoard.Time.Add( _
                    TimeSpan.FromMinutes(ProbabilityDistributions.Gaussian(RNG.R("simple_job_deadline"), SimulationParameters.SimpleDeadlineAverage))))
    End Sub

    'These functions return the time taken to collect/deliver. If the customer
    'does not act in time, the delivery takes MAX time.
    'It fails and is cancelled with partial
    'refund given or rerouted to the depot at some time (no deadline).
    Function Collect() As Integer
        Debug.Assert(Status = JobStatus.PENDING_PICKUP)

        'Assume best case scenario with outbound depot jobbies.
        If PickupName.Contains(DEPOT_STRING_IDENTIFIER) Then
            Status = JobStatus.PENDING_DELIVERY
            Return Customers.WaitTimeMinSeconds
        End If

        'Pickup fails
        If RNG.R("pickup").NextDouble < SimulationParameters.ProbPickupFail Then
            Status = JobStatus.CANCELLED
            Return Customers.WaitTimeMaxSeconds
        End If

        'Usual case - pickup is successful
        Status = JobStatus.PENDING_DELIVERY
        Return GenerateRandomWaitTime()
    End Function

    Function Deliver() As Integer
        Debug.Assert(Status = JobStatus.PENDING_DELIVERY)

        'Full refund if late - even if going to depot or 
        If Deadline < NoticeBoard.Time Then
            'Still happens occasionally.
            Debug.WriteLine("Minutes late: " & (NoticeBoard.Time - Deadline).TotalMinutes)
            FullRefund()
        End If

        'Assume best case scenario with returned jobs.
        If IsFailedDelivery() OrElse DeliveryName.Contains(DEPOT_STRING_IDENTIFIER) Then
            Status = JobStatus.COMPLETED
            Return Customers.WaitTimeMinSeconds
        End If

        'Delivery fails. Never the case if the delivery position is a depot.
        If RNG.R("dropoff").NextDouble < SimulationParameters.ProbDeliveryFail Then
            Status = JobStatus.PENDING_DELIVERY
            DeliveryPosition = Nothing
            Deadline += TimeSpan.FromSeconds(DEADLINE_TO_DEPOT)
            Return Customers.WaitTimeMaxSeconds
        End If

        'Usual case - delivery is successful.
        Status = JobStatus.COMPLETED
        Return GenerateRandomWaitTime()
    End Function

    Function GetDirectRoute() As Route
        Return GetDirectRoute(NoticeBoard.Time)
    End Function
    Function GetDirectRoute(ByVal Time As TimeSpan) As Route
        Return RouteCache.GetRoute(PickupPosition, OriginalDeliveryPosition, Time)
    End Function

    Function IsFailedDelivery() As Boolean
        Return Not OriginalDeliveryPosition.Equals(DeliveryPosition)
    End Function

    Public Sub CalculateFee(ByVal EstimatedExtraCostOfDriving As Double)
        OriginalCustomerFee = Math.Round(SimulationParameters.FeeBasePrice + EstimatedExtraCostOfDriving * SimulationParameters.FeeHourlyPrice, 2)
        CustomerFee = OriginalCustomerFee
    End Sub

    'Uncollected deliveries are partially refunded to the base fee
    'Or if the customer was charged less than this, no refund.
    'Cost saving could be in the 1000s if late deliveries were avoided.
    'eg. the delivery waypoint of this job that would otherwise be late.
    Public Sub PartialRefund(ByVal CostSaving As Double)
        Dim AmountToRefund As Double = CostSaving * SimulationParameters.FeeHourlyPrice
        CustomerFee = Math.Round(Math.Max(SimulationParameters.FeeBasePrice, CustomerFee - AmountToRefund), 2)
    End Sub

    'Full refund if the deadline is missed.
    Private Sub FullRefund()
        CustomerFee = 0
    End Sub

    'Uniform distribution
    Private Function GenerateRandomWaitTime()
        Return RNG.R("job_wait").Next(Customers.WaitTimeMinSeconds, Customers.WaitTimeMaxSeconds)
    End Function

    Public Overrides Function ToString() As String
        Return "{" & Math.Round(CubicMetres, 2) & "} " & PickupPosition.ToString & "->" & DeliveryPosition.ToString
    End Function
End Class
