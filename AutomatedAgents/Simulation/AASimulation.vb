Public MustInherit Class AASimulation

    Property Agents As New List(Of Agent)
    Property IsRunning As Boolean = False

    Protected Time As TimeSpan
    Protected TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)

    Overridable Sub AddAgent(ByVal Map As StreetMap)
        Dim Agent As New Agent(Map, GetRandomColor)
        Agents.Add(Agent)
    End Sub

    'Returns whether the state has changed.
    Overridable Function Tick() As Boolean
        Time = Time.Add(TIME_INCREMENT)

        For Each Agent As Agent In Agents
            Agent.Move()
        Next

        Return True
    End Function

    Function GetTimeString() As String
        If IsRunning Then
            Return Time.ToString
        Else
            Return "paused"
        End If
    End Function

    Sub Start()
        IsRunning = True
    End Sub
    Sub Pause()
        IsRunning = False
    End Sub
End Class
