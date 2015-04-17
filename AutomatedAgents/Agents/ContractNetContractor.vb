﻿Class ContractNetContractor
    Public Const NO_BID As Double = -1
    Private JobToReview As CourierJob
    Private JobToBeAwarded As CourierJob
    Private CurrentBid As Double = NO_BID
    Private AwardedJob As CourierJob = Nothing
    Private Agent As Agent
    Private TentativeSolver As ISolver = Nothing
    Private Policy As ContractNetPolicy

    Property Solver As ISolver = Nothing

    Sub New(ByVal Agent As Agent, ByVal Policy As ContractNetPolicy)
        Me.Agent = Agent
        Me.Policy = Policy
    End Sub

    Sub AnnounceJob(ByVal CourierJob As CourierJob)
        JobToReview = CourierJob
    End Sub
    Sub AwardJob()
        AwardedJob = JobToBeAwarded
        JobToBeAwarded = Nothing
        AwardedJob.Status = JobStatus.PENDING_PICKUP
    End Sub

    Private Sub EvaluateJobToReview()
        CurrentBid = NO_BID

        'Common basic preliminary checks:
        If JobToReview.CubicMetres > Agent.GetVehicleMaxCapacity Then
            Exit Sub
        End If

        'The current cost is 0 if idle or whatever the updated plan says. UpdateAndGetCost() necessary for all policies
        Dim CurrentDrivingCost As Double = If(Solver Is Nothing, 0, Agent.Plan.UpdateAndGetCost())

        Select Case Policy
            Case ContractNetPolicy.CNP1
                'Only if idle, bid S->A->B cost. No bid if cannot be done on time
                If Agent.Plan.IsIdle() Then
                    Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition)
                    Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition)
                    Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(NoticeBoard.CurrentTime)
                    Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(NoticeBoard.CurrentTime + Route1Time)
                    If NoticeBoard.CurrentTime + Agent.Plan.GetDiversionTimeEstimate + MinTime > _
                        JobToReview.Deadline - SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB Then
                        Exit Sub
                    End If
                    CurrentBid = Route1.GetCostForAgent(Agent) + Route2.GetCostForAgent(Agent)
                End If
            Case ContractNetPolicy.CNP2
                'Sum current route times and bid only if the job can be done by the deadline. Bid the cost of appending it to the end of its route.
                Dim TimeSum As TimeSpan = TimeSpan.Zero
                For Each R As Route In Agent.Plan.Routes
                    TimeSum += R.GetEstimatedTime(NoticeBoard.CurrentTime + TimeSum) + SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB
                Next
                Dim StartingPoint As IPoint = If(Agent.Plan.IsIdle, Agent.Plan.RoutePosition.GetPoint, Agent.Plan.WayPoints.Last.Position)
                Dim Route1 As Route = RouteCache.GetRoute(StartingPoint, JobToReview.PickupPosition) 'TODO use timesum for route
                Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(NoticeBoard.CurrentTime + TimeSum)
                TimeSum += Route1Time
                Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition) 'TODO use timesum for route
                TimeSum += Route2.GetEstimatedTime(NoticeBoard.CurrentTime + TimeSum)
                If NoticeBoard.CurrentTime + Agent.Plan.GetDiversionTimeEstimate + TimeSum > JobToReview.Deadline Then
                    Exit Sub
                End If
                CurrentBid = Route1.GetCostForAgent(Agent) + Route2.GetCostForAgent(Agent)
            Case ContractNetPolicy.CNP3
                TentativeSolver = New CNP3Solver(Agent, JobToReview)

                'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
                CurrentBid = If(TentativeSolver.IsSuccessful, TentativeSolver.GetTotalCost - CurrentDrivingCost, NO_BID)

            Case ContractNetPolicy.CNP4
                'Check if the deadline is too slim, even if the agent fulfills it immediately
                Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition)
                Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition)
                Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(NoticeBoard.CurrentTime)
                Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(NoticeBoard.CurrentTime + Route1Time)
                If NoticeBoard.CurrentTime + MinTime > _
                    JobToReview.Deadline - SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB Then
                    Exit Sub
                End If

                TentativeSolver = New NNSearchSolver(Agent.Plan, _
                        New SolverPunctualityStrategy(SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_ROUTE), _
                        Agent.RouteFindingMinimiser, JobToReview)

                'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
                CurrentBid = If(TentativeSolver.IsSuccessful, TentativeSolver.GetTotalCost - CurrentDrivingCost, NO_BID)

            Case ContractNetPolicy.CNP5
                Throw New NotImplementedException
        End Select

        'Sometimes the solver will be directed to a lower cost route with this new job, making the difference negative.
        CurrentBid = If(CurrentBid <> NO_BID, Math.Max(0, CurrentBid), NO_BID)
    End Sub

    Sub PlaceBid()
        If JobToReview Is Nothing Then
            Exit Sub
        End If
        EvaluateJobToReview()
        JobToBeAwarded = JobToReview
        JobToReview = Nothing
    End Sub

    Function GetBid() As Double
        Dim Temp As Double = CurrentBid
        CurrentBid = NO_BID
        Return Temp
    End Function

    Function CollectJob() As CourierJob
        Solver = TentativeSolver
        Dim Temp As CourierJob = AwardedJob
        AwardedJob = Nothing
        Return Temp
    End Function

    Function CNP5ImmediateBid(ByVal Job As CourierJob) As Double
        'In case a job has just been awarded and the CNP strategy has not called CollectJob yet.
        If AwardedJob IsNot Nothing Then
            Agent.Plan = TentativeSolver.GetPlan
            AwardedJob = Nothing
        End If

        'Common basic preliminary checks:
        If JobToReview.CubicMetres > Agent.GetVehicleMaxCapacity Then
            Return NO_BID
        End If

        'The current cost is 0 if idle or whatever the updated plan says. UpdateAndGetCost() necessary for all policies
        Dim CurrentDrivingCost As Double = If(Solver Is Nothing, 0, Agent.Plan.UpdateAndGetCost())

        'Check if the deadline is too slim, even if the agent fulfills it immediately
        Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition)
        Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition)
        Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(NoticeBoard.CurrentTime)
        Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(NoticeBoard.CurrentTime + Route1Time)
        If NoticeBoard.CurrentTime + MinTime > _
            JobToReview.Deadline - SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB Then
            Return NO_BID
        End If

        TentativeSolver = New NNSearchSolver(Agent.Plan, _
                New SolverPunctualityStrategy(SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_ROUTE), _
                Agent.RouteFindingMinimiser, JobToReview)

        'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
        Return If(TentativeSolver.IsSuccessful, TentativeSolver.GetTotalCost - CurrentDrivingCost, NO_BID)
    End Function
    Sub CNP5ImmediateAward()
        Debug.Assert(TentativeSolver IsNot Nothing AndAlso TentativeSolver.GetPlan IsNot Nothing)
        Agent.Plan = TentativeSolver.GetPlan
    End Sub
End Class
