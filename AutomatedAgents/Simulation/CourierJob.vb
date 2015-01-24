Public Class CourierJob
    'By default £2 + 20p/km
    Private Const DEFAULT_FEE_BASE As Double = 2
    Private Const DEFAULT_FEE_DISTANCE_MULTIPLIER As Double = 0.2

    'http://www.greenlogistics.org.uk/SiteResources/e6f341e0-125e-4f21-864a-97ebbafdbee2_JE-LRN%20-%20Failed%20deliveries%20-%20Presentation.pdf
    Private Const PROBABILITY_COLLECTION_SUCCESS As Double = 1 '0.99
    Private Const PROBABILITY_DELIVERY_SUCCESS As Double = 1 '0.9

    Private Const CUSTOMER_WAIT_TIME_MIN As Integer = 20 ' 20 sekonds
    Private Const CUSTOMER_WAIT_TIME_MAX As Integer = 20 ' 2 minutes
    Public Const CUSTOMER_WAIT_TIME_AVG As Integer = _
        (CUSTOMER_WAIT_TIME_MAX + CUSTOMER_WAIT_TIME_MIN) / 2

    'TODO: refactor and use System.Date instead
    Private Const DEADLINE_OFFSET_MIN As Integer = 30 * 60 ' 30 minutes
    Private Const DEADLINE_OFFSET_MAX As Integer = 30 * 60 ' 30 minutes
    Private Const DEADLINE_TO_DEPOT As Integer = 60 * 60 * 24 ' 24 hours

    Public PickupPosition As HopPosition
    Public DeliveryPosition As HopPosition
    Public ReadOnly OriginalDeliveryPosition As HopPosition
    Public Deadline As TimeSpan
    Public CubicMetres As Double
    Public CustomerFee As Decimal
    Public Status As JobStatus = JobStatus.UNALLOCATED

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal Size As Double, ByVal Fee As Decimal, ByVal Deadline As TimeSpan)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
        Me.OriginalDeliveryPosition = DeliveryPosition
        Me.CubicMetres = Size
        Me.CustomerFee = Math.Round(Fee, 2)
        Me.Deadline = Deadline
    End Sub
    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal Size As Double)
        Me.New(PickupPosition, DeliveryPosition, Size, _
            DEFAULT_FEE_BASE + _
            GetDistance(PickupPosition, DeliveryPosition) * _
            DEFAULT_FEE_DISTANCE_MULTIPLIER,
                    NoticeBoard.CurrentTime.Add( _
                    TimeSpan.FromSeconds(DEADLINE_OFFSET_MAX)))
    End Sub

    'These functions return the time taken to collect/deliver. If the customer
    'does not act in time, the delivery takes MAX time.
    'It fails and is cancelled with partial
    'refund given or rerouted to the depot at some time (no deadline).
    Function Collect() As Integer
        If Rnd() < PROBABILITY_COLLECTION_SUCCESS Then
            Status = JobStatus.PENDING_DELIVERY
            Return GenerateRandomWaitTime()
        End If
        Status = JobStatus.CANCELLED
        PartialRefund()
        Return CUSTOMER_WAIT_TIME_MAX
    End Function
    Function Deliver() As Integer
        If DeliveryPosition.Equals(NoticeBoard.DepotPoint) Then
            Status = JobStatus.COMPLETED
            Return CUSTOMER_WAIT_TIME_MIN
        ElseIf Rnd() < PROBABILITY_DELIVERY_SUCCESS Then
            Status = JobStatus.COMPLETED
            Debug.WriteLine(String.Format("With {0} minutes to spare!", (Deadline - NoticeBoard.CurrentTime).TotalMinutes))
            If Deadline < NoticeBoard.CurrentTime Then
                FullRefund()
            End If
            Return GenerateRandomWaitTime()
        Else
            Status = JobStatus.PENDING_DELIVERY
            DeliveryPosition = NoticeBoard.DepotPoint
            Deadline += TimeSpan.FromSeconds(DEADLINE_TO_DEPOT)
            Return CUSTOMER_WAIT_TIME_MAX
        End If
    End Function


    'Uncollected deliveries are partially refunded to the base fee
    'Or if the customer was charged less than this, no refund.
    Private Sub PartialRefund()
        CustomerFee = Math.Min(DEFAULT_FEE_BASE, CustomerFee)
    End Sub

    Private Sub FullRefund()
        CustomerFee = 0
    End Sub

    Private Function GenerateRandomWaitTime()
        Return New Random().Next(CUSTOMER_WAIT_TIME_MIN, CUSTOMER_WAIT_TIME_MAX)
    End Function
End Class
