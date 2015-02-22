Class AACourierSimulation
    Inherits AASimulation

    Private Dispatcher As IDispatcher

    Sub New(ByVal Map As StreetMap)
        Dispatcher = New SimpleDispatcher(Map)
        'Dispatcher = New DebuggingDispatcher(Map)
        NoticeBoard.Clear()
        NoticeBoard.DepotPoint = Map.NodesAdjacencyList.GetRandomPoint
    End Sub

    'Returns whether the state has changed.
    Public Overrides Function Tick() As Boolean
        Time = Time.Add(TIME_INCREMENT)
        NoticeBoard.CurrentTime = Time
        Dim Modified As Boolean = True

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
        Return Modified
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        NoticeBoard.Clear()
    End Sub

End Class
