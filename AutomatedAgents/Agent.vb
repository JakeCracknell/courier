Public Class Agent
    Protected Const FIXED_KM_PER_SECOND As Double = 0.01341

    Public Position As RoutePosition
    Protected Map As StreetMap
    Public Color As Color

    'Protected PlannedRoute As List(Of Hop)
    Protected RoutePosition As Integer = 0
    Protected NodeToRouteTo As Node

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        Me.Map = Map
        Me.Color = Color

        WarpToRandomNode()
    End Sub

    Protected Sub WarpToRandomNode()
        NodeToRouteTo = Map.NodesAdjacencyList.GetRandomNode
    End Sub

    Protected Sub MoveRandomly()
        Dim AdjacentNodes As List(Of NodesAdjacencyListCell) = _
            Map.NodesAdjacencyList.Rows(Position.GetOldNode.ID).Cells
        Dim Cell As NodesAdjacencyListCell = _
            AdjacentNodes(Int(Rnd() * AdjacentNodes.Count))
        NodeToRouteTo = Cell.Node
    End Sub

    Public Overridable Sub Move()
        If Position.RouteCompleted Then
            SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
        Else
            'RoutePosition may be changed here - ByRef!
            Position.GetNextPosition(FIXED_KM_PER_SECOND)
            'HopPosition = New HopPosition(PlannedRoute(RoutePosition).ToNode)
            'HopPosition.Hop.FromNode.VisitNode()
            'CurrentWay = PlannedRoute(RoutePosition).Way
            'RoutePosition += 1
        End If
    End Sub


    Public Overridable Sub SetRouteTo(ByVal DestinationNode As Node)
        Dim RouteFinder As RouteFinder = New AStarSearch(GetCurrentNode, DestinationNode, Map.NodesAdjacencyList)
        Position = New RoutePosition(RouteFinder.GetRoute)
    End Sub

    Protected Function GetCurrentNode() As Node
        If Position IsNot Nothing AndAlso Position.GetOldNode IsNot Nothing Then
            Return Position.GetOldNode
        Else
            Return Map.NodesAdjacencyList.GetRandomNode
        End If
    End Function
End Class
