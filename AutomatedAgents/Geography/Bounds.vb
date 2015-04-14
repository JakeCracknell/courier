
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

    Function Encloses(ByVal Point As IPoint) As Boolean
        Return Encloses(Point.GetLatitude, Point.GetLongitude)
    End Function

    Function Encloses(Lat As Double, Lon As Double) As Boolean
        Return Lat > MinLatitude AndAlso Lat < MaxLatitude AndAlso Lon > MinLongitude AndAlso Lon < MaxLongitude
    End Function

End Class
