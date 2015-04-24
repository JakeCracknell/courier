Public Class CourierJob
    Public Const CUSTOMER_WAIT_TIME_MIN As Integer = 20 ' 20 sekonds
    Public Const CUSTOMER_WAIT_TIME_MAX As Integer = 2 * 60 ' 2 minutes
    Public Const CUSTOMER_WAIT_TIME_AVG As Integer = _
        (CUSTOMER_WAIT_TIME_MAX + CUSTOMER_WAIT_TIME_MIN) / 2

    Private Const DEADLINE_TO_DEPOT As Integer = 60 * 60 * 12 ' 12 hours

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

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal PickupName As String, ByVal DeliveryName As String, ByVal Size As Double, ByVal Deadline As TimeSpan)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
        Me.OriginalDeliveryPosition = DeliveryPosition
        Me.PickupName = PickupName
        Me.DeliveryName = DeliveryName
        Me.CubicMetres = Size
        Me.Deadline = Deadline
    End Sub
    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition)
        Me.New(PickupPosition, DeliveryPosition, PickupPosition.ToString, DeliveryPosition.ToString, _
               Math.Max(SimulationParameters.CubicMetresMin, ProbabilityDistributions.Exponential(SimulationParameters.PackageSizeLambda, RNG.R("simple_size").NextDouble)), _
                    NoticeBoard.Time.Add( _
                    TimeSpan.FromMinutes(ProbabilityDistributions.Gaussian(RNG.R("simple_job_deadline"), SimulationParameters.DeadlineAverage))))
    End Sub

    'These functions return the time taken to collect/deliver. If the customer
    'does not act in time, the delivery takes MAX time.
    'It fails and is cancelled with partial
    'refund given or rerouted to the depot at some time (no deadline).
    Function Collect() As Integer
        Debug.Assert(Status = JobStatus.PENDING_PICKUP)
        If RNG.R("pickup").NextDouble > SimulationParameters.ProbPickupFail Then
            Status = JobStatus.PENDING_DELIVERY
            Return GenerateRandomWaitTime()
        End If
        Status = JobStatus.CANCELLED
        PartialRefund()
        Return CUSTOMER_WAIT_TIME_MAX
    End Function

    Function Deliver() As Integer
        Debug.Assert(Status = JobStatus.PENDING_DELIVERY)

        If IsFailedDelivery() Then
            Status = JobStatus.COMPLETED
            Return CUSTOMER_WAIT_TIME_MIN
        ElseIf RNG.R("dropoff").NextDouble > SimulationParameters.ProbDeliveryFail Then
            Status = JobStatus.COMPLETED
            If Deadline < NoticeBoard.Time Then
                'Still happens occasionally.
                Debug.WriteLine("Minutes late: " & (NoticeBoard.Time - Deadline).TotalMinutes)
                FullRefund()
            End If
            Return GenerateRandomWaitTime()
        Else
            Status = JobStatus.PENDING_DELIVERY
            DeliveryPosition = Nothing
            Deadline += TimeSpan.FromSeconds(DEADLINE_TO_DEPOT)
            Return CUSTOMER_WAIT_TIME_MAX
        End If
    End Function

    Function GetDirectRoute() As Route
        Return RouteCache.GetRoute(PickupPosition, OriginalDeliveryPosition)
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
    'TODO: should recalculate based on what solver says or triangle inequality
    Private Sub PartialRefund()
        CustomerFee = Math.Min(SimulationParameters.FeeBasePrice, CustomerFee) 'WRONG
    End Sub

    'Full refund if the deadline is missed.
    Private Sub FullRefund()
        CustomerFee = 0
    End Sub

    'Uniform distribution
    Private Function GenerateRandomWaitTime()
        Return RNG.R("job_wait").Next(CUSTOMER_WAIT_TIME_MIN, CUSTOMER_WAIT_TIME_MAX)
    End Function

    Public Overrides Function ToString() As String
        Return "{" & Math.Round(CubicMetres, 2) & "} " & PickupPosition.ToString & "->" & DeliveryPosition.ToString
    End Function
End Class
