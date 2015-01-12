Module RouteFinderBenchmark
    Private Const BENCHMARK_MSGBOX_FORMAT As String = _
        "{0} routes found in {1} ms." & vbNewLine & "{2} ms each, on average"
    Private Const COMPARATIVE_BENCHMARK_MSGBOX_FORMAT As String = _
        "Against Basic A*, the alternative algorithm scored:" & vbNewLine & "{0} better" & vbNewLine & "{1} the same" & vbNewLine & "{2} worse"
    Private Const BENCHMARK_TIME_MS As Integer = 5000

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Sub RunRouteFinderBenchmark(ByVal _AdjacencyList As NodesAdjacencyList)
        AdjacencyList = _AdjacencyList

        'Get far apart coordinates
        SourceNode = AdjacencyList.GetNearestNode(90, -180)
        DestinationNode = AdjacencyList.GetNearestNode(-90, 180)

        Dim Engine As New RouteFinderBenchmarkEngine
        Engine.RunAsync()
    End Sub

    Private Class RouteFinderBenchmarkEngine
        Sub RunAsync()
            Threading.ThreadPool.QueueUserWorkItem(AddressOf ComparativeBenchmarkAlgorithmSpeed)
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
                        Debug.WriteLine(AStarRoute1.GetKM / AStarRoute2.GetKM)
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
                    Wins += IIf(AStarRoute1.GetEstimatedHours < AStarRoute2.GetEstimatedHours, 1, 0)
                    Draws += IIf(AStarRoute1.GetEstimatedHours = AStarRoute2.GetEstimatedHours, 1, 0)
                    Losses += IIf(AStarRoute1.GetEstimatedHours > AStarRoute2.GetEstimatedHours, 1, 0)
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
    End Class
End Module
