Public Class HopPosition
    Implements IEquatable(Of HopPosition)
    Implements RoutingPoint
    Public Hop As Hop
    Public PercentageTravelled As Double

    Private Longitude, Latitude As Double

    Sub New(ByVal Hop As Hop, ByVal PercentageTravelled As Double)
        Me.Hop = Hop

        'Coalesce HopPositions.
        Dim IsHopAPairOfHopPositions As Integer = If(TypeOf Hop.FromPoint Is HopPosition, 1, 0) + If(TypeOf Hop.ToPoint Is HopPosition, 10, 0)
        Select Case IsHopAPairOfHopPositions
            Case 0 'A<---------------->B
                Me.Hop = Hop
            Case 1 'A----------X<------>B
                Dim InnerHopPosition As HopPosition = CType(Hop.FromPoint, HopPosition)
                Dim InnerHop As Hop = InnerHopPosition.Hop
                Me.Hop = New Hop(InnerHop.FromPoint, InnerHop.ToPoint, InnerHop.Way)
                PercentageTravelled = PercentageTravelled + _
                    (1 - PercentageTravelled) * InnerHopPosition.PercentageTravelled
            Case 10 'A<---------->X------B
                Dim InnerHopPosition As HopPosition = CType(Hop.ToPoint, HopPosition)
                Dim InnerHop As Hop = CType(Hop.ToPoint, HopPosition).Hop
                Me.Hop = New Hop(InnerHop.FromPoint, InnerHop.ToPoint, InnerHop.Way)
                PercentageTravelled = PercentageTravelled * InnerHopPosition.PercentageTravelled
            Case 11 'A----X<------>X------B
                'Assume not like this 'A----X<---B---->X----C and IHP1.% < ICP2.%
                Dim IHPLeft As HopPosition = CType(Hop.FromPoint, HopPosition)
                Dim IHPRight As HopPosition = CType(Hop.ToPoint, HopPosition)
                Dim InnerHop As Hop = IHPLeft.Hop
                Me.Hop = New Hop(InnerHop.FromPoint, InnerHop.ToPoint, InnerHop.Way)
                PercentageTravelled = IHPLeft.PercentageTravelled + _
                    (IHPRight.PercentageTravelled - IHPLeft.PercentageTravelled) * PercentageTravelled
        End Select

        If TypeOf Hop.FromPoint Is HopPosition Then
            Dim InnerFrom As HopPosition = Hop.FromPoint
            Me.Hop = New Hop(InnerFrom.Hop.FromPoint, InnerFrom.Hop.ToPoint, InnerFrom.Hop.Way)
        ElseIf TypeOf Hop.ToPoint Is HopPosition Then
            Dim InnerFrom As HopPosition = Hop.ToPoint
            Me.Hop = New Hop(InnerFrom.Hop.FromPoint, InnerFrom.Hop.ToPoint, InnerFrom.Hop.Way)
        End If

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
