Imports System.Xml

Namespace HERETrafficData
    Module HERETrafficData

        Sub LoadHereXML(ByVal Map As StreetMap, ByVal TrafficDirectory As String)
            If Not IO.Directory.Exists(TrafficDirectory) Then
                Exit Sub
            End If
            Dim XMLFileInfos As IO.FileInfo() = New IO.DirectoryInfo(TrafficDirectory).GetFiles
            If XMLFileInfos.Count = 0 Then
                Exit Sub
            End If
            Dim Time As Integer = 0
            Dim HerePointToNodeMap As New Dictionary(Of String, Node)
            Dim NullNode As New Node(0, 0, 0)

            For Each XMLFile As IO.FileInfo In New IO.DirectoryInfo(TrafficDirectory).GetFiles

                Dim t As Stopwatch = Stopwatch.StartNew

                Dim xDoc As XmlDocument = New XmlDocument()
                xDoc.Load(XMLFile.FullName)

                Dim xFlowItems As XmlNodeList = xDoc.GetElementsByTagName("FI")
                For Each xFlowItem As XmlElement In xFlowItems
                    Dim xCurrentFlow As XmlElement = xFlowItem.GetElementsByTagName("CF")(0)
                    Dim Speed As Double = xCurrentFlow.GetAttribute("SP")
                    Dim xShapes As XmlNodeList = xFlowItem.GetElementsByTagName("SHP")
                    Dim CoordinateList As String = xFlowItem.InnerText
                    For Each CoordinateString As String In CoordinateList.Split(" ")
                        If CoordinateString <> "" Then
                            If Not HerePointToNodeMap.ContainsKey(CoordinateString) Then
                                Dim Coordinates() As String = CoordinateString.Split(",")
                                Dim ClosestNode As Node = Map.ConnectedNodesGrid.GetNearestNode(CDbl(Coordinates(0)), CDbl(Coordinates(1)), 1)
                                If ClosestNode IsNot Nothing Then
                                    HerePointToNodeMap.Add(CoordinateString, ClosestNode)
                                Else
                                    HerePointToNodeMap.Add(CoordinateString, NullNode)
                                End If
                            End If
                            HerePointToNodeMap(CoordinateString).SpeedAtTime(Time) = Speed * 1.6
                        End If
                    Next
                Next
                Debug.Write(Time & " ")
                Time += 1
            Next

        End Sub

        Sub ExportWaySpeedData(ByVal Ways As SortedList(Of Long, Way))
            FileOpen(1, "ways.txt", OpenMode.Output)
            For Each W As Way In Ways.Values
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
                PrintLine(1, W.ID & ":" & SB.ToString)
FailedWay:
            Next
            FileClose(1)
        End Sub
    End Module
End Namespace