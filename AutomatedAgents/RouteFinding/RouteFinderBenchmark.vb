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
            Threading.ThreadPool.QueueUserWorkItem(AddressOf ComparativeBenchmark)
        End Sub

        Protected Sub RunBenchmark()
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Dim CompletedRoutes As Integer = 0
            Do
                Dim AStar As New AStarSearch(SourceNode, DestinationNode, AdjacencyList)
                CompletedRoutes += 1
            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS
            Dim TotalMS As Integer = Stopwatch.ElapsedMilliseconds
            MsgBox(String.Format(BENCHMARK_MSGBOX_FORMAT, CompletedRoutes, TotalMS, TotalMS / CompletedRoutes))
        End Sub


        Protected Sub ComparativeBenchmark()
            Dim Wins, Draws, Losses As Integer
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Do
                Dim S As Node = AdjacencyList.GetRandomNode
                Dim D As Node = AdjacencyList.GetRandomNode

                Dim AStar1 As New AStarSearch(S, D, AdjacencyList)
                Dim AStar2 As New AStarSearchArrayBased(S, D, AdjacencyList)

                Wins += IIf(AStar1.GetCost < AStar2.GetCost, 1, 0)
                Draws += IIf(AStar1.GetCost = AStar2.GetCost, 1, 0)
                Losses += IIf(AStar1.GetCost > AStar2.GetCost, 1, 0)

                If AStar1.GetCost > AStar2.GetCost * 1.3 Then
                    Debug.WriteLine(AStar1.GetCost / AStar2.GetCost)
                    MapGraphics.DrawRoute(AStar1.GetRoute).Save("C:\pics\" & Stopwatch.ElapsedMilliseconds & "A.bmp")
                    MapGraphics.DrawRoute(AStar2.GetRoute).Save("C:\pics\" & Stopwatch.ElapsedMilliseconds & "B.bmp")
                End If
            Loop Until Stopwatch.ElapsedMilliseconds > BENCHMARK_TIME_MS
            MsgBox(String.Format(COMPARATIVE_BENCHMARK_MSGBOX_FORMAT, Wins, Draws, Losses))
        End Sub
    End Class
End Module
