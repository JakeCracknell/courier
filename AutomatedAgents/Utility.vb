﻿Module Utility
    Public dEbUgVaRiAbLe As Object

    Function GetRandomColor() As Color
        Return Color.FromArgb(Rnd() * 255, Rnd() * 255, Rnd() * 255)
    End Function
    Function GetDistance(ByVal Node1 As Node, ByVal Node2 As Node) As Double
        Return GetDistance(Node1.Latitude, Node1.Longitude, Node2.Latitude, Node2.Longitude)
    End Function
    Function GetDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Dim dDistance As Double
        Dim dLat1InRad As Double = lat1 * (Math.PI / 180.0)
        Dim dLong1InRad As Double = lon1 * (Math.PI / 180.0)
        Dim dLat2InRad As Double = lat2 * (Math.PI / 180.0)
        Dim dLong2InRad As Double = lon2 * (Math.PI / 180.0)

        Dim dLongitude As Double = dLong2InRad - dLong1InRad
        Dim dLatitude As Double = dLat2InRad - dLat1InRad

        ' Intermediate result a.
        Dim a As Double = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2.0), 2.0)

        ' Intermediate result c (great circle distance in Radians).
        Dim c As Double = 2.0 * Math.Asin(Math.Sqrt(a))

        ' Distance.
        Const kEarthRadiusKms As Double = 6376.5
        dDistance = kEarthRadiusKms * c

        Return dDistance
    End Function

    'Inaccurate. Can be up to 50m above actual sometimes
    Function GetFuzzyDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Return Math.Sqrt((lat1 - lat2) ^ 2 + (lon1 - lon2) ^ 2) * 111.2
    End Function
End Module
