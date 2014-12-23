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

        Node.TotalNodesTraffic = 1

        '.NETs garbage collector will not dispose the XML document
        'until a while later. SW London = 1GB -> 200MB after GC

        Map.NodesAdjacencyList.RemoveDisconnectedComponents()

        Return Map
    End Function



End Class
