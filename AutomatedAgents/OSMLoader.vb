﻿Imports System.Xml

Public Class OSMLoader
    Private FilePath As String = "C:\Users\Jake\Downloads\map.osm"

    Public Sub New(ByVal FilePath As String)
        Me.FilePath = FilePath
    End Sub

    Function CreateMap() As StreetMap
        Dim FileContents As String = IO.File.ReadAllText(FilePath)
        Dim xDoc As XmlDocument = New XmlDocument()
        xDoc.LoadXml(FileContents)

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
            Dim xWayID As Integer = xItem.GetAttribute("id")
            Dim WayType As WayType = WayType.UNSPECIFIED

            'If xWayID = 4005815 Then
            '    Console.Beep()
            'End If


            Dim xTags As XmlNodeList = xItem.GetElementsByTagName("tag")
            For Each xTag As XmlElement In xTags
                If xTag.GetAttribute("k") = "highway" Then
                    WayType = DecodeHighWayType(xTag.GetAttribute("v"))
                End If
            Next

            'Do not add non-highways to way list.
            If WayType <> WayType.UNSPECIFIED Then
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

                Dim Way As New Way(xWayID, Nds.ToArray, WayType)
                Map.Ways.Add(Way)
                Map.NodesAdjacencyList.AddWay(Way)
            End If

        Next



        Return Map
    End Function



End Class
