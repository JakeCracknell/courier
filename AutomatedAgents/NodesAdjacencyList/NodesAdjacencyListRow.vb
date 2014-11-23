Public Class NodesAdjacencyListRow
    'Implements IEnumerable()
    Public NodeKey As Node
    Public Cells As New List(Of NodesAdjacencyListCell)

    Public Sub New(ByVal NodeKey As Node)
        Me.NodeKey = NodeKey
    End Sub

    Public Sub AddCell(ByVal Cell As NodesAdjacencyListCell)
        'Believe it or not, some ways go like A,B,C,D,D,E,F...
        '2951577727 on way: http://www.openstreetmap.org/way/291688733
        If NodeKey.ID <> Cell.Node.ID Then
            Cells.Add(Cell)
        End If
    End Sub

End Class