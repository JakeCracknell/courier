Class AACourierSimulation
    Inherits AASimulation

    Private Dispatcher As IDispatcher

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        NoticeBoard.Clear()
        RouteCache.Initialise(Map.NodesAdjacencyList, RouteFindingMinimiser.DISTANCE)
        Dispatcher = If(Map.Businesses.Count > 10, New CityDispatcher(Map), New SimpleDispatcher(Map))
        InitialiseLoggingModules()
    End Sub

    'Returns whether the state has changed.
    Public Overrides Function Tick() As Boolean
        NoticeBoard.CurrentTime += SimulationParameters.SIMULATION_TIME_INCREMENT
        Dim Modified As Boolean = False

        'Moved here for CNP
        Dispatcher.Tick()
        NoticeBoard.Tick()

        'Iterate through agents in semi-random order
        Dim MidIndex As Integer = Int(Rnd() * Agents.Count)
        For i = MidIndex To MidIndex + Agents.Count - 1
            Agents(i Mod Agents.Count).Move()
            If Not HaversineDistance(NoticeBoard.AgentPositions(i Mod Agents.Count), Agents(i Mod Agents.Count).Plan.RoutePosition.GetPoint) < 0.05 Then
                Console.Beep()
            End If
            'If Agents(i Mod Agents.Count).Plan.Routes.Count > 0 Then
            '    Dim Last As HopPosition = Agents(i Mod Agents.Count).Plan.Routes(0).GetStartPoint
            '    For Each R As Route In Agents(i Mod Agents.Count).Plan.Routes
            '        If Not Last.ApproximatelyEquals(R.GetStartPoint) Then
            '            Console.Beep()
            '        End If
            '        Last = R.GetEndPoint
            '    Next
            'End If

            NoticeBoard.AgentPositions(i Mod Agents.Count) = Agents(i Mod Agents.Count).Plan.RoutePosition.GetPoint
        Next

        Return Agents.Count > 0
    End Function

End Class
