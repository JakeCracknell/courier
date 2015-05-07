Imports System.Text

Module RouteFinderBenchmark
    Private Const BENCHMARK_MSGBOX_FORMAT As String = _
        "{0} routes found in {1} ms." & vbNewLine & "{2} ms each, on average"
    Private Const COMPARATIVE_BENCHMARK_MSGBOX_FORMAT As String = _
        "Against Basic A*, the alternative algorithm scored:" & vbNewLine & "{0} better" & vbNewLine & "{1} the same" & vbNewLine & "{2} worse"
    Private Const COMPARE_DISTANCE_MSGBOX_FORMAT As String = _
        "The actual street distance was on average {0}x greater than straight-line"

    Private Const BENCHMARK_TIME_MS As Integer = 5000

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Private Map As StreetMap
    Sub RunRouteFinderBenchmark(ByVal _Map As StreetMap)
        AdjacencyList = _Map.NodesAdjacencyList
        Map = _Map

        'Get far apart coordinates
        SourceNode = AdjacencyList.GetNearestNode(90, -180)
        DestinationNode = AdjacencyList.GetNearestNode(-90, 180)

        Dim Engine As New RouteFinderBenchmarkEngine
        Engine.RunAsync()
    End Sub

    Private Class RouteFinderBenchmarkEngine
        Sub RunAsync()
            Threading.ThreadPool.QueueUserWorkItem(AddressOf BenchmarkAStarEpsilon)
        End Sub

        Protected Sub Test()
            Dim Point1 As HopPosition = AdjacencyList.GetRandomPoint
            Dim Point2 As New HopPosition(New Hop(Point1.Hop.FromPoint, Point1.Hop.ToPoint, Point1.Hop.Way), Point1.PercentageTravelled / 2)

            Dim AStar As New AStarSearch(Point1, Point2, AdjacencyList, RouteFindingMinimiser.DISTANCE)

            MsgBox(AStar.GetRoute.GetHopList.Count)
        End Sub

        Protected Sub RunBenchmark()
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Dim CompletedRoutes As Integer = 0
            Do
                Dim AStar As New AStarSearch(SourceNode, DestinationNode, AdjacencyList, RouteFindingMinimiser.DISTANCE)
                CompletedRoutes += 1
            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS
            Dim TotalMS As Integer = Stopwatch.ElapsedMilliseconds
            MsgBox(String.Format(BENCHMARK_MSGBOX_FORMAT, CompletedRoutes, TotalMS, TotalMS / CompletedRoutes))
        End Sub

        Protected Sub CompareEuclidianVSAStarKM()
            Dim EuclidianTotal, AStarTotal As Double
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Do
                Dim S As IPoint = AdjacencyList.GetRandomPoint
                Dim D As IPoint = AdjacencyList.GetRandomPoint

                AStarTotal += New AStarSearch(S, D, AdjacencyList, RouteFindingMinimiser.DISTANCE).GetRoute.GetKM
                EuclidianTotal += HaversineDistance(S, D)

            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS
            MsgBox(String.Format(COMPARE_DISTANCE_MSGBOX_FORMAT, AStarTotal / EuclidianTotal))
        End Sub

        Protected Sub ComparativeBenchmarkKM()
            Dim Wins, Draws, Losses As Integer
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Do
                Dim S As Node = AdjacencyList.GetRandomNode
                Dim D As Node = AdjacencyList.GetRandomNode

                Dim AStar1 As New AStarSearch(S, D, AdjacencyList, RouteFindingMinimiser.DISTANCE)
                Dim AStar2 As New AStarSearch(S, D, AdjacencyList, RouteFindingMinimiser.TIME_NO_TRAFFIC)
                Dim AStarRoute1 As Route = AStar1.GetRoute
                Dim AStarRoute2 As Route = AStar2.GetRoute
                If AStarRoute1 IsNot Nothing And AStarRoute2 IsNot Nothing Then
                    Wins += IIf(AStarRoute1.GetKM < AStarRoute2.GetKM, 1, 0)
                    Draws += IIf(AStarRoute1.GetKM = AStarRoute2.GetKM, 1, 0)
                    Losses += IIf(AStarRoute1.GetKM > AStarRoute2.GetKM, 1, 0)

                    If AStarRoute1.GetKM > AStarRoute2.GetKM * 1.3 Then
                        MapGraphics.DrawRoute(AStarRoute1).Save("C:\pics\" & Stopwatch.ElapsedMilliseconds & "A.bmp")
                        MapGraphics.DrawRoute(AStarRoute2).Save("C:\pics\" & Stopwatch.ElapsedMilliseconds & "B.bmp")
                    End If
                End If
            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS
            MsgBox(String.Format(COMPARATIVE_BENCHMARK_MSGBOX_FORMAT, Wins, Draws, Losses))
        End Sub

        Protected Sub ComparativeBenchmarkJourneyTime()
            Dim Wins, Draws, Losses As Integer
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Do
                Dim S As Node = AdjacencyList.GetRandomNode
                Dim D As Node = AdjacencyList.GetRandomNode

                Dim AStar1 As New AStarSearch(S, D, AdjacencyList, RouteFindingMinimiser.DISTANCE)
                Dim AStar2 As New AStarSearch(S, D, AdjacencyList, RouteFindingMinimiser.TIME_NO_TRAFFIC)
                Dim AStarRoute1 As Route = AStar1.GetRoute
                Dim AStarRoute2 As Route = AStar2.GetRoute
                If AStarRoute1 IsNot Nothing And AStarRoute2 IsNot Nothing Then
                    Wins += IIf(AStarRoute1.GetHoursWithoutTraffic < AStarRoute2.GetHoursWithoutTraffic, 1, 0)
                    Draws += IIf(AStarRoute1.GetHoursWithoutTraffic = AStarRoute2.GetHoursWithoutTraffic, 1, 0)
                    Losses += IIf(AStarRoute1.GetHoursWithoutTraffic > AStarRoute2.GetHoursWithoutTraffic, 1, 0)
                End If
            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS
            MsgBox(String.Format(COMPARATIVE_BENCHMARK_MSGBOX_FORMAT, Wins, Draws, Losses))
        End Sub

        Protected Sub ComparativeBenchmarkAlgorithmSpeed()
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Dim CompletedRoutes As Integer = 0
            Do
                Dim AStar As New AStarSearch(SourceNode, DestinationNode, AdjacencyList, RouteFindingMinimiser.DISTANCE)
                CompletedRoutes += 1
            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS / 2
            Dim TotalMS As Integer = Stopwatch.ElapsedMilliseconds

            Dim Stopwatch1 As New Stopwatch
            Stopwatch1.Start()
            Dim CompletedRoutes1 As Integer = 0
            Do
                Dim AStar As New AStarSearch(SourceNode, DestinationNode, AdjacencyList, RouteFindingMinimiser.TIME_NO_TRAFFIC)
                CompletedRoutes1 += 1
            Loop Until Stopwatch1.ElapsedMilliseconds > BENCHMARK_TIME_MS / 2
            Dim TotalMS1 As Integer = Stopwatch1.ElapsedMilliseconds
            MsgBox(String.Format(BENCHMARK_MSGBOX_FORMAT, CompletedRoutes, TotalMS, TotalMS / CompletedRoutes) & _
                            vbNewLine & _
                    String.Format(BENCHMARK_MSGBOX_FORMAT, CompletedRoutes1, TotalMS1, TotalMS1 / CompletedRoutes1))
        End Sub

        Protected Sub BenchmarkAStarEpsilon()
            Dim OldAccelerator As Double = SimulationParameters.AStarAccelerator

            Dim t As Stopwatch = Stopwatch.StartNew
            Do
                Dim AStar As New AStarSearch(AdjacencyList.GetRandomNode, AdjacencyList.GetRandomNode, AdjacencyList, RouteFindingMinimiser.DISTANCE)
            Loop Until t.ElapsedMilliseconds >= 500

            Dim RouteCount As Integer = 50
            Dim StartPoints As New List(Of Node)
            Dim EndPoints As New List(Of Node)
            For i = 1 To RouteCount
                StartPoints.Add(AdjacencyList.GetRandomNode)
                EndPoints.Add(AdjacencyList.GetRandomNode)
            Next

            Dim Parameters() As Double = {1, 1.05, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2, 2.2, 2.4, 2.6, 2.8, 3, 4, 5}
            Dim Distances(Parameters.Count) As Double
            Dim Times(Parameters.Count) As Double
            For Param = 0 To Parameters.Length - 1
                t = Stopwatch.StartNew
                Dim TotalDistance As Double = 0
                SimulationParameters.AStarAccelerator = Parameters(Param)
                For Route = 0 To RouteCount - 1
                    Dim AStar As New AStarSearch(StartPoints(Route), EndPoints(Route), AdjacencyList, RouteFindingMinimiser.DISTANCE)
                    TotalDistance += AStar.GetRoute.GetKM
                    If t.ElapsedMilliseconds > 10000 AndAlso Param = 0 Then
                        RouteCount = Route + 1 'Short circuit if too much computation time
                        Exit For
                    End If
                Next
                Distances(Param) = TotalDistance
                Times(Param) = t.ElapsedMilliseconds / RouteCount
            Next
            For i = 1 To Parameters.Length - 1
                Distances(i) /= Distances(0)
            Next
            Distances(0) = 1.0

            Dim SB As New System.Text.StringBuilder
            SB.AppendLine("Accelerator // Distance % // Average Execution Time")
            For i = 0 To Parameters.Length - 1
                SB.Append(Parameters(i))
                SB.Append(vbTab)
                SB.Append(Distances(i))
                SB.Append(vbTab)
                SB.Append(Times(i))
                SB.Append(vbNewLine)
            Next

            SimulationParameters.AStarAccelerator = OldAccelerator

            MsgBox(SB.ToString)
        End Sub

        Protected Sub HaversineVsAStar()
            Dim t As Stopwatch = Stopwatch.StartNew
            Dim sb As New StringBuilder
            Dim Count As Integer = 0
            Do
                Dim p1 As HopPosition = AdjacencyList.GetRandomPoint
                Dim p2 As HopPosition = AdjacencyList.GetRandomPoint
                Dim AStar As New AStarSearch(p1, p2, AdjacencyList, RouteFindingMinimiser.TIME_WITH_TRAFFIC, TimeSpan.FromHours(12))
                sb.Append(HaversineDistance(p1, p2))
                sb.Append(",")
                sb.Append(AStar.GetRoute.GetEstimatedHours)
                sb.AppendLine()
                Count += 1
            Loop Until t.ElapsedMilliseconds > 5000
            Debug.WriteLine(sb.ToString)
            MsgBox(Count & " routes generated. See console for results.")

        End Sub

        Protected Sub PlannerTestEasy()
            RNG.Initialise()
            RouteCache.Initialise(Map.NodesAdjacencyList, RouteFindingMinimiser.FUEL_WITH_TRAFFIC)
            Dim A As New Agent(123, Map, Color.AliceBlue)
            Dim Dispatcher As New CityDispatcher(Map)
            Dim OldPlan As New CourierPlan(AdjacencyList.GetRandomPoint, Map, RouteFindingMinimiser.TIME_WITH_TRAFFIC, 1, Vehicles.Type.CAR)
            For i = 1 To 10
                Dim J As CourierJob = Dispatcher.GenerateJob()
                J.Deadline += TimeSpan.FromHours(12)
                OldPlan.WayPoints.AddRange(WayPoint.CreateWayPointList(J))
            Next
            OldPlan.RecreateRouteListFromWaypoints()
            A.Plan = OldPlan

            Dim Planner As New NNGAPlanner(A, True)
            Dim NewPlan As CourierPlan = Planner.GetPlan
            MsgBox(Planner.GetTotalCost + NewPlan.LateWaypointsCount * 1000)
        End Sub
    End Class
End Module
