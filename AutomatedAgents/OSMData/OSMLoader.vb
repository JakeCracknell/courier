Imports System.Xml
Imports System.IO
Imports System.Text.RegularExpressions

Public Class OSMLoader
    Private OSMFilePath As String
    Private AAFilePath As String
    Private TrafficDirectory As String
    Public Sub New(ByVal FilePath As String)
        Me.OSMFilePath = FilePath
        Me.AAFilePath = FilePath.Replace(".osm", ".aa")
        Me.TrafficDirectory = FilePath.Replace(".osm", "")
    End Sub

    Function CreateMap() As StreetMap
        Dim Bounds As Bounds
        Dim Map As StreetMap
        Dim NodeHashMap As New Dictionary(Of Long, Node)
        Dim UnconfirmedFuelPoints As New List(Of Node)
        Dim Nodes As New List(Of Node)

        Using OSMFile = File.Open(OSMFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = XmlReader.Create(OSMFile)

                reader.MoveToContent()
                reader.ReadStartElement("osm")

                Do Until reader.Name = "bounds"
                    reader.Skip()
                Loop
                Bounds = New Bounds(reader.GetAttribute("minlat"), _
                                    reader.GetAttribute("minlon"), _
                                    reader.GetAttribute("maxlat"), _
                                    reader.GetAttribute("maxlon"))
                Map = New StreetMap(IO.Path.GetFileNameWithoutExtension(OSMFilePath), Bounds)

                Do Until reader.Name = "node"
                    reader.Skip()
                Loop

                Do Until reader.Name = "way"
                    Dim Node As New Node(Long.Parse(reader.GetAttribute("id")), Double.Parse(reader.GetAttribute("lat")), Double.Parse(reader.GetAttribute("lon")))
                    reader.ReadStartElement()
                    reader.Skip()

                    Dim NodeName As String = Nothing
                    Dim IsBusiness As Boolean = False
                    While reader.Name = "tag"
                        Dim AttributeName As String = reader.GetAttribute("k")
                        Select Case AttributeName
                            'Businesses:
                            Case "shop", "office"
                                IsBusiness = True
                                NodeName = If(NodeName, reader.GetAttribute("v").Replace("_", " ") & " " & AttributeName)
                            Case "amenity"
                                Select Case reader.GetAttribute("v")
                                    Case "fuel"
                                        UnconfirmedFuelPoints.Add(Node)
                                    Case "school", "restaurant", "bank", "fast_food", "cafe", "kindergarten", "pharmacy", "hospital", "pub", "bar", "fire_station", "police", "townhall"
                                        IsBusiness = True
                                        NodeName = If(NodeName, reader.GetAttribute("v").Replace("_", " "))
                                End Select
                            Case "craft"
                                IsBusiness = True
                                NodeName = If(NodeName, reader.GetAttribute("v").Replace("_", " "))
                            Case "name"
                                NodeName = reader.GetAttribute("v")


                                'Road delays. Enum is ordered by severity, so pick tag with highest severity
                            Case "railway"
                                If reader.GetAttribute("v") = "level_crossing" Then
                                    Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.LEVEL_CROSSING)
                                End If
                            Case "crossing"
                                Select Case reader.GetAttribute("v")
                                    Case "traffic_signals"
                                        Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.TRAFFIC_LIGHT_CROSSING)
                                    Case "uncontrolled", "zebra"
                                        Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.ZEBRA_CROSSING)
                                End Select
                            Case "highway"
                                Select Case reader.GetAttribute("v")
                                    Case "traffic_signals"
                                        Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.TRAFFIC_LIGHTS)
                                    Case "crossing"
                                        Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.ZEBRA_CROSSING)
                                End Select
                            Case "crossing_ref"
                                Select Case reader.GetAttribute("v")
                                    Case "zebra"
                                        Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.ZEBRA_CROSSING)
                                    Case "pelican", "toucan", "pegasus", "puffin"
                                        Node.RoadDelay = Math.Max(Node.RoadDelay, RoadDelay.TRAFFIC_LIGHT_CROSSING)
                                End Select
                        End Select

                        reader.ReadStartElement()
                        reader.Skip()
                    End While

                    If Bounds.Encloses(Node) Then
                        If IsBusiness Then
                            Node.Description = NodeName 'might be named something like 'music shop' or 'lawyer office'
                            Map.Businesses.Add(Node)
                        End If
                        Map.Nodes.Add(Node)
                        NodeHashMap.Add(Node.ID, Node)
                    End If

                    If reader.NodeType = XmlNodeType.EndElement Then
                        reader.ReadEndElement()
                        reader.Skip()
                    End If
                Loop

                Do Until reader.Name = "relation" OrElse reader.NodeType = XmlNodeType.EndElement
                    Dim WayID As Long = Long.Parse(reader.GetAttribute("id"))
                    Dim WayType As WayType = WayType.UNSPECIFIED_WAY
                    Dim WayName As String = ""
                    Dim MaxSpeedOverrideKMH As Integer = -1
                    Dim OneWay As String = ""
                    Dim AccessAllowed As Boolean = True

                    reader.ReadStartElement()
                    reader.Skip()

                    Dim Nds As New List(Of Node)
                    While reader.Name = "nd"
                        Dim NodeRef As Long = Long.Parse(reader.GetAttribute("ref"))
                        Dim Node As Node = Nothing
                        NodeHashMap.TryGetValue(NodeRef, Node)
                        If Node IsNot Nothing Then
                            Nds.Add(Node)
                        End If

                        reader.ReadStartElement()
                        reader.Skip()
                    End While

                    While reader.Name = "tag"
                        Dim AttributeName As String = reader.GetAttribute("k")
                        If AttributeName = "highway" Then
                            WayType = DecodeHighWayType(reader.GetAttribute("v"))
                        ElseIf AttributeName = "name" Then
                            WayName = reader.GetAttribute("v")
                        ElseIf AttributeName = "oneway" Then
                            OneWay = reader.GetAttribute("v")
                        ElseIf AttributeName = "access" Then
                            AccessAllowed = AccessAllowed And DecodeHighWayAccessLevel(reader.GetAttribute("v"))
                        ElseIf AttributeName = "maxspeed" Then
                            MaxSpeedOverrideKMH = DecodeHighwayMaxSpeed(reader.GetAttribute("v"))
                        ElseIf AttributeName = "amenity" Then
                            If reader.GetAttribute("v") = "fuel" Then
                                UnconfirmedFuelPoints.Add(Nds(0))
                            End If
                        End If
                        reader.ReadStartElement()
                        reader.Skip()
                    End While

                    If WayType <> WayType.UNSPECIFIED_WAY And AccessAllowed Then
                        Dim Way As New Way(WayID, Nds.ToArray, WayType, WayName)

                        If OneWay <> "" Then
                            Way.SetOneWay(OneWay)
                        End If

                        If MaxSpeedOverrideKMH <> -1 Then
                            Way.SetSpeedLimit(MaxSpeedOverrideKMH)
                        End If

                        Map.Ways.Add(Way.ID, Way)
                        Map.NodesAdjacencyList.AddWay(Way)
                    End If

                    If reader.NodeType = XmlNodeType.EndElement Then
                        reader.ReadEndElement()
                        reader.Skip()
                    End If
                Loop

            End Using
        End Using

        Try
            If IO.File.Exists(AAFilePath) Then
                Dim AAFile() As String = IO.File.ReadAllLines(AAFilePath)
                For Each NodeID As String In AAFile(0).Split(",")
                    Map.Depots.Add(Map.NodesAdjacencyList.Rows(Long.Parse(NodeID)).NodeKey)
                    Map.FuelPoints.Add(Map.NodesAdjacencyList.Rows(Long.Parse(NodeID)).NodeKey)
                Next
                For Each NodeID As String In AAFile(1).Split(",")
                    Map.FuelPoints.Add(Map.NodesAdjacencyList.Rows(Long.Parse(NodeID)).NodeKey)
                Next

                For i = 2 To AAFile.Length - 1
                    Dim WayTrafficLine() As String = AAFile(i).Split(":")
                    Dim WayID As Long = Long.Parse(WayTrafficLine(0))
                    Dim TrafficWay As Way = Nothing
                    Map.Ways.TryGetValue(WayID, TrafficWay)
                    If TrafficWay IsNot Nothing Then
                        TrafficWay.ParseTrafficTrace(WayTrafficLine(1))
                        Map.WaysWithTraffic.Add(TrafficWay)
                    End If
                Next

            End If
        Catch ex As Exception
            MsgBox(IO.Path.GetFileName(AAFilePath) & " could not be parsed: " & ex.Message)
        End Try

        If Map.Depots.Count = 0 Then
            Dim CentralPoint As PointF = Bounds.GetCentralPoint
            Dim CentralNode As Node = Map.NodesAdjacencyList.GetNearestNode(CentralPoint.X, CentralPoint.Y)
            Map.Depots.Add(CentralNode)
            Map.FuelPoints.Add(CentralNode)
        End If

        'Prune disconnected components
        Map.NodesAdjacencyList.RemoveDisconnectedComponents(Map.Depots(0))
        For Each N As Node In Map.Nodes
            If Not Map.NodesAdjacencyList.Rows.ContainsKey(N.ID) Then
                N.Connected = False
                N.RoadDelay = RoadDelay.NONE
            ElseIf N.Connected Then
                N.RoadDelay = Math.Max(N.RoadDelay, RoadDelay.UNEXPECTED)
                Map.ConnectedNodesGrid.AddNode(N)
            End If
        Next

        'Remove road delays that are crossings in residential roads. Too infrequent.
        For Each W As Way In Map.Ways.Values
            If W.Type <= WayType.UNCLASSIFIED_ROAD Then
                For Each N As Node In W.Nodes
                    If N.RoadDelay = RoadDelay.ZEBRA_CROSSING Then
                        N.RoadDelay = RoadDelay.UNEXPECTED
                    End If
                Next
            End If
        Next

        Debug.WriteLine("All Nodes: " & Map.Nodes.Count)
        Debug.WriteLine("Connected Nodes: " & Map.ConnectedNodesGrid.Count)
        Debug.WriteLine("Ways: " & Map.Ways.Count)

        Return Map
    End Function


    'Way parsing utility functions
    Private Function DecodeHighWayType(ByVal str As String) As WayType
        Select Case str
            Case "service"
                Return WayType.SERVICE_ROAD
            Case "unclassified"
                Return WayType.UNCLASSIFIED_ROAD 'e.g. vine road
            Case "residential"
                Return WayType.RESIDENTIAL_ROAD
            Case "tertiary", "tertiary_link"
                Return WayType.TERTIARY_ROAD
            Case "secondary", "secondary_link"
                Return WayType.SECONDARY_ROAD
            Case "primary", "primary_link"
                Return WayType.PRIMARY_ROAD
            Case "trunk", "trunk_link"
                Return WayType.TRUNK_ROAD
            Case "motorway", "motorway_link"
                Return WayType.MOTORWAY
            Case Else
                Return WayType.UNSPECIFIED_WAY
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

    Private Sub ListUnconfirmedFuelPoints(Map As StreetMap, UnconfirmedFuelPoints As List(Of Node))
        Debug.WriteLine("A number of unconfirmed fuel points were found:")
        Dim NodeSet As New HashSet(Of Node)
        For Each N As Node In UnconfirmedFuelPoints
            Dim ClosestNode As Node = Map.ConnectedNodesGrid.GetNearestNode(N)
            If Not NodeSet.Contains(ClosestNode) Then
                NodeSet.Add(ClosestNode)
            End If
        Next

        For Each N As Node In NodeSet
            Debug.Write(N.ID & ",")
        Next

    End Sub

End Class
