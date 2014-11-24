Public Class Hop
    Public FromNode As Node
    Public Way As Way
    Public ToNode As Node

    Public Sub New(ByVal FromNode As Node, ByVal ToNode As Node, ByVal Way As Way)
        Me.FromNode = FromNode
        Me.ToNode = ToNode
        Me.Way = Way
    End Sub

    Public Sub New(ByVal FromNode As Node, ByVal Cell As NodesAdjacencyListCell)
        Me.FromNode = FromNode
        Me.ToNode = Cell.Node
        Me.Way = Cell.Way
    End Sub

    Public Function GetCost() As Double
        Return GetDistance(FromNode, ToNode)
    End Function

    Public Shared Function CloneList(ByVal List As List(Of Hop)) As List(Of Hop)
        Dim NewList As New List(Of Hop)
        For Each Hop As Hop In List
            NewList.Add(Hop)
        Next
        Return NewList
    End Function
End Class
