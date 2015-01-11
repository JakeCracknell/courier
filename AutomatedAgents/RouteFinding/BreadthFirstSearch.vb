Public Class BreadthFirstSearch
    Implements RouteFinder

    Private SourceNode As Node
    Private DestinationNode As Node
    Private AdjacencyList As NodesAdjacencyList
    Private HopList As List(Of Hop)
    Private Cost As Integer
    Private NodesSearched As New List(Of Node)

    Sub New(ByVal SourceNode As Node, ByVal DestinationNode As Node, ByVal AdjacencyList As NodesAdjacencyList)
        Me.SourceNode = SourceNode
        Me.DestinationNode = DestinationNode
        Me.AdjacencyList = AdjacencyList
        DoBFS()
    End Sub

    Public Function GetRoute() As Route Implements RouteFinder.GetRoute
        Return New Route(HopList)
    End Function

    Private Sub DoBFS()
        Dim Queue As New Queue(Of List(Of Hop))
        Dim InitialItem As New List(Of Hop)
        InitialItem.Add(New Hop(SourceNode, SourceNode, Nothing))
        Queue.Enqueue(InitialItem)

        Do Until Queue.Count = 0
            Dim ListOfHops = Queue.Dequeue
            Dim CurrentNode As Node = ListOfHops(ListOfHops.Count - 1).ToNode
            If CurrentNode.ID = DestinationNode.ID Then
                HopList = ListOfHops
                Exit Sub
            End If

            Dim Cells As List(Of NodesAdjacencyListCell) = AdjacencyList.Rows(CurrentNode.ID).Cells
            For Each NextPath As NodesAdjacencyListCell In Cells
                'Checks not going back on itself or revisiting a node.
                Dim AlreadyTraversed As Boolean = False
                For Each PastHop As Hop In ListOfHops
                    If PastHop.FromNode.ID = NextPath.Node.ID Then
                        AlreadyTraversed = True
                        Exit For
                    End If
                Next
                If Not AlreadyTraversed Then
                    Dim NextHop As New Hop(CurrentNode, NextPath)
                    Dim NextList As List(Of Hop) = ListOfHops.ToList 'Cloned
                    NextList.Add(NextHop)
                    Queue.Enqueue(NextList)
                    NodesSearched.Add(NextPath.Node)
                End If

            Next
        Loop

        Debug.WriteLine("No route found - disconnected graph?")
    End Sub

    Public Function GetNodesSearched() As List(Of Node) Implements RouteFinder.GetNodesSearched
        Return NodesSearched
    End Function
End Class
