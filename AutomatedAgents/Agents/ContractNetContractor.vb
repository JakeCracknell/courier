Class ContractNetContractor
    Public Const NO_BID As Double = -1
    Private JobToReview As CourierJob
    Private JobToBeAwarded As CourierJob
    Private CurrentBid As Double = NO_BID
    Private AwardedJob As CourierJob = Nothing
    Private Agent As Agent
    Private TentativeSolver As RouteInsertionSolver = Nothing

    Property Solver As RouteInsertionSolver = Nothing

    Sub New(ByVal Agent As Agent)
        Me.Agent = Agent
    End Sub

    Sub AnnounceJob(ByVal CourierJob As CourierJob)
        JobToReview = CourierJob
    End Sub
    Sub AwardJob()
        AwardedJob = JobToBeAwarded
        JobToBeAwarded = Nothing
        AwardedJob.Status = JobStatus.PENDING_PICKUP
    End Sub

    Sub PlaceBid()
        If JobToReview Is Nothing Then
            Exit Sub
        End If

        Dim CurrentDrivingCost As Double = 0
        If Solver IsNot Nothing Then
            CurrentDrivingCost = Agent.Plan.UpdateAndGetCost(Agent.Position)
        End If

        TentativeSolver = New RouteInsertionSolver(Agent.Plan, JobToReview)

        If TentativeSolver.GetPlan Is Nothing Then
            'Impossible to fit into schedule
            CurrentBid = NO_BID
        Else
            CurrentBid = TentativeSolver.RouteCost - CurrentDrivingCost
        End If

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
