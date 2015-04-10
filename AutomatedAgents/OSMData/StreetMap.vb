Public Class StreetMap
    Public Name As String
    Public Bounds As Bounds

    Public Nodes As New List(Of Node)
    Public Ways As New List(Of Way)
    Public NodesAdjacencyList As New NodesAdjacencyList

    Public Depots As List(Of IPoint)
    Public FuelPoints As New List(Of IPoint)

    Public Sub New(ByVal Name As String, ByVal Bounds As Bounds)
        Me.Name = Name
        Me.Bounds = Bounds
    End Sub

    Public Function GetNearestDepot(ByVal Point As IPoint) As IPoint
        Return GetNearestLandmark(Point, Depots)
    End Function
    Public Function GetNearestFuelPoint(ByVal Point As IPoint) As IPoint
        Return GetNearestLandmark(Point, FuelPoints)
    End Function
End Class
