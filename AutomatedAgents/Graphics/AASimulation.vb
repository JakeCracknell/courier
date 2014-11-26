
Class AASimulation
    Property Agents As New List(Of Agent)
    Property IsRunning As Boolean = False

    Private Time As TimeSpan
    Private TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)

    'TODO: Need to fix the case where the route is null due to disconnected grpah
    'Probably just fix the adj/node list as we don't want in final prog
    Sub AddAgent(ByVal Map As StreetMap)
        Dim Agent As New AgentAsync(Map, GetRandomColor)
        Agent.SetRouteTo(Map.NodesAdjacencyList.GetRandomNode)
        Agents.Add(Agent)
    End Sub

    'Returns whether the state has changed.
    Function Tick() As Boolean
        Time = Time.Add(TIME_INCREMENT)
        Dim Modified As Boolean = False
        For Each Agent As Agent In Agents
            Agent.Move()
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
