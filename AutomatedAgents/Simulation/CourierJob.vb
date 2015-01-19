Public Class CourierJob
    Public PickupPosition As HopPosition
    Public DeliveryPosition As HopPosition
    Public Deadline As Long
    Public MinVehicleSize As VehicleSize
    Public Priority As Double

    Sub New(ByVal PickupPosition As HopPosition, ByVal DeliveryPosition As HopPosition)
        'Debug.WriteLine(PickupPosition.Hop.Way.Name & " -> " & DeliveryPosition.Hop.Way.Name)
        Me.PickupPosition = PickupPosition
        Me.DeliveryPosition = DeliveryPosition
    End Sub
End Class
