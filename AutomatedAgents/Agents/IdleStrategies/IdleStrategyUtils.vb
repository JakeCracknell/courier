Module IdleStrategyUtils
    Public Enum IdleStrategyStatus
        INITIAL
        GOING_TO_F
        REFUELING
        GOING_TO_S
        SLEEPING
    End Enum

    Public Function GetOptimalFuelRoute(ByVal Agent As Agent, ByVal EndPoint As HopPosition) As Route
        Dim Start As HopPosition = Agent.Plan.RoutePosition.GetPoint
        Dim MidPoint As PointF = GetMidpointOfTwoPoints(Start, EndPoint)
        Dim FuelPoints As IOrderedEnumerable(Of Node) = Agent.Map.FuelPoints.OrderBy(Function(N)
                                                                                         Return HaversineDistance(N, EndPoint)
                                                                                     End Function)
        Dim BestFuelRoute As Route = Nothing
        Dim BestCost As Double = Double.MaxValue

        'Check the top three results
        For i = 0 To Math.Min(FuelPoints.Count - 1, 2)
            Dim Route1 As Route = RouteCache.GetRoute(Start, FuelPoints(i))
            Dim Route2 As Route = RouteCache.GetRoute(FuelPoints(i), EndPoint)
            Dim Cost As Double = Route1.GetCostForAgent(Agent) + Route2.GetCostForAgent(Agent)
            If Cost < BestCost Then
                BestCost = Cost
                BestFuelRoute = Route1
            End If
        Next
        Return BestFuelRoute
    End Function
End Module
