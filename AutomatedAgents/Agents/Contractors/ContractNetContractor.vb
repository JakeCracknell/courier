Class ContractNetContractor
    Implements IContractor

    Public Const NO_BID As Double = -1
    Private JobToReview As CourierJob
    Private JobToBeAwarded As CourierJob
    Private CurrentBid As Double = NO_BID
    Private AwardedJob As CourierJob = Nothing
    Private Agent As Agent
    Private TentativePlanner As IPlanner = Nothing
    Private Policy As ContractNetPolicy

    Property Planner As IPlanner = Nothing

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
        Dim CurrentDrivingCost As Double = If(Planner Is Nothing, 0, Agent.Plan.UpdateAndGetCost())
        Dim StartingTime As TimeSpan = NoticeBoard.Time + Agent.Plan.GetDiversionTimeEstimate
        Select Case Policy
            Case ContractNetPolicy.CNP1
                'Only if idle, bid S->A->B cost. No bid if cannot be done on time
                If Agent.Plan.IsIdle() Then
                    Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition, StartingTime)
                    Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(StartingTime) + Customers.WaitTimeAvg
                    Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition, StartingTime + Route1Time)
                    Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(StartingTime + Route1Time)
                    If StartingTime + MinTime > _
                        JobToReview.Deadline - SimulationParameters.DEADLINE_REDUNDANCY Then
                        Exit Sub
                    End If
                    CurrentBid = Route1.GetCostForAgent(Agent) + Route2.GetCostForAgent(Agent)
                End If
            Case ContractNetPolicy.CNP2
                'Sum current route times and bid only if the job can be done by the deadline. Bid the cost of appending it to the end of its route.
                'It will still bid if older jobs are going to be delivered late. It only cares about the new one :).
                Dim TimeSum As TimeSpan = TimeSpan.Zero
                For Each R As Route In Agent.Plan.Routes
                    TimeSum += R.GetEstimatedTime(StartingTime + TimeSum) + _
                        SimulationParameters.DEADLINE_REDUNDANCY + _
                            Customers.WaitTimeAvg
                Next
                Dim StartingPoint As IPoint = If(Agent.Plan.IsIdle, Agent.Plan.RoutePosition.GetPoint, Agent.Plan.WayPoints.Last.Position)
                Dim Route1 As Route = RouteCache.GetRoute(StartingPoint, JobToReview.PickupPosition, StartingTime + TimeSum)
                TimeSum += Route1.GetEstimatedTime(StartingTime + TimeSum) + Customers.WaitTimeAvg
                Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition, StartingTime + TimeSum)
                TimeSum += Route2.GetEstimatedTime(StartingTime + TimeSum)
                If StartingTime + TimeSum > JobToReview.Deadline - SimulationParameters.DEADLINE_REDUNDANCY Then
                    Exit Sub
                End If
                CurrentBid = Route1.GetCostForAgent(Agent) + Route2.GetCostForAgent(Agent)
            Case ContractNetPolicy.CNP3
                TentativePlanner = New CNP3Planner(Agent, JobToReview)

                'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
                CurrentBid = If(TentativePlanner.IsSuccessful, TentativePlanner.GetTotalCost - CurrentDrivingCost, NO_BID)

            Case ContractNetPolicy.CNP4, ContractNetPolicy.CNP5
                'Check if the deadline is too slim, even if the agent fulfills it immediately
                Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition, StartingTime)
                Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(StartingTime) + Customers.WaitTimeAvg
                Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition, StartingTime + Route1Time)
                Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(NoticeBoard.Time + Route1Time)
                If StartingTime + MinTime > _
                    JobToReview.Deadline - SimulationParameters.DEADLINE_REDUNDANCY Then
                    Exit Sub
                End If

                TentativePlanner = New NNGAPlanner(Agent, False, JobToReview)

                'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
                CurrentBid = If(TentativePlanner.IsSuccessful, TentativePlanner.GetTotalCost - CurrentDrivingCost, NO_BID)
        End Select

        'Sometimes the planner will be directed to a lower cost route with this new job, making the difference negative.
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

    Function CollectJob() As CourierJob Implements IContractor.CollectJob
        Planner = TentativePlanner
        Dim Temp As CourierJob = AwardedJob
        AwardedJob = Nothing
        Return Temp
    End Function

    Function CNP5ImmediateBid(ByVal Job As CourierJob) As Double
        'In case a job has just been awarded and the CNP strategy has not called CollectJob yet.
        If AwardedJob IsNot Nothing Then
            Agent.Plan = TentativePlanner.GetPlan
            AwardedJob = Nothing
        End If

        'Common basic preliminary checks:
        If Job.CubicMetres > Agent.GetVehicleMaxCapacity Then
            Return NO_BID
        End If

        'The current cost is 0 if idle or whatever the updated plan says. UpdateAndGetCost() necessary for all policies
        Dim CurrentDrivingCost As Double = If(Planner Is Nothing, 0, Agent.Plan.UpdateAndGetCost())
        Dim StartingTime As TimeSpan = NoticeBoard.Time + Agent.Plan.GetDiversionTimeEstimate

        'Check if the deadline is too slim, even if the agent fulfills it immediately
        Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, Job.PickupPosition, StartingTime)
        Dim Route1Time As TimeSpan = Route1.GetEstimatedTime(StartingTime) + Customers.WaitTimeAvg
        Dim Route2 As Route = RouteCache.GetRoute(Job.PickupPosition, Job.DeliveryPosition, StartingTime + Route1Time)
        Dim MinTime As TimeSpan = Route1Time + Route2.GetEstimatedTime(NoticeBoard.Time + Route1Time)
        If StartingTime + MinTime > _
            Job.Deadline - SimulationParameters.DEADLINE_REDUNDANCY Then
            Return NO_BID
        End If

        TentativePlanner = New NNGAPlanner(Agent, False, Job)

        'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
        Return If(TentativePlanner.IsSuccessful, TentativePlanner.GetTotalCost - CurrentDrivingCost, NO_BID)
    End Function
    Sub CNP5ImmediateAward()
        Debug.Assert(TentativePlanner IsNot Nothing AndAlso TentativePlanner.GetPlan IsNot Nothing)
        Agent.Plan = TentativePlanner.GetPlan
    End Sub

    Public Function GetID() As Integer Implements IContractor.GetID
        Return Agent.AgentID
    End Function
End Class
