Imports System.Xml
Imports System.Text.RegularExpressions

Module WayParser
    Public Function ParseWay(ByVal xItem As XmlElement, ByVal NodeHashMap As Dictionary(Of Long, Node)) As Way
        Dim xWayID As Integer = xItem.GetAttribute("id")
        Dim WayType As WayType = WayType.UNSPECIFIED
        Dim WayName As String = ""
        Dim MaxSpeedOverrideKMH As Integer = -1
        Dim OneWay As String = ""
        Dim AccessAllowed As Boolean = True

        Dim xTags As XmlNodeList = xItem.GetElementsByTagName("tag")
        For Each xTag As XmlElement In xTags
            Dim AttributeName As String = xTag.GetAttribute("k")
            If AttributeName = "highway" Then
                WayType = DecodeHighWayType(xTag.GetAttribute("v"))
            End If
            If AttributeName = "name" Then
                WayName = xTag.GetAttribute("v")
            End If
            If AttributeName = "oneway" Then
                OneWay = xTag.GetAttribute("v")
            End If
            If AttributeName = "access" Then
                AccessAllowed = AccessAllowed And DecodeHighWayAccessLevel(xTag.GetAttribute("v"))
            End If
            If AttributeName = "maxspeed" Then
                MaxSpeedOverrideKMH = DecodeHighwayMaxSpeed(xTag.GetAttribute("v"))
            End If
        Next

        'Do not add non-highways or inaccessible roads to way list.
        If WayType <> WayType.UNSPECIFIED And AccessAllowed Then
            Dim Nds As New List(Of Node)
            Dim xNds As XmlNodeList = xItem.GetElementsByTagName("nd")
            For Each xNd As XmlElement In xNds
                Dim NodeRef As Long = xNd.GetAttribute("ref")
                Dim Node As Node = Nothing
                NodeHashMap.TryGetValue(NodeRef, Node)
                If Node IsNot Nothing Then
                    Nds.Add(Node)
                End If
            Next

            Dim Way As New Way(xWayID, Nds.ToArray, WayType, WayName)

            If OneWay <> "" Then
                Way.SetOneWay(OneWay)
            End If

            If MaxSpeedOverrideKMH <> -1 Then
                Way.SetSpeedLimit(MaxSpeedOverrideKMH)
            End If

            Return Way
        End If
        Return Nothing
    End Function

    Private Function DecodeHighWayType(ByVal str As String) As WayType
        Select Case str
            Case "service"
                Return WayType.ROAD_SERVICE
            Case "unclassified"
                Return WayType.ROAD_UNCLASSIFIED 'e.g. vine road
            Case "residential"
                Return WayType.ROAD_RESIDENTIAL
            Case "tertiary", "tertiary_link"
                Return WayType.ROAD_TERTIARY
            Case "secondary", "secondary_link"
                Return WayType.ROAD_SECONDARY
            Case "primary", "primary_link"
                Return WayType.ROAD_PRIMARY
            Case "trunk", "trunk_link"
                Return WayType.ROAD_TRUNK
            Case "motorway", "motorway_link"
                Return WayType.ROAD_MOTORWAY
            Case Else
                Return WayType.UNSPECIFIED
        End Select
    End Function

    Private Function DecodeHighWayAccessLevel(ByVal str As String) As Boolean
        Select Case str
            Case "yes", "delivery", "public", "unknown"
                Return True
            Case "no", "private", "permissive", "destination", "agricultural", "customers", "designated", "psv", "forestry", "military"
                Return False
            Case Else
                'If in doubt, return false
                Return False
        End Select


    End Function

    Private Function DecodeHighwayMaxSpeed(ByVal str As String) As Double
        If IsNumeric(str) Then
            Return CDec(str)
        End If

        If str.Contains("mph") Then
            Return CDec(Regex.Match(str, "\d+").Value * Vehicles.MILE_LENGTH_IN_KM)
        End If

        Debug.WriteLine("Could not parse maxspeed: " & str)
        'Otherwise unparsable - go by the WayType
        Return -1
    End Function

End Module
