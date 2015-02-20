Public Class NodesAdjacencyListCell
    Public Node As Node
    Public Way As Way
    Public Distance As Double
    Public Sub New(ByVal Node As Node, ByVal Way As Way)
        Me.Node = Node
        Me.Way = Way
    End Sub
    Public Sub New(ByVal Node As Node, ByVal Way As Way, ByVal Distance As Double)
        Me.Node = Node
        Me.Way = Way
        Me.Distance = Distance
    End Sub
End Class
