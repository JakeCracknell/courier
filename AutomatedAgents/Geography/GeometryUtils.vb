Module GeometryUtils
    Function HaversineDistance(ByVal Node1 As IPoint, ByVal Node2 As IPoint) As Double
        Return HaversineDistance(Node1.GetLatitude, Node1.GetLongitude, Node2.GetLatitude, Node2.GetLongitude)
    End Function
    Function HaversineDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Dim dLat1InRad As Double = lat1 * (Math.PI / 180.0)
        Dim dLong1InRad As Double = lon1 * (Math.PI / 180.0)
        Dim dLat2InRad As Double = lat2 * (Math.PI / 180.0)
        Dim dLong2InRad As Double = lon2 * (Math.PI / 180.0)

        Dim dLongitude As Double = dLong2InRad - dLong1InRad
        Dim dLatitude As Double = dLat2InRad - dLat1InRad

        ' Intermediate result a. Math.Pow(_, 2) is much slower than *.
        Dim slat As Double = Math.Sin(dLatitude / 2.0)
        slat = slat * slat
        Dim slon As Double = Math.Sin(dLongitude / 2.0)
        slon = slon * slon
        Dim a As Double = slat + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * slon

        ' Intermediate result c (great circle distance in Radians).
        Dim c As Double = 2.0 * Math.Asin(Math.Sqrt(a))

        ' Distance.
        Const kEarthRadiusKms As Double = 6371
        Dim dDistance As Double = kEarthRadiusKms * c

        'SimulationParameters.DisplayedDebugVariable += 1

        Return dDistance
    End Function

    'Inaccurate. Can be up to 50m above actual sometimes
    Function GetFuzzyDistance(ByVal Node1 As IPoint, ByVal Node2 As IPoint) As Double
        Return GetFuzzyDistance(Node1.GetLatitude, Node1.GetLongitude, Node2.GetLatitude, Node2.GetLongitude)
    End Function
    Function GetFuzzyDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Return Math.Sqrt((lat1 - lat2) ^ 2 + (lon1 - lon2) ^ 2) * 79.45
    End Function

    Function GetCentralPointInPlane(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As PointF
        Dim MinX As Double = Math.Min(X1, X2)
        Dim MaxX As Double = Math.Max(X1, X2)
        Dim MinY As Double = Math.Min(Y1, Y2)
        Dim MaxY As Double = Math.Max(Y1, Y2)

        Return New PointF(MinX + (MaxX - MinX) / 2, MinY + (MaxY - MinY) / 2)
    End Function

    Function GetMidpointOfTwoPoints(ByVal p1 As IPoint, ByVal p2 As IPoint) As PointF
        Return GetMidpointOfTwoPoints(p1.GetLatitude, p1.GetLatitude, p2.GetLongitude, p2.GetLongitude)
    End Function
    Function GetMidpointOfTwoPoints(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As PointF
        Return New PointF((X1 + X2) / 2, (Y1 + Y2) / 2)
    End Function


    Public Function GetBearing(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double) As Double
        Dim dLon = lon2 - lon1
        Dim y = Math.Sin(dLon) * Math.Cos(lat2)
        Dim x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon)
        Return (180.0 / Math.PI) * Math.Atan2(y, x)
    End Function

    'Euclidian is too inaccurate. This is only called after each waypoint completion.
    Function PointsAreApproximatelyEqual(ByVal p1 As IPoint, ByVal p2 As IPoint) As Boolean
        Return HaversineDistance(p1, p2) < 0.001 ' 1 metre radius allowed.
    End Function
End Module
