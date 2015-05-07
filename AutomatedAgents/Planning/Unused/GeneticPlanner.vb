Public Class GeneticPlanner
    Implements IPlanner

    Private _OldPlan As CourierPlan
    Private _Start As HopPosition
    Private _ExtraJob As CourierJob
    Private _AllWaypoints As List(Of WayPoint)
    Private _Agent As Agent
    Private _WaypointsToLock As Integer

    Private _NewPlan As CourierPlan

    Private _HaversineToTimeRatio As Double

    Sub New(ByVal Agent As Agent, Optional ByVal ExtraJob As CourierJob = Nothing)
        _Agent = Agent
        _OldPlan = Agent.Plan
        _Start = _OldPlan.RoutePosition.GetPoint
        _AllWaypoints = New List(Of WayPoint)(_OldPlan.WayPoints)
        If ExtraJob IsNot Nothing Then
            _ExtraJob = ExtraJob
            _AllWaypoints.AddRange(WayPoint.CreateWayPointList(ExtraJob))
        End If
        _AllWaypoints = _AllWaypoints.OrderBy(Function(W As WayPoint) W.Job.Deadline.Ticks).ToList
        Solve()
    End Sub

    Sub Solve()
        _WaypointsToLock = GetWaypointsToLock()

        Dim NNSolution As List(Of WayPoint) = GetNearestNeighbourSolution()
        Dim SolutionPool As New SortedList(Of Double, List(Of WayPoint))
        Dim TopSolutions As New List(Of List(Of WayPoint))
        Dim SolutionsEvaluated As New HashSet(Of Integer)
        SolutionsEvaluated.Add(GetSolutionHashCode(NNSolution))
        SolutionPool.Add(0, NNSolution)

        For i = 1 To 500
            Dim SolutionToMutate As List(Of WayPoint) = SolutionPool.Values(0)
            SolutionPool.RemoveAt(0)
            TopSolutions.Add(SolutionToMutate)
            For Each Mutation As List(Of WayPoint) In GetAllMutations(SolutionToMutate)
                Dim Hash As Integer = GetSolutionHashCode(Mutation)
                If Not SolutionsEvaluated.Contains(Hash) Then
                    SolutionsEvaluated.Add(Hash)
                    Dim Score As Double = GetFuzzySolutionScore(Mutation)
                    Do Until Not SolutionPool.ContainsKey(Score)
                        Score += 0.000001
                    Loop
                    SolutionPool.Add(Score, Mutation)
                End If
            Next

            If SolutionPool.Count = 0 Then
                Exit For
            End If
        Next


        TopSolutions = TopSolutions.OrderBy(Function(S) GetFuzzySolutionScore(S)).ToList

        If IsInDebugMode() Then
            Debug.WriteLine("Top solutions" & vbNewLine)
            For Each Solution In TopSolutions
                Debug.Write(GetRealSolutionScore(Solution))
                Debug.WriteLine(GetFuzzySolutionScore(Solution))
            Next
            Debug.WriteLine("Pool solutions" & vbNewLine)
            For Each SolutionPoolItem In SolutionPool
                Debug.Write(GetRealSolutionScore(SolutionPoolItem.Value))
                Debug.WriteLine(SolutionPoolItem.Key)
            Next
        End If
        'Debug.WriteLine(TopSolutions.Count & "   " & SolutionPool.Count)


        _NewPlan = New CourierPlan(_Start, _OldPlan.Map, _OldPlan.Minimiser, _OldPlan.CapacityLeft, _OldPlan.VehicleType, TopSolutions(0))
        Dim NNPlan = New CourierPlan(_Start, _OldPlan.Map, _OldPlan.Minimiser, _OldPlan.CapacityLeft, _OldPlan.VehicleType, NNSolution)
        'Debug.WriteLine("NN Cost: " & GetRealSolutionScore(NNSolution) & NNPlan.IsBehindSchedule)
        'Debug.WriteLine("GA Cost: " & GetRealSolutionScore(TopSolutions(0)) & _NewPlan.IsBehindSchedule)


    End Sub


    'This protects against a rare edge case that would otherwise force an agent to pickup from a locked waypoint when it does not have enough capacity.
    'Scenario: job A is 0.999m3 and is about to be delivered. Call to nearby job B saying about to pickup in 5 min.
    '           job A is a failed delivery. Job B pickup in 5 promise must be revoked.
    'This function protects aginst this by locking waypoints until the capcity would be exceeded.
    Private Function GetWaypointsToLock() As Integer
        Dim GuarenteedTime As Double
        Dim WaypointsToLock As Integer = 0
        Dim CapacityLeft As Double = _Agent.GetVehicleCapacityLeft

        For i = 0 To _OldPlan.Routes.Count - 1
            CapacityLeft -= _OldPlan.WayPoints(i).VolumeDelta
            GuarenteedTime += _OldPlan.Routes(i).GetEstimatedHours
            If GuarenteedTime <= SimulationParameters.COURTESY_CALL_LOCK_TIME_HOURS AndAlso CapacityLeft > 0 Then
                WaypointsToLock += 1
            Else
                Exit For
            End If
        Next
        Return WaypointsToLock
    End Function

    Private Function GetNearestNeighbourSolution() As List(Of WayPoint)
        Dim TotalStraightLineDistance As Double = 0

        Dim WaypointListSolution As New List(Of WayPoint)(_AllWaypoints.Count)
        Dim WaypointsLeft As New List(Of WayPoint)(_AllWaypoints)
        Dim CapacityLeft As Double = _Agent.GetVehicleCapacityLeft
        Dim Position As HopPosition = _Start
        Dim Time As TimeSpan = NoticeBoard.Time

        For i = 0 To _WaypointsToLock - 1
            Dim NextWaypoint As WayPoint = _OldPlan.WayPoints(i)
            WaypointListSolution.Add(NextWaypoint)
            WaypointsLeft.Remove(NextWaypoint)
            TotalStraightLineDistance += HaversineDistance(Position, NextWaypoint.Position)
            Position = NextWaypoint.Position
            CapacityLeft -= NextWaypoint.VolumeDelta
            Time += _OldPlan.Routes(i).GetEstimatedTime(Time) + Customers.WaitTimeAvg
        Next


        Do Until WaypointsLeft.Count = 0
            Dim OrderedWaypoints As IOrderedEnumerable(Of WayPoint)
            OrderedWaypoints = WaypointsLeft.Where( _
                Function(W) CapacityLeft - W.VolumeDelta >= 0 AndAlso _
                    Not WaypointsLeft.Contains(W.Predecessor)). _
                        OrderBy(Function(W) HaversineDistance(Position, W.Position)). _
                            Take(2).OrderBy(Function(W) RouteCache.GetRoute(Position, W.Position).GetCostForAgent(_Agent, Time))

            Dim NextWaypoint As WayPoint = OrderedWaypoints(0)
            Dim Route As Route = RouteCache.GetRoute(Position, NextWaypoint.Position)
            WaypointsLeft.Remove(NextWaypoint)
            WaypointListSolution.Add(NextWaypoint)
            TotalStraightLineDistance += HaversineDistance(Position, NextWaypoint.Position)
            Position = NextWaypoint.Position
            CapacityLeft -= NextWaypoint.VolumeDelta
            Time += Route.GetEstimatedTime(Time) + Customers.WaitTimeAvg
        Loop

        _HaversineToTimeRatio = TotalStraightLineDistance / _
            (Time - NoticeBoard.Time - TimeSpan.FromSeconds(Customers.WaitTimeAvgSeconds * _AllWaypoints.Count)).TotalHours

        Return WaypointListSolution
    End Function

    Private Function GetFuzzySolutionScore(ByVal Solution As List(Of WayPoint)) As Double
        'Assumes solution is valid.
        Dim Distance As Double = 0
        Dim Latenesses As Integer = 0
        Dim LastPoint As IPoint = _Start
        Dim Time As TimeSpan = NoticeBoard.Time
        For Each WayPoint As WayPoint In Solution
            Distance += HaversineDistance(LastPoint, WayPoint.Position)
            Time += TimeSpan.FromHours(Distance / _HaversineToTimeRatio)
            If Time > WayPoint.Job.Deadline Then
                Latenesses += 1
            End If
            Time += Customers.WaitTimeMax
            LastPoint = WayPoint.Position
        Next
        Return Distance + Latenesses * 1000
    End Function

    Private Function GetRealSolutionScore(ByVal Solution As List(Of WayPoint)) As Double
        'Assumes solution is valid.
        Dim Cost As Double = 0
        Dim Latenesses As Integer = 0
        Dim LastPoint As IPoint = _Start
        Dim Time As TimeSpan = NoticeBoard.Time
        For Each WayPoint As WayPoint In Solution
            Dim Route As Route = RouteCache.GetRoute(LastPoint, WayPoint.Position) 'TODO at time.
            Cost += Route.GetCostForAgent(_Agent)
            Time += Route.GetEstimatedTime()
            If Time > WayPoint.Job.Deadline Then
                Latenesses += 1
            End If
            Time += Customers.WaitTimeMax + SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_WAYPOINT
            LastPoint = WayPoint.Position
        Next
        Return Cost + Latenesses * 1000
    End Function


    'Gives at most n-1 mutations.
    Function GetAllMutations(ByVal Solution As List(Of WayPoint)) As List(Of List(Of WayPoint))
        Dim Mutations As New List(Of List(Of WayPoint))

        'The capacity just before the waypoint is fulfilled. We know it to be 100% at the end.
        Dim SolutionCapacityLefts As New List(Of Double)(Solution.Count)
        SolutionCapacityLefts.Add(_Agent.GetVehicleCapacityLeft)
        For i = 0 To Solution.Count - 2
            SolutionCapacityLefts.Add(SolutionCapacityLefts.Last - Solution(i).VolumeDelta)
        Next
        Debug.Assert(SolutionCapacityLefts.Min > 0) 'TODO REMOVE FOR EFFICIENCY

        For i = _WaypointsToLock To Solution.Count - 2

            '1. Swapping any two waypoints of the same status
            '2. Swapping ' -> pick(X) -> del(Y) -> ', as long as X <> Y!
            '3. Swapping ' -> del(X) -> pick(Y) -> ', as long as there is enough space for Y at X

            Dim Swappable As Boolean = Solution(i).DefinedStatus = Solution(i + 1).DefinedStatus _
                                       OrElse (Solution(i + 1).DefinedStatus = JobStatus.PENDING_DELIVERY AndAlso Not Solution(i).Job.Equals(Solution(i + 1).Job)) _
                                       OrElse (Solution(i + 1).DefinedStatus = JobStatus.PENDING_PICKUP AndAlso Solution(i + 1).VolumeDelta < SolutionCapacityLefts(i))

            If Swappable Then
                Dim Mutation As New List(Of WayPoint)(Solution)
                Mutation(i) = Solution(i + 1)
                Mutation(i + 1) = Solution(i)
                Mutations.Add(Mutation)
            End If
        Next

        Return Mutations
    End Function

    Private Function GetSolutionHashCode(ByVal Solution As List(Of WayPoint)) As Integer
        Const Prime As Integer = 31
        Dim Hashcode As Long = 1
        For Each WayPoint As WayPoint In Solution
            Hashcode = (Hashcode * Prime + WayPoint.GetHashCode) And &H7FFFFFFFL
        Next
        Return CInt(Hashcode And &H7FFFFFFFL) 'Avoids overflow
    End Function



    Public Function GetPlan() As CourierPlan Implements IPlanner.GetPlan
        Return _NewPlan
    End Function

    Public Function GetTotalCost() As Double Implements IPlanner.GetTotalCost
        Return _NewPlan.UpdateAndGetCost
    End Function

    Public Function IsSuccessful() As Boolean Implements IPlanner.IsSuccessful
        Return Not _NewPlan.IsBehindSchedule
    End Function
End Class
