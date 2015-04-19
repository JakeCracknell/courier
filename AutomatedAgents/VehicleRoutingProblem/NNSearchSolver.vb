Public Class NNSearchSolver
    Implements ISolver

    Private Minimiser As RouteFindingMinimiser
    Private PunctualityStrategy As SolverPunctualityStrategy
    Private StartState As CourierPlanState
    Public OldPlan As CourierPlan
    Private Solution As CourierPlan
    Private TotalCost As Double
    Private Shared r As New Random

    'TODO: BYVAL locked waypoints - those that must occur first. EG wp ->1 and ->2 already got their courtesy call
    Sub New(ByVal CourierPlan As CourierPlan, ByVal PunctualityStrategy As SolverPunctualityStrategy, ByVal Minimiser As RouteFindingMinimiser, Optional ByVal ExtraJob As CourierJob = Nothing)
        OldPlan = CourierPlan
        Me.Minimiser = Minimiser
        Me.PunctualityStrategy = PunctualityStrategy
        StartState.CapacityLeft = CourierPlan.CapacityLeft
        StartState.FuelLeft = Double.MaxValue
        StartState.Time = NoticeBoard.CurrentTime + OldPlan.GetDiversionTimeEstimate
        StartState.Point = CourierPlan.RoutePosition.GetPoint
        StartState.WayPointsLeft = New List(Of WayPoint)(CourierPlan.WayPoints)

        If ExtraJob IsNot Nothing Then
            StartState.WayPointsLeft.AddRange(WayPoint.CreateWayPointList(ExtraJob))
        End If
        Solve()
    End Sub

    Private Sub RecreateCourierPlan(ByVal NNSolution As NNSearchNode)
        Dim WayPoints As New List(Of WayPoint)
        Dim CurrentNode As NNSearchNode = NNSolution
        Do
            If CurrentNode.WayPoint IsNot Nothing Then
                WayPoints.Insert(0, CurrentNode.WayPoint)
            End If
            CurrentNode = CurrentNode.Parent
        Loop Until CurrentNode Is Nothing
        RecreateCourierPlan(WayPoints)
    End Sub
    Private Sub RecreateCourierPlan(ByVal WayPoints As List(Of WayPoint))
        Dim Routes As New List(Of Route)
        Dim LastPoint As IPoint = OldPlan.StartPoint
        For Each W As WayPoint In WayPoints
            Routes.Add(RouteCache.GetRoute(LastPoint, W.Position))
            LastPoint = W.Position
        Next
        Solution = New CourierPlan(OldPlan.StartPoint, OldPlan.Map, OldPlan.Minimiser, OldPlan.CapacityLeft, WayPoints, Routes)
    End Sub

    Private Function SolveUsingNNBFS()
        Dim FailedBranches As Integer = 0
        Dim GreedyBFSStack As New Stack(Of NNSearchNode)
        GreedyBFSStack.Push(New NNSearchNode(StartState))
        Do
            Dim Node As NNSearchNode = GreedyBFSStack.Pop
            If Node.State.WayPointsLeft.Count = 0 Then
                TotalCost = Node.TotalCost
                RecreateCourierPlan(Node)
                Return True
            End If

            Dim NextNodes As New SortedList(Of Double, NNSearchNode)
            For Each W As WayPoint In Node.State.WayPointsLeft
                Dim NextState As CourierPlanState
                NextState.Point = W.Position
                NextState.CapacityLeft = Node.State.CapacityLeft - W.VolumeDelta
                'PRUNE this branch if the vehicle is overcapacity OR
                ' if the pickup is yet to have occurred
                If NextState.CapacityLeft < 0 OrElse _
                    (W.Predecessor IsNot Nothing AndAlso Node.State.WayPointsLeft.Contains(W.Predecessor)) Then
                    'Debug.Write(If(NextState.CapacityLeft < 0, "C", ""))
                    Continue For
                End If

                Dim Route As Route = RouteCache.GetRoute(Node.State.Point, W.Position)
                NextState.Time = Node.State.Time + Route.GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                NextState.FuelLeft = Node.State.FuelLeft - Route.GetKM * 0.1 'TODO fuel, also maybe leave extra time?
                'PRUNE this branch if it is unreachable given fuel reserves
                If NextState.FuelLeft < 0 Then
                    FailedBranches += 1
                    Continue For
                End If

                NextState.WayPointsLeft = New List(Of WayPoint)(Node.State.WayPointsLeft)
                NextState.WayPointsLeft.Remove(W)


                'TODO: this part needs an Agent object to calc fuel.
                Dim ExtraCost As Double
                Select Case Minimiser
                    Case RouteFindingMinimiser.DISTANCE : ExtraCost = Route.GetKM
                    Case RouteFindingMinimiser.FUEL_WITH_TRAFFIC : ExtraCost = Route.GetKM * 0.1
                    Case Else : ExtraCost = Route.GetHoursWithoutTraffic
                End Select

                Dim NextNode As New NNSearchNode(Node, NextState, W, ExtraCost)
                'If any jobs' deadlines come before the time of this route,
                ' subtracting redundancy time (if any), PRUNE this branch
                ' The failed branch may be used later on for contingency planning
                If Node.State.WayPointsLeft.Any(Function(x)
                                                    Return x.Job.Deadline - PunctualityStrategy.RedundancyTime < NextState.Time
                                                End Function) Then
                    FailedBranches += 1
                    Continue For
                Else
                    'SortedList cannot contain dupe keys. Will occur if there are 2+ depot waypoints.
                    Do Until Not NextNodes.ContainsKey(ExtraCost)
                        ExtraCost += 0.0000000001
                    Loop
                    NextNodes.Add(ExtraCost, NextNode)
                End If
            Next
            For i = NextNodes.Count - 1 To 0 Step -1
                GreedyBFSStack.Push(NextNodes.Values(i))
            Next

        Loop Until GreedyBFSStack.Count = 0 Or FailedBranches > 720

        Return False
    End Function

    Private Function SolveUsingAStar()
        Dim t As New Stopwatch
        t.Start()
        Dim PriorityQueue As New SortedList(Of Double, NNSearchNode)
        PriorityQueue.Add(0, New NNSearchNode(StartState))
        Do
            Dim Node As NNSearchNode = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If Node.State.WayPointsLeft.Count = 0 Then
                TotalCost = Node.TotalCost
                RecreateCourierPlan(Node)
                Return True
            End If

            Dim NextNodes As New SortedList(Of Double, NNSearchNode)
            For Each W As WayPoint In Node.State.WayPointsLeft
                Dim NextState As CourierPlanState
                NextState.Point = W.Position
                NextState.CapacityLeft = Node.State.CapacityLeft - W.VolumeDelta
                'PRUNE this branch if the vehicle is overcapacity OR
                ' if the pickup is yet to have occurred
                If NextState.CapacityLeft < 0 OrElse _
                    (W.Predecessor IsNot Nothing AndAlso Node.State.WayPointsLeft.Contains(W.Predecessor)) Then
                    'Debug.Write(If(NextState.CapacityLeft < 0, "C", ""))
                    Continue For
                End If

                Dim Route As Route = RouteCache.GetRoute(Node.State.Point, W.Position)
                NextState.Time = Node.State.Time + Route.GetEstimatedTime
                NextState.FuelLeft = Node.State.FuelLeft - Route.GetKM * 0.1 'TODO fuel, also maybe leave extra time?
                'PRUNE this branch if it is unreachable given fuel reserves
                If NextState.FuelLeft < 0 Then
                    'Debug.Write("F")
                    Continue For
                End If

                NextState.WayPointsLeft = New List(Of WayPoint)(Node.State.WayPointsLeft)
                NextState.WayPointsLeft.Remove(W)

                Dim ExtraCost As Double
                Select Case Minimiser
                    Case RouteFindingMinimiser.DISTANCE : ExtraCost = Route.GetKM
                    Case RouteFindingMinimiser.FUEL_WITH_TRAFFIC : ExtraCost = Route.GetKM * 0.1
                    Case Else : ExtraCost = Route.GetHoursWithoutTraffic
                End Select

                Dim NextNode As New NNSearchNode(Node, NextState, W, ExtraCost)
                Dim HeuristicCost As Double = Heuristic(NextNode.State)

                Do Until Not PriorityQueue.ContainsKey(HeuristicCost)
                    HeuristicCost += Double.MinValue
                Loop
                PriorityQueue.Add(HeuristicCost, NextNode)
                'TODO:try combining with extra cost?
            Next

        Loop Until PriorityQueue.Count = 0

        Return False
    End Function

    Private Function FindRandomSolution() As TSPSolution
        Dim LeastNumberOfLateDeliveries As Integer = Integer.MaxValue
        Dim SolutionWithLeastNumberOfLateDeliveries As List(Of WayPoint) = Nothing
        For i = 1 To 1000
            Dim CapacityLeft As Double = StartState.CapacityLeft
            Dim State As CourierPlanState = StartState
            Dim CandidateList As New List(Of WayPoint)(State.WayPointsLeft.Count)
            Do Until State.WayPointsLeft.Count = 0
                'Random next waypoint to try first
                Dim NextWayPointI As Integer = r.Next(State.WayPointsLeft.Count)
                Do
                    If ValidStateTransformation(State, State.WayPointsLeft(NextWayPointI)) Then
                        Exit Do
                    End If
                    NextWayPointI = (NextWayPointI + 1) Mod State.WayPointsLeft.Count
                Loop
                CandidateList.Add(State.WayPointsLeft(NextWayPointI))
                Dim WaypointsLeft As New List(Of WayPoint)(State.WayPointsLeft)
                WaypointsLeft.Remove(State.WayPointsLeft(NextWayPointI))
                State.CapacityLeft -= State.WayPointsLeft(NextWayPointI).VolumeDelta
                State.WayPointsLeft = WaypointsLeft
            Loop
            State = StartState
            Dim LateDeliveries As Integer = 0
            For Each W As WayPoint In CandidateList
                State.Time += RouteCache.GetRoute(State.Point, W.Position).GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                If W.Job.Deadline < State.Time Then
                    LateDeliveries += 1
                End If
            Next
            If LateDeliveries = 0 Then
                'Return any solution that works
                Return New TSPSolution(CandidateList)
            ElseIf LateDeliveries < LeastNumberOfLateDeliveries Then
                LeastNumberOfLateDeliveries = LateDeliveries
                SolutionWithLeastNumberOfLateDeliveries = CandidateList
            End If
        Next

        'In a pinch, return the least late solution
        Return New TSPSolution(SolutionWithLeastNumberOfLateDeliveries, LeastNumberOfLateDeliveries)
    End Function

    Private Shared Function ValidStateTransformation(ByVal S As CourierPlanState, ByVal W As WayPoint) As Boolean
        If W.Predecessor IsNot Nothing AndAlso S.WayPointsLeft.Contains(W.Predecessor) Then
            Return False
        End If
        If S.CapacityLeft - W.VolumeDelta < 0 Then
            Return False
        End If
        'Fuel
        Return True
    End Function

    Private Function SolveUsingMonteCarlo()
        Dim BestSolution As TSPSolution = Nothing
        Dim BestCost As Double = Double.MaxValue
        Dim t As New Stopwatch
        t.Start()
        Do
            Dim Solution As TSPSolution = FindRandomSolution()
            Dim Cost As Double = Solution.Cost(StartState)
            'Debug.WriteLine(Cost & " at " & t.ElapsedMilliseconds)
            If Cost < BestCost Then
                BestCost = Cost
                BestSolution = Solution
            End If
        Loop Until t.ElapsedMilliseconds > 1000
        RecreateCourierPlan(BestSolution.WayPoints)
        Return True
    End Function

    Private Function SolveUsingSimulatedAnnealing()
        Dim AnnealingIteration As Integer = -1

        Dim Temperature As Double = 10000.0
        Dim DeltaCost As Double = 0
        Dim CoolingRate As Double = 0.9999
        Dim AbsoluteTemperature As Double = 0.00001

        Dim Solution As TSPSolution = FindRandomSolution()
        Dim Cost As Double = Solution.Cost(StartState)

        Dim t As New Stopwatch
        t.Start()
        While Temperature > AbsoluteTemperature And t.ElapsedMilliseconds <= 100
            Dim NextSolution As TSPSolution = Solution.Mutate(StartState)

            DeltaCost = NextSolution.Cost(StartState) - Cost
            'Debug.Write(DeltaCost & " ")

            'if the new order has a smaller distance
            'or if the new order has a larger distance but satisfies Boltzman condition then accept the arrangement
            If (DeltaCost < 0) OrElse (Cost > 0 AndAlso Math.Exp(-DeltaCost / Temperature) > r.NextDouble()) Then
                Solution = NextSolution
                Cost = DeltaCost + Cost
            End If

            'cool down the temperature
            Temperature *= CoolingRate

            AnnealingIteration += 1
        End While
        RecreateCourierPlan(Solution.WayPoints)
        Return True
    End Function

    Private Function Heuristic(ByVal State As CourierPlanState) As Double
        Dim H As Double = 0
        Dim WaypointsCounted As Integer = State.WayPointsLeft.Count
        For Each W As WayPoint In State.WayPointsLeft
            'Either pickup or dropoff that is currently owned
            Dim RealDeadline As TimeSpan = W.Job.Deadline
            If W.DefinedStatus = JobStatus.PENDING_PICKUP Then
                RealDeadline -= RouteCache.GetRoute(W.Job.PickupPosition, W.Job.DeliveryPosition).GetEstimatedTime
            ElseIf State.WayPointsLeft.Contains(W.Predecessor) Then
                Continue For
            End If
            If RealDeadline <= State.Time Then
                H += 2
            ElseIf RealDeadline <= State.Time + RouteCache.GetRoute(State.Point, W.Position).GetEstimatedTime Then
                H += 1
            Else
                H += RouteCache.GetRoute(State.Point, W.Position).GetEstimatedTime.TotalSeconds / (RealDeadline - State.Time).TotalSeconds
            End If
            WaypointsCounted += 1
        Next
        Return H / WaypointsCounted 'State.WayPointsLeft may give better results?
    End Function

    Private Sub Solve()
        If SolveUsingNNBFS() Then
            Exit Sub
        End If

        'TODO: use this strat
        If PunctualityStrategy.Strategy = SolverPunctualityStrategy.PStrategy.REDUNDANCY_TIME Then
            Solution = Nothing
            Exit Sub
        Else
            SolveUsingMonteCarlo()
        End If

    End Sub

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


    End Class

    Private Class TSPSolution
        Public WayPoints As List(Of WayPoint)
        Public LateDeliveries As Integer
        Sub New(ByVal WayPoints As List(Of WayPoint), Optional ByVal LateDeliveries As Integer = 0)
            Me.WayPoints = WayPoints
            Me.LateDeliveries = LateDeliveries
        End Sub
        Function Cost(ByVal StartState As CourierPlanState) As Double
            If LateDeliveries <> 0 Then
                Return LateDeliveries * 10000000
            Else
                Dim TotalCost As Double = 0
                Dim LastPoint As IPoint = StartState.Point
                For Each WP As WayPoint In WayPoints
                    TotalCost += RouteCache.GetRoute(LastPoint, WP.Position).GetHoursWithoutTraffic
                    LastPoint = WP.Position
                Next
                Return TotalCost
            End If

        End Function
        Function Mutate(ByVal StartState As CourierPlanState) As TSPSolution
            For i = 1 To 500
                Dim CandidateList As New List(Of WayPoint)(WayPoints)

                'Swap 2 different indices
                Dim SwapWP1i As Integer = r.Next(WayPoints.Count)
                Dim SwapWP1 As WayPoint = WayPoints(SwapWP1i)
                Dim SwapWP2i As Integer = (SwapWP1i + 1 + r.Next(WayPoints.Count - 1)) Mod WayPoints.Count
                Dim SwapWP2 As WayPoint = WayPoints(SwapWP2i)

                CandidateList(SwapWP2i) = SwapWP1
                CandidateList(SwapWP1i) = SwapWP2

                Dim CapacityLeft As Double = StartState.CapacityLeft
                Dim State As CourierPlanState = StartState
                For Each WP As WayPoint In CandidateList
                    If ValidStateTransformation(State, WP) Then
                        GoTo FailedMutation
                    End If
                    Dim WaypointsLeft As New List(Of WayPoint)(State.WayPointsLeft)
                    WaypointsLeft.Remove(WP)
                    State.CapacityLeft -= WP.VolumeDelta
                    State.WayPointsLeft = WaypointsLeft
                Next
                State = StartState
                Dim MLateDeliveries As Integer = 0
                For Each W As WayPoint In CandidateList
                    State.Time += RouteCache.GetRoute(State.Point, W.Position).GetEstimatedTime + TimeSpan.FromSeconds(CourierJob.CUSTOMER_WAIT_TIME_MAX)
                    If W.Job.Deadline < State.Time Then
                        MLateDeliveries += 1
                    End If
                Next
                If LateDeliveries >= MLateDeliveries Then
                    Return New TSPSolution(CandidateList)
                End If
FailedMutation:
            Next
            Return Me
        End Function

    End Class

    Public Function GetPlan() As CourierPlan Implements ISolver.GetPlan
        Return Solution
    End Function

    Public Function GetTotalCost() As Double Implements ISolver.GetTotalCost
        Return TotalCost
    End Function

    Public Function IsSuccessful() As Boolean Implements ISolver.IsSuccessful
        Return Solution IsNot Nothing
    End Function
End Class
