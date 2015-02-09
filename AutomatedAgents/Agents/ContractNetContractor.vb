Class ContractNetContractor
    Public Const NO_BID As Double = -1
    Private JobToReview As CourierJob
    Private CurrentBid As Double = NO_BID
    Private AwardedJob As CourierJob = Nothing

    Sub AnnounceJob(ByVal CourierJob As CourierJob)
        JobToReview = CourierJob
    End Sub
    Sub AwardJob()
        AwardedJob = JobToReview
        AwardedJob.Status = JobStatus.PENDING_PICKUP
    End Sub

    Sub PlaceBid(ByVal Agent As Agent)
        If JobToReview Is Nothing Then
            Exit Sub
        End If

        Dim CurrentDrivingCost As Double = 0
        If Agent.AssignedJobs.Count > 0 Then
            CurrentDrivingCost = New NearestNeighbourSolver( _
                                        Agent.Position.GetRoutingPoint, _
                                        Agent.AssignedJobs, _
                                        Agent.GetVehicleCapacityLeft, _
                                        Agent.Map, Agent.RouteFindingMinimiser) _
                                        .NNCost
        End If


        Dim JobsToPlan As New List(Of CourierJob)(Agent.AssignedJobs.Count)
        JobsToPlan.AddRange(Agent.AssignedJobs)
        JobsToPlan.Add(JobToReview)
        Dim NNS As New NearestNeighbourSolver(Agent.Position.GetRoutingPoint, _
                                              JobsToPlan, Agent.GetVehicleCapacityLeft, _
                                              Agent.Map, Agent.RouteFindingMinimiser)

        If NNS.PointList IsNot Nothing Then
            'Impossible to fit into schedule
            'stopped deving here
            CurrentBid = NO_BID
        Else
            CurrentBid = NNS.NNCost - CurrentDrivingCost
        End If

        JobToReview = Nothing
    End Sub

    Function GetBid() As Double
        Return CurrentBid
    End Function

    Function CollectJob() As CourierJob
        Dim Temp As CourierJob = AwardedJob
        AwardedJob = Nothing
        Return Temp
    End Function
End Class
