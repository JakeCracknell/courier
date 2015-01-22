Module NoticeBoard

    Property DepotPoint As HopPosition

    Property UnallocatedJobs As New List(Of CourierJob)
    Property UnpickedJobs As New List(Of CourierJob)
    Property PickedJobs As New List(Of CourierJob)

    Property IncompleteJobs As New List(Of CourierJob)
    Property CompletedJobs As New List(Of CourierJob)

    Property JobRevenue As Decimal = 0

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
        Return True
    End Function

    Sub Tidy()
        For i = UnallocatedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnallocatedJobs(i)
            If Job.Status = JobStatus.CANCELLED Then
                CompletedJobs.Add(Job)
                UnallocatedJobs.Remove(Job)
            End If
        Next

        For i = PickedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = PickedJobs(i)
            If Job.Status = JobStatus.COMPLETED Then
                JobRevenue += Job.CustomerFee
                Debug.WriteLine(JobRevenue)
                CompletedJobs.Add(Job)
                IncompleteJobs.Remove(Job)
                PickedJobs.Remove(Job)
            End If
        Next

        For i = UnpickedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnpickedJobs(i)
            If Job.Status = JobStatus.PENDING_DELIVERY Then
                PickedJobs.Add(Job)
                UnpickedJobs.Remove(Job)
            End If
            If Job.Status = JobStatus.CANCELLED Then
                CompletedJobs.Add(Job)
                UnpickedJobs.Remove(Job)
            End If
        Next
    End Sub

    Sub Clear()
        UnallocatedJobs.Clear()
        UnpickedJobs.Clear()
        PickedJobs.Clear()
        IncompleteJobs.Clear()
        CompletedJobs.Clear()
    End Sub

End Module
