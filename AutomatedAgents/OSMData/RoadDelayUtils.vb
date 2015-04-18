Public Module RoadDelayUtils
    'This enum is ordered by the severity of the potential driving delay.
    Public Enum RoadDelay
        NONE
        ZEBRA_CROSSING
        TRAFFIC_LIGHT_CROSSING
        TRAFFIC_LIGHTS
        LEVEL_CROSSING
    End Enum

    Private TrafficDistribution() As Integer = {24, 15, 10, 9, 10, 15, 24, 37, 55, 90, 129, 153, 158, 152, 148, 152, 156, 147, 129, 108, 84, 61, 41, 26, 16, 10, 8, 10, 20, 50, 114, 182, 185, 152, 147, 150, 150, 150, 154, 165, 189, 195, 147, 96, 67, 51, 37, 23, 15, 11, 10, 11, 19, 45, 112, 188, 193, 154, 139, 139, 140, 144, 152, 169, 199, 204, 157, 100, 68, 51, 39, 25, 16, 11, 10, 11, 19, 44, 110, 189, 195, 156, 141, 141, 143, 147, 156, 173, 203, 209, 163, 107, 72, 55, 41, 27, 16, 12, 10, 12, 19, 44, 109, 188, 195, 157, 145, 146, 149, 154, 163, 180, 208, 212, 169, 117, 81, 60, 45, 29, 18, 13, 11, 13, 19, 42, 99, 171, 177, 149, 153, 166, 176, 182, 191, 201, 208, 200, 165, 126, 90, 64, 47, 34, 23, 15, 12, 12, 15, 24, 41, 65, 96, 129, 158, 172, 168, 158, 147, 141, 140, 136, 116, 88, 63, 47, 40, 33}
    Private Const TrafficDistributionMax As Integer = 212

    Private Const TRAFFIC_LIGHT_FREQ As Integer = 60
    Private Const TRAFFIC_LIGHT_LENGTH As Integer = 30
    Private Const LEVEL_CROSSING_FREQ As Integer = 600
    Private Const LEVEL_CROSSING_LENGTH As Integer = 120
    Private Const ZEBRA_CROSSING_LENGTH As Integer = 15
    Private Const ZEBRA_CROSSING_PERIOD As Integer = 30


    Public Function GetAverageDelay(ByVal Node As Node, ByVal Way As Way) As Double
        'By default use the delay expected on a weekday at noon.
        Return GetAverageDelay(Node, Way, TimeSpan.FromHours(36))
    End Function
    Public Function GetAverageDelay(ByVal Node As Node, ByVal Way As Way, ByVal Time As TimeSpan) As Double
        Dim HourOfWeek As Double = TimeSpan.FromTicks(Time.Ticks Mod TimeSpan.FromDays(7).Ticks).TotalHours
        'TODO check this = integer!

        If Way.Type <> WayType.ROAD_MOTORWAY AndAlso Node.RoadDelay = RoadDelay.NONE Then
            Return 0
        End If

        Dim TrafficIntensity As Double = TrafficDistribution(HourOfWeek)
        Select Case Node.RoadDelay
            Case RoadDelay.NONE
                Return TrafficDistribution(HourOfWeek)

        End Select

    End Function

    Public Function IsDelayedAtTime(ByVal Node As Node, ByVal Way As Way, ByVal Time As TimeSpan) As Boolean
        Dim HourOfWeek As Integer = TimeSpan.FromTicks(Time.Ticks Mod TimeSpan.FromDays(7).Ticks).TotalHours

        If Way.Type <> WayType.ROAD_MOTORWAY AndAlso Node.RoadDelay = RoadDelay.NONE Then
            Return False
        End If


        Select Case Node.RoadDelay
            Case RoadDelay.TRAFFIC_LIGHTS
                Return (Time.TotalSeconds + Node.ID) Mod TRAFFIC_LIGHT_FREQ < TRAFFIC_LIGHT_LENGTH
            Case RoadDelay.LEVEL_CROSSING
                Return (Time.TotalSeconds + Node.ID) Mod LEVEL_CROSSING_FREQ < LEVEL_CROSSING_LENGTH


                'TODO: NEED TO DISCUSS HOW TO DO THIS properly?

                'First take a sample from Bounoulli(p) where p is proportional to TrafficDistribution(Now).
                '   This determines wheteher a delay will occur this PERIOD.
                '   This enforces only one delay per period.
                '   At peak time, this will equate to once every PERIOD seconds there will be a delay
                'Next spawn a random number uniformly to determine when the delay of set length L occurs within the period
                '   and return TRUE iff the current time is within L.
            Case RoadDelay.ZEBRA_CROSSING
                Dim Seed As Long = (Time.TotalSeconds \ ZEBRA_CROSSING_PERIOD) + Node.ID
                Dim RNG As New Random(Seed Mod Integer.MaxValue)

                'TrafficDistribution(HourOfWeek) * (1 / TrafficDistributionMax) = 1, at peak time. Means there will definitely be a delay sometime this period.
                Dim OccursThisPeriod As Boolean = RNG.NextDouble < TrafficDistribution(HourOfWeek) * (1 / TrafficDistributionMax)
                If OccursThisPeriod Then
                    Dim WhenDelayStartsThisPeriod As Integer = RNG.Next(0, ZEBRA_CROSSING_PERIOD - ZEBRA_CROSSING_LENGTH)
                    Dim CurrentPositionInPeriod As Integer = Time.TotalSeconds Mod ZEBRA_CROSSING_PERIOD
                    Return CurrentPositionInPeriod >= WhenDelayStartsThisPeriod AndAlso _
                        CurrentPositionInPeriod <= WhenDelayStartsThisPeriod + ZEBRA_CROSSING_LENGTH
                End If
                Return False
            Case RoadDelay.TRAFFIC_LIGHT_CROSSING
                Throw New NotImplementedException 'do same way, new consts
            Case RoadDelay.NONE
                'Account for short 2 second delays, plus rare 30 second delays
                Throw New NotImplementedException
        End Select
    End Function
End Module
