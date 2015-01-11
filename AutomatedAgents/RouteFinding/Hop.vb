Public Class Hop
    Public FromNode As Node
    Public Way As Way
    Public ToNode As Node

    Public Sub New(ByVal FromNode As Node, ByVal ToNode As Node, ByVal Way As Way)
        Me.FromNode = FromNode
        Me.ToNode = ToNode
        Me.Way = Way
    End Sub

    Public Sub New(ByVal FromNode As Node, ByVal Cell As NodesAdjacencyListCell)
        Me.FromNode = FromNode
        Me.ToNode = Cell.Node
        Me.Way = Cell.Way
    End Sub

    Public Function GetCost() As Double
        Return GetDistance(FromNode, ToNode)
    End Function

    Public Function GetEstimatedTravelTime() As Double
        Return GetDistance(FromNode, ToNode) / Way.GetMaxSpeedKMH(VehicleSize.CAR)
    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim other As Hop = CType(obj, Hop)
        Return FromNode = other.FromNode And ToNode = other.ToNode ' And Way = other.Way
    End Function
End Class
