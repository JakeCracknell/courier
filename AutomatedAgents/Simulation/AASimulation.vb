Public MustInherit Class AASimulation
    Property Map As StreetMap
    Property Agents As New List(Of Agent)
    Property IsRunning As Boolean = False
    Property Statistics As StatisticsLogger

    Protected TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)

    Overridable Sub AddAgent()
        Dim Agent As New Agent(UIDAssigner.NewID("agent", 0), Map, GetSequentialColor())
        Agents.Add(Agent)
    End Sub

    'Returns whether the state has changed.
    Overridable Function Tick() As Boolean
        NoticeBoard.CurrentTime += TIME_INCREMENT

        For Each Agent As Agent In Agents
            Agent.Move()
        Next

        Return Agents.Count > 0
    End Function

    Function GetTimeString() As String
        Return NoticeBoard.CurrentTime.ToString & If(IsRunning, "", " paused")
    End Function

    Sub InitialiseLoggingModules()
        If Statistics Is Nothing Then 'without this frmStats could cause a synclock exception. TODO refactor
            Statistics = New StatisticsLogger(Now, Map)
        End If
        NoticeBoard.Clear()
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
