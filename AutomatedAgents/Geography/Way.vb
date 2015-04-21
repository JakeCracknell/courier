Public Class Way
    Implements IEquatable(Of Way)
    Public ID As Long
    Public Nodes As Node()
    Public Type As WayType
    Public Name As String
    Private OSMSpeedLimit As Double = Double.MinValue
    Private MaximumSpeedWithTraffic As Double = Double.MinValue
    Private SpeedAtTime As Double()
    Public OneWay As Boolean

    Public Sub New(ByVal ID As Integer, ByVal Nodes As Node(), ByVal Type As WayType, ByVal Name As String)
        Me.ID = ID
        Me.Nodes = Nodes
        Me.Type = Type
        Me.Name = Name
    End Sub

    Public Shared Operator =(ByVal Way1 As Way, ByVal Way2 As Way) As Boolean
        Return Way1.Equals(Way2)
    End Operator

    Public Shared Operator <>(ByVal Way1 As Way, ByVal Way2 As Way) As Boolean
        Return Not Way1.Equals(Way2)
    End Operator

    Public Overloads Function Equals(ByVal Way1 As Way) As Boolean _
        Implements System.IEquatable(Of Way).Equals
        Return Me.ID = Way1.ID
    End Function

    Public Sub SetOneWay(ByVal Value As String)
        Select Case Value
            Case "yes", "true", "1"
                'yes, Most common tag
                OneWay = True
            Case "no", "false", "0"
                'Superfluous tag, sometimes used to confirm a bidirectional street, against mapping errors.
                OneWay = False
            Case "-1", "reverse"
                'One way in opposite direction of node list
                Nodes = Nodes.Reverse().ToArray
                OneWay = True
            Case "reversible", ""
                'Very tricky case where highway direction can change throughout the day.
                'Very rare, so will assume to be one-way
                OneWay = True
            Case Else
                Debug.WriteLine("I cannot categorise this oneway tag: " & Value)
                OneWay = False
        End Select
    End Sub

    Sub SetSpeedLimit(ByVal MaxSpeedOverrideKMH As Double)
        Me.OSMSpeedLimit = MaxSpeedOverrideKMH
    End Sub

    Function GetSpeedLimit() As Double
        If OSMSpeedLimit > 0 Then
            Return OSMSpeedLimit
        End If

        'UK speeds available from https://www.gov.uk/speed-limits
        'However will use estimates as simulation designed for built-up areas.
        Select Case Type
            Case WayType.MOTORWAY
                Return 112
            Case WayType.TRUNK_ROAD
                Return 80
            Case WayType.PRIMARY_ROAD
                Return 64
            Case WayType.RESIDENTIAL_ROAD
                Return 32
            Case WayType.SERVICE_ROAD
                Return 8
            Case Else
                Return 48
        End Select
    End Function

    Public Function GetSpeedAtTime(ByVal Time As TimeSpan) As Double
        If SpeedAtTime IsNot Nothing Then
            Dim TimeIndex As Integer = GetTimeIndex(Time)
            Return Math.Min(SpeedAtTime(TimeIndex), GetSpeedLimit())
        Else
            Return GetSpeedLimit()
        End If
    End Function

    'Expected format is 34.56,36.89,32.67, ... 39.74
    Sub ParseTrafficTrace(ByVal WayTrafficLine As String)
        ReDim SpeedAtTime(2016)
        If WayTrafficLine IsNot Nothing Then
            Dim SplitTrace() As String = WayTrafficLine.Split(",")
            If SplitTrace.Length >= 2016 Then
                For i = 0 To 2015
                    SpeedAtTime(i) = Math.Max(Math.Min(Double.Parse(SplitTrace(i)), GetSpeedLimit), SimulationParameters.MIN_POSSIBLE_SPEED_KMH)
                    MaximumSpeedWithTraffic = Math.Max(MaximumSpeedWithTraffic, SpeedAtTime(i))
                Next
            End If
        End If
    End Sub

    Public Function GetSpeedDifferenceAtTime(ByVal Time As TimeSpan) As Double
        Return MaximumSpeedWithTraffic - GetSpeedAtTime(Time)
    End Function

    Public Function HasRealTimeTraffic() As Boolean
        Return SpeedAtTime IsNot Nothing
    End Function

    Public Overrides Function ToString() As String
        Return If(Name <> "", Name, Type.ToString("G"))
    End Function

End Class
