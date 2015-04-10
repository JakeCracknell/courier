Class ContractNetContractor
    Public Const NO_BID As Double = -1
    Private JobToReview As CourierJob
    Private JobToBeAwarded As CourierJob
    Private CurrentBid As Double = NO_BID
    Private AwardedJob As CourierJob = Nothing
    Private Agent As Agent
    Private TentativeSolver As NNSearchSolver = Nothing
    Private Policy As ContractNetPolicy

    Property Solver As NNSearchSolver = Nothing

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

        Select Case Policy
            Case ContractNetPolicy.CNP1

            Case ContractNetPolicy.CNP2

            Case ContractNetPolicy.CNP3

            Case ContractNetPolicy.CNP4
                'Check if the deadline is too slim, even if the agent fulfills it immediately
                Dim Route1 As Route = RouteCache.GetRoute(Agent.Plan.RoutePosition.GetPoint, JobToReview.PickupPosition)
                Dim Route2 As Route = RouteCache.GetRoute(JobToReview.PickupPosition, JobToReview.DeliveryPosition)
                Dim MinTime As TimeSpan = Route1.GetEstimatedTime + Route2.GetEstimatedTime
                If NoticeBoard.CurrentTime + MinTime > JobToReview.Deadline - DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB Then
                    Exit Sub
                End If

                'The current cost is 0 if idle or whatever the updated plan says.
                Dim CurrentDrivingCost As Double = If(Solver Is Nothing, 0, Agent.Plan.UpdateAndGetCost())

                TentativeSolver = New NNSearchSolver(Agent.Plan, New SolverPunctualityStrategy(DEADLINE_PLANNING_REDUNDANCY_TIME_PER_ROUTE), Agent.RouteFindingMinimiser, JobToReview)

                'Solution is Nothing iff impossible to fit into schedule (though as we only use NN, this is often untrue)
                CurrentBid = If(TentativeSolver.Solution Is Nothing, NO_BID, TentativeSolver.TotalCost - CurrentDrivingCost)

            Case ContractNetPolicy.CNP5
                Throw New NotImplementedException
        End Select

        'Basic preliminary checks before starting something big





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
End Class
