﻿Class AACourierSimulation
    Inherits AASimulation

    Private Dispatcher As IDispatcher

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        RouteCache.Initialise(Map.NodesAdjacencyList, RouteFindingMinimiser.DISTANCE)
        Dispatcher = New SimpleDispatcher(Map)
        'Dispatcher = New DebuggingDispatcher(Map)
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
            NoticeBoard.AgentPositions(i Mod Agents.Count) = Agents(i Mod Agents.Count).Plan.RoutePosition.GetPoint
        Next

        Return Agents.Count > 0
    End Function

End Class
