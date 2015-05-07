Public Interface IPlanner
    Function IsSuccessful() As Boolean
    Function GetPlan() As CourierPlan
    Function GetTotalCost() As Double
End Interface
