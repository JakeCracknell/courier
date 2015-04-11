Public Class CourierJob
    Public Const CUSTOMER_WAIT_TIME_MIN As Integer = 20 ' 20 sekonds
    Public Const CUSTOMER_WAIT_TIME_MAX As Integer = 2 * 60 ' 2 minutes
    Public Const CUSTOMER_WAIT_TIME_AVG As Integer = _
        (CUSTOMER_WAIT_TIME_MAX + CUSTOMER_WAIT_TIME_MIN) / 2

    Private Const DEADLINE_TO_DEPOT As Integer = 60 * 60 * 12 ' 12 hours

    Public ReadOnly JobID As Integer = NewID("job")
    Public ReadOnly PickupPosition As HopPosition
    Public DeliveryPosition As HopPosition
    Public ReadOnly OriginalDeliveryPosition As HopPosition
    Public Deadline As TimeSpan
    Public ReadOnly CubicMetres As Double
    Public CustomerFee As Decimal 'Potentially refunded
    Public OriginalCustomerFee As Decimal
    Public Status As JobStatus = JobStatus.UNALLOCATED

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal Size As Double, ByVal Deadline As TimeSpan)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
        Me.OriginalDeliveryPosition = DeliveryPosition
        Me.CubicMetres = Size
        Me.Deadline = Deadline
    End Sub
    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition)
        Me.New(PickupPosition, DeliveryPosition, _
               Math.Max(SimulationParameters.CubicMetresMin, Gaussian(SimulationParameters.CubicMetresAverage)), _
                    NoticeBoard.CurrentTime.Add( _
                    TimeSpan.FromMinutes(Gaussian(SimulationParameters.DeadlineAverage))))
    End Sub

    'These functions return the time taken to collect/deliver. If the customer
    'does not act in time, the delivery takes MAX time.
    'It fails and is cancelled with partial
    'refund given or rerouted to the depot at some time (no deadline).
    Function Collect() As Integer
        Debug.Assert(Status = JobStatus.PENDING_PICKUP)
        If Rnd() > SimulationParameters.ProbPickupFail Then
            Status = JobStatus.PENDING_DELIVERY
            Return GenerateRandomWaitTime()
        End If
        Status = JobStatus.CANCELLED
        PartialRefund()
        Return CUSTOMER_WAIT_TIME_MAX
    End Function

    Function Deliver() As Integer
        Debug.Assert(Status = JobStatus.PENDING_DELIVERY)

        If IsGoingToDepot() Then
            Status = JobStatus.COMPLETED
            Return CUSTOMER_WAIT_TIME_MIN
        ElseIf Rnd() > SimulationParameters.ProbDeliveryFail Then
            Status = JobStatus.COMPLETED
            If Deadline < NoticeBoard.CurrentTime Then
                'Still happens occasionally.
                Debug.WriteLine("Minutes late: " & (NoticeBoard.CurrentTime - Deadline).TotalMinutes)
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

    Function IsGoingToDepot() As Boolean
        Return Not OriginalDeliveryPosition.Equals(DeliveryPosition)
    End Function

    Public Sub CalculateFee(ByVal EstimatedExtraHoursOfDriving As Double)
        OriginalCustomerFee = Math.Round(SimulationParameters.FeeBasePrice + EstimatedExtraHoursOfDriving * SimulationParameters.FeeHourlyPrice, 2)
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
        Return New Random().Next(CUSTOMER_WAIT_TIME_MIN, CUSTOMER_WAIT_TIME_MAX)
    End Function

    Public Overrides Function ToString() As String
        Return "{" & Math.Round(CubicMetres, 2) & "} " & PickupPosition.ToString & "->" & DeliveryPosition.ToString
    End Function
End Class
