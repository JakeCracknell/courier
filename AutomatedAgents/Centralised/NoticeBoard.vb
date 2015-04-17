Namespace NoticeBoard
    Module NoticeBoard
        'Private Const START_DATE As Date = Date.Parse("2015-01-01 00:00:00")
        Public CurrentTime As TimeSpan

        Property UnallocatedJobs As New List(Of CourierJob)
        Property UnpickedJobs As New List(Of CourierJob)
        Property PickedJobs As New List(Of CourierJob)

        Property IncompleteJobs As New List(Of CourierJob) 'In the process of picking up or delivering
        Property CompletedJobs As New List(Of CourierJob) 'Includes jobs where collection or delivery failed
        Property RefusedJobs As New List(Of CourierJob) 'No bids

        Property JobRevenue As Decimal = 0
        Property FuelBill As Decimal = 0
        Property TotalTimeEarly As Long = 0
        Property TotalTimeLate As Long = 0
        Property LateJobs As Integer = 0

        'For CNP Only
        Public AvailableContractors As New List(Of ContractNetContractor)
        Property AgentPositions As HopPosition() = {}

        Function AreJobsWaiting() As Boolean
            Return UnallocatedJobs.Count > 0
        End Function

        Function GetJob() As CourierJob
            If UnallocatedJobs.Count > 0 Then
                Dim Job As CourierJob = UnallocatedJobs(0)
                Job.Status = JobStatus.PENDING_PICKUP
                UnallocatedJobs.RemoveAt(0)
                UnpickedJobs.Add(Job)
                Return Job
            End If
            Return Nothing
        End Function

        'Allocate one particular job
        Sub AllocateJob(ByVal Job As CourierJob)
            UnallocatedJobs.Remove(Job)
            Job.Status = JobStatus.PENDING_PICKUP
            UnpickedJobs.Add(Job)
        End Sub

        Function AddJob(ByVal Job As CourierJob)
            IncompleteJobs.Add(Job)
            UnallocatedJobs.Add(Job)
            Parallel.ForEach(Of ContractNetContractor)(AvailableContractors,
                Sub(Contractor)
                    Contractor.AnnounceJob(Job)
                    Contractor.PlaceBid()
                End Sub)
            SimulationState.NewEvent(LogMessages.JobBroadcasted(Job.JobID))
            Return True
        End Function

        'TODO perhaps experiment with not excluding the owner?
        'TODO: log messages
        Function RetractJobs(ByVal Owner As ContractNetContractor, ByVal RetractableJobs As List(Of CourierJob)) As List(Of CourierJob)
            Dim OtherContractors As New List(Of ContractNetContractor)(AvailableContractors)
            OtherContractors.Remove(Owner)

            Dim OtherContractorsBids(OtherContractors.Count - 1) As Double
            Dim UnallocatedJobs As New List(Of CourierJob)
            For Each Job As CourierJob In RetractableJobs
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
                Else
                    UnallocatedJobs.Add(Job)
                End If
            Next
            Return UnallocatedJobs
        End Function

        Sub AwardJobs()
            If AvailableContractors.Count > 0 AndAlso UnallocatedJobs.Count > 0 Then
                Dim JobOffered As CourierJob = UnallocatedJobs(0)

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
                    SimulationState.NewEvent(LogMessages.BidsReceived(JobOffered.JobID, Bids))
                    Winner.AwardJob()
                    AllocateJob(JobOffered)
                    JobOffered.CalculateFee(BestBid)
                    SimulationState.NewEvent(LogMessages.JobAwarded(JobOffered.JobID, BestBid))
                Else
                    SimulationState.NewEvent(LogMessages.JobRefused(JobOffered.JobID))
                    JobOffered.Status = JobStatus.CANCELLED
                End If
            End If
        End Sub

        Sub Tick()
            AwardJobs()

            For i = UnallocatedJobs.Count - 1 To 0 Step -1
                Dim Job As CourierJob = UnallocatedJobs(i)
                If Job.Status = JobStatus.CANCELLED OrElse Job.Deadline < CurrentTime Then
                    IncompleteJobs.Remove(Job)
                    RefusedJobs.Add(Job)
                    UnallocatedJobs.Remove(Job)
                ElseIf Job.Status <> JobStatus.UNALLOCATED Then
                    UnallocatedJobs.Remove(Job)
                    UnpickedJobs.Add(Job)
                End If
            Next

            For i = PickedJobs.Count - 1 To 0 Step -1
                Dim Job As CourierJob = PickedJobs(i)
                If Job.Status = JobStatus.COMPLETED Then
                    JobRevenue += Job.CustomerFee
                    CompletedJobs.Add(Job)
                    IncompleteJobs.Remove(Job)
                    PickedJobs.Remove(Job)
                    Dim TimeSpare As Integer = (Job.Deadline - CurrentTime).TotalSeconds
                    If TimeSpare < 0 Then
                        LateJobs += 1
                        TotalTimeLate += -TimeSpare
                    Else
                        TotalTimeEarly += TimeSpare
                    End If
                End If
            Next

            For i = UnpickedJobs.Count - 1 To 0 Step -1
                Dim Job As CourierJob = UnpickedJobs(i)
                If Job.Status = JobStatus.PENDING_DELIVERY Then
                    PickedJobs.Add(Job)
                    UnpickedJobs.Remove(Job)
                End If
                If Job.Status = JobStatus.CANCELLED Then
                    JobRevenue += Job.CustomerFee
                    'Debug.WriteLine(JobRevenue)
                    IncompleteJobs.Remove(Job)
                    CompletedJobs.Add(Job)
                    UnpickedJobs.Remove(Job)
                End If
            Next
        End Sub

        Sub Clear()
            CurrentTime = TimeSpan.Zero
            UnallocatedJobs.Clear()
            UnpickedJobs.Clear()
            PickedJobs.Clear()
            IncompleteJobs.Clear()
            CompletedJobs.Clear()
            AvailableContractors.Clear()
            Array.Clear(AgentPositions, 0, AgentPositions.Length)
        End Sub

    End Module
End Namespace
