Public Class Way
    Implements IEquatable(Of Way)
    Public ID As Long
    Public Nodes As Node()
    Public Type As WayType
    Public Name As String
    Private MaxSpeedOverrideKMH As Double = Double.MinValue

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

    Sub SetSpeed(ByVal MaxSpeedOverrideKMH As Double)
        Me.MaxSpeedOverrideKMH = MaxSpeedOverrideKMH
    End Sub

    Function GetMaxSpeedKMH(ByVal Vehicle As Vehicles.Type)
        If MaxSpeedOverrideKMH > 0 Then
            Return MaxSpeedOverrideKMH
        End If

        'UK speeds available from https://www.gov.uk/speed-limits
        'However will use estimates as simulation designed for built-up areas.
        Select Case Type
            Case WayType.ROAD_MOTORWAY
                Return 112
            Case WayType.ROAD_TRUNK
                Return 80
            Case WayType.ROAD_PRIMARY
                Return 64
            Case WayType.ROAD_RESIDENTIAL
                Return 32
            Case WayType.ROAD_SERVICE
                Return 8
            Case Else
                Return 48
        End Select


    End Function

    Public Overrides Function ToString() As String
        Return If(Name <> "", Name, Type.ToString("G"))
    End Function
End Class
