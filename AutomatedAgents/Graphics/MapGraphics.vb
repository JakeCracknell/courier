Module MapGraphics
    Private BG_COLOR As Color = Color.White
    Private Const AGENT_DRAW_SIZE As Integer = 10
    Private Const SPECIAL_NODE_DRAW_SIZE As Integer = 10
    Private OVERLAY_FONT As New Font("TimesNewRoman", 12)
    Private ERROR_FONT As New Font("TimesNewRoman", 36, FontStyle.Bold)
    Private DEPOT_FONT As New Font("TimesNewRoman", 7)
    Private CENTRED_STRING_FORMAT As New StringFormat With {.LineAlignment = StringAlignment.Center, .Alignment = StringAlignment.Center}
    Private ROUTE_PEN As New Pen(New SolidBrush(Color.Gold), 3)
    Private ROAD_THIN_PEN As New Pen(New SolidBrush(Color.Black), 1)
    Private ROAD_THICK_PEN_OUTER As New Pen(New SolidBrush(Color.Black), 5)
    Private ROAD_THICK_PEN_INNER_TWOWAY As New Pen(New SolidBrush(Color.White), 3)
    Private ROAD_THICK_PEN_INNER_ONEWAY As New Pen(New SolidBrush(Color.Red), 3)
    Private DELIVERY_FAIL_CROSS_PEN As New Pen(New SolidBrush(Color.Black), 2)
    Private DEPOT_THICK_PEN As New Pen(Brushes.Black, 2)
    Private Const ROUTE_TO_LABEL_FORMAT As String = "TO ({0} hops, {1} km, {2} min)"

    Private MapBitmapOriginal As Bitmap
    Private MapBitmapOverlay As Bitmap
    Private Width As Integer
    Private Height As Integer
    Private CC As CoordinateConverter

    Public ConfigDrawNodes As Boolean = False
    Public ConfigDrawRoads As Integer = 1
    Public ConfigDrawAgentLines As Boolean = True
    Public ConfigDrawNodeLabels As Boolean = False

    Sub Resize(ByVal _Width As Integer, ByVal _Height As Integer, ByVal Bounds As Bounds)
        Width = _Width
        Height = _Height
        CC = New CoordinateConverter(Bounds, Width, Height)
    End Sub
    Function DrawMap(ByVal Map As StreetMap) As Image
        MapBitmapOverlay = New Bitmap(Width, Height)
        Dim gr As Graphics = Graphics.FromImage(MapBitmapOverlay)
        gr.Clear(Color.Transparent)

        MapBitmapOriginal = New Bitmap(Width, Height)
        gr = Graphics.FromImage(MapBitmapOriginal)
        gr.Clear(BG_COLOR)

        If ConfigDrawNodes Then
            For Each N As Node In Map.Nodes
                Dim Point As Point = CC.GetPoint(N)
                Dim PointSize As Integer = 1 'Math.Min(Width * Height / 130000, ((Map.Nodes.Count / 10) * N.GetAgentTraffic / Node.TotalNodesTraffic))
                gr.DrawRectangle(Pens.Blue, Point.X, Point.Y, PointSize, PointSize)
                If Not N.Connected Then
                    gr.FillRectangle(Brushes.Red, Point.X, Point.Y, 10, 10)
                End If
                If ConfigDrawNodeLabels Then
                    gr.DrawString(N.ID Mod 1000, OVERLAY_FONT, Brushes.Black, Point)
                End If
            Next
        End If

        If ConfigDrawRoads <> 0 Then
            For Each W As Way In Map.Ways
                If W.Nodes.Length >= 2 Then
                    Dim LastPoint As Point = CC.GetPoint(W.Nodes(0))

                    For i = 1 To W.Nodes.Length - 1
                        Dim CurrentPoint As Point = CC.GetPoint(W.Nodes(i))
                        Select Case ConfigDrawRoads
                            Case 1
                                gr.DrawLine(ROAD_THIN_PEN, LastPoint, CurrentPoint)
                            Case 2
                                gr.DrawLine(ROAD_THICK_PEN_OUTER, LastPoint, CurrentPoint)
                                If W.OneWay Then
                                    gr.DrawLine(ROAD_THICK_PEN_INNER_ONEWAY, LastPoint, CurrentPoint)
                                Else
                                    gr.DrawLine(ROAD_THICK_PEN_INNER_TWOWAY, LastPoint, CurrentPoint)
                                End If

                        End Select
                        LastPoint = CurrentPoint
                    Next
                End If
            Next
        End If

        DrawDepot(gr)

        Return MapBitmapOriginal.Clone
    End Function

    Sub DrawDepot(ByVal gr As Graphics)
        If NoticeBoard.DepotPoint IsNot Nothing Then
            Dim DepotPoint As Point = CC.GetPoint(NoticeBoard.DepotPoint)
            Dim Len2 As Integer = SPECIAL_NODE_DRAW_SIZE \ 2
            gr.FillRectangle(Brushes.White, DepotPoint.X - Len2, _
                             DepotPoint.Y - Len2, _
                             SPECIAL_NODE_DRAW_SIZE, SPECIAL_NODE_DRAW_SIZE)

            gr.DrawRectangle(DEPOT_THICK_PEN, DepotPoint.X - Len2, _
                             DepotPoint.Y - Len2, _
                             SPECIAL_NODE_DRAW_SIZE, SPECIAL_NODE_DRAW_SIZE)
            DepotPoint.Offset(0, 0)
            gr.DrawString("D", DEPOT_FONT, Brushes.Black, DepotPoint, CENTRED_STRING_FORMAT)
        End If
    End Sub

    Sub DrawAgent(ByVal Agent As Agent, ByVal gr As Graphics)
        If Agent.Position IsNot Nothing Then
            Dim CurrentPoint As Point = CC.GetPoint(Agent.Position.GetRoutingPoint)
            Dim Brush As New SolidBrush(Agent.Color)
            gr.FillPie(Brush, CInt(CurrentPoint.X - AGENT_DRAW_SIZE / 2), _
                           CInt(CurrentPoint.Y - AGENT_DRAW_SIZE / 2), _
                           AGENT_DRAW_SIZE, AGENT_DRAW_SIZE, 0, _
                           CSng(Agent.Delayer.GetPercentage * 360))

            If ConfigDrawAgentLines Then
                Dim TargetPoint As Point = CC.GetPoint(Agent.Position.GetEndPoint)
                Dim Pen As New Pen(Agent.Color)
                gr.DrawLine(Pen, CurrentPoint, TargetPoint)
            End If
        End If
    End Sub

    Private Sub DrawNodeRectangle(ByVal Point As Point, ByVal gr As Graphics)
        Dim Len2 As Integer = SPECIAL_NODE_DRAW_SIZE \ 2
        gr.DrawRectangle(Pens.Red, Point.X - Len2, Point.Y - Len2, _
                         SPECIAL_NODE_DRAW_SIZE, _
                         SPECIAL_NODE_DRAW_SIZE)
    End Sub

    Function DrawHighlightedNode(ByVal Node As Node, ByVal Cursor As Point) As Image
        Dim NodePoint As Point = CC.GetPoint(Node)

        Dim OverlayBitmapCopy As Bitmap = MapBitmapOverlay.Clone
        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)

        DrawNodeRectangle(Cursor, grOverlay)
        Dim POINT_TO_NODE_PEN As New Pen(Brushes.Purple, 1)
        POINT_TO_NODE_PEN.EndCap = Drawing2D.LineCap.ArrowAnchor
        grOverlay.DrawLine(POINT_TO_NODE_PEN, Cursor, NodePoint)

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(OverlayBitmapCopy, 0, 0)
        OverlayBitmapCopy.Dispose()
        grMap.Dispose()
        grOverlay.Dispose()
        Return MapBitmapCopy
    End Function

    Function DrawRouteStart(ByVal Point As IPoint) As Image
        Return DrawRoute(New Route(Point), Nothing)
    End Function
    Function DrawRoute(ByVal Route As Route) As Image
        Return DrawRoute(Route, Nothing)
    End Function

    Function DrawRoute(ByVal Route As Route, ByVal NodesSearched As List(Of Node)) As Image
        Dim grOverlay As Graphics = Graphics.FromImage(MapBitmapOverlay)

        Dim RouteFromPoint As IPoint = Route.GetStartPoint
        Dim NodePoint As Point = CC.GetPoint(RouteFromPoint)
        grOverlay.Clear(Color.Transparent)

        DrawNodeRectangle(NodePoint, grOverlay)
        grOverlay.DrawString("FROM", OVERLAY_FONT, Brushes.Black, NodePoint)

        If Route.HopCount > 0 Then
            If NodesSearched IsNot Nothing Then
                For Each N As Node In NodesSearched
                    Dim Point As Point = CC.GetPoint(N)
                    grOverlay.FillRectangle(New SolidBrush(Color.Green), Point.X - 1, Point.Y - 1, 2, 2)
                Next
            End If

            Dim RouteToNode = Route.GetEndPoint
            NodePoint = CC.GetPoint(RouteToNode)
            DrawNodeRectangle(NodePoint, grOverlay)

            For Each Hop As Hop In Route.GetHopList()
                Dim FromPoint As Point = CC.GetPoint(Hop.FromPoint)
                Dim ToPoint As Point = CC.GetPoint(Hop.ToPoint)
                grOverlay.DrawLine(ROUTE_PEN, FromPoint, ToPoint)
            Next

            Dim ToLabel As String = String.Format(ROUTE_TO_LABEL_FORMAT, Route.HopCount, _
                    Math.Round(Route.GetKM, 1), Math.Round(Route.GetEstimatedHours * 60, 1))
            grOverlay.DrawString(ToLabel, OVERLAY_FONT, Brushes.Black, NodePoint)


        End If

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(MapBitmapOverlay, 0, 0)

        grOverlay.Dispose()
        Return MapBitmapCopy
    End Function

    Function DrawOverlay(ByVal Agents As List(Of Agent), ByVal UnpickedJobs As List(Of CourierJob)) As Image
        Dim OverlayBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)

        DrawDepot(grOverlay)

        For Each Job As CourierJob In UnpickedJobs
            DrawNodeRectangle(CC.GetPoint(Job.PickupPosition), grOverlay)
        Next

        For Each Agent As Agent In Agents
            Dim AgentIsWaiting As Boolean = Agent.Delayer.IsWaiting

            DrawAgent(Agent, grOverlay)
            For Each Job As CourierJob In Agent.AssignedJobs
                Dim PickupPt As Point = CC.GetPoint(Job.PickupPosition)
                Dim DropOffPt As Point = CC.GetPoint(Job.OriginalDeliveryPosition)
                Dim Brush As New SolidBrush(Agent.Color)
                Dim RoutePen As New Pen(New SolidBrush(Agent.Color), 3)
                RoutePen.EndCap = Drawing2D.LineCap.ArrowAnchor

                Dim Len2 As Integer = SPECIAL_NODE_DRAW_SIZE \ 2
                If Job.Status = JobStatus.PENDING_PICKUP Then
                    DrawNodeRectangle(PickupPt, grOverlay)
                ElseIf Job.Status = JobStatus.PENDING_DELIVERY Then
                    If AgentIsWaiting Then
                        DrawNodeRectangle(CC.GetPoint(Job.PickupPosition), grOverlay)
                    End If
                    If Job.DeliveryPosition.Equals(Job.OriginalDeliveryPosition) Then
                        grOverlay.FillRectangle(Brush, DropOffPt.X - 5, DropOffPt.Y - 5, 10, 10)
                    Else
                        'If rerouted to depot
                        grOverlay.DrawLine(DELIVERY_FAIL_CROSS_PEN, DropOffPt.X - 5, DropOffPt.Y - 5, DropOffPt.X + 5, DropOffPt.Y + 5)
                        grOverlay.DrawLine(DELIVERY_FAIL_CROSS_PEN, DropOffPt.X - 5, DropOffPt.Y + 5, DropOffPt.X + 5, DropOffPt.Y - 5)
                        grOverlay.DrawLine(RoutePen, DropOffPt, CC.GetPoint(Job.DeliveryPosition))
                    End If
                End If

                grOverlay.DrawLine(RoutePen, PickupPt, DropOffPt)
            Next

        Next

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(OverlayBitmapCopy, 0, 0)
        OverlayBitmapCopy.Dispose()
        grMap.Dispose()
        grOverlay.Dispose()
        Return MapBitmapCopy
    End Function

    Function DrawQuestionMark(ByVal MousePoint As Point) As Image
        Dim OverlayBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)
        grOverlay.DrawString("?", ERROR_FONT, Brushes.Red, MousePoint)

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(OverlayBitmapCopy, 0, 0)
        OverlayBitmapCopy.Dispose()
        grMap.Dispose()
        grOverlay.Dispose()
        Return MapBitmapCopy
    End Function

End Module
