Public Class Agent
    Public CurrentNode As Node
    Public CurrentWay As Way
    Protected Map As StreetMap
    Public Color As Color

    Protected PlannedRoute As List(Of Hop)
    Protected RoutePosition As Integer = 0

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.Map = Map
        Me.Color = Color

        WarpToRandomNode()
    End Sub

    Protected Sub WarpToRandomNode()
        CurrentNode = Map.NodesAdjacencyList.GetRandomNode
    End Sub

    Protected Sub MoveRandomly()
        Dim AdjacentNodes As List(Of NodesAdjacencyListCell) = _
            Map.NodesAdjacencyList.Rows(CurrentNode.ID).Cells
        Dim Cell As NodesAdjacencyListCell = _
            AdjacentNodes(Int(Rnd() * AdjacentNodes.Count))
        CurrentNode = Cell.Node
        CurrentWay = Cell.Way
    End Sub

    Public Overridable Sub Move()
        If CurrentNode = GetTargetNode() Then
            SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
        Else
            CurrentNode = PlannedRoute(RoutePosition).ToNode
            CurrentWay = PlannedRoute(RoutePosition).Way
            RoutePosition += 1
        End If
    End Sub

    Public Function GetTargetNode() As Node
        If PlannedRoute IsNot Nothing AndAlso PlannedRoute.Count > 0 Then
            Return PlannedRoute(PlannedRoute.Count - 1).ToNode
        Else
            Return CurrentNode
        End If
    End Function

    Public Overridable Sub SetRouteTo(ByVal DestinationNode As Node)
        Dim RouteFinder As RouteFinder = New AStarSearch(CurrentNode, DestinationNode, Map.NodesAdjacencyList)
        PlannedRoute = RouteFinder.GetRoute
        RoutePosition = 0
    End Sub
End Class
