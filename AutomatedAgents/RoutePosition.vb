Public Class RoutePosition
    Public Route As Route
    Public PercentageTravelled As Double
    Public HopPosition As Integer

    Public Function GetOldNode() As Node
        Return Route.At(HopPosition).FromNode
    End Function

    Public Function GetNextNode() As Node
        Return Route.At(HopPosition).ToNode
    End Function

    Public Function GetCurrentWay() As Way
        Return Route.At(HopPosition).Way
    End Function

    Public Function GetEndNode() As Node
        Return Route.GetEndNode
    End Function

    Public Function GetCurrentWayPosition() As WayPosition
        Return New WayPosition(Route.At(HopPosition), PercentageTravelled)
    End Function

    Public Sub New(ByVal Route As Route)
        Me.Route = Route
        Me.PercentageTravelled = 0
        Me.HopPosition = 0
    End Sub

    'Note this will change JourneyPosition if required. BYREF!!!
    Public Sub GetNextPosition(ByVal DistanceIncrementMetres As Double)
        If DistanceIncrementMetres = 0 Then
            Exit Sub
        End If

        Dim PercentageTravelledTemp As Double = PercentageTravelled
        Dim DistanceLeftTemp As Double = DistanceIncrementMetres
        Do
            Dim FullHopDistance As Double = Route.At(HopPosition).GetCost
            Dim NextHopDistance As Double = FullHopDistance * (1 - PercentageTravelledTemp)
            If NextHopDistance > DistanceLeftTemp Then
                PercentageTravelled = PercentageTravelledTemp + DistanceLeftTemp / FullHopDistance
                Exit Do
            Else
                DistanceLeftTemp -= NextHopDistance
                PercentageTravelledTemp = 0
                HopPosition += 1
                If HopPosition >= Route.HopCount Then
                    PercentageTravelled = 1
                    HopPosition = Route.HopCount - 1
                    Exit Do
                End If
            End If
        Loop
    End Sub

    Function RouteCompleted() As Boolean
        Return PercentageTravelled = 1 And HopPosition = Route.HopCount - 1
    End Function

End Class
