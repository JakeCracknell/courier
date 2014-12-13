Public Class WayPosition
    Public Hop As Hop
    Public PercentageTravelled As Double

    Sub New(ByVal Hop As Hop, ByVal PercentageTravelled As Double)
        Me.Hop = Hop
        Me.PercentageTravelled = PercentageTravelled
    End Sub
End Class
