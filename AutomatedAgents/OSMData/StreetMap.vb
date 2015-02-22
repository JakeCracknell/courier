Public Class StreetMap
    Public Nodes As New List(Of Node)
    Public Ways As New List(Of Way)
    Public FuelNodes As New List(Of IPoint)
    Public Bounds As Bounds
    Public NodesAdjacencyList As New NodesAdjacencyList

    Public Sub New(ByVal Bounds As Bounds)
        Me.Bounds = Bounds
    End Sub
End Class
