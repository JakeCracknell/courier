Public Module RoadDelayUtils
    'This enum is ordered by the severity of the potential driving delay.
    Public Enum RoadDelay
        NONE
        UNEXPECTED
        ZEBRA_CROSSING
        TRAFFIC_LIGHT_CROSSING
        TRAFFIC_LIGHTS
        LEVEL_CROSSING
    End Enum

    Private TrafficDistribution() As Integer = {16, 10, 8, 10, 20, 50, 114, 182, 185, 152, 147, 150, 150, 150, 154, 165, 189, 195, 147, 96, 67, 51, 37, 23, 15, 11, 10, 11, 19, 45, 112, 188, 193, 154, 139, 139, 140, 144, 152, 169, 199, 204, 157, 100, 68, 51, 39, 25, 16, 11, 10, 11, 19, 44, 110, 189, 195, 156, 141, 141, 143, 147, 156, 173, 203, 209, 163, 107, 72, 55, 41, 27, 16, 12, 10, 12, 19, 44, 109, 188, 195, 157, 145, 146, 149, 154, 163, 180, 208, 212, 169, 117, 81, 60, 45, 29, 18, 13, 11, 13, 19, 42, 99, 171, 177, 149, 153, 166, 176, 182, 191, 201, 208, 200, 165, 126, 90, 64, 47, 34, 23, 15, 12, 12, 15, 24, 41, 65, 96, 129, 158, 172, 168, 158, 147, 141, 140, 136, 116, 88, 63, 47, 40, 33, 24, 15, 10, 9, 10, 15, 24, 37, 55, 90, 129, 153, 158, 152, 148, 152, 156, 147, 129, 108, 84, 61, 41, 26}
    Private Const TrafficDistributionMax As Integer = 212

    Private Const TRAFFIC_LIGHT_FREQ As Integer = 60
    Private Const TRAFFIC_LIGHT_LENGTH As Integer = 30
    Private Const LEVEL_CROSSING_FREQ As Integer = 600
    Private Const LEVEL_CROSSING_LENGTH As Integer = 120
    Private Const ZEBRA_CROSSING_LENGTH As Integer = 10
    Private Const ZEBRA_CROSSING_PERIOD As Integer = 30
    Private Const ZEBRA_CROSSING_COEFFICIENT As Double = 1.0
    Private Const TRAFFICLIGHT_CROSSING_LENGTH As Integer = 15
    Private Const TRAFFICLIGHT_CROSSING_PERIOD As Integer = 45
    Private Const TRAFFICLIGHT_CROSSING_COEFFICIENT As Double = 1.0
    Private Const MINOR_UNEXPECTED_DELAY_LENGTH As Integer = 2
    Private Const MINOR_UNEXPECTED_DELAY_PERIOD As Integer = 2
    Private Const MINOR_UNEXPECTED_DELAY_COEFFICIENT As Double = 4 / 3600 '4 times an hour at peak time
    Private Const MAJOR_UNEXPECTED_DELAY_LENGTH As Integer = 30
    Private Const MAJOR_UNEXPECTED_DELAY_PERIOD As Integer = 30
    Private Const MAJOR_UNEXPECTED_DELAY_COEFFICIENT As Double = 0.5 / 3600 '0.5 times an hour at peak time

    Public Function IsDelayedAtTime(ByVal NodeOrHopPosition As IPoint, ByVal Way As Way, ByVal Time As TimeSpan) As Boolean
        Dim Node As Node = TryCast(NodeOrHopPosition, Node)
        If Node IsNot Nothing Then
            Return IsDelayedAtTime(Node, Way, Time)
        Else
            Return False
        End If
    End Function
    Public Function IsDelayedAtTime(ByVal Node As Node, ByVal Way As Way, ByVal Time As TimeSpan) As Boolean
        If (Way Is Nothing OrElse Way.Type = WayType.MOTORWAY) AndAlso Node.RoadDelay = RoadDelay.UNEXPECTED Then
            Return False
        End If

        Select Case Node.RoadDelay
            'Traffic lights and level crossing assumed to meet a fixed, regular schedule
            Case RoadDelay.TRAFFIC_LIGHTS
                Return (Time.TotalSeconds + Node.ID) Mod TRAFFIC_LIGHT_FREQ < TRAFFIC_LIGHT_LENGTH
            Case RoadDelay.LEVEL_CROSSING
                If Not (Time.Hours >= 1 And Time.Hours <= 4) Then
                    Return (Time.TotalSeconds + Node.ID) Mod LEVEL_CROSSING_FREQ < LEVEL_CROSSING_LENGTH
                End If


                'First take a sample from Bounoulli(p) where p is proportional to TrafficDistribution(Now).
                '   This determines wheteher a delay will occur this PERIOD.
                '   This enforces only one delay per period.
                '   At peak time, this will equate to once every PERIOD seconds there will be a delay
                'Next spawn a random number uniformly to determine when the delay of set length L occurs within the period
                '   and return TRUE iff the current time is within L.

                'TrafficDistribution(HourOfWeek) * (1 / TrafficDistributionMax) = 1, at peak time. Means there will definitely be a delay sometime this period.
            Case RoadDelay.ZEBRA_CROSSING
                Return PeriodicDelayHelper(Node.ID, Time, ZEBRA_CROSSING_PERIOD, ZEBRA_CROSSING_LENGTH, ZEBRA_CROSSING_COEFFICIENT)
            Case RoadDelay.TRAFFIC_LIGHT_CROSSING
                Return PeriodicDelayHelper(Node.ID, Time, TRAFFICLIGHT_CROSSING_PERIOD, TRAFFICLIGHT_CROSSING_LENGTH, TRAFFICLIGHT_CROSSING_COEFFICIENT)
            Case RoadDelay.UNEXPECTED
                Dim MinorDelay As Boolean = PeriodicDelayHelper(Node.ID, Time, MINOR_UNEXPECTED_DELAY_PERIOD, MINOR_UNEXPECTED_DELAY_LENGTH, MINOR_UNEXPECTED_DELAY_COEFFICIENT)
                Dim MajorDelay As Boolean = PeriodicDelayHelper(Node.ID, Time, MAJOR_UNEXPECTED_DELAY_PERIOD, MAJOR_UNEXPECTED_DELAY_LENGTH, MAJOR_UNEXPECTED_DELAY_COEFFICIENT)
                Return MinorDelay Or MajorDelay
        End Select
        Return False
    End Function

    Function PeriodicDelayHelper(ByVal IDSeed As Long, ByVal Time As TimeSpan, ByVal Period As Integer, ByVal Length As Integer, ByVal Coefficient As Double) As Boolean
        Dim HourOfWeek As Integer = Int(TimeSpan.FromTicks(Time.Ticks Mod TimeSpan.FromDays(7).Ticks).TotalHours)
        Dim Seed As Long = (Time.TotalSeconds \ Period) + IDSeed
        Dim RNG As New Random(Seed Mod Integer.MaxValue)

        Dim OccursThisPeriod As Boolean = RNG.NextDouble < TrafficDistribution(HourOfWeek) * (Coefficient / TrafficDistributionMax)
        If OccursThisPeriod Then
            Dim WhenDelayStartsThisPeriod As Integer = RNG.Next(0, Period - Length)
            Dim CurrentPositionInPeriod As Integer = Time.TotalSeconds Mod Period
            Return CurrentPositionInPeriod >= WhenDelayStartsThisPeriod AndAlso _
                CurrentPositionInPeriod <= WhenDelayStartsThisPeriod + Length
        End If
        Return False
    End Function

    Public Function GetAverageDelayLength(ByVal Hop As Hop, ByVal Time As TimeSpan) As Double
        Dim Node As Node = TryCast(Hop.FromPoint, Node)
        If Node IsNot Nothing Then
            Return GetAverageDelayLength(Node, Hop.Way, Time)
        Else
            Return 0
        End If
    End Function
    Public Function GetAverageDelayLength(ByVal Node As Node, ByVal Way As Way) As Double
        'By default use the delay expected on a weekday at noon.
        Return GetAverageDelayLength(Node, Way, TimeSpan.FromHours(12))
    End Function
    Public Function GetAverageDelayLength(ByVal Node As Node, ByVal Way As Way, ByVal Time As TimeSpan) As Double
        Dim HourOfWeek As Integer = Int(TimeSpan.FromTicks(Time.Ticks Mod TimeSpan.FromDays(7).Ticks).TotalHours)

        If Way.Type = WayType.MOTORWAY AndAlso Node.RoadDelay = RoadDelay.UNEXPECTED Then
            Return 0
        End If

        Dim TrafficIntensity As Double = TrafficDistribution(HourOfWeek)
        Select Case Node.RoadDelay
            'If level crossing will make the vehicle stop for 120s, every 600s:
            '   The chance of being delayed is 120/600=0.2
            '   The average length of the delay will be 120/2=60
            '   So the average delay is 60*0.2=30s
            Case RoadDelay.LEVEL_CROSSING
                Return LEVEL_CROSSING_LENGTH ^ 2 / LEVEL_CROSSING_FREQ * 2
            Case RoadDelay.TRAFFIC_LIGHTS
                If Not (Time.Hours >= 1 And Time.Hours <= 4) Then
                    Return TRAFFIC_LIGHT_LENGTH ^ 2 / TRAFFIC_LIGHT_FREQ * 2
                End If


                'Same as above but multiplied by the chance that the delay event will happen in this period.
            Case RoadDelay.ZEBRA_CROSSING
                Dim ChanceThisPeriod As Double = TrafficDistribution(HourOfWeek) * (ZEBRA_CROSSING_COEFFICIENT / TrafficDistributionMax)
                Dim AverageLengthOfDelayInsidePeriod As Double = ZEBRA_CROSSING_LENGTH ^ 2 / ZEBRA_CROSSING_PERIOD * 2
                Return ChanceThisPeriod * AverageLengthOfDelayInsidePeriod
            Case RoadDelay.TRAFFIC_LIGHT_CROSSING
                Dim ChanceThisPeriod As Double = TrafficDistribution(HourOfWeek) * (TRAFFICLIGHT_CROSSING_COEFFICIENT / TrafficDistributionMax)
                Dim AverageLengthOfDelayInsidePeriod As Double = TRAFFICLIGHT_CROSSING_LENGTH ^ 2 / TRAFFICLIGHT_CROSSING_PERIOD * 2
                Return ChanceThisPeriod * AverageLengthOfDelayInsidePeriod
            Case RoadDelay.UNEXPECTED
                Dim MinorChanceThisPeriod As Double = TrafficDistribution(HourOfWeek) * (MINOR_UNEXPECTED_DELAY_COEFFICIENT / TrafficDistributionMax)
                Dim MinorAverageLengthOfDelayInsidePeriod As Double = MINOR_UNEXPECTED_DELAY_LENGTH ^ 2 / MINOR_UNEXPECTED_DELAY_PERIOD * 2
                Dim MinorChance As Double = MinorChanceThisPeriod * MinorAverageLengthOfDelayInsidePeriod
                Dim MajorChanceThisPeriod As Double = TrafficDistribution(HourOfWeek) * (MAJOR_UNEXPECTED_DELAY_COEFFICIENT / TrafficDistributionMax)
                Dim MajorAverageLengthOfDelayInsidePeriod As Double = MAJOR_UNEXPECTED_DELAY_LENGTH ^ 2 / MAJOR_UNEXPECTED_DELAY_PERIOD * 2
                Dim MajorChance As Double = MajorChanceThisPeriod * MajorAverageLengthOfDelayInsidePeriod
                Return MinorChance + MajorChance - (MinorChance * MajorChance)
        End Select
        Return 0
    End Function

End Module
