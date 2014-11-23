
Class AASimulation
    Property Agents As New List(Of Agent)
    Property IsRunning As Boolean = False

    Private Time As TimeSpan
    Private TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)


    Sub AddAgent(ByVal Map As StreetMap)
        Agents.Add(New Agent(Map, GetRandomColor))
    End Sub

    'Returns whether the state has changed.
    Function Tick() As Boolean
        Time = Time.Add(TIME_INCREMENT)
        Dim Modified As Boolean = False
        For Each Agent As Agent In Agents
            Agent.MoveRandomly()
            Modified = True
        Next
        Return Modified
    End Function

    Function GetSimulationTime() As String
        If IsRunning Then
            Return Time.ToString
        Else
            Return "paused"
        End If
    End Function

    Sub StartSimulation()
        IsRunning = True
    End Sub
    Sub StopSimulation()
        IsRunning = False
    End Sub
End Class
