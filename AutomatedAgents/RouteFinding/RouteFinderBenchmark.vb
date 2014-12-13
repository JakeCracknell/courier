Module RouteFinderBenchmark
    Private Const BENCHMARK_MSGBOX_FORMAT As String = _
        "{0} routes found in {1} ms." & vbNewLine & "{2} ms each, on average"
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
            Threading.ThreadPool.QueueUserWorkItem(AddressOf RunBenchmark)
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
    End Class
End Module
