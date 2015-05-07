Public Class ContractNetBroadcaster
    Implements IBroadcaster

    Private AvailableContractors As New List(Of ContractNetContractor)

    Private LastJobBroadcasted As CourierJob = Nothing

    Public Sub BroadcastJob(ByVal Job As CourierJob) Implements IBroadcaster.BroadcastJob
        LastJobBroadcasted = Job
        Parallel.ForEach(Of ContractNetContractor)(AvailableContractors,
                                                                        Sub(Contractor)
                                                                            Contractor.AnnounceJob(Job)
                                                                            Contractor.PlaceBid()
                                                                        End Sub)
    End Sub

    Public Function ReallocateJobs(OwnerID As Integer, ReallocatableJobs As List(Of CourierJob)) As List(Of CourierJob) Implements IBroadcaster.ReallocateJobs
        Dim OtherContractors As List(Of ContractNetContractor)
        OtherContractors = AvailableContractors.Where(Function(C) C.GetID <> OwnerID).ToList

        Dim OtherContractorsBids(OtherContractors.Count - 1) As Double
        Dim UnallocatedJobs As New List(Of CourierJob)
        For Each Job As CourierJob In ReallocatableJobs
            Parallel.For(0, OtherContractors.Count, _
                            Sub(c)
                                OtherContractorsBids(c) = OtherContractors(c).CNP5ImmediateBid(Job)
                            End Sub)
            Dim Winner As ContractNetContractor = Nothing
            Dim BestBid As Double = Double.MaxValue
            For i = 0 To OtherContractors.Count - 1
                If OtherContractorsBids(i) <> ContractNetContractor.NO_BID Then
                    If OtherContractorsBids(i) < BestBid Then
                        BestBid = OtherContractorsBids(i)
                        Winner = OtherContractors(i)
                    End If
                End If
            Next
            If Winner IsNot Nothing Then
                Winner.CNP5ImmediateAward()
                SimulationState.NewEvent(LogMessages.CNP5JobTransfer(Job.JobID, OwnerID, Winner.GetID()))
            Else
                UnallocatedJobs.Add(Job)
            End If
        Next
        Return UnallocatedJobs
    End Function

    Sub AwardJobs() Implements IBroadcaster.AwardJobs
        If LastJobBroadcasted IsNot Nothing Then
            Dim Winner As ContractNetContractor = Nothing
            Dim Bids As New List(Of Double)
            Dim BestBid As Double = Double.MaxValue
            For Each Contractor As ContractNetContractor In AvailableContractors
                Dim Bid As Double = Contractor.GetBid
                If Bid <> ContractNetContractor.NO_BID Then
                    Bids.Add(Bid)
                    If Bid < BestBid Then
                        BestBid = Bid
                        Winner = Contractor
                    End If
                End If

            Next
            If Winner IsNot Nothing Then
                Bids.Sort()
                SimulationState.NewEvent(LogMessages.BidsReceived(LastJobBroadcasted.JobID, Bids))
                Winner.AwardJob()
                LastJobBroadcasted.Status = JobStatus.PENDING_PICKUP
                LastJobBroadcasted.CalculateFee(BestBid)
                SimulationState.NewEvent(LogMessages.JobAwarded(LastJobBroadcasted.JobID, BestBid))
            Else
                SimulationState.NewEvent(LogMessages.JobRefused(LastJobBroadcasted.JobID))
                LastJobBroadcasted.Status = JobStatus.CANCELLED
            End If

            LastJobBroadcasted = Nothing
        End If
    End Sub


    Public Sub RegisterContractor(Contractor As IContractor) Implements IBroadcaster.RegisterContractor
        AvailableContractors.Add(Contractor)
    End Sub

End Class
