Public Class RoutePosition
    Private Const EPSILON As Double = 0.000000001

    Public Route As Route
    Public PercentageTravelled As Double
    Public HopIndex As Integer
    Public RoadDelay As Boolean = False

    Public Function GetOldNode() As Node
        Return Route.At(HopIndex).FromPoint
    End Function

    Public Function GetNextNode() As Node
        Return Route.At(HopIndex).ToPoint
    End Function

    Public Function GetCurrentWay() As Way
        Return Route.At(HopIndex).Way
    End Function

    Public Function GetEndPoint() As IPoint
        Return Route.GetEndPoint
    End Function

    Public Function GetPoint() As HopPosition
        Return New HopPosition(Route.At(HopIndex), PercentageTravelled)
    End Function

    Public Sub New(ByVal Route As Route)
        Me.Route = Route
        Me.PercentageTravelled = 0
        Me.HopIndex = 0
    End Sub

    'Distance to travel based on speed of current way.
    Public Function Move(Optional ByVal SpeedCap As Double = SimulationParameters.MAX_POSSIBLE_SPEED_KMH) As Double
        Dim Way As Way = Route.At(HopIndex).Way
        If Way IsNot Nothing Then
            Dim DistanceToTravel As Double = Math.Min(Way.GetSpeedAtTime(NoticeBoard.Time), SpeedCap) / 3600
            GetNextPosition(DistanceToTravel)
            Return DistanceToTravel
        Else
            PercentageTravelled = 1
            Return 0
        End If
    End Function

    Public Sub GetNextPosition(ByVal DistanceIncrementMetres As Double)
        If DistanceIncrementMetres = 0 Then
            Exit Sub
        End If

        Dim PercentageTravelledTemp As Double = PercentageTravelled
        Dim DistanceLeftTemp As Double = DistanceIncrementMetres
        Do
            Dim FullHopDistance As Double = Math.Max(Route.At(HopIndex).GetDistance, EPSILON)
            Dim NextHopDistance As Double = FullHopDistance * (1 - PercentageTravelledTemp)
            If NextHopDistance > DistanceLeftTemp Then
                PercentageTravelled = PercentageTravelledTemp + DistanceLeftTemp / FullHopDistance
                Exit Do
            Else
                If TryIncrementHop() Then
                    DistanceLeftTemp -= NextHopDistance
                    PercentageTravelledTemp = 0
                Else
                    DistanceLeftTemp = 0
                    PercentageTravelledTemp = 1 - EPSILON
                End If
                If HopIndex >= Route.HopCount Then
                    PercentageTravelled = 1
                    HopIndex = Route.HopCount - 1
                    Exit Do
                End If
            End If
        Loop
    End Sub

    Private Function TryIncrementHop() As Boolean
        RoadDelay = IsDelayedAtTime(Route.At(HopIndex).ToPoint, Route.At(HopIndex).Way, NoticeBoard.Time)
        If Not RoadDelay Then
            HopIndex += 1
        End If
        Return Not RoadDelay
    End Function

    Function RouteCompleted() As Boolean
        Return Route Is Nothing OrElse (PercentageTravelled = 1 And HopIndex = Route.HopCount - 1)
    End Function

    Function GetCurrentSpeed(ByVal VehicleSize As Vehicles.Type) As Double
        Dim Way As Way = GetCurrentWay()
        If Way IsNot Nothing Then
            Return Way.GetSpeedAtTime(NoticeBoard.Time)
        Else
            Return 0
        End If
    End Function

    Function GetSubRoute() As Route
        Dim HopList As List(Of Hop) = Route.GetHopList()
        Dim SubRoute As New Route(HopList.GetRange(HopIndex, HopList.Count - HopIndex))
        Return SubRoute
    End Function

End Class
