Module RouteCache
    Private Minimiser As RouteFindingMinimiser
    Private NodesAdjacencyList As NodesAdjacencyList
    Private RouteCount, CacheHits, CacheMisses As Integer

    Private OuterDictionary As Dictionary(Of IPoint, Dictionary(Of IPoint, List(Of Route)))

    Private Const HOURS_UNTIL_STALE As Double = 0.5

    Sub Initialise(NAL As AutomatedAgents.NodesAdjacencyList, RouteFindingMinimiser As RouteFindingMinimiser)
        NodesAdjacencyList = NAL
        Minimiser = RouteFindingMinimiser
        OuterDictionary = New Dictionary(Of IPoint, Dictionary(Of IPoint, List(Of Route)))
    End Sub

    Function GetRouteIfCached(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint) As Route
        Return GetRouteIfCached(FromPoint, ToPoint, NoticeBoard.Time)
    End Function
    Function GetRouteIfCached(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint, ByVal StartTime As TimeSpan) As Route
        Dim InternalDict As Dictionary(Of IPoint, List(Of Route)) = Nothing
        Dim RouteList As List(Of Route) = Nothing
        If OuterDictionary.TryGetValue(FromPoint, InternalDict) Then
            InternalDict.TryGetValue(ToPoint, RouteList)
            If RouteList IsNot Nothing Then
                Return RouteList.Find(Function(R) Math.Abs(R.StartTime.TotalHours - StartTime.TotalHours) < HOURS_UNTIL_STALE)
            End If
        End If
        Return Nothing
    End Function
    Function GetRoute(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint) As Route
        Return GetRoute(FromPoint, ToPoint, NoticeBoard.Time)
    End Function
    Function GetRoute(ByVal FromPoint As IPoint, ByVal ToPoint As IPoint, ByVal StartTime As TimeSpan) As Route
        Dim Route As Route = GetRouteIfCached(FromPoint, ToPoint, StartTime)
        If Route IsNot Nothing Then
            CacheHits += 1
            Return Route
        End If
        Route = New AStarSearch(FromPoint, ToPoint, NodesAdjacencyList, Minimiser, StartTime).GetRoute
        CacheMisses += 1

        SyncLock OuterDictionary
            Dim InternalDict As Dictionary(Of IPoint, List(Of Route)) = Nothing
            Dim RouteList As List(Of Route) = Nothing
            OuterDictionary.TryGetValue(FromPoint, InternalDict)
            If InternalDict Is Nothing Then
                InternalDict = New Dictionary(Of IPoint, List(Of Route))
                OuterDictionary.Add(FromPoint, InternalDict)
            End If
            InternalDict.TryGetValue(ToPoint, RouteList)
            If RouteList Is Nothing Then
                RouteList = New List(Of Route)(1) 'Most lists are of length 1.
                InternalDict.Add(ToPoint, RouteList)
            End If
            RouteList.Add(Route)
            RouteCount += 1
            SimulationParameters.DisplayedDebugVariable = RouteCount
        End SyncLock
        Return Route
    End Function

    'Call this every 24 hours to prevent memory leak.
    Sub CleanUp()
        OuterDictionary = New Dictionary(Of IPoint, Dictionary(Of IPoint, List(Of Route)))
    End Sub

End Module
