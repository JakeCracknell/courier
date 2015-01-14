Public Class CourierJob
    Public PickupPosition As HopPosition
    Public DeliveryPosition As HopPosition
    Public Deadline As Long
    Public MinVehicleSize As VehicleSize
    Public Priority As Double

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
    End Sub
End Class
