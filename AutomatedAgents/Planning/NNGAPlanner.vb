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
        If Solution Is Nothing And False Then
            Dim NNSolution As List(Of WayPoint) = GetNearestNeighbourSolution()
            Solution = SolveUsingGeneticAlgorithm(NNSolution)
            Debug.Assert(Solution IsNot Nothing)
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
                Return Node.GetWaypointsList
            End If

            'Evaluate all next nodes that could be taken. Are they valid and on-time?
            'If so, add them to a sorted list and in reverse order, push these onto the stack.
            'Allow backtracking when deliveries are expected to be late, but cap the
            'number of failed branches, as there are potentially n! of them.
            Dim NextNodes As New SortedList(Of Double, NNSearchNode)
            For Each W As WayPoint In Node.State.WayPointsLeft
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
                Do Until Not NextNodes.ContainsKey(ExtraCost)
                    ExtraCost += 0.0000000001
                Loop
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
                            Take(2).OrderBy(Function(W) RouteCache.GetRoute(Position, W.Position).GetCostForAgent(_Agent, Time))

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

    Private Function SolveUsingGeneticAlgorithm(ByVal SeedSolution As List(Of WayPoint)) As List(Of WayPoint)
        Return SeedSolution
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
        Return _NewPlan IsNot Nothing
    End Function
End Class
