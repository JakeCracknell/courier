Public MustInherit Class AASimulation
    Property Map As StreetMap
    Property Agents As New List(Of Agent)
    Property IsRunning As Boolean = False
    Property Statistics As StatisticsLogger

    Overridable Sub AddAgent()
        Dim ID As Integer = UIDAssigner.NewID("agent", 0)
        Dim Agent As New Agent(ID, Map, GetSequentialColor(ID))
        Agents.Add(Agent)
        ReDim Preserve NoticeBoard.AgentPositions(Agents.Count - 1)
        NoticeBoard.AgentPositions(Agents.Count - 1) = Agent.Plan.RoutePosition.GetPoint
    End Sub

    'Returns whether the state has changed.
    Overridable Function Tick() As Boolean
        NoticeBoard.Time += SimulationParameters.SIMULATION_TIME_INCREMENT

        For Each Agent As Agent In Agents
            Agent.Move()
        Next

        Return Agents.Count > 0
    End Function

    Function GetTimeString() As String
        Dim DayOfWeek As DayOfWeek = (Int(NoticeBoard.Time.TotalDays) + 1) Mod 7
        Return DayOfWeek.ToString("G") & " " & NoticeBoard.Time.ToString & If(IsRunning, "", " paused")
    End Function

    Sub InitialiseAllModules()
        NoticeBoard.Initialise()
        UIDAssigner.Initialise()
        RouteCache.Initialise(Map.NodesAdjacencyList, RouteFindingMinimiser.DISTANCE)
        If Statistics Is Nothing Then
            Statistics = New StatisticsLogger(Now, Map)
        End If
        RNG.Initialise()
        SimulationState.Initialise()
    End Sub

    Sub LogStatistics()
        Statistics.Log(Agents)
    End Sub

    Sub Start()
        IsRunning = True
    End Sub
    Sub Pause()
        IsRunning = False
    End Sub
End Class
