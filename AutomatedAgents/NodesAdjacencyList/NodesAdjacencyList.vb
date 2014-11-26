Public Class NodesAdjacencyList
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

    Public Sub AddNodesWay(ByVal Node1 As Node, ByVal Node2 As Node, ByVal Way As Way)
        Dim Row As NodesAdjacencyListRow = AddNodeEmpty(Node1)

        Dim Cell As New NodesAdjacencyListCell(Node2, Way)
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
        Return Rows.Values(Int(Rnd() * Rows.Count)).NodeKey
    End Function

    Function GetNearestNode(ByVal Latitude As Double, ByVal Longitude As Double)
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue
        For Each Row As NodesAdjacencyListRow In Rows.Values
            Dim Distance As Double = GetDistance(Row.NodeKey.Latitude, Row.NodeKey.Longitude, Latitude, Longitude)
            If Distance < BestDistance Then
                BestNode = Row.NodeKey
                BestDistance = Distance
            End If
        Next
        Return BestNode
    End Function
End Class
