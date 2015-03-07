Imports System.Xml

Public Class OSMLoader
    Private FilePath As String
    Public Sub New(ByVal FilePath As String)
        Me.FilePath = FilePath
    End Sub

    Function CreateMap() As StreetMap
        Dim xDoc As XmlDocument = New XmlDocument()
        xDoc.Load(FilePath)

        Dim xBounds As XmlElement = xDoc.GetElementsByTagName("bounds")(0)
        Dim Bounds As New Bounds(xBounds.GetAttribute("minlat"), _
                                 xBounds.GetAttribute("minlon"), _
                                 xBounds.GetAttribute("maxlat"), _
                                 xBounds.GetAttribute("maxlon"))

        Dim Map As New StreetMap(Bounds)
        Dim NodeHashMap As New Dictionary(Of Long, Node)

        Dim xNodes As XmlNodeList = xDoc.GetElementsByTagName("node")
        For Each xItem As XmlElement In xNodes
            Dim xNodeID As Long = xItem.GetAttribute("id")
            Dim xLat As Double = xItem.GetAttribute("lat")
            Dim xLon As Double = xItem.GetAttribute("lon")
            Dim Node As New Node(xNodeID, xLat, xLon)
            Map.Nodes.Add(Node)
            NodeHashMap.Add(xNodeID, Node)
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

        Node.TotalNodesTraffic = 1

        Map.NodesAdjacencyList.RemoveDisconnectedComponents()

        ''Find fuel locations
        'For Each xItem As XmlElement In xNodes
        '    Dim xTags As XmlNodeList = xItem.GetElementsByTagName("tag")
        '    For Each xTag As XmlElement In xTags
        '        Dim AttributeName As String = xTag.GetAttribute("k")
        '        If AttributeName = "amenity" Then
        '            Dim ValueName As String = xTag.GetAttribute("v")
        '            If ValueName = "fuel" Then
        '                Dim NodeRef As Long = xItem.GetAttribute("id")
        '                Dim Node As Node = NodeHashMap(NodeRef)
        '                Map.FuelNodes.Add(Map.NodesAdjacencyList.GetNearestNode(Node.Latitude, Node.Longitude))
        '            End If
        '        End If
        '    Next
        'Next
        'For Each xItem As XmlElement In xWays
        '    Dim xTags As XmlNodeList = xItem.GetElementsByTagName("tag")
        '    For Each xTag As XmlElement In xTags
        '        Dim AttributeName As String = xTag.GetAttribute("k")
        '        If AttributeName = "amenity" Then
        '            Dim ValueName As String = xTag.GetAttribute("v")
        '            If ValueName = "fuel" Then
        '                Dim xNd As XmlElement = xItem.SelectSingleNode("nd")
        '                Dim NodeRef As Long = xNd.GetAttribute("ref")
        '                Dim Node As Node = NodeHashMap(NodeRef)
        '                Map.FuelNodes.Add(Map.NodesAdjacencyList.GetNearestNode(Node.Latitude, Node.Longitude))
        '            End If
        '        End If
        '    Next
        'Next
        Debug.WriteLine("Fuels: " & Map.FuelNodes.Count)


        Return Map
    End Function

End Class
