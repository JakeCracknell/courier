Namespace NoticeBoard
    Module NoticeBoard
        Public Time As TimeSpan

        Public UnallocatedJobs, UnpickedJobs, PickedJobs, IncompleteJobs, _
                                CompletedJobs, RefusedJobs As List(Of CourierJob)

        Property JobRevenue As Decimal
        Property BillableJobCost As Double
        Property FuelBill As Decimal
        Property TotalTimeEarly As Long
        Property TotalTimeLate As Long
        Property LateJobs As Integer

        Public Broadcaster As IBroadcaster
        Private Dispatcher As IDispatcher
        Private DepotDispatcher As DepotDispatcher

        Property AgentPositions As HopPosition() = {}

        Sub Initialise()
            Time = TimeSpan.Zero
            JobRevenue = 0
            FuelBill = 0
            TotalTimeEarly = 0
            TotalTimeLate = 0
            LateJobs = 0
            BillableJobCost = 0

            UnallocatedJobs = New List(Of CourierJob)
            UnpickedJobs = New List(Of CourierJob)
            PickedJobs = New List(Of CourierJob)
            IncompleteJobs = New List(Of CourierJob)
            CompletedJobs = New List(Of CourierJob)
            RefusedJobs = New List(Of CourierJob)

            If SimulationParameters.RoutingStrategy = 0 Then
                Broadcaster = New ContractNetBroadcaster()
            Else
                Broadcaster = New RoundRobinBroadcaster()
            End If

            Array.Clear(AgentPositions, 0, AgentPositions.Length)
        End Sub

        Sub SetDispatcher(ByVal AppropriateDispatcher As IDispatcher)
            Dispatcher = AppropriateDispatcher
            DepotDispatcher = New DepotDispatcher()
        End Sub

        Function PostJob(ByVal Job As CourierJob)
            IncompleteJobs.Add(Job)
            UnallocatedJobs.Add(Job)
            Broadcaster.BroadcastJob(Job)
            SimulationState.NewEvent(LogMessages.JobBroadcasted(Job.JobID))
            Return True
        End Function

        Sub Tick()
            Dim JobHasBeenGenerated As Boolean = DepotDispatcher.Tick() OrElse Dispatcher.Tick()
            Broadcaster.AwardJobs()
            TidyJobs()
        End Sub

        Private Sub TidyJobs()
            For i = UnallocatedJobs.Count - 1 To 0 Step -1
                Dim Job As CourierJob = UnallocatedJobs(i)
                If Job.Status = JobStatus.CANCELLED OrElse Job.Deadline < Time Then
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
                    BillableJobCost += Job.BillableCost
                    CompletedJobs.Add(Job)
                    IncompleteJobs.Remove(Job)
                    PickedJobs.Remove(Job)
                    Dim TimeSpare As Integer = (Job.Deadline - Time).TotalSeconds
                    If TimeSpare < 0 Then
                        LateJobs += 1
                        TotalTimeLate += -TimeSpare
                    Else
                        TotalTimeEarly += TimeSpare
                    End If

                    If Job.IsFailedDelivery AndAlso SimulationParameters.FailToDepot = True AndAlso SimulationParameters.ENABLE_DEPOT_DISPATCHER Then
                        DepotDispatcher.AddPotentialJob(Job)
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
                    BillableJobCost += Job.BillableCost
                    IncompleteJobs.Remove(Job)
                    CompletedJobs.Add(Job)
                    UnpickedJobs.Remove(Job)
                End If
            Next
        End Sub
    End Module
End Namespace
