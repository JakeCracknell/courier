Public Class Hop
    Public FromPoint As IPoint
    Public Way As Way
    Public ToPoint As IPoint

    Public Sub New(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint, ByVal Way As Way)
        Me.FromPoint = FromPoint
        Me.ToPoint = ToPoint
        Me.Way = Way
    End Sub

    Public Sub New(ByVal FromNode As IPoint, ByVal Cell As NodesAdjacencyListCell)
        Me.FromPoint = FromNode
        Me.ToPoint = Cell.Node
        Me.Way = Cell.Way
    End Sub

    Public Function GetCost() As Double
        Return GetDistance(FromPoint, ToPoint)
    End Function

    Public Function GetEstimatedTravelTime() As Double
        If Way IsNot Nothing Then
            Return GetDistance(FromPoint, ToPoint) / Way.GetMaxSpeedKMH(VehicleSize.CAR)
        Else
            Return 0
        End If
    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim other As Hop = CType(obj, Hop)
        Return FromPoint.Equals(other.FromPoint) And ToPoint.Equals(other.ToPoint) ' And Way = other.Way
    End Function
End Class
