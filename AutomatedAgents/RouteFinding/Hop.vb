Public Class Hop
    Public FromPoint As RoutingPoint
    Public Way As Way
    Public ToPoint As RoutingPoint

    Public Sub New(ByVal FromPoint As RoutingPoint, ByVal ToPoint As RoutingPoint, ByVal Way As Way)
        Me.FromPoint = FromPoint
        Me.ToPoint = ToPoint
        Me.Way = Way
    End Sub

    Public Sub New(ByVal FromNode As RoutingPoint, ByVal Cell As NodesAdjacencyListCell)
        Me.FromPoint = FromNode
        Me.ToPoint = Cell.Node
        Me.Way = Cell.Way
    End Sub

    Public Function GetCost() As Double
        Return GetDistance(FromPoint, ToPoint)
    End Function

    Public Function GetEstimatedTravelTime() As Double
        Return GetDistance(FromPoint, ToPoint) / Way.GetMaxSpeedKMH(VehicleSize.CAR)
    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim other As Hop = CType(obj, Hop)
        Return FromPoint.Equals(other.FromPoint) And ToPoint.Equals(other.ToPoint) ' And Way = other.Way
    End Function
End Class
