Public Class CNP3Solver
    Implements ISolver

    Private Solution As CourierPlan = Nothing
    Private Agent As Agent
    Private ExtraJob As CourierJob
    Private TotalCost As Double = 0

    Sub New(ByVal Agent As Agent, ByVal Job As CourierJob)
        Me.Agent = Agent
        Me.ExtraJob = Job
        Solve()
    End Sub

    'Uses variant of nearest neighbour algorithm
    Sub Solve()
        'Does not account for the 5 minute rule for pickups
        'Ensure pending deliveries come first. Later jobs plus new one can be reordered
        Dim JobSolution As New List(Of CourierJob)
        Dim JobsLeft As New List(Of CourierJob)
        For Each J In Agent.Plan.GetCurrentJobs
            If J.Status = JobStatus.PENDING_DELIVERY Then
                JobSolution.Insert(0, J)
            Else
                JobsLeft.Add(J)
            End If
        Next
        JobsLeft.Add(ExtraJob)

        'Set starting state
        Dim State As New CourierPlanState
        State.FuelLeft = Double.MaxValue  'Agent.PetroleumLitres
        State.Time = NoticeBoard.Time + Agent.Plan.GetDiversionTimeEstimate
        State.Point = Agent.Plan.StartPoint
        If JobSolution.Count > 0 Then
            State.FuelLeft -= Agent.Plan.Routes(0).GetOptimalFuelUsageWithoutTraffic(Agent.VehicleType)
            State.Time += Agent.Plan.Routes(0).GetEstimatedTime()
            State.Point = Agent.Plan.Routes(0).GetEndPoint
            TotalCost = Agent.Plan.Routes(0).GetCostForAgent(Agent)
        End If

        Do Until JobsLeft.Count = 0
            Dim BestNextJob As CourierJob = Nothing
            Dim BestNextCost As Double = Double.MaxValue
            For Each J As CourierJob In JobsLeft
                Dim RouteToPickup As Route = RouteCache.GetRoute(State.Point, J.PickupPosition)
                Dim Cost As Double = RouteToPickup.GetCostForAgent(Agent)
                If Not (State.Time + RouteToPickup.GetEstimatedTime + J.GetDirectRoute.GetEstimatedTime < _
                        J.Deadline - SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_WAYPOINT) Then
                    Continue For 'Would not get there on time.
                End If
                'Fuel constraints are ignored
                If Cost < BestNextCost Then
                    BestNextCost = Cost
                    BestNextJob = J
                End If
            Next
            If BestNextJob IsNot Nothing Then
                JobSolution.Add(BestNextJob)
                TotalCost += BestNextCost + BestNextJob.GetDirectRoute.GetCostForAgent(Agent)
                Dim Route1 As Route = RouteCache.GetRoute(State.Point, BestNextJob.PickupPosition) 'TODO route time????????????
                Dim Route2 As Route = BestNextJob.GetDirectRoute
                State.Time += Route1.GetEstimatedTime _
                    + Route2.GetEstimatedTime _
                    + SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_WAYPOINT
                State.FuelLeft -= (Route1.GetOptimalFuelUsageWithoutTraffic(Agent.VehicleType) _
                    + Route2.GetOptimalFuelUsageWithoutTraffic(Agent.VehicleType))
                State.Point = BestNextJob.DeliveryPosition
                JobsLeft.Remove(BestNextJob)
            Else
                'Route impossible
                Exit Sub
            End If
        Loop

        'Route found
        Dim WayPoints As List(Of WayPoint) = WayPoint.CreateWayPointList(JobSolution)
        Solution = New CourierPlan(Agent.Plan.StartPoint, Agent.Map, Agent.RouteFindingMinimiser, Agent.GetVehicleCapacityLeft, Agent.VehicleType, WayPoints)
    End Sub

    Public Function GetPlan() As CourierPlan Implements ISolver.GetPlan
        Return Solution
    End Function

    Public Function GetTotalCost() As Double Implements ISolver.GetTotalCost
        Return TotalCost
    End Function

    Public Function IsSuccessful() As Boolean Implements ISolver.IsSuccessful
        Return Solution IsNot Nothing
    End Function
End Class
