Public Class Agent
    Public CurrentNode As Node
    Public CurrentWay As Way
    Private Map As StreetMap
    Public Color As Color

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.Map = Map
        Me.Color = Color

        WarpToRandomNode()
    End Sub

    Private Sub WarpToRandomNode()
        CurrentNode = Map.NodesAdjacencyList.GetRandomNode
    End Sub

    Public Sub MoveRandomly()
        Dim AdjacentNodes As List(Of NodesAdjacencyListCell) = _
            Map.NodesAdjacencyList.Rows(CurrentNode.ID).Cells
        Dim Cell As NodesAdjacencyListCell = _
            AdjacentNodes(Int(Rnd() * AdjacentNodes.Count))
        CurrentNode = Cell.Node
        CurrentWay = Cell.Way
    End Sub

    Public Sub GetRoute()
        Dim DestinationNode As Node = Map.NodesAdjacencyList.GetRandomNode
        Dim RouteFinder As RouteFinder = New BreadthFirstSearch(CurrentNode, DestinationNode, Map.NodesAdjacencyList)

        MsgBox(RouteFinder.GetCost)
    End Sub
End Class
