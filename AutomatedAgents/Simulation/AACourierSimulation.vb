Class AACourierSimulation
    Inherits AASimulation

    Private Dispatcher As IDispatcher

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        RouteCache.Initialise(Map.NodesAdjacencyList, RouteFindingMinimiser.DISTANCE)
        Dispatcher = New SimpleDispatcher(Map)
        'Dispatcher = New DebuggingDispatcher(Map)
        NoticeBoard.Clear()
        Map.Depots.Add(Map.NodesAdjacencyList.GetRandomPoint) 'TODO: remove this
        InitialiseLogger()
    End Sub

    'Returns whether the state has changed.
    Public Overrides Function Tick() As Boolean
        Time = Time.Add(TIME_INCREMENT)
        NoticeBoard.CurrentTime = Time
        Dim Modified As Boolean = False

        'Moved here for CNP
        Dispatcher.Tick()
        NoticeBoard.Tick()

        'Iterate through agents in semi-random order
        Dim MidIndex As Integer = Int(Rnd() * Agents.Count)
        For i = MidIndex To Agents.Count - 1
            Agents(i).Move()
        Next
        For i = 0 To MidIndex - 1
            Agents(i).Move()
        Next

        'Dispatcher.Tick()
        'NoticeBoard.Tick()
        Return Agents.Count > 0
    End Function

End Class
