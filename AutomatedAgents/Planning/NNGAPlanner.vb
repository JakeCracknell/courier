Public Class NNGAPlanner
    Implements ISolver

    Private _OldPlan As CourierPlan
    Private _NewPlan As CourierPlan

    Private _Start As HopPosition
    Private _ExtraJob As CourierJob
    Private _AllWaypoints As List(Of WayPoint)
    Private _Agent As Agent
    Private _WaypointsToLock As Integer
    Private _RunGeneticAlgorithm As Boolean

    Private _HaversineToTimeRatio As Double
    Private StartState As CourierPlanState

    Private Const MAX_FAILED_NNS_BRANCHES As Integer = 720
    Private Const NEAREST_NEIGHBOUR_ROUTES_TO_CALCULATE As Integer = 3

    Sub New(ByVal Agent As Agent, Optional ByVal ExtraJob As CourierJob = Nothing)
        _Agent = Agent
        _OldPlan = Agent.Plan
        _Start = _OldPlan.RoutePosition.GetPoint
        _AllWaypoints = New List(Of WayPoint)(_OldPlan.WayPoints)
        _RunGeneticAlgorithm = ExtraJob Is Nothing
        If ExtraJob IsNot Nothing Then
            _ExtraJob = ExtraJob
            _AllWaypoints.AddRange(WayPoint.CreateWayPointList(ExtraJob))
        End If
        _AllWaypoints = _AllWaypoints.OrderBy(Function(W As WayPoint) W.Job.Deadline.Ticks).ToList
        Solve()
    End Sub

    Private Sub Solve()
        BuildStartState()

        Dim Solution As List(Of WayPoint) = SolveUsingNNBFS()
        If Solution Is Nothing Then
            If _RunGeneticAlgorithm Then
                Dim NNSolution As List(Of WayPoint) = GetNearestNeighbourSolution()
                Solution = SolveUsingGeneticAlgorithm(NNSolution)
                Debug.Assert(Solution IsNot Nothing)
            End If
        End If

        If Solution IsNot Nothing Then
            _NewPlan = New CourierPlan(_OldPlan.StartPoint, _Agent.Map, _OldPlan.Minimiser, _OldPlan.CapacityLeft, _Agent.VehicleType, Solution)
        End If


    End Sub

    'This protects against a rare edge case that would otherwise force an agent to pickup from a locked waypoint when it does not have enough capacity.
    'Scenario: job A is 0.999m3 and is about to be delivered. Call to nearby job B saying about to pickup in 5 min.
    '           job A is a failed delivery. Job B pickup in 5 promise must be revoked.
    'This function protects aginst this by locking waypoints until the capcity would be exceeded.
    'No waypoints are locked if the vehicle is on a 5+ minute diversion.
    Private Sub BuildStartState() 'Now with Waypoint locking!
        Dim DiversionTime As TimeSpan = _OldPlan.GetDiversionTimeEstimate
        _WaypointsToLock = 0
        StartState.CapacityLeft = _Agent.GetVehicleCapacityLeft
        StartState.Time = NoticeBoard.Time + DiversionTime
        StartState.Point = _OldPlan.RoutePosition.GetPoint
        StartState.WayPointsLeft = New List(Of WayPoint)(_AllWaypoints)

        Dim GuarenteedTime As Double = DiversionTime.TotalHours
        For i = 0 To _OldPlan.Routes.Count - 1
            Dim CapacityLeft As Double = StartState.CapacityLeft - _OldPlan.WayPoints(i).VolumeDelta
            Dim RouteTime As TimeSpan = _OldPlan.Routes(i).GetEstimatedTime
            GuarenteedTime += RouteTime.TotalHours
            If GuarenteedTime <= SimulationParameters.COURTESY_CALL_LOCK_TIME_HOURS AndAlso CapacityLeft > 0 Then
                _WaypointsToLock += 1
                StartState.CapacityLeft = CapacityLeft
                StartState.Time += RouteTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG)
                StartState.WayPointsLeft.Remove(_OldPlan.WayPoints(i))
                StartState.Point = _OldPlan.WayPoints(i).Position
            Else
                Exit For
            End If
        Next
    End Sub

    Private Function SolveUsingNNBFS() As List(Of WayPoint)
        Dim FailedBranches As Integer = 0
        Dim BFSStack As New Stack(Of NNSearchNode)
        BFSStack.Push(New NNSearchNode(StartState))
        Do
            Dim Node As NNSearchNode = BFSStack.Pop
            If Node.State.WayPointsLeft.Count = 0 Then
                Dim Solution As List(Of WayPoint) = _OldPlan.WayPoints.GetRange(0, _WaypointsToLock)
                Solution.AddRange(Node.GetWaypointsList)
                Return Solution
            End If

            'Evaluate all next nodes that could be taken. Are they valid?
            'If so, properly evaluate the top 3 and add them to a sorted list if all
            'next waypoints are on time. Then in reverse order, push these onto the stack.
            'Allow backtracking when deliveries are expected to be late, but cap the
            'number of failed branches, as there are potentially n! of them.
            Dim NextNodes As New SortedList(Of Double, NNSearchNode)
            Dim WaypointsToConsider As IOrderedEnumerable(Of WayPoint)
            WaypointsToConsider = Node.State.WayPointsLeft.Where( _
                Function(W) Node.State.CapacityLeft - W.VolumeDelta >= 0 AndAlso _
                    Not Node.State.WayPointsLeft.Contains(W.Predecessor)). _
                         OrderBy(Function(W) HaversineDistance(Node.State.Point, W.Position)). _
                            Take(NEAREST_NEIGHBOUR_ROUTES_TO_CALCULATE). _
                            OrderBy(Function(W) RouteCache.GetRoute(Node.State.Point, W.Position). _
                                        GetCostForAgent(_Agent, Node.State.Time)) 'TODO route time.
            For Each W As WayPoint In WaypointsToConsider
                Dim NextState As CourierPlanState
                NextState.Point = W.Position
                NextState.CapacityLeft = Node.State.CapacityLeft - W.VolumeDelta

                'PRUNE this branch if the vehicle is overcapacity OR if the pickup is yet to have occurred
                If NextState.CapacityLeft < 0 OrElse Node.State.WayPointsLeft.Contains(W.Predecessor) Then
                    Continue For
                End If

                Dim Route As Route = RouteCache.GetRoute(Node.State.Point, W.Position) 'TODO: at time.
                NextState.Time = Node.State.Time + Route.GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)

                'If any jobs' deadlines (not just the current waypoint's) come before the time of this route, 
                ' subtracting redundancy time (if any), PRUNE this branch
                If Node.State.WayPointsLeft.Any(Function(x)
                                                    Return x.Job.Deadline - SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB < NextState.Time
                                                End Function) Then
                    FailedBranches += 1
                    Continue For
                End If

                NextState.WayPointsLeft = New List(Of WayPoint)(Node.State.WayPointsLeft)
                NextState.WayPointsLeft.Remove(W)
                Dim ExtraCost As Double = Route.GetCostForAgent(_Agent, Node.State.Time)
                Dim NextNode As New NNSearchNode(Node, NextState, W, ExtraCost)
                'SortedList cannot contain dupe keys. Will occur if there are 2+ depot waypoints.
                While NextNodes.ContainsKey(ExtraCost)
                    ExtraCost += 0.0000000001
                End While
                NextNodes.Add(ExtraCost, NextNode)

            Next
            For i = NextNodes.Count - 1 To 0 Step -1
                BFSStack.Push(NextNodes.Values(i))
            Next

        Loop Until BFSStack.Count = 0 Or FailedBranches > MAX_FAILED_NNS_BRANCHES

        Return Nothing
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
            Time += _OldPlan.Routes(i).GetEstimatedTime(Time) + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG)
        Next


        Do Until WaypointsLeft.Count = 0
            Dim OrderedWaypoints As IOrderedEnumerable(Of WayPoint)
            OrderedWaypoints = WaypointsLeft.Where( _
                Function(W) CapacityLeft - W.VolumeDelta >= 0 AndAlso _
                    Not WaypointsLeft.Contains(W.Predecessor)). _
                        OrderBy(Function(W) HaversineDistance(Position, W.Position)). _
                            Take(NEAREST_NEIGHBOUR_ROUTES_TO_CALCULATE). _
                            OrderBy(Function(W) RouteCache.GetRoute(Position, W.Position).GetCostForAgent(_Agent, Time))

            Dim NextWaypoint As WayPoint = OrderedWaypoints(0)
            Dim Route As Route = RouteCache.GetRoute(Position, NextWaypoint.Position)
            WaypointsLeft.Remove(NextWaypoint)
            WaypointListSolution.Add(NextWaypoint)
            TotalStraightLineDistance += HaversineDistance(Position, NextWaypoint.Position)
            Position = NextWaypoint.Position
            CapacityLeft -= NextWaypoint.VolumeDelta
            Time += Route.GetEstimatedTime(Time) + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG)
        Loop

        _HaversineToTimeRatio = TotalStraightLineDistance / _
            (Time - NoticeBoard.Time - TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_AVG * _AllWaypoints.Count)).TotalHours

        Return WaypointListSolution
    End Function

    Private Function SolveUsingGeneticAlgorithm(ByVal NNSolution As List(Of WayPoint)) As List(Of WayPoint)
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


        '_NewPlan = New CourierPlan(_Start, _OldPlan.Map, _OldPlan.Minimiser, _OldPlan.CapacityLeft, _OldPlan.VehicleType, TopSolutions(0))
        'Dim NNPlan = New CourierPlan(_Start, _OldPlan.Map, _OldPlan.Minimiser, _OldPlan.CapacityLeft, _OldPlan.VehicleType, NNSolution)
        'Debug.WriteLine("NN Cost: " & GetRealSolutionScore(NNSolution) & NNPlan.IsBehindSchedule)
        'Debug.WriteLine("GA Cost: " & GetRealSolutionScore(TopSolutions(0)) & _NewPlan.IsBehindSchedule)
        Return TopSolutions(0)
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
            Time += TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
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
            Time += TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX) + SimulationParameters.DEADLINE_PLANNING_REDUNDANCY_TIME_PER_JOB
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

    Private Class NNSearchNode
        Public Parent As NNSearchNode
        Public WayPoint As WayPoint
        Public TotalCost As Double
        Public State As CourierPlanState

        Public Sub New(ByVal State As CourierPlanState)
            TotalCost = 0
            Me.State = State
        End Sub

        Public Sub New(ByVal OldTree As NNSearchNode, ByVal State As CourierPlanState, ByVal WayPoint As WayPoint, ByVal ExtraCost As Double)
            Parent = OldTree
            TotalCost = OldTree.TotalCost + ExtraCost
            Me.WayPoint = WayPoint
            Me.State = State
        End Sub

        Public Function GetWaypointsList() As List(Of WayPoint)
            Dim WayPoints As New List(Of WayPoint)
            Dim CurrentNode As NNSearchNode = Me
            Do
                If CurrentNode.WayPoint IsNot Nothing Then
                    WayPoints.Insert(0, CurrentNode.WayPoint)
                End If
                CurrentNode = CurrentNode.Parent
            Loop Until CurrentNode Is Nothing
            Return WayPoints
        End Function
    End Class


    Public Function GetPlan() As CourierPlan Implements ISolver.GetPlan
        Return _NewPlan
    End Function

    Public Function GetTotalCost() As Double Implements ISolver.GetTotalCost
        Return _NewPlan.UpdateAndGetCost
    End Function

    Public Function IsSuccessful() As Boolean Implements ISolver.IsSuccessful
        Return _NewPlan IsNot Nothing AndAlso Not _NewPlan.IsBehindSchedule
    End Function
End Class
