Public Interface RouteFinder
    Function GetRoute() As List(Of Hop)
    Function GetCost() As Double

    Function GetNodesSearched() As List(Of Node)

End Interface
