Public Class HopPosition
    Implements IEquatable(Of HopPosition)
    Implements IPoint

    Private Const APPROXIMATElY_EQUALS_EPSILON As Double = 0.001
    Public Hop As Hop
    Public PercentageTravelled As Double

    Private Longitude, Latitude As Double

    Sub New(ByVal _Hop As Hop, ByVal _PercentageTravelled As Double)
        Me.Hop = _Hop

        'Coalesce HopPositions.
        Dim IsHopAPairOfHopPositions As Integer = If(TypeOf _Hop.FromPoint Is HopPosition, 1, 0) + If(TypeOf _Hop.ToPoint Is HopPosition, 10, 0)
        Select Case IsHopAPairOfHopPositions
            Case 0 'A<---------------->B
                Me.Hop = _Hop
            Case 1 'A----------X<------>B
                Dim InnerHopPosition As HopPosition = CType(_Hop.FromPoint, HopPosition)
                Dim InnerHop As Hop = InnerHopPosition.Hop
                Me.Hop = New Hop(InnerHop.FromPoint, InnerHop.ToPoint, InnerHop.Way)
                Me.PercentageTravelled = _PercentageTravelled + _
                    (1 - _PercentageTravelled) * InnerHopPosition.PercentageTravelled
            Case 10 'A<---------->X------B
                Dim InnerHopPosition As HopPosition = CType(_Hop.ToPoint, HopPosition)
                Dim InnerHop As Hop = CType(_Hop.ToPoint, HopPosition).Hop
                Me.Hop = New Hop(InnerHop.FromPoint, InnerHop.ToPoint, InnerHop.Way)
                Me.PercentageTravelled = _PercentageTravelled * InnerHopPosition.PercentageTravelled
            Case 11 'A----X<------>X------B
                'Note this was never called in testing, probably because of the coalescing above.
                'Assume not like this 'A----X<---B---->X----C and IHP1.% < ICP2.%
                Dim IHPLeft As HopPosition = CType(_Hop.FromPoint, HopPosition)
                Dim IHPRight As HopPosition = CType(_Hop.ToPoint, HopPosition)
                Dim InnerHop As Hop = IHPLeft.Hop
                Me.Hop = New Hop(InnerHop.FromPoint, InnerHop.ToPoint, InnerHop.Way)
                Me.PercentageTravelled = IHPLeft.PercentageTravelled + _
                    (IHPRight.PercentageTravelled - IHPLeft.PercentageTravelled) * _PercentageTravelled
        End Select

        If TypeOf _Hop.FromPoint Is HopPosition Then
            Dim InnerFrom As HopPosition = _Hop.FromPoint
            Me.Hop = New Hop(InnerFrom.Hop.FromPoint, InnerFrom.Hop.ToPoint, InnerFrom.Hop.Way)
        ElseIf TypeOf _Hop.ToPoint Is HopPosition Then
            Dim InnerFrom As HopPosition = _Hop.ToPoint
            Me.Hop = New Hop(InnerFrom.Hop.FromPoint, InnerFrom.Hop.ToPoint, InnerFrom.Hop.Way)
        End If

        Me.PercentageTravelled = _PercentageTravelled

        If _Hop.FromPoint.GetLatitude < _Hop.ToPoint.GetLatitude Then
            Latitude = _Hop.FromPoint.GetLatitude + (_Hop.ToPoint.GetLatitude - _Hop.FromPoint.GetLatitude) * _PercentageTravelled
        Else
            Latitude = _Hop.ToPoint.GetLatitude + (_Hop.FromPoint.GetLatitude - _Hop.ToPoint.GetLatitude) * (1 - _PercentageTravelled)
        End If

        If _Hop.FromPoint.GetLongitude < _Hop.ToPoint.GetLongitude Then
            Longitude = _Hop.FromPoint.GetLongitude + (_Hop.ToPoint.GetLongitude - _Hop.FromPoint.GetLongitude) * _PercentageTravelled
        Else
            Longitude = _Hop.ToPoint.GetLongitude + (_Hop.FromPoint.GetLongitude - _Hop.ToPoint.GetLongitude) * (1 - _PercentageTravelled)
        End If
    End Sub

    Function GetLatitude() As Double Implements IPoint.GetLatitude
        Return Latitude
    End Function

    Function GetLongitude() As Double Implements IPoint.GetLongitude
        Return Longitude
    End Function

    Public Overloads Function Equals(ByVal Other As HopPosition) As Boolean _
            Implements System.IEquatable(Of HopPosition).Equals
        Debug.WriteLine(PercentageTravelled & " = " & Other.PercentageTravelled)
        Return PercentageTravelled = Other.PercentageTravelled AndAlso Hop.Equals(Other.Hop)
    End Function

    Public Function IsOnSameHop(ByVal Other As HopPosition) As Boolean
        Return (Hop.FromPoint.Equals(Other.Hop.FromPoint) AndAlso Hop.ToPoint.Equals(Other.Hop.ToPoint)) OrElse _
                   (Hop.FromPoint.Equals(Other.Hop.ToPoint) AndAlso Hop.ToPoint.Equals(Other.Hop.FromPoint))
    End Function

    Public Function ApproximatelyEquals(ByVal Other As HopPosition) As Boolean
        Return IsOnSameHop(Other) And GetDistance(Me, Other) < APPROXIMATElY_EQUALS_EPSILON
    End Function
End Class
