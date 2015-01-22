Public Class CourierJob
    'By default £2 + 20p/km
    Private Const DEFAULT_FEE_BASE As Double = 2
    Private Const DEFAULT_FEE_DISTANCE_MULTIPLIER As Double = 0.2

    'http://www.greenlogistics.org.uk/SiteResources/e6f341e0-125e-4f21-864a-97ebbafdbee2_JE-LRN%20-%20Failed%20deliveries%20-%20Presentation.pdf
    Private Const PROBABILITY_COLLECTION_SUCCESS As Double = 0.99
    Private Const PROBABILITY_DELIVERY_SUCCESS As Double = 0.9

    Private Const CUSTOMER_WAIT_TIME_MIN As Integer = 20 ' 20 sekonds
    Private Const CUSTOMER_WAIT_TIME_MAX As Integer = 120 ' 2 minutes

    Public PickupPosition As HopPosition
    Public DeliveryPosition As HopPosition
    Public Deadline As Long
    Public CubicMetres As Double
    Public CustomerFee As Decimal
    Public Status As JobStatus = JobStatus.UNALLOCATED

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal Size As Double, ByVal Fee As Decimal)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
        Me.CubicMetres = Size
        Me.CustomerFee = Math.Round(Fee, 2)
    End Sub
    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition, ByVal Size As Double)
        Me.New(PickupPosition, DeliveryPosition, Size, _
                            DEFAULT_FEE_BASE + _
                            GetDistance(PickupPosition, DeliveryPosition) * _
                                    DEFAULT_FEE_DISTANCE_MULTIPLIER)
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
        If Rnd() < PROBABILITY_DELIVERY_SUCCESS Then
            Status = JobStatus.PENDING_DELIVERY
            Return GenerateRandomWaitTime()
        End If
        Status = JobStatus.PENDING_DELIVERY
        DeliveryPosition = NoticeBoard.DepotPoint
        Deadline = Long.MaxValue
        Return CUSTOMER_WAIT_TIME_MAX
    End Function


    'Uncollected deliveries are partially refunded to the base fee
    'Or if the customer was charged less than this, no refund.
    Private Sub PartialRefund()
        CustomerFee = Math.Min(DEFAULT_FEE_BASE, CustomerFee)
    End Sub

    Private Function GenerateRandomWaitTime()
        Return New Random().Next(CUSTOMER_WAIT_TIME_MIN, CUSTOMER_WAIT_TIME_MAX)
    End Function
End Class
