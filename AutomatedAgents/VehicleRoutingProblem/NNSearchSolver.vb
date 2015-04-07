﻿Public Class NNSearchSolver
    Private Minimiser As RouteFindingMinimiser
    Private PunctualityStrategy As SolverPunctualityStrategy
    Private StartState As CourierPlanState
    Public OldPlan As CourierPlan
    Public Solution As CourierPlan
    Public TotalCost As Double

    'TODO: BYVAL locked waypoints - those that must occur first. EG wp ->1 and ->2 already got their courtesy call
    Sub New(ByVal CourierPlan As CourierPlan, ByVal PunctualityStrategy As SolverPunctualityStrategy, ByVal Minimiser As RouteFindingMinimiser, Optional ByVal ExtraJob As CourierJob = Nothing)
        OldPlan = CourierPlan
        Me.Minimiser = Minimiser
        Me.PunctualityStrategy = PunctualityStrategy
        StartState.CapacityLeft = CourierPlan.CapacityLeft
        StartState.FuelLeft = Double.MaxValue
        StartState.Time = NoticeBoard.CurrentTime
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
                NextState.Time = Node.State.Time + Route.GetEstimatedTime
                NextState.FuelLeft = Node.State.FuelLeft - Route.GetKM * 0.1 'TODO fuel, also maybe leave extra time?
                'PRUNE this branch if it is unreachable given fuel reserves
                If NextState.FuelLeft < 0 Then
                    FailedBranches += 1
                    Continue For
                End If

                NextState.WayPointsLeft = New List(Of WayPoint)(Node.State.WayPointsLeft)
                NextState.WayPointsLeft.Remove(W)

                Dim ExtraCost As Double
                Select Case Minimiser
                    Case RouteFindingMinimiser.DISTANCE : ExtraCost = Route.GetKM
                    Case RouteFindingMinimiser.FUEL : ExtraCost = Route.GetKM * 0.1
                    Case Else : ExtraCost = Route.GetEstimatedHours
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
                        ExtraCost += Double.MinValue
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
                    Debug.Write(If(NextState.CapacityLeft < 0, "C", ""))
                    Continue For
                End If

                Dim Route As Route = RouteCache.GetRoute(Node.State.Point, W.Position)
                NextState.Time = Node.State.Time + Route.GetEstimatedTime
                NextState.FuelLeft = Node.State.FuelLeft - Route.GetKM * 0.1 'TODO fuel, also maybe leave extra time?
                'PRUNE this branch if it is unreachable given fuel reserves
                If NextState.FuelLeft < 0 Then
                    Debug.Write("F")
                    Continue For
                End If

                NextState.WayPointsLeft = New List(Of WayPoint)(Node.State.WayPointsLeft)
                NextState.WayPointsLeft.Remove(W)

                Dim ExtraCost As Double
                Select Case Minimiser
                    Case RouteFindingMinimiser.DISTANCE : ExtraCost = Route.GetKM
                    Case RouteFindingMinimiser.FUEL : ExtraCost = Route.GetKM * 0.1
                    Case Else : ExtraCost = Route.GetEstimatedHours
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
            Debug.Assert(SolveUsingAStar)
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

    Private Structure CourierPlanState 'TODO: Is class faster?
        Dim Point As IPoint
        Dim Time As TimeSpan
        Dim FuelLeft As Double
        Dim CapacityLeft As Double
        Dim WayPointsLeft As List(Of WayPoint)
    End Structure
End Class
