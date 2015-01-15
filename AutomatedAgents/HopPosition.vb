Public Class HopPosition
    Implements IEquatable(Of HopPosition)
    Implements RoutingPoint
    Public Hop As Hop
    Public PercentageTravelled As Double

    Private Longitude, Latitude As Double

    Sub New(ByVal Hop As Hop, ByVal PercentageTravelled As Double)
        Me.Hop = Hop
        Me.PercentageTravelled = PercentageTravelled

        If Hop.FromPoint.GetLatitude < Hop.ToPoint.GetLatitude Then
            Latitude = Hop.FromPoint.GetLatitude + (Hop.ToPoint.GetLatitude - Hop.FromPoint.GetLatitude) * PercentageTravelled
        Else
            Latitude = Hop.ToPoint.GetLatitude + (Hop.FromPoint.GetLatitude - Hop.ToPoint.GetLatitude) * (1 - PercentageTravelled)
        End If

        If Hop.FromPoint.GetLongitude < Hop.ToPoint.GetLongitude Then
            Longitude = Hop.FromPoint.GetLongitude + (Hop.ToPoint.GetLongitude - Hop.FromPoint.GetLongitude) * PercentageTravelled
        Else
            Longitude = Hop.ToPoint.GetLongitude + (Hop.FromPoint.GetLongitude - Hop.ToPoint.GetLongitude) * (1 - PercentageTravelled)
        End If
    End Sub

    Function GetLatitude() As Double Implements RoutingPoint.GetLatitude
        Return Latitude
    End Function

    Function GetLongitude() As Double Implements RoutingPoint.GetLongitude
        Return Longitude
    End Function

    Public Overloads Function Equals(ByVal Other As HopPosition) As Boolean _
            Implements System.IEquatable(Of HopPosition).Equals
        Return PercentageTravelled = Other.PercentageTravelled AndAlso Hop.Equals(Other.Hop)
    End Function

End Class
