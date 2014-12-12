Public Class AStarSearch
    Implements RouteFinder

    Private Enum ASTAR_MODE
        OPTIMUM_SLOW
        FUZZY_FAST
    End Enum
    Private Const MODE As ASTAR_MODE = ASTAR_MODE.OPTIMUM_SLOW

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Private HopList As List(Of Hop)
    Private NodesSearched As New List(Of Node)
    Private Cost As Double
    Private Const EPSILON As Double = 0.0000001

    Sub New(ByVal SourceNode As Node, ByVal DestinationNode As Node, ByVal AdjacencyList As NodesAdjacencyList)
        Me.SourceNode = SourceNode
        Me.DestinationNode = DestinationNode
        Me.AdjacencyList = AdjacencyList
        DoAStar()
    End Sub

    Public Sub DoAStar()
        'For debugging and assessing performance
        Dim IterationCount As Integer = 0
        Dim Stopwatch As New Stopwatch
        Stopwatch.Start()

        Dim PriorityQueue As New SortedList(Of Double, AStarTreeNode)
        Dim AlreadyVisited As New HashSet(Of Long)
        Dim BestDistancesToNodess As New SortedList(Of Long, Double)
        Dim BestDistancesToNodes As New Dictionary(Of Long, Double)
        PriorityQueue.Add(0, New AStarTreeNode(SourceNode))
        Do
            Dim AStarTreeNode As AStarTreeNode = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If AStarTreeNode.GetCurrentNode = DestinationNode Then
                HopList = AStarTreeNode.Route
                Cost = AStarTreeNode.TotalCost
                dEbUgVaRiAbLe = Stopwatch.ElapsedMilliseconds & "ms iterations: " & IterationCount & " metres: " & Cost * 1000
                Exit Sub
            End If

            'MapGraphics.DrawRoute(AStarTreeNode.Route).Save("C:\pics\" & i & ".bmp")
            IterationCount += 1
            'Debug.WriteLine(PriorityQueue.Count)


            NodesSearched.Add(AStarTreeNode.GetCurrentNode)
            AlreadyVisited.Add(AStarTreeNode.GetCurrentNode.ID)

            Dim Row As NodesAdjacencyListRow = AdjacencyList.Rows(AStarTreeNode.GetCurrentNode.ID)
            For Each Cell As NodesAdjacencyListCell In Row.Cells
                If Not AlreadyVisited.Contains(Cell.Node.ID) Then
                    Dim NextAStarTreeNode As New AStarTreeNode(AStarTreeNode, Cell)

                    Dim HeuristicCost As Double = GetDistance(Cell.Node, DestinationNode)
                    Dim F_Cost As Double = HeuristicCost + NextAStarTreeNode.TotalCost
                    Do Until Not PriorityQueue.ContainsKey(F_Cost) 'Exception can occur otherwise
                        F_Cost += EPSILON
                    Loop

                    
                    'Both modes are the same speed now, so should delete fuzzy.
                    'Keep it until refactoring a* complete
                    Select Case MODE
                        Case ASTAR_MODE.FUZZY_FAST
                            'Note, this is a non-standard optimisation of the A* algorithm:
                            'Adding a node to the visitedlist before it is actually explored
                            'See astar_test map for an example of where this fails to give an optimal route.
                            AlreadyVisited.Add(Cell.Node.ID)
                            PriorityQueue.Add(F_Cost, NextAStarTreeNode)


                        Case ASTAR_MODE.OPTIMUM_SLOW
                            If BestDistancesToNodes.ContainsKey(Cell.Node.ID) Then
                                Dim CompetingDistance As Double = BestDistancesToNodes(Cell.Node.ID)
                                If F_Cost < CompetingDistance Then
                                    BestDistancesToNodes(Cell.Node.ID) = F_Cost
                                    PriorityQueue.Add(F_Cost, NextAStarTreeNode)
                                End If
                            Else
                                BestDistancesToNodes.Add(Cell.Node.ID, F_Cost)
                                PriorityQueue.Add(F_Cost, NextAStarTreeNode)
                            End If
                    End Select
                    'also investigate if I am insane or if it is looking too far
                    'to the side of the map with a*. crete for example!
                    

                End If
            Next
        Loop Until PriorityQueue.Count = 0

        Debug.WriteLine("No route found - disconnected graph?")

    End Sub

    Public Function GetCost() As Double Implements RouteFinder.GetCost
        Return Cost
    End Function

    Public Function GetRoute() As List(Of Hop) Implements RouteFinder.GetRoute
        Return HopList
    End Function

    Public Function GetNodesSearched() As List(Of Node) Implements RouteFinder.GetNodesSearched
        Return NodesSearched
    End Function

    Private Class AStarTreeNode
        Public Route As List(Of Hop)
        Public ReadOnly TotalCost As Double

        Public Sub New(ByVal StartNode As Node)
            Route = New List(Of Hop)
            Route.Add(New Hop(StartNode, StartNode, Nothing))
            TotalCost = 0
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastHop As Hop)
            Route = Hop.CloneList(OldTree.Route)
            Route.Add(LastHop)
            TotalCost = OldTree.TotalCost + LastHop.GetCost
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNode, ByVal LastNodeWay As NodesAdjacencyListCell)
            Me.New(OldTree, New Hop(OldTree.Route.Last.ToNode, LastNodeWay))
        End Sub

        Public Function GetCurrentNode() As Node
            Return Route.Last.ToNode
        End Function
    End Class
End Class
