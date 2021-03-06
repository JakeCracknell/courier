﻿Public Class NodesAdjacencyList
    Public Rows As New Dictionary(Of Long, NodesAdjacencyListRow)
    Public Sub AddWay(ByVal Way As Way)
        If Way.Nodes.Length >= 2 Then
            Dim LastNode As Node = Way.Nodes(0)

            For i = 1 To Way.Nodes.Length - 1
                Dim CurrentNode As Node = Way.Nodes(i)
                AddNodesWay(LastNode, CurrentNode, Way)
                If Not Way.OneWay Then
                    AddNodesWay(CurrentNode, LastNode, Way)
                Else
                    AddNodeEmpty(CurrentNode)
                End If
                LastNode = CurrentNode
            Next
        End If
    End Sub

    Public Function GetRow(ByVal RoutingPoint As IPoint) As NodesAdjacencyListRow
        If TypeOf RoutingPoint Is Node Then
            Dim Node As Node = RoutingPoint
            Return Rows(Node.ID)
        Else
            Dim HopPosition As HopPosition = RoutingPoint
            Dim R As New NodesAdjacencyListRow(Nothing)
            R.AddCell(New NodesAdjacencyListCell(HopPosition.Hop.ToPoint, HopPosition.Hop.Way, HopPosition.Hop.GetDistance))
            If Not HopPosition.Hop.Way.OneWay Then
                R.AddCell(New NodesAdjacencyListCell(HopPosition.Hop.FromPoint, HopPosition.Hop.Way, HopPosition.Hop.GetDistance))
            End If
            Return R
        End If
    End Function

    Public Sub AddNodesWay(ByVal Node1 As Node, ByVal Node2 As Node, ByVal Way As Way)
        Dim Row As NodesAdjacencyListRow = AddNodeEmpty(Node1)

        Dim Cell As New NodesAdjacencyListCell(Node2, Way, HaversineDistance(Node1, Node2))
        Row.AddCell(Cell)
    End Sub

    Public Function AddNodeEmpty(ByVal Node As Node) As NodesAdjacencyListRow
        Dim Row As NodesAdjacencyListRow = Nothing
        Rows.TryGetValue(Node.ID, Row)
        If Row Is Nothing Then
            Row = New NodesAdjacencyListRow(Node)
            Rows.Add(Node.ID, Row)
        End If
        Return Row
    End Function

    Function GetRandomNode() As Node
        Dim Node As Node
        Do
            Node = Rows.Values(Int(RNG.R("random_row").NextDouble * Rows.Count)).NodeKey
        Loop Until Node.Connected
        Return Node
    End Function

    Function GetHopPositionFromNode(ByVal Node As Node) As HopPosition
        Return GetHopPositionFromNode(Node.ID)
    End Function
    Function GetHopPositionFromNode(ByVal NodeID As Long) As HopPosition
        Dim Row As NodesAdjacencyListRow = Rows(NodeID)
        Dim Hop As New Hop(Row.NodeKey, Row.Cells(0))
        Return New HopPosition(Hop, 0)
    End Function

    Function GetRandomPoint() As HopPosition
        Dim NodeA As Node = Nothing
        Dim NodeB As Node = Nothing
        Dim Way As Way = Nothing
        Do
            Dim RandomRow As NodesAdjacencyListRow = Rows.Values(Int(RNG.R("random_row").NextDouble * Rows.Count))
            If RandomRow.Cells.Count > 0 Then
                Dim RandomCell As NodesAdjacencyListCell = RandomRow.Cells(Int(RNG.R("random_cell").NextDouble * RandomRow.Cells.Count))
                NodeA = RandomRow.NodeKey
                NodeB = RandomCell.Node
                Way = RandomCell.Way
            End If
        Loop Until NodeA IsNot Nothing AndAlso NodeB IsNot Nothing AndAlso NodeA.Connected AndAlso NodeB.Connected
        Dim RandomPercentageTravelled As Double = RNG.R("random_hop_pc").NextDouble
        Return New HopPosition(New Hop(NodeA, NodeB, Way), RandomPercentageTravelled)
    End Function

    Function GetNearestPoint(ByVal Point As IPoint)
        Return GetHopPositionFromNode(GetNearestNode(Point.GetLatitude, Point.GetLongitude))
    End Function

    Function GetNearestNode(ByVal Point As IPoint)
        Return GetNearestNode(Point.GetLatitude, Point.GetLongitude)
    End Function
    Function GetNearestNode(ByVal Latitude As Double, ByVal Longitude As Double)
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue
        For Each Row As NodesAdjacencyListRow In Rows.Values
            Dim Node As Node = Row.NodeKey
            If Node.Connected Then
                Dim Distance As Double = HaversineDistance(Node.Latitude, Node.Longitude, Latitude, Longitude)
                If Distance < BestDistance Then
                    BestNode = Node
                    BestDistance = Distance
                End If
            End If
        Next
        Return BestNode
    End Function
    Function GetNearestNodeFuzzy(ByVal Latitude As Double, ByVal Longitude As Double)
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue
        For Each Row As NodesAdjacencyListRow In Rows.Values
            Dim Node As Node = Row.NodeKey
            If Node.Connected Then
                Dim Distance As Double = Math.Sqrt((Node.Latitude - Latitude) ^ 2 + (Node.Longitude - Longitude) ^ 2)
                If Distance < BestDistance Then
                    BestNode = Node
                    BestDistance = Distance
                End If
            End If
        Next
        Return BestNode
    End Function

    Sub RemoveDisconnectedComponents(ByVal CentralStartingNode As Node)
        Dim t As Stopwatch = Stopwatch.StartNew

        Dim FullyExploredNodes As New HashSet(Of Node)
        Dim DFSStack As New Stack(Of NodesAdjacencyListRow)
        Dim DFSStackIDs As New HashSet(Of Long)
        Dim VerifiedNodes As New HashSet(Of Node)

        'Starting from a given (or randomly chosen) node, run DFS and discover all connected nodes
        'that are reachable somehow from this node.
        Dim StartRow As NodesAdjacencyListRow = Rows(CentralStartingNode.ID)
        DFSStack.Push(StartRow)
        VerifiedNodes.Add(StartRow.NodeKey)
        Do
            Dim CurrentRow As NodesAdjacencyListRow = DFSStack.Peek
            For Each AdjacentCell As NodesAdjacencyListCell In CurrentRow.Cells
                If Not DFSStackIDs.Contains(AdjacentCell.Node.ID) AndAlso _
                        Not FullyExploredNodes.Contains(AdjacentCell.Node) Then
                    DFSStack.Push(Rows(AdjacentCell.Node.ID))
                    DFSStackIDs.Add(AdjacentCell.Node.ID)
                    Continue Do
                End If
            Next

            DFSStack.Pop()
            DFSStackIDs.Remove(CurrentRow.NodeKey.ID)
            FullyExploredNodes.Add(CurrentRow.NodeKey)
        Loop Until DFSStack.Count = 0

        Dim UnverifiedNodes As New HashSet(Of Node)
        Dim NodesToPrune As New HashSet(Of Node)
        UnverifiedNodes = New HashSet(Of Node)(FullyExploredNodes)
        UnverifiedNodes.ExceptWith(VerifiedNodes)

        'For each node discovered, verify them using a DFS to any known verified node.
        'Remove any that cannot find its way. This sections out inescapable node clusters.
        'Parallel.Foreach would require manual locking of all hashsets and did not bring
        'better performance due to this overhead.
        For Each Node As Node In UnverifiedNodes
            If Not VerifiedNodes.Contains(Node) Then
                'VerifiedNodes will be modified, as paths are found
                If Not DFSToAny(Node, VerifiedNodes, Me) Then
                    NodesToPrune.Add(Node)
                End If
            End If
        Next

        'Remove any disconnected components - those not found by the first DFS.
        For Each Row As NodesAdjacencyListRow In Rows.Values
            If Not FullyExploredNodes.Contains(Row.NodeKey) Then
                NodesToPrune.Add(Row.NodeKey)
            End If
        Next
        For Each NodeToPrune In NodesToPrune
            NodeToPrune.Connected = False
            Rows.Remove(NodeToPrune.ID)
        Next
        For Each Row As NodesAdjacencyListRow In Rows.Values
            For CellIndex = Row.Cells.Count - 1 To 0 Step -1
                If NodesToPrune.Contains(Row.Cells(CellIndex).Node) Then
                    Row.Cells.RemoveAt(CellIndex)
                End If
            Next
        Next

        Debug.WriteLine(NodesToPrune.Count & " nodes pruned, in " & t.ElapsedMilliseconds & " ms")
    End Sub


End Class
