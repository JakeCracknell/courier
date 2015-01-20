Module NoticeBoard
    Private WaitingJobs As New List(Of CourierJob)
    Private UnpickedJobs As New List(Of CourierJob)
    Private PickedJobs As New List(Of CourierJob)
    Private CompletedJobs As New List(Of CourierJob)

    Function AreJobsWaiting() As Boolean
        Return WaitingJobs.Count > 0
    End Function

    Function GetJob() As CourierJob
        If WaitingJobs.Count > 0 Then
            Dim Job As CourierJob = WaitingJobs(0)
            Job.Status = JobStatus.PENDING_PICKUP
            WaitingJobs.RemoveAt(0)
            UnpickedJobs.Add(Job)
            Return Job
        End If
        Return Nothing
    End Function

    Function AddJob(ByVal Job As CourierJob)
        WaitingJobs.Add(Job)
        Return True
    End Function

    Function GetUnpickedJobs() As List(Of CourierJob)
        Return UnpickedJobs
    End Function

    Sub Tidy()
        For i = PickedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = PickedJobs(i)
            If Job.Status = JobStatus.COMPLETED Then
                CompletedJobs.Add(Job)
                PickedJobs.Remove(Job)
            End If
        Next

        For i = UnpickedJobs.Count - 1 To 0 Step -1
            Dim Job As CourierJob = UnpickedJobs(i)
            If Job.Status = JobStatus.PENDING_DELIVERY Then
                PickedJobs.Add(Job)
                UnpickedJobs.Remove(Job)
            End If
        Next
    End Sub

End Module
