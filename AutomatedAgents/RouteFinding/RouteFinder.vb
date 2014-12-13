Public Interface RouteFinder
    Function GetRoute() As Route
    Function GetCost() As Double

    Function GetNodesSearched() As List(Of Node)

End Interface
