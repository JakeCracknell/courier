Public Structure CourierPlanState
    Dim Point As IPoint
    Dim Time As TimeSpan
    Dim FuelLeft As Double
    Dim CapacityLeft As Double
    Dim WayPointsLeft As List(Of WayPoint)
End Structure
