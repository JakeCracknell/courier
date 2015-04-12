Imports System.Xml

Public Class OSMLoader
    Private OSMFilePath As String
    Private AAFilePath As String
    Public Sub New(ByVal FilePath As String)
        Me.OSMFilePath = FilePath
        Me.AAFilePath = FilePath.Replace(".osm", ".aa")
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
            End If
        Next

        Return Map
    End Function

End Class
