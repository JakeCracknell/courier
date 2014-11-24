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
            If RouteFinder IsNot Nothing AndAlso RouteFinder.PlannedRoute IsNot Nothing Then
                PlannedRoute = RouteFinder.PlannedRoute
                AwaitingRoute = False
                dEbUgVaRiAbLe = TicksWaited
                TicksWaited = 0
            Else
                'Skip a turn if still waiting on GPS
                TicksWaited += 1
                Exit Sub
            End If
        End If

        If CurrentNode = GetTargetNode() Then
            SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
        Else
            CurrentNode = PlannedRoute(RoutePosition).ToNode
            CurrentWay = PlannedRoute(RoutePosition).Way
            RoutePosition += 1
        End If

    End Sub

    Public Overrides Sub SetRouteTo(ByVal DestinationNode As Node)
        RouteFinder = New AsyncRouteFinder(CurrentNode, DestinationNode, Map.NodesAdjacencyList)
        AwaitingRoute = True
        RoutePosition = 0
    End Sub

    Private Class AsyncRouteFinder
        Private FromNode As Node
        Private ToNode As Node
        Private AdjacencyList As NodesAdjacencyList
        Public PlannedRoute As List(Of Hop)

        Public Sub New(ByVal FromNode As Node, ByVal ToNode As Node, ByVal AdjacencyList As NodesAdjacencyList)
            Me.FromNode = FromNode
            Me.ToNode = ToNode
            Me.AdjacencyList = AdjacencyList
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf Run)
        End Sub

        Protected Sub Run()
            Dim RouteFinder As RouteFinder = New AStarSearch(FromNode, ToNode, AdjacencyList)
            PlannedRoute = RouteFinder.GetRoute
        End Sub
    End Class
End Class
