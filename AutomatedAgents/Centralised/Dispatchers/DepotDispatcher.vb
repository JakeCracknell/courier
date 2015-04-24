Public Class DepotDispatcher
    Implements IDispatcher

    'Half of failed deliveries will be retried within 48 hours, so use Exp(1/48), which has a mean of 48.
    Private Const RETRY_TIME_LAMBDA As Double = 1 / 48
    Private MAX_RETRY_TIME As Double = 48

    Private PotentialJobs As New SortedList(Of TimeSpan, CourierJob)

    Public Sub AddPotentialJob(ByVal Job As CourierJob)
        'TODO: ask Krysia if there's a better way she can think of. It should be less likely this job is spawned, or deadline createed around midnight.
        Dim TimeToRetry As Double = ProbabilityDistributions.Exponential(RETRY_TIME_LAMBDA, RNG.R("retry_depot").NextDouble)

        If TimeToRetry <= MAX_RETRY_TIME Then
            Dim ScheduledTime As TimeSpan = NoticeBoard.Time + TimeSpan.FromHours(TimeToRetry)
            Do Until Not PotentialJobs.ContainsKey(ScheduledTime)
                ScheduledTime += TimeSpan.FromTicks(1)
            Loop
            PotentialJobs.Add(ScheduledTime, Job)
        End If
    End Sub

    Public Function Tick() As Boolean Implements IDispatcher.Tick
        If PotentialJobs.Count <> 0 Then
            Dim NextDispatch As TimeSpan = PotentialJobs.Keys(0)
            If NextDispatch <= NoticeBoard.Time Then
                Dim OldJob As CourierJob = PotentialJobs.Values(0)
                PotentialJobs.RemoveAt(0)

                Dim DirectRoute As Route = RouteCache.GetRoute(OldJob.DeliveryPosition, OldJob.OriginalDeliveryPosition)
                Dim Deadline As TimeSpan = NoticeBoard.Time + DirectRoute.GetEstimatedTime() + TimeSpan.FromHours(ProbabilityDistributions.Gamma(RNG.R("retry_deadline"), 2, 1))
                Dim DepotName As String = CType(OldJob.DeliveryPosition.Hop.FromPoint, Node).Description
                Dim NewJob As New CourierJob(OldJob.DeliveryPosition, OldJob.OriginalDeliveryPosition, DepotName, OldJob.OriginalDeliveryName, OldJob.CubicMetres, Deadline)
                NoticeBoard.PostJob(NewJob)
                Return True
            End If
        End If
        Return False
    End Function
End Class
