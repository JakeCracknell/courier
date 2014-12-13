Public Class Route
    Private Hops As List(Of Hop)
    Private TotalKM As Double = -1

    Public Sub New(ByVal Hops As List(Of Hop))
        Debug.Assert(Hops IsNot Nothing)
        Me.Hops = Hops
    End Sub

    'A route of 0
    Public Sub New(ByVal OnlyNode As Node)
        Me.Hops = New List(Of Hop)
        Me.Hops.Add(New Hop(OnlyNode, OnlyNode, Nothing))
    End Sub

    Public Function At(ByVal Index As Integer) As Hop
        Return Hops(Index)
    End Function

    Public Function GetStartNode() As Node
        Return Hops.First.FromNode
    End Function

    Public Function GetEndNode() As Node
        Return Hops.Last.ToNode
    End Function

    Public Function GetHopList() As List(Of Hop)
        Return Hops
    End Function

    Public Function HopCount() As Integer
        Return Hops.Count
    End Function

    Public Function GetKM() As Double
        'Lazily calculated, as I don't want a heavyweight constructor.
        If TotalKM < 0 Then
            For Each Hop As Hop In Hops
                TotalKM += Hop.GetCost
            Next
        End If
        Return TotalKM
    End Function
End Class
