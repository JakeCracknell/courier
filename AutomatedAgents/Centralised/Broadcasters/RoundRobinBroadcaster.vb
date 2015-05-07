Public Class RoundRobinBroadcaster
    Implements IBroadcaster

    Private AvailableContractors As New List(Of BasicContractor)
    Private NextAgent As Integer = 0
    Private LastJobBroadcasted As CourierJob = Nothing

    Public Sub BroadcastJob(ByVal Job As CourierJob) Implements IBroadcaster.BroadcastJob
        LastJobBroadcasted = Job
        'Don't need to broadcast!
    End Sub

    Public Function ReallocateJobs(Owner As Integer, ReallocatableJobs As List(Of CourierJob)) As List(Of CourierJob) Implements IBroadcaster.ReallocateJobs
        Return ReallocatableJobs
        'No reallocation allowed. >:(
    End Function

    Sub AwardJobs() Implements IBroadcaster.AwardJobs
        If AvailableContractors.Count <> 0 AndAlso LastJobBroadcasted IsNot Nothing Then
            AvailableContractors(NextAgent).AllocateJob(LastJobBroadcasted)
            LastJobBroadcasted.CalculateFee(LastJobBroadcasted.GetDirectRoute.GetEstimatedHours)
            LastJobBroadcasted.Status = JobStatus.PENDING_PICKUP
            SimulationState.NewEvent(LogMessages.JobAwarded(LastJobBroadcasted.JobID, 0))
            NextAgent = (NextAgent + 1) Mod AvailableContractors.Count
            LastJobBroadcasted = Nothing
        End If
    End Sub

    Public Sub RegisterContractor(Contractor As IContractor) Implements IBroadcaster.RegisterContractor
        AvailableContractors.Add(Contractor)
    End Sub
End Class
