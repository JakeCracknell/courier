Public Class ScatterIdleStrategy
    Implements IIdleStrategy
    Private Agent As Agent
    Private Status As IdleStrategyStatus
    Private LastJobCount As Integer = -1

    Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    'Starting state at point P
    '1. Determine sleeping location S
    'IF refuel needed:
    '    2. Pick an F such that the cost for P->F->S is minimised
    '    3. Go to F
    '    4. Refuel at F
    '5. Go to S
    '6. Sleep until new jobs come in.

    Public Sub Run() Implements IIdleStrategy.Run
        If Agent.TotalCompletedJobs = 0 Then
            'Algorithm is no good if the agents begin in very similar starting positions.
            Exit Sub
        End If
        If LastJobCount <> Agent.TotalCompletedJobs Then
            Status = IdleStrategyStatus.INITIAL 'Previously non-idle
            LastJobCount = Agent.TotalCompletedJobs
        End If

        Select Case Status
            Case IdleStrategyStatus.INITIAL
                Dim SleepingPoint As HopPosition = FindOptimalSleepingPosition()
                If Not Agent.FuelTankIsFull() Then
                    Dim RouteToFuel As Route = GetOptimalFuelRoute(Agent, SleepingPoint)
                    Agent.Plan.SetNewRoute(RouteToFuel)
                    Status = IdleStrategyStatus.GOING_TO_F
                Else
                    Dim RouteStraightToS As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, SleepingPoint)
                    Agent.Plan.SetNewRoute(RouteStraightToS)
                    Status = IdleStrategyStatus.GOING_TO_S
                End If
            Case IdleStrategyStatus.GOING_TO_F
                If Agent.Plan.RoutePosition.RouteCompleted Then
                    Agent.Delayer = New Delayer(SimulationParameters.REFUELLING_TIME_SECONDS)
                    Agent.Refuel()
                    Status = IdleStrategyStatus.REFUELING
                End If
            Case IdleStrategyStatus.REFUELING
                If Not Agent.Delayer.IsWaiting Then 'Refueling complete
                    'The state of the world may have changed, so it is preferable to find a new sleeping point,
                    'rather than using the old one. If it hasn't changed it is an inexpensive query anyway
                    Dim NewOptimalSleepingPoint As HopPosition = FindOptimalSleepingPosition()
                    Dim RouteToS As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, NewOptimalSleepingPoint)
                    Agent.Plan.SetNewRoute(RouteToS)
                    Status = IdleStrategyStatus.GOING_TO_S
                End If
            Case IdleStrategyStatus.GOING_TO_S
                If Agent.Plan.RoutePosition.RouteCompleted Then
                    Status = IdleStrategyStatus.SLEEPING
                End If
            Case IdleStrategyStatus.SLEEPING
                'Do nothing. When jobs come in, this function will stop being called.
                'and when it is next called, the status will reset to INITIAL.
        End Select
    End Sub

    Private Function FindOptimalSleepingPosition() As HopPosition
        Dim BestNode As Node = Nothing
        Dim BestMaxMinDistance As Double
        For Each N As Node In Agent.Map.Nodes
            If N.Connected Then
                Dim NeighbourDistances() As Double = {Agent.Map.Bounds.MaxLatitude - N.Latitude, _
                                           N.Latitude - Agent.Map.Bounds.MinLatitude, _
                                           Agent.Map.Bounds.MaxLongitude - N.Longitude, _
                                           N.Longitude - Agent.Map.Bounds.MinLongitude, _
                                           NoticeBoard.AgentPositions.Min(Function(Pos)
                                                                              Return Math.Sqrt((Pos.GetLatitude - N.GetLatitude) ^ 2 + (Pos.GetLongitude - N.GetLongitude) ^ 2)
                                                                          End Function)}
                Dim Min As Double = NeighbourDistances.Min()
                If Min > BestMaxMinDistance Then
                    If Min = BestMaxMinDistance Then
                        Debug.Write(Min & " ")
                    End If
                    BestMaxMinDistance = Min
                    BestNode = N
                End If
            End If
        Next
        Return Agent.Map.NodesAdjacencyList.GetHopPositionFromNode(BestNode.ID)
    End Function
End Class
