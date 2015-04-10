
Public Class Bounds
    Public MinLatitude, MinLongitude, MaxLatitude, MaxLongitude As Double
    Public Sub New(ByVal MinLatitude As Double, ByVal MinLongitude As Double, _
                   ByVal MaxLatitude As Double, ByVal MaxLongitude As Double)
        Me.MinLatitude = MinLatitude
        Me.MinLongitude = MinLongitude
        Me.MaxLatitude = MaxLatitude
        Me.MaxLongitude = MaxLongitude
    End Sub

    Function GetCentralPoint() As PointF

        Return New PointF(MinLatitude + (MaxLatitude - MinLatitude) / 2, MinLongitude + (MaxLongitude - MinLongitude) / 2)
    End Function
End Class
