Public Class StreetMap
    Public Nodes As New List(Of Node)
    Public Ways As New List(Of Way)
    Public FuelNodes As New List(Of IPoint)
    Public Bounds As Bounds
    Public NodesAdjacencyList As New NodesAdjacencyList
    Public Name As String

    Public Sub New(ByVal Name As String, ByVal Bounds As Bounds)
        Me.Name = Name
        Me.Bounds = Bounds
    End Sub
End Class
