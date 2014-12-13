Public Class CourierJob
    Public PickupPosition As WayPosition
    Public DeliveryPosition As WayPosition
    Public Deadline As Long
    Public MinVehicleSize As VehicleSize
    Public Priority As Double

    Sub New(ByVal PickupPosition As WayPosition, ByVal DeliveryPosition As WayPosition)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
    End Sub
End Class
