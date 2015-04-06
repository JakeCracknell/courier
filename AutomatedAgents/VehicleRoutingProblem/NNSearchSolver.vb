Public Class NNSearchSolver
    Private Minimiser As RouteFindingMinimiser
    Private PunctualityStrategy As SolverPunctualityStrategy
    Private StartState As CourierPlanState
    Private OldPlan As CourierPlan
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
        StartState.Point = CourierPlan.StartPoint
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

    Private Sub Solve()
        Dim t As New Stopwatch
        t.Start()
        'Preprocess to predetermine if will be late - searching whole tree takes a lot of time.
        Dim LateNodes As New List(Of NNSearchNode)
        Dim GreedyBFSStack As New Stack(Of NNSearchNode)
        GreedyBFSStack.Push(New NNSearchNode(StartState))
        Do
            Dim Node As NNSearchNode = GreedyBFSStack.Pop
            If Node.State.WayPointsLeft.Count = 0 Then
                TotalCost = Node.TotalCost
                RecreateCourierPlan(Node)
                Exit Sub
            End If

            Dim NextNodes As New SortedList(Of Double, NNSearchNode)
            For Each W As WayPoint In Node.State.WayPointsLeft
                Dim NextState As CourierPlanState
                NextState.Point = W.Position
                NextState.CapacityLeft = Node.State.CapacityLeft + W.VolumeDelta
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
                'If any jobs' deadlines come before the time of this route,
                ' subtracting redundancy time (if any), PRUNE this branch
                ' The failed branch may be used later on for contingency planning
                If Node.State.WayPointsLeft.Any(Function(x)
                                                    Return x.Job.Deadline - PunctualityStrategy.RedundancyTime < NextState.Time
                                                End Function) Then
                    If PunctualityStrategy.Strategy <> SolverPunctualityStrategy.PStrategy.REDUNDANCY_TIME Then
                        LateNodes.Add(NextNode)
                    End If
                    Debug.Write("T")
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

        Loop Until GreedyBFSStack.Count = 0 Or t.ElapsedMilliseconds >= 200

        If PunctualityStrategy.Strategy = SolverPunctualityStrategy.PStrategy.REDUNDANCY_TIME Then
            Solution = Nothing
            Exit Sub
        End If

        'Contingency using A*
        '
        '
        '
        '
        '
        '
        '
        '
        '?!

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
