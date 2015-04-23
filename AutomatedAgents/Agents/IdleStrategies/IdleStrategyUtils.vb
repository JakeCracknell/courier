Module IdleStrategyUtils
    Public Enum IdleStrategyStatus
        INITIAL
        GOING_TO_F
        REFUELING
        GOING_TO_S
        SLEEPING
    End Enum

    'Ideally, route to a fuel point exactly between the agent and the desired end point.
    'If that route is impossible given current fuel restraints, route to the nearest point first.
    Public Function GetOptimalFuelRoute(ByVal Agent As Agent, ByVal EndPoint As HopPosition) As Route
        Dim Start As HopPosition = Agent.Plan.RoutePosition.GetPoint
        Dim MidPoint As PointF = GetMidpointOfTwoPoints(Start, EndPoint)
        Dim FuelPoints As IOrderedEnumerable(Of Node) = Agent.Map.FuelPoints.OrderBy(Function(N)
                                                                                         Return HaversineDistance(Agent.Plan.RoutePosition.GetPoint, N) + HaversineDistance(N, EndPoint)
                                                                                     End Function)
        Dim BestFuelRoute As Route = Nothing
        Dim BestCost As Double = Double.MaxValue

        'Check the top three results
        For i = 0 To FuelPoints.Count - 1
            Dim Route1 As Route = RouteCache.GetRoute(Start, FuelPoints(i))
            Dim Route2 As Route = RouteCache.GetRoute(FuelPoints(i), EndPoint)
            Dim Cost As Double = Route1.GetCostForAgent(Agent) + Route2.GetCostForAgent(Agent)
            If Cost < BestCost And Route1.GetOptimalFuelUsageWithTraffic(Agent.VehicleType) < Agent.FuelLitres Then
                BestCost = Cost
                BestFuelRoute = Route1
            End If
            If i >= 2 AndAlso BestFuelRoute IsNot Nothing Then
                'Don't check all fuel points. Stop at 3 unless a feasible one has yet to be found.
                Exit For
            End If
        Next

        If BestFuelRoute Is Nothing Then
            BestFuelRoute = RouteCache.GetRoute(Start, FuelPoints(0))
            Debug.WriteLine("ERROR: Agent is stuck with low fuel and cannot reach any local fuel point!")
        End If

        Return BestFuelRoute
    End Function


End Module
