Public Class HopPosition
    Public Hop As Hop
    Public PercentageTravelled As Double

    Public Longitude, Latitude As Double

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

End Class
