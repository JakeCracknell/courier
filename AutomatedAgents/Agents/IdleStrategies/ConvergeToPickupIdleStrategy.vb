Public Class ConvergeToPickupIdleStrategy
    Implements IIdleStrategy
    Private Agent As Agent
    Private Status As IdleStrategyStatus
    Private LastJobCount As Integer = -1
    Private Const MAX_POINTS_TO_CONSIDER As Integer = 200

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
            'Strategy cannot run if there are no pickup positions to consider
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
        Dim MeanLatitude As Double = 0
        Dim MeanLongitude As Double = 0
        Dim PickupPositionsCount As Integer = Math.Min(MAX_POINTS_TO_CONSIDER, Agent.PickupPoints.Count)
        For i = Agent.PickupPoints.Count - 1 To Math.Max(0, Agent.PickupPoints.Count - MAX_POINTS_TO_CONSIDER) Step -1
            MeanLatitude += Agent.PickupPoints(i).GetLatitude
            MeanLongitude += Agent.PickupPoints(i).GetLongitude
            'Debug.WriteLine(Agent.PickupPoints(i).GetLatitude & "   " & Agent.PickupPoints(i).GetLongitude)
        Next
        MeanLatitude /= PickupPositionsCount
        MeanLongitude /= PickupPositionsCount

     

        Dim BestNode As Node = Agent.Map.NodesAdjacencyList.GetNearestNode(MeanLatitude, MeanLongitude)
        Dim BestHopPosition As HopPosition = Agent.Map.NodesAdjacencyList.GetHopPositionFromNode(BestNode.ID)




        If Not Agent.Map.Bounds.Encloses(BestHopPosition) Then
            'This happened once for Guernsey. Longitude was way off. When fixed, refactor above to use grid.
            FindOptimalSleepingPosition()
        End If



        Return BestHopPosition
    End Function
End Class
