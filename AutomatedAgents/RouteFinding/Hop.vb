Public Class Hop
    Public FromPoint As IPoint
    Public Way As Way
    Public ToPoint As IPoint
    Private Distance As Double

    Public Sub New(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint, ByVal Way As Way)
        Me.FromPoint = FromPoint
        Me.ToPoint = ToPoint
        Me.Way = Way
        Me.Distance = HaversineDistance(FromPoint, ToPoint)
    End Sub

    Public Sub New(ByVal FromNode As IPoint, ByVal Cell As NodesAdjacencyListCell)
        Me.FromPoint = FromNode
        Me.ToPoint = Cell.Node
        Me.Way = Cell.Way
        Me.Distance = Cell.Distance
    End Sub

    Public Function GetDistance() As Double
        Return Distance
    End Function

    'TODO traffic
    Public Function GetMinimumTravelTime() As Double
        If Way IsNot Nothing Then
            Return Distance / Way.GetSpeedLimit()
        Else
            Return 0
        End If
    End Function

    Public Function GetEstimatedTravelTimeAtTime(ByVal Time As TimeSpan) As Double
        If Way IsNot Nothing Then
            Return Distance / Way.GetSpeedAtTime(Time) + GetAverageDelayLength(Me, Time) / 3600
        Else
            Return 0
        End If
    End Function

    Public Function GetOptimalFuelUsage(Optional ByVal VehicleType As Vehicles.Type = Vehicles.Type.CAR) As Double
        Dim FuelAndTime As Tuple(Of Double, Double) = Vehicles.OptimalFuelUsageAndTime(Distance, Way.GetSpeedLimit(), VehicleType)
        Return FuelAndTime.Item1
    End Function

    Public Function GetOptimalFuelUsageAtTime(ByVal Time As TimeSpan, Optional ByVal VehicleType As Vehicles.Type = Vehicles.Type.CAR) As Double
        Dim FuelAndTime As Tuple(Of Double, Double) = Vehicles.OptimalFuelUsageAndTime(Distance, Way.GetSpeedAtTime(Time), VehicleType)
        Return FuelAndTime.Item1
    End Function
    Public Function GetEstimatedTravelTimeWithOptimalFuelUsage(ByVal Time As TimeSpan, Optional ByVal VehicleType As Vehicles.Type = Vehicles.Type.CAR) As Double
        Dim FuelAndTime As Tuple(Of Double, Double) = Vehicles.OptimalFuelUsageAndTime(Distance, Way.GetSpeedAtTime(Time), VehicleType)
        Return FuelAndTime.Item2
    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim other As Hop = CType(obj, Hop)
        Return FromPoint.Equals(other.FromPoint) And ToPoint.Equals(other.ToPoint) ' And Way = other.Way
    End Function
End Class
