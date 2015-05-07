Class AACourierSimulation
    Inherits AASimulation

    Private Dispatcher As IDispatcher

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        InitialiseAllModules()
        Select Case SimulationParameters.Dispatcher
            Case 0
                NoticeBoard.SetDispatcher(If(Map.Businesses.Count > 50, New CityDispatcher(Map), New RuralDispatcher(Map)))
            Case 1
                NoticeBoard.SetDispatcher(New SingleBusinessDispatcher(Map))
        End Select
    End Sub

    'Returns whether the state has changed.
    Public Overrides Function Tick() As Boolean
        NoticeBoard.Time += SimulationParameters.SIMULATION_TIME_INCREMENT
        Dim Modified As Boolean = False

        'New job may be generated, broadcasted, bid for and then awarded.
        NoticeBoard.Tick()

        'Iterate through agents in semi-random order
        Dim MidIndex As Integer = RNG.R("agent_tick").Next(0, Agents.Count)
        For i = MidIndex To MidIndex + Agents.Count - 1
            Agents(i Mod Agents.Count).Move()

            Dim MovedAmount As Double = HaversineDistance(NoticeBoard.AgentPositions(i Mod Agents.Count), Agents(i Mod Agents.Count).Plan.RoutePosition.GetPoint)
            If Not MovedAmount < 0.05 Then
                Debug.WriteLine(MovedAmount * 1000 & "m in one second?")
                SimulationParameters.SimulationSpeed = 1
                Console.Beep() 'TODO: Occasionally might flit by one node. 0.05 is 112mph, so this never should be called.
            End If
            'If Agents(i Mod Agents.Count).Plan.Routes.Count > 0 Then
            '    Dim Last As HopPosition = Agents(i Mod Agents.Count).Plan.Routes(0).GetStartPoint
            '    For Each R As Route In Agents(i Mod Agents.Count).Plan.Routes
            '        If Not Last.ApproximatelyEquals(R.GetStartPoint) Then
            '            Console.Beep(100, 300)
            '        End If
            '        Last = R.GetEndPoint
            '    Next
            'End If

            NoticeBoard.AgentPositions(i Mod Agents.Count) = Agents(i Mod Agents.Count).Plan.RoutePosition.GetPoint
        Next

        Return Agents.Count > 0
    End Function

End Class
