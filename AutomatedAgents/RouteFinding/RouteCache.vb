Module RouteCache
    Private Minimiser As RouteFindingMinimiser
    Private NodesAdjacencyList As NodesAdjacencyList
    Private RouteCount As Integer = 0
    Private OuterDictionary As New Dictionary(Of IPoint, Dictionary(Of IPoint, Route))
    'Use ConcurrentDictionary if not thread safe

    Sub Initialise(NAL As AutomatedAgents.NodesAdjacencyList, RouteFindingMinimiser As RouteFindingMinimiser)
        NodesAdjacencyList = NAL
        Minimiser = RouteFindingMinimiser
    End Sub


    Function GetRouteIfCached(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint) As Route
        Dim IntermDict As Dictionary(Of IPoint, Route) = Nothing
        Dim Route As Route = Nothing
        If OuterDictionary.TryGetValue(FromPoint, IntermDict) Then
            IntermDict.TryGetValue(ToPoint, Route)
        End If
        Return Route
    End Function

    Function GetRoute(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint) As Route
        Debug.WriteLine(RouteCount)
        Dim Route As Route = GetRouteIfCached(FromPoint, ToPoint)
        If Route Is Nothing Then
            Route = New AStarSearch(FromPoint, ToPoint, NodesAdjacencyList, Minimiser).GetRoute
        End If
        SyncLock OuterDictionary
            If Not OuterDictionary.ContainsKey(FromPoint) Then
                OuterDictionary.Add(FromPoint, New Dictionary(Of IPoint, Route))
            End If
            If Not OuterDictionary(FromPoint).ContainsKey(ToPoint) Then
                OuterDictionary(FromPoint).Add(ToPoint, Route)
                RouteCount += 1
            End If
        End SyncLock
        Return Route
    End Function



End Module
