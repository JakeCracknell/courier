Public Class NodesGrid
    Private Const CELL_COUNT_EDGE As Integer = 100

    'Mostly empty. Best case on 64 bit: 80KB
    Private NodeLists(CELL_COUNT_EDGE, CELL_COUNT_EDGE) As List(Of Node)
    Private Bounds As Bounds
    Private CellSizeLat As Double
    Private CellSizeLon As Double

    Public Sub New(ByVal Bounds As Bounds)
        Me.Bounds = Bounds
        CellSizeLat = (Bounds.MaxLatitude - Bounds.MinLatitude) / 100
        CellSizeLon = (Bounds.MaxLongitude - Bounds.MinLongitude) / 100
    End Sub

    Public Function GetNearestNode(ByVal IPoint As IPoint) As Node
        Return GetNearestNode(IPoint.GetLatitude, IPoint.GetLongitude)
    End Function
    Public Function GetNearestNode(ByVal Lat As Double, ByVal Lon As Double, Optional ByVal MaxRadius As Integer = CELL_COUNT_EDGE) As Node 'TODO: use this everywhere
        If Not Bounds.Encloses(Lat, Lon) Then
            Return Nothing
        End If

        Dim CentralCellX As Integer = CInt((Lat - Bounds.MinLatitude) / CellSizeLat)
        Dim CentralCellY As Integer = CInt((Lon - Bounds.MinLongitude) / CellSizeLon)
        Dim BestNode As Node = Nothing
        Dim BestDistance As Double = Double.MaxValue

        'Starting with a quadrant length of 3, reach of 1, (meaning to check all neighbouring cells. A 3x3 grid).
        'Expand out by a reach of 1 each time until any node is found.
        For QuadrantLength As Integer = 3 To Math.Min(CELL_COUNT_EDGE, MaxRadius * 2 + 1) Step 2
            Dim QuadrantRadius As Integer = QuadrantLength \ 2
            For x = Math.Max(0, CentralCellX - QuadrantRadius) To Math.Min(CELL_COUNT_EDGE - 1, CentralCellX + QuadrantRadius)
                For y = Math.Max(0, CentralCellY - QuadrantRadius) To Math.Min(CELL_COUNT_EDGE - 1, CentralCellY + QuadrantRadius)
                    Dim NodeList As List(Of Node) = NodeLists(x, y)
                    If NodeList IsNot Nothing Then
                        For Each Node In NodeList
                            Dim Distance As Double = HaversineDistance(Node.Latitude, Node.Longitude, Lat, Lon)
                            If Distance < BestDistance Then
                                BestNode = Node
                                BestDistance = Distance
                            End If
                        Next
                    End If
                Next
            Next
            If BestNode IsNot Nothing Then
                Return BestNode
            End If
        Next

        Debug.Assert(False) 'Only if the map has no nodes or maxradius is set.
        Return Nothing
    End Function

    Public Sub AddNode(ByVal Node As Node)
        Dim CentralCellX As Integer = CInt((Node.Latitude - Bounds.MinLatitude) / CellSizeLat)
        Dim CentralCellY As Integer = CInt((Node.Longitude - Bounds.MinLongitude) / CellSizeLon)
        If NodeLists(CentralCellX, CentralCellY) Is Nothing Then
            NodeLists(CentralCellX, CentralCellY) = New List(Of Node)
        End If
        NodeLists(CentralCellX, CentralCellY).Add(Node)
    End Sub

    Public Sub ListAll()
        For x = 0 To CELL_COUNT_EDGE - 1
            For y = 0 To CELL_COUNT_EDGE - 1
                Dim List = NodeLists(x, y)
                Debug.Write(If(List IsNot Nothing, List.Count, 0) & " ")
            Next
            Debug.Write(vbNewLine)
        Next
    End Sub

    Sub DrawAsText()
        For x = 0 To CELL_COUNT_EDGE - 1
            For y = 0 To CELL_COUNT_EDGE - 1
                Dim List = NodeLists(x, y)
                Debug.Write(If(List IsNot Nothing, "#", " "))
            Next
            Debug.Write(vbNewLine)
        Next
    End Sub

End Class
