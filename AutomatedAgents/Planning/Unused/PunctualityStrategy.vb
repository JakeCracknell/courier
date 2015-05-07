Public Class PunctualityStrategy
    Enum PStrategy
        REDUNDANCY_TIME
        MINIMISE_LATENESS_TOTAL
        MINIMISE_LATE_DELIVERIES
        MINIMISE_ASTAR_COST
    End Enum
    Public Strategy As PStrategy
    Public RedundancyTime As TimeSpan = TimeSpan.Zero

    Sub New(ByVal Strategy As PStrategy)
        Me.Strategy = Strategy
    End Sub

    Sub New(ByVal Redundancy As TimeSpan)
        Me.Strategy = PStrategy.REDUNDANCY_TIME
        RedundancyTime = Redundancy
    End Sub
End Class
