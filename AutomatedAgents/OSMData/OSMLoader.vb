Imports System.Xml

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
        Dim xDoc As XmlDocument = New XmlDocument()
        xDoc.Load(OSMFilePath)

        Dim xBounds As XmlElement = xDoc.GetElementsByTagName("bounds")(0)
        Dim Bounds As New Bounds(xBounds.GetAttribute("minlat"), _
                                 xBounds.GetAttribute("minlon"), _
                                 xBounds.GetAttribute("maxlat"), _
                                 xBounds.GetAttribute("maxlon"))


        Dim Map As New StreetMap(IO.Path.GetFileNameWithoutExtension(OSMFilePath), Bounds)
        Dim NodeHashMap As New Dictionary(Of Long, Node)

        Dim xNodes As XmlNodeList = xDoc.GetElementsByTagName("node")
        For Each xItem As XmlElement In xNodes
            Dim xNodeID As Long = xItem.GetAttribute("id")
            Dim xLat As Double = xItem.GetAttribute("lat")
            Dim xLon As Double = xItem.GetAttribute("lon")
            Dim Node As New Node(xNodeID, xLat, xLon)
            If Bounds.Encloses(Node) Then
                Dim xTags As XmlNodeList = xItem.GetElementsByTagName("tag")
                Dim IsBusiness As Boolean = False
                Dim NodeName As String = Nothing
                For Each xTag As XmlElement In xTags
                    Dim AttributeName As String = xTag.GetAttribute("k")
                    If AttributeName = "shop" OrElse AttributeName = "office" Then
                        IsBusiness = True
                        NodeName = If(NodeName, xTag.GetAttribute("v").Replace("_", " ") & " " & AttributeName)
                    ElseIf AttributeName = "name" Then
                        NodeName = xTag.GetAttribute("v")
                    End If
                Next
                If IsBusiness Then
                    Node.Description = NodeName 'might be named something like 'music shop' or 'lawyer office'
                    Map.Businesses.Add(Node)
                End If

                Map.Nodes.Add(Node)
                NodeHashMap.Add(xNodeID, Node)
            End If
        Next

        Dim xWays As XmlNodeList = xDoc.GetElementsByTagName("way")
        For Each xItem As XmlElement In xWays
            Dim Way As Way = ParseWay(xItem, NodeHashMap)
            If Way IsNot Nothing Then
                Map.Ways.Add(Way)
                Map.NodesAdjacencyList.AddWay(Way)
            End If
        Next

        Debug.WriteLine("Nodes: " & Map.Nodes.Count)
        Debug.WriteLine("Ways: " & Map.Ways.Count)

        Try
            If IO.File.Exists(AAFilePath) Then
                Dim AAFile() As String = IO.File.ReadAllLines(AAFilePath)
                For Each NodeID As String In AAFile(0).Split(",")
                    Map.Depots.Add(Map.NodesAdjacencyList.Rows(CLng(NodeID)).NodeKey)
                    Map.FuelPoints.Add(Map.NodesAdjacencyList.Rows(CLng(NodeID)).NodeKey)
                Next
                For Each NodeID As String In AAFile(1).Split(",")
                    Map.FuelPoints.Add(Map.NodesAdjacencyList.Rows(CLng(NodeID)).NodeKey)
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

        Dim t As New Stopwatch : t.Start()
        Map.NodesAdjacencyList.RemoveDisconnectedComponents(Map.Depots(0))
        For Each N As Node In Map.Nodes
            If Not Map.NodesAdjacencyList.Rows.ContainsKey(N.ID) Then
                N.Connected = False
            Else
                Map.ConnectedNodesGrid.AddNode(N)
            End If
        Next

        'LoadHereXML(Map)
        'ExportWaySpeedData(Map.Ways)
        Return Map
    End Function

    Sub LoadHereXML(ByVal Map As StreetMap)
        If Not IO.Directory.Exists(TrafficDirectory) Then
            Exit Sub
        End If
        Dim XMLFileInfos As IO.FileInfo() = New IO.DirectoryInfo(TrafficDirectory).GetFiles
        If XMLFileInfos.Count = 0 Then
            Exit Sub
        End If
        Dim StartTime As Long = CLng(XMLFileInfos(0).Name.Substring(XMLFileInfos(0).Name.IndexOf(",") + 1).Split(".")(0))

        For Each XMLFile As IO.FileInfo In New IO.DirectoryInfo(TrafficDirectory).GetFiles

            Dim t = Stopwatch.StartNew
            Dim Time As Integer = TimeSpan.FromTicks(CLng(XMLFile.Name.Substring(XMLFile.Name.IndexOf(",") + 1).Split(".")(0)) - StartTime).TotalMinutes \ 5

            Dim xDoc As XmlDocument = New XmlDocument()
            xDoc.Load(XMLFile.FullName)

            Dim i = 0
            Dim xFlowItems As XmlNodeList = xDoc.GetElementsByTagName("FI")
            For Each xFlowItem As XmlElement In xFlowItems
                Dim xCurrentFlow As XmlElement = xFlowItem.GetElementsByTagName("CF")(0)
                Dim Speed As Double = xCurrentFlow.GetAttribute("SP")
                Dim xShapes As XmlNodeList = xFlowItem.GetElementsByTagName("SHP")
                Dim CoordinateList As String = xFlowItem.InnerText
                For Each CoordinateString As String In CoordinateList.Split(" ")
                    If CoordinateString <> "" Then
                        Dim Coordinates() As String = CoordinateString.Split(",")
                        Dim ClosestNode As Node = Map.ConnectedNodesGrid.GetNearestNode(CDbl(Coordinates(0)), CDbl(Coordinates(1)))
                        If ClosestNode IsNot Nothing Then
                            ClosestNode.SpeedAtTime(Time) = Speed * 1.6
                        End If
                    End If
                Next
                Debug.Write(i & " ")
                i += 1
            Next
        Next

    End Sub

    Sub ExportWaySpeedData(ByVal Ways As List(Of Way))
        FileOpen(1, "ways.txt", OpenMode.Output)
        For Each W As Way In Ways
            Dim SB As New System.Text.StringBuilder
            For i = 0 To 2015
                Dim SpeedSum As Double = 0
                Dim NodesCounted As Integer = 0
                For Each Node In W.Nodes
                    Dim Speed As Double
                    Node.SpeedAtTime.TryGetValue(i, Speed)
                    If Speed <> 0 Then
                        NodesCounted += 1
                        SpeedSum += Node.SpeedAtTime(i)
                    End If
                Next
                If NodesCounted >= 2 AndAlso NodesCounted / W.Nodes.Count > 0.2 Then
                    Dim Speed As Double = SpeedSum / NodesCounted
                    SB.Append(Math.Round(Speed, 2)).Append(",")
                Else
                    GoTo FailedWay
                End If
            Next
            PrintLine(1, W.ID & "," & SB.ToString)
FailedWay:
        Next
        FileClose(1)
    End Sub
End Class
