﻿Public Class NearestNeighbourEuclidianStrategy
    Implements IAgentStrategy
    Private Const MARGINAL_COST_ACCEPTANCE_COEFFICIENT As Double = 10

    Private LastJobConsidered As CourierJob = Nothing

    Private PlannedRoute As New List(Of HopPosition)
    Private PlannedJobRoute As New List(Of CourierJob)
    Private Agent As Agent

    Public Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Public Sub Run() Implements IAgentStrategy.Run
        Debug.WriteLine(Agent.GetVehicleCapacityPercentage)
        'Do nothing if there are no jobs allocated.
        If Agent.AssignedJobs.Count = 0 Then
            GetOneJob()
            Exit Sub
        End If

        GetGoodJobs(Agent.Position)

        'If a route somewhere has just been completed...
        If Agent.Position.RouteCompleted Then
            If Agent.Position.GetRoutingPoint.ApproximatelyEquals(PlannedRoute(0)) Then
                Dim Job As CourierJob = PlannedJobRoute(0)
                Select Case Job.Status
                    Case JobStatus.PENDING_PICKUP
                        Agent.Delayer = New Delayer(Job.Collect())
                        If Job.Status = JobStatus.CANCELLED Then
                            PlannedRoute.RemoveAt(0)
                            PlannedJobRoute.RemoveAt(0)
                            Dim DeliveryIndex As Integer = PlannedJobRoute.IndexOf(Job)
                            PlannedJobRoute.RemoveAt(DeliveryIndex)
                            PlannedRoute.RemoveAt(DeliveryIndex)
                            Agent.AssignedJobs.Remove(Job)
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            Agent.VehicleCapacityUsed += Job.CubicMetres
                            PlannedJobRoute.RemoveAt(0)
                            PlannedRoute.RemoveAt(0)
                        End If
                    Case JobStatus.PENDING_DELIVERY
                        Agent.Delayer = New Delayer(Job.Deliver())
                        If Job.Status = JobStatus.COMPLETED Then
                            Agent.VehicleCapacityUsed -= Job.CubicMetres
                            PlannedJobRoute.RemoveAt(0)
                            PlannedRoute.RemoveAt(0)
                            Agent.AssignedJobs.Remove(Job)
                        ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                            'Fail -> Depot
                            PlannedRoute(0) = Job.DeliveryPosition
                            RecalculateRoute(Agent.Position)
                        End If
                End Select

            End If

            If PlannedRoute.Count > 0 Then
                Agent.Position = New RoutePosition(New AStarSearch(Agent.Position.GetRoutingPoint, PlannedRoute(0), Agent.Map.NodesAdjacencyList, Agent.RouteFindingMinimiser).GetRoute)
            End If
        End If


    End Sub

    Sub GetOneJob()
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count > 0 Then
            Dim BestJob As CourierJob = Nothing
            Dim BestCost As Double = Double.MaxValue
            For Each Job As CourierJob In UnallocatedJobs
                'Cost of getting to pickup position. no longer looks at dropoff
                Dim Cost As Double = GetDistance(Agent.Position.GetRoutingPoint, Job.PickupPosition)

                If BestCost > Cost Then
                    BestCost = Cost
                    BestJob = Job
                End If
            Next

            NoticeBoard.AllocateJob(BestJob)

            Agent.AssignedJobs.Add(BestJob)
            PlannedJobRoute.Add(BestJob)
            PlannedJobRoute.Add(BestJob)
            PlannedRoute.Add(BestJob.PickupPosition)
            PlannedRoute.Add(BestJob.DeliveryPosition)
            LastJobConsidered = Nothing
        End If
    End Sub

    Sub GetGoodJobs(ByVal Position As RoutePosition)
        Dim UnallocatedJobs As List(Of CourierJob) = NoticeBoard.UnallocatedJobs
        If UnallocatedJobs.Count = 0 OrElse UnallocatedJobs.Last.Equals(LastJobConsidered) Then
            'Exit if no new jobs added to noticeboard
            Exit Sub
        End If

        Dim CurrentRouteDistance As Double = New NearestNeighbourSolver(Position.GetRoutingPoint, Agent.AssignedJobs, Agent.GetVehicleCapacityLeft).NNCost
        For i = UnallocatedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnallocatedJobs(i)
            Dim JobsToPlan As New List(Of CourierJob)(Agent.AssignedJobs.Count)
            JobsToPlan.AddRange(Agent.AssignedJobs)
            JobsToPlan.Add(Job)
            Dim NNS As New NearestNeighbourSolver(Position.GetRoutingPoint, JobsToPlan, Agent.GetVehicleCapacityLeft)
            Dim MarginalCost As Double = NNS.NNCost - CurrentRouteDistance

            If MarginalCost * MARGINAL_COST_ACCEPTANCE_COEFFICIENT < CurrentRouteDistance Then
                NoticeBoard.AllocateJob(Job)
                Agent.AssignedJobs.Add(Job)
                PlannedRoute = NNS.PointList
                PlannedJobRoute = NNS.JobList
            End If

        Next

        LastJobConsidered = If(UnallocatedJobs.Count > 0, UnallocatedJobs.Last, Nothing)

    End Sub

    Private Sub RecalculateRoute(ByVal Position As RoutePosition)
        Dim NNS As New NearestNeighbourSolver(Position.GetRoutingPoint, Agent.AssignedJobs, Agent.GetVehicleCapacityLeft)
        Debug.Assert(NNS.PointList IsNot Nothing)

        PlannedRoute = NNS.PointList
        PlannedJobRoute = NNS.JobList
    End Sub
End Class
