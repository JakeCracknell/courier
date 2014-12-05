Public Class RoutePosition
    Public Route As List(Of Hop)
    Public PercentageTravelled As Double
    Public HopPosition As Integer

    Public Function GetOldNode() As Node
        Return Route(HopPosition).FromNode
    End Function

    Public Function GetNextNode() As Node
        Return Route(HopPosition).ToNode
    End Function

    Public Function GetCurrentWay() As Way
        Return Route(HopPosition).Way
    End Function

    Public Function GetEndNode() As Node
        Return Route(Route.Count - 1).ToNode
    End Function

    Public Sub New(ByVal Route As List(Of Hop))
        Me.Route = Route
        Me.PercentageTravelled = 0
        Me.HopPosition = 0
    End Sub

    'Public Sub New(ByVal Hop As Route, ByVal PercentageTravelled As Double)
    '    Me.Hop = Hop
    '    Me.PercentageTravelled = PercentageTravelled
    'End Sub

    'Public Sub New(ByVal NodePrevious As Node, ByVal NodeNext As Node, ByVal CurrentWay As Way)
    '    Hop = New Hop(NodePrevious, NodeNext, CurrentWay)
    '    Me.PercentageTravelled = 0
    'End Sub

    'Public Sub New(ByVal NodePrevious As Node, ByVal NodeNext As Node, ByVal CurrentWay As Way, ByVal PercentageTravelled As Double)
    '    Hop = New Hop(NodePrevious, NodeNext, CurrentWay)
    '    Me.PercentageTravelled = PercentageTravelled
    'End Sub

    'Note this will change JourneyPosition if required. BYREF!!!
    Public Sub GetNextPosition(ByVal DistanceIncrementMetres As Double)
        If DistanceIncrementMetres = 0 Then
            Exit Sub
        End If

        Dim PercentageTravelledTemp As Double = PercentageTravelled
        Dim DistanceLeftTemp As Double = DistanceIncrementMetres
        Do
            Dim FullHopDistance As Double = Route(HopPosition).GetCost
            Dim NextHopDistance As Double = FullHopDistance * (1 - PercentageTravelledTemp)
            If NextHopDistance > DistanceLeftTemp Then
                PercentageTravelled = PercentageTravelledTemp + DistanceLeftTemp / FullHopDistance
                Exit Do
            Else
                DistanceLeftTemp -= NextHopDistance
                PercentageTravelledTemp = 0
                HopPosition += 1
                If HopPosition >= Route.Count Then
                    PercentageTravelled = 1
                    HopPosition = Route.Count - 1
                    Exit Do
                End If
            End If
        Loop
    End Sub

    Function RouteCompleted() As Boolean
        Return PercentageTravelled = 1 And HopPosition = Route.Count - 1
    End Function

End Class
