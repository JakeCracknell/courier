Public Class SleepingIdleStrategy
    Implements IIdleStrategy
    Private Agent As Agent
    Private RouteToFuel As Route

    Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Public Sub Run() Implements IIdleStrategy.Run
        If Not Agent.FuelTankIsFull() AndAlso Agent.Plan.IsStationary Then
            If Agent.Plan.RoutePosition.Route.Equals(RouteToFuel) Then
                If Agent.Plan.RoutePosition.RouteCompleted AndAlso Not Agent.Delayer.IsWaiting Then
                    Agent.Delayer = New Delayer(SimulationParameters.REFUELLING_TIME_SECONDS)
                    Agent.Refuel()
                Else
                    'Refuelling now
                End If
            Else
                Dim CurrentPoint As HopPosition = Agent.Plan.RoutePosition.GetPoint
                Dim NearestFuel As HopPosition = Agent.Map.GetNearestFuelPoint(CurrentPoint)
                RouteToFuel = RouteCache.GetRoute(CurrentPoint, NearestFuel)
                Agent.Plan.SetNewRoute(RouteToFuel)
            End If
        Else
            'On our way to fuel. Could be interrupted at any time if a job comes in.
            'OR (Once refuelled), we are parked, possibly at the fuel point.
        End If
    End Sub
End Class
