Public Interface RouteFinder
    Function GetRoute() As List(Of Hop)
    Function GetCost() As Double
End Interface
