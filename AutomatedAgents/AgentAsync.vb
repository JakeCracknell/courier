Public Class AgentAsync
    Inherits Agent

    Private RouteFinder As AsyncRouteFinder
    Private AwaitingRoute As Boolean = True
    Private TicksWaited As Integer = 0

    Public Sub New(ByVal Map As StreetMap, ByVal Color As Color)
        MyBase.New(Map, Color)
    End Sub

    Public Overrides Sub Move()
        If AwaitingRoute Then
            If RouteFinder IsNot Nothing AndAlso RouteFinder.RoutingComplete Then
                If RouteFinder.PlannedRoute IsNot Nothing Then
                    Position = New RoutePosition(RouteFinder.PlannedRoute)
                    AwaitingRoute = False
                Else
                    'This handles the case where the agent tried to route to a disconnected subgraph
                    'What about when it was placed on a small disconnected subgraph - stuck!
                    SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
                    Exit Sub
                End If

                TicksWaited = 0
            Else
                'Skip a turn if still waiting on GPS
                TicksWaited += 1
                Exit Sub
            End If
        End If

        If Position.RouteCompleted() Then
            SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
        Else
            'RoutePosition may be changed here - ByRef!
            Position.GetNextPosition(FIXED_KM_PER_SECOND)

            'JUST FOR FUN
            'CurrentNode.Latitude += IIf(Rnd() < 0.5, 0.0001, -0.0001)
            'CurrentNode.Longitude += IIf(Rnd() < 0.5, 0.0001, -0.0001)
            'If CurrentWay IsNot Nothing Then

            '    CurrentWay.OneWay = Not CurrentWay.OneWay
            'End If

            'RoutePosition += 1
        End If

    End Sub

    Public Overrides Sub SetRouteTo(ByVal DestinationNode As Node)
        RouteFinder = New AsyncRouteFinder(GetCurrentNode(), DestinationNode, Map.NodesAdjacencyList)
        AwaitingRoute = True
        RoutePosition = 0
    End Sub

    Private Class AsyncRouteFinder
        Private FromNode As Node
        Private ToNode As Node
        Private AdjacencyList As NodesAdjacencyList
        Public PlannedRoute As List(Of Hop)
        Public RoutingComplete As Boolean = False 'Might have failed

        Public Sub New(ByVal FromNode As Node, ByVal ToNode As Node, ByVal AdjacencyList As NodesAdjacencyList)
            Me.FromNode = FromNode
            Me.ToNode = ToNode
            Me.AdjacencyList = AdjacencyList
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf Run)
        End Sub

        Protected Sub Run()
            Dim RouteFinder As RouteFinder = New AStarSearch(FromNode, ToNode, AdjacencyList)
            PlannedRoute = RouteFinder.GetRoute
            RoutingComplete = True
        End Sub
    End Class

End Class
