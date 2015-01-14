Public Class RoutePosition
    Public Route As Route
    Public PercentageTravelled As Double
    Public HopIndex As Integer

    Public Function GetOldNode() As Node
        Return Route.At(HopIndex).FromPoint
    End Function

    Public Function GetNextNode() As Node
        Return Route.At(HopIndex).ToPoint
    End Function

    Public Function GetCurrentWay() As Way
        Return Route.At(HopIndex).Way
    End Function

    Public Function GetEndPoint() As RoutingPoint
        Return Route.GetEndPoint
    End Function

    Public Function GetRoutingPoint() As HopPosition
        Return New HopPosition(Route.At(HopIndex), PercentageTravelled)
    End Function

    Public Sub New(ByVal Route As Route)
        Me.Route = Route
        Me.PercentageTravelled = 0
        Me.HopIndex = 0
    End Sub

    'Distance to travel based on speed of current way. Close enough for now, as 1 second is not long.
    Public Function Move(ByVal Vehicle As VehicleSize) As Double
        Dim DistanceToTravel As Double = Route.At(HopIndex).Way.GetMaxSpeedKMH(Vehicle) / 3600
        GetNextPosition(DistanceToTravel)
        Return DistanceToTravel
    End Function

    Public Sub GetNextPosition(ByVal DistanceIncrementMetres As Double)
        If DistanceIncrementMetres = 0 Then
            Exit Sub
        End If

        Dim PercentageTravelledTemp As Double = PercentageTravelled
        Dim DistanceLeftTemp As Double = DistanceIncrementMetres
        Do
            Dim FullHopDistance As Double = Route.At(HopIndex).GetCost
            Dim NextHopDistance As Double = FullHopDistance * (1 - PercentageTravelledTemp)
            If NextHopDistance > DistanceLeftTemp Then
                PercentageTravelled = PercentageTravelledTemp + DistanceLeftTemp / FullHopDistance
                Exit Do
            Else
                DistanceLeftTemp -= NextHopDistance
                PercentageTravelledTemp = 0
                HopIndex += 1
                If HopIndex >= Route.HopCount Then
                    PercentageTravelled = 1
                    HopIndex = Route.HopCount - 1
                    Exit Do
                End If
            End If
        Loop
    End Sub

    Function RouteCompleted() As Boolean
        Return Route Is Nothing OrElse (PercentageTravelled = 1 And HopIndex = Route.HopCount - 1)
    End Function

End Class
