Public Class ExhaustiveTSPPlanner
    Private Map As StreetMap = Nothing
    Private Minimiser As RouteFindingMinimiser = Nothing

    Property RouteCost As Double = 0
    Property PointList As List(Of WayPoint)
    Property JobList As List(Of CourierJob)

    Sub New(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal VehicleCapacityLeft As Double, ByVal Map As StreetMap, ByVal Minimiser As RouteFindingMinimiser)
        Me.Map = Map
        Me.Minimiser = Minimiser
        RunAlgorithm(Start, Jobs, VehicleCapacityLeft)
    End Sub

    Private Sub RunAlgorithm(ByVal Start As IPoint, ByVal Jobs As List(Of CourierJob), ByVal CapacityLeft As Double)
        Dim WayPoints As List(Of WayPoint) = WayPoint.CreateWayPointList(Jobs)
        Dim WayPointPermutations As List(Of List(Of WayPoint)) = (New ListPermutator(Of WayPoint)).GetAllPermutations(WayPoints)

        Dim BestRoute As List(Of WayPoint) = Nothing
        Dim BestRouteScore As Double = Double.MaxValue

        For Each Route As List(Of WayPoint) In WayPointPermutations
            'Ensure pickups -> dropoffs
            For i = 0 To Route.Count - 1
                Dim Pred As WayPoint = Route(i).Predecessor
                If Pred IsNot Nothing AndAlso Route.IndexOf(Pred) > i Then
                    Continue For
                End If
            Next

            'Ensure never overfull
            Dim CapacityRemaining As Double = CapacityLeft
            For Each WP As WayPoint In Route
                CapacityRemaining -= WP.VolumeDelta
                If CapacityRemaining < 0 Then
                    Continue For
                End If
            Next

            'Calculate routes now
            Dim LastPoint As IPoint = Start
            Dim Time As TimeSpan = NoticeBoard.Time
            Dim Latenesses As Integer = 0
            For i = 0 To Route.Count - 1
                Dim AStarRoute As Route = RouteCache.GetRoute(LastPoint, Route(i).Position)
                Time += AStarRoute.GetEstimatedTime(Time) + Customers.WaitTimeAvg + SimulationParameters.DEADLINE_REDUNDANCY
                If Route(i).DefinedStatus = JobStatus.PENDING_DELIVERY AndAlso Route(i).Job.Deadline < Time Then
                    Latenesses += 1
                End If
                LastPoint = Route(i).Position
            Next

            'Is it any better?
            Dim Score As Double = (Time - NoticeBoard.Time).TotalHours + Latenesses * 1000
            If Score < BestRouteScore Then
                BestRouteScore = Score
                BestRoute = Route
            End If
        Next

        If BestRoute IsNot Nothing Then
            PointList = New List(Of WayPoint)(BestRoute.Count)
            RouteCost = BestRouteScore
            For Each WayPoint As WayPoint In BestRoute
                PointList.Add(WayPoint)
            Next
        End If
    End Sub
End Class
