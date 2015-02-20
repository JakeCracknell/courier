Class ContractNetContractor
    Public Const NO_BID As Double = -1
    Private JobToReview As CourierJob
    Private JobToBeAwarded As CourierJob
    Private CurrentBid As Double = NO_BID
    Private AwardedJob As CourierJob = Nothing
    Private Agent As Agent

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
        If JobToReview Is Nothing OrElse Agent.AssignedJobs.Count > 3 Then
            Exit Sub
        End If

        Dim CurrentDrivingCost As Double = 0
        If Agent.AssignedJobs.Count > 0 Then
            CurrentDrivingCost = New TSPSolver( _
                                        Agent.Position.GetRoutingPoint, _
                                        Agent.AssignedJobs, _
                                        Agent.GetVehicleCapacityLeft, _
                                        Agent.Map, Agent.RouteFindingMinimiser) _
                                        .RouteCost
        End If


        Dim JobsToPlan As New List(Of CourierJob)(Agent.AssignedJobs.Count)
        JobsToPlan.AddRange(Agent.AssignedJobs)
        JobsToPlan.Add(JobToReview)
        Dim Solver As New TSPSolver(Agent.Position.GetRoutingPoint, _
                                              JobsToPlan, Agent.GetVehicleCapacityLeft, _
                                              Agent.Map, Agent.RouteFindingMinimiser)

        If Solver.PointList Is Nothing Then
            'Impossible to fit into schedule
            'stopped deving here
            CurrentBid = NO_BID
        Else
            CurrentBid = Solver.RouteCost - CurrentDrivingCost
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
        Dim Temp As CourierJob = AwardedJob
        AwardedJob = Nothing
        Return Temp
    End Function
End Class
