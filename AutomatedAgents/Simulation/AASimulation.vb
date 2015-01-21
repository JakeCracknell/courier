
Class AASimulation
    Property Agents As New List(Of Agent)
    Property IsRunning As Boolean = False
    Property Dispatcher As SimpleDispatcher

    Private Time As TimeSpan
    Private TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)

    Sub AddAgent(ByVal Map As StreetMap)
        Dim Agent As New Agent(Map, GetRandomColor)
        Agents.Add(Agent)
    End Sub

    'Returns whether the state has changed.
    Function Tick() As Boolean
        Time = Time.Add(TIME_INCREMENT)
        Dim Modified As Boolean = True

        'Iterate through agents in semi-random order
        Dim MidIndex As Integer = Int(Rnd() * Agents.Count)
        For i = MidIndex To Agents.Count - 1
            Agents(i).Move()
        Next
        For i = 0 To MidIndex - 1
            Agents(i).Move()
        Next

        Dispatcher.Tick()
        NoticeBoard.Tidy()
        Return Modified
    End Function

    Function GetSimulationTime() As String
        If IsRunning Then
            Return Time.ToString
        Else
            Return "paused"
        End If
    End Function

    Sub InitialiseDispatcher(ByVal Map As StreetMap)
        Dispatcher = New SimpleDispatcher(Map)
        NoticeBoard.Clear()
    End Sub

    Sub StartSimulation()
        IsRunning = True
    End Sub
    Sub StopSimulation()
        IsRunning = False
    End Sub
End Class
