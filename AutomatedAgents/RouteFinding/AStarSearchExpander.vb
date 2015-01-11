'Does not guarentee optimal results and is in most cases,
'slower than AStarSearch due to the extra overhead of expanding nodes:
'For Guernsey: 88ms vs 52ms, For Crete: 1467 vs 2663, For Leoti: 19 vs 4.
'Much better for long winding roads. Slower for urban areas.
Public Class AStarSearchExpander
    Implements RouteFinder

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Private Route As Route
    Private NodesSearched As New List(Of Node)
    Private Cost As Double
    Private Const EPSILON As Double = 0.0000001

    Sub New(ByVal SourceNode As Node, ByVal DestinationNode As Node, ByVal AdjacencyList As NodesAdjacencyList)
        Me.SourceNode = SourceNode
        Me.DestinationNode = DestinationNode
        Me.AdjacencyList = AdjacencyList
        DoAStar()
    End Sub

    Private Sub DoAStar()
        'For debugging and assessing performance
        Dim IterationCount As Integer = 0
        Dim Stopwatch As New Stopwatch
        Stopwatch.Start()

        Dim PriorityQueue As New SortedList(Of Double, AStarTreeNodeExpandable)
        Dim AlreadyVisitedNodes As New HashSet(Of Node)
        Dim BestDistancesToNodes As New Dictionary(Of Long, Double)
        PriorityQueue.Add(0, New AStarTreeNodeExpandable(SourceNode))
        Do
            Dim AStarTreeNode As AStarTreeNodeExpandable = PriorityQueue.Values(0)
            PriorityQueue.RemoveAt(0)
            If AStarTreeNode.GetCurrentNode = DestinationNode Then
                Route = New Route(New List(Of Hop)(AStarTreeNode.RouteHops))
                Cost = AStarTreeNode.TotalCost
                dEbUgVaRiAbLe = Stopwatch.ElapsedMilliseconds & "ms iterations: " & IterationCount & " metres: " & Cost * 1000
                Exit Sub
            End If

            'MapGraphics.DrawRoute(New Route(AStarTreeNode.RouteHops.ToList)).Save("C:\pics\" & IterationCount & ".bmp")
            IterationCount += 1
            'Debug.WriteLine(PriorityQueue.Count)


            NodesSearched.Add(AStarTreeNode.GetCurrentNode)

            Dim Row As NodesAdjacencyListRow = AdjacencyList.Rows(AStarTreeNode.GetCurrentNode.ID)
            For Each Cell As NodesAdjacencyListCell In Row.Cells
                If Not AlreadyVisitedNodes.Contains(Cell.Node) Then
                    Dim NextAStarTreeNode As New AStarTreeNodeExpandable(AStarTreeNode, Cell)

                    NextAStarTreeNode.Expand(AdjacencyList, DestinationNode)

                    Dim HeuristicCost As Double = GetDistance(Cell.Node, DestinationNode)
                    Dim F_Cost As Double = HeuristicCost + NextAStarTreeNode.TotalCost
                    Do Until Not PriorityQueue.ContainsKey(F_Cost) 'Exception can occur otherwise
                        F_Cost += EPSILON
                    Loop


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

                    'TODO investigate if I am insane or if it is looking too far
                    'to the side of the map with a*. crete for example!


                End If
            Next
        Loop Until PriorityQueue.Count = 0

        Debug.WriteLine("No route found - disconnected graph?")

    End Sub

    Public Function GetRoute() As Route Implements RouteFinder.GetRoute
        Return Route
    End Function

    Public Function GetNodesSearched() As List(Of Node) Implements RouteFinder.GetNodesSearched
        Return NodesSearched
    End Function



    Private Class AStarTreeNodeExpandable
        Public RouteHops As Hop()
        Public TotalCost As Double

        Public Sub New(ByVal StartNode As Node)
            RouteHops = {New Hop(StartNode, StartNode, Nothing)}
            TotalCost = 0
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNodeExpandable, ByVal LastHop As Hop)
            ReDim RouteHops(OldTree.RouteHops.Length) 'One index larger
            Array.Copy(OldTree.RouteHops, RouteHops, OldTree.RouteHops.Length)
            RouteHops(RouteHops.Length - 1) = LastHop
            TotalCost = OldTree.TotalCost + LastHop.GetCost
        End Sub

        Public Sub New(ByVal OldTree As AStarTreeNodeExpandable, ByVal LastNodeWay As NodesAdjacencyListCell)
            Me.New(OldTree, New Hop(OldTree.RouteHops.Last.ToNode, LastNodeWay))
        End Sub

        Public Sub Expand(ByVal AdjList As NodesAdjacencyList, ByVal StopNode As Node)
            If GetCurrentNode() = StopNode Then
                Exit Sub
            End If

            Dim ExtraHops As New List(Of Hop)
            Dim Row As NodesAdjacencyListRow = AdjList.Rows(RouteHops.Last.ToNode.ID)
            Dim OnlyCell As NodesAdjacencyListCell = Row.Cells.FirstOrDefault
            Dim LastNode As Node = RouteHops.Last.FromNode

            Do Until OnlyCell Is Nothing OrElse OnlyCell.Node = StopNode
                Dim AdjacentUnseenNodes As Integer = 0
                Dim AdjacentNodeIndex As Integer = 0
                While AdjacentNodeIndex < Row.Cells.Count And AdjacentUnseenNodes < 2
                    If LastNode <> Row.Cells(AdjacentNodeIndex).Node Then
                        AdjacentUnseenNodes += 1
                        OnlyCell = Row.Cells(AdjacentNodeIndex)
                    End If
                    AdjacentNodeIndex += 1
                End While
                If AdjacentUnseenNodes = 1 Then
                    Dim ExtraHop As Hop = New Hop(Row.NodeKey, OnlyCell)
                    ExtraHops.Add(ExtraHop)
                    TotalCost += ExtraHop.GetCost

                    Row = AdjList.Rows(OnlyCell.Node.ID)
                    LastNode = ExtraHop.FromNode
                Else
                    Exit Do
                End If
            Loop

            'Add additional hops to existing array and hashset
            Array.Resize(RouteHops, RouteHops.Length + ExtraHops.Count)
            Array.Copy(ExtraHops.ToArray, 0, RouteHops, RouteHops.Length - ExtraHops.Count, ExtraHops.Count)
        End Sub

        Public Function GetCurrentNode() As Node
            Return RouteHops.Last.ToNode
        End Function
    End Class
End Class