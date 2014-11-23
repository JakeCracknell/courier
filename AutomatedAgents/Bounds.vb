
Public Class Bounds
    Public MinLatitude, MinLongitude, MaxLatitude, MaxLongitude As Double
    Public Sub New(ByVal MinLatitude As Double, ByVal MinLongitude As Double, _
                   ByVal MaxLatitude As Double, ByVal MaxLongitude As Double)
        Me.MinLatitude = MinLatitude
        Me.MinLongitude = MinLongitude
        Me.MaxLatitude = MaxLatitude
        Me.MaxLongitude = MaxLongitude
    End Sub
End Class
