Public Class StreetMap
    Public Name As String
    Public Bounds As Bounds

    Public Nodes As New List(Of Node)
    Public Ways As New SortedList(Of Long, Way)
    Public NodesAdjacencyList As New NodesAdjacencyList
    Public ConnectedNodesGrid As NodesGrid

    Public Depots As New List(Of Node)
    Public FuelPoints As New List(Of Node)
    Public Businesses As New List(Of Node)

    Public Sub New(ByVal Name As String, ByVal Bounds As Bounds)
        Me.Name = Name
        Me.Bounds = Bounds
        ConnectedNodesGrid = New NodesGrid(Bounds)
    End Sub

    'Starting point is a randomly chosen depot
    Public Function GetStartingPoint() As HopPosition
        Return NodesAdjacencyList.GetHopPositionFromNode(Depots(Int(Rnd() * Depots.Count)).ID)
    End Function
    Public Function GetNearestDepot(ByVal Point As IPoint) As HopPosition
        Return NodesAdjacencyList.GetHopPositionFromNode(GetNearestLandmark(Point, Depots).ID)
    End Function
    Public Function GetNearestFuelPoint(ByVal Point As IPoint) As HopPosition
        Return NodesAdjacencyList.GetHopPositionFromNode(GetNearestLandmark(Point, FuelPoints).ID)
    End Function
End Class
