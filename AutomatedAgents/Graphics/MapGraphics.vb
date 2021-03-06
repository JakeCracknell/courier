﻿Module MapGraphics
    Private BG_COLOR As Color = Color.White
    Private Const LANDMARK_NODE_DRAW_SIZE As Integer = 10
    Private Const SPECIAL_NODE_DRAW_SIZE_THIN As Integer = 3
    Private Const SPECIAL_NODE_DRAW_SIZE_THICK As Integer = 5
    Private Const TRAFFIC_WAY_THICKNESS As Integer = 5
    Private OVERLAY_FONT As New Font("TimesNewRoman", 12)
    Private ERROR_FONT As New Font("TimesNewRoman", 36, FontStyle.Bold)
    Private DEPOT_FONT As New Font("TimesNewRoman", 7)
    Private CENTRED_STRING_FORMAT As New StringFormat With _
        {.LineAlignment = StringAlignment.Center, .Alignment = StringAlignment.Center}
    Private ROUTE_PEN_OUTER As New Pen(New SolidBrush(Color.FromArgb(128, Color.Black)), 10)
    Private ROUTE_PEN_INNER As New Pen(New SolidBrush(Color.Gold), 5)
    Private ROAD_THIN_PEN As New Pen(New SolidBrush(Color.Black), 1)
    Private ROAD_THICK_PEN_OUTER As New Pen(New SolidBrush(Color.Black), 5)
    Private ROAD_THICK_PEN_INNER_TWOWAY As New Pen(New SolidBrush(Color.White), 3)
    Private ROAD_THICK_PEN_INNER_ONEWAY As New Pen(New SolidBrush(Color.SandyBrown), 3) With {.DashStyle = Drawing2D.DashStyle.Dot}
    Private NODE_BUSINESS_BRUSH As Brush = Brushes.Yellow
    Private NODE_ROAD_DELAY_BRUSH As Brush = Brushes.Red
    Private NODE_ROAD_DELAY_PEN As Pen = Pens.Red
    Private QUADRANT_GRID_PEN As New Pen(Brushes.Gray) With {.DashStyle = Drawing2D.DashStyle.Dash}
    Private DELIVERY_FAIL_CROSS_PEN As New Pen(New SolidBrush(Color.Black), 2)
    Private LANDMARK_BORDER_PEN As New Pen(Brushes.Black, 2)
    Private FUEL_LANDMARK_BRUSH As Brush = Brushes.White
    Private DEPOT_LANDMARK_BRUSH As Brush = Brushes.LightPink
    Private WAYPOINT_BRUSH As Brush = Brushes.Black

    Private Const ROUTE_TO_LABEL_FORMAT As String = "TO ({0} hops, {1} km, {2}-{3} min, {4}-{5} L)"

    Private MapBitmapOriginal As Bitmap
    Private MapBitmapOverlay As Bitmap
    Private Width As Integer
    Private Height As Integer
    Private CC As CoordinateConverter

    Public ConfigDrawRoadDelayNodes As Boolean = False
    Public ConfigDrawBusinessNodes As Boolean = False
    Public ConfigDrawRoads As Integer = 1
    Public ConfigDrawAgentLines As Boolean = True
    Public ConfigDrawNodeLabels As Boolean = False
    Public ConfigDrawAgentRoutes As Integer = 0
    Public ConfigDrawLandmarks As Boolean = True
    Public ConfigDrawGrid As Boolean = False
    Public ConfigDrawTrafficLayer As Boolean = False
    Public MapRedrawRequired As Boolean

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

        If ConfigDrawRoads <> 0 Then
            For Each W As Way In Map.Ways.Values
                If W.Nodes.Length >= 2 Then
                    Dim LastPoint As Point = CC.GetPoint(W.Nodes(0))

                    For i = 1 To W.Nodes.Length - 1
                        Dim CurrentPoint As Point = CC.GetPoint(W.Nodes(i))
                        Select Case ConfigDrawRoads
                            Case 1
                                gr.DrawLine(ROAD_THIN_PEN, LastPoint, CurrentPoint)
                            Case 2
                                'Draws white road with black outline. One-way roads have a dotted brown line on top.
                                gr.DrawLine(ROAD_THICK_PEN_OUTER, LastPoint, CurrentPoint)
                                gr.DrawLine(ROAD_THICK_PEN_INNER_TWOWAY, LastPoint, CurrentPoint)
                                If W.OneWay Then
                                    gr.DrawLine(ROAD_THICK_PEN_INNER_ONEWAY, LastPoint, CurrentPoint)
                                End If

                        End Select
                        LastPoint = CurrentPoint
                    Next
                End If
            Next
        End If

        If ConfigDrawBusinessNodes Or ConfigDrawRoadDelayNodes Then
            Dim NodeDrawSize As Integer = If(ConfigDrawRoads = 2, SPECIAL_NODE_DRAW_SIZE_THICK, SPECIAL_NODE_DRAW_SIZE_THIN)
            Dim Len2 As Integer = NodeDrawSize \ 2
            For Each N As Node In Map.Nodes
                Dim Point As Point = CC.GetPoint(N)
                Dim PointSize As Integer = 1 'Math.Min(Width * Height / 130000, ((Map.Nodes.Count / 10) * N.GetAgentTraffic / Node.TotalNodesTraffic))
                If ConfigDrawBusinessNodes AndAlso N.Description IsNot Nothing Then
                    gr.FillRectangle(NODE_BUSINESS_BRUSH, Point.X - Len2, Point.Y - Len2, NodeDrawSize, NodeDrawSize)
                End If
                If ConfigDrawRoadDelayNodes AndAlso N.RoadDelay > RoadDelay.UNEXPECTED Then
                    gr.DrawRectangle(NODE_ROAD_DELAY_PEN, Point.X - Len2, Point.Y - Len2, NodeDrawSize, NodeDrawSize)
                End If
                If ConfigDrawNodeLabels Then
                    gr.DrawString(N.ID Mod 1000, OVERLAY_FONT, Brushes.Black, Point)
                End If
            Next
        End If

        If ConfigDrawLandmarks Then
            For Each FuelPoint As IPoint In Map.FuelPoints
                DrawLandmark(gr, CC.GetPoint(FuelPoint), "F"c, FUEL_LANDMARK_BRUSH)
            Next
            For Each Depot As Node In Map.Depots
                DrawLandmark(gr, CC.GetPoint(Depot), Depot.Description(Depot.Description.Length - 1), DEPOT_LANDMARK_BRUSH)
            Next
        End If

        If ConfigDrawGrid Then
            For x = 0 To Width Step Math.Max(1, Width \ 100)
                gr.DrawLine(QUADRANT_GRID_PEN, x, 0, x, Height)
            Next
            For y = 0 To Height Step Math.Max(1, Height \ 100)
                gr.DrawLine(QUADRANT_GRID_PEN, 0, y, Width, y)
            Next
        End If

        Return MapBitmapOriginal.Clone
    End Function

    Sub DrawTriangle(ByVal gr As Graphics, ByVal Point As Point, ByVal Upwards As Boolean)
        Dim y1 As Integer = Point.Y - SimulationParameters.AgentDrawSize \ 2
        Dim y2 As Integer = Point.Y + SimulationParameters.AgentDrawSize \ 2
        Dim x1 As Integer = Point.X - SimulationParameters.AgentDrawSize \ 2
        Dim x2 As Integer = Point.X + SimulationParameters.AgentDrawSize \ 2

        Dim gp As New Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
        If Upwards Then
            Dim ptsArray As PointF() = {New Point(x1, y1), New Point(x2, y1), _
                                    New Point(Point.X, y2), New Point(x1, y1)}
            gp.AddLines(ptsArray)
        Else
            Dim ptsArray As PointF() = {New Point(x1, y2), New Point(x2, y2), _
                                    New Point(Point.X, y1), New Point(x1, y2)}
            gp.AddLines(ptsArray)

        End If
        gp.CloseFigure()
        gr.FillPath(WAYPOINT_BRUSH, gp)
    End Sub

    Sub DrawLandmark(ByVal gr As Graphics, ByVal DrawPoint As Point, ByVal Character As Char, ByVal Brush As Brush)
        Dim Len2 As Integer = LANDMARK_NODE_DRAW_SIZE \ 2
        gr.FillRectangle(Brush, DrawPoint.X - Len2, _
                         DrawPoint.Y - Len2, _
                         LANDMARK_NODE_DRAW_SIZE, LANDMARK_NODE_DRAW_SIZE)

        gr.DrawRectangle(LANDMARK_BORDER_PEN, DrawPoint.X - Len2, _
                         DrawPoint.Y - Len2, _
                         LANDMARK_NODE_DRAW_SIZE, LANDMARK_NODE_DRAW_SIZE)
        gr.DrawString(Character, DEPOT_FONT, Brushes.Black, DrawPoint, CENTRED_STRING_FORMAT)
    End Sub

    Sub DrawAgent(ByVal Agent As Agent, ByVal gr As Graphics)
        If Agent.Plan.RoutePosition IsNot Nothing Then
            Dim CurrentPoint As Point = CC.GetPoint(Agent.Plan.RoutePosition.GetPoint)
            Dim Brush As New SolidBrush(Agent.Color)
            Dim Rectangle As New Rectangle(CInt(CurrentPoint.X - SimulationParameters.AgentDrawSize / 2), _
                               CInt(CurrentPoint.Y - SimulationParameters.AgentDrawSize / 2), _
                               SimulationParameters.AgentDrawSize, SimulationParameters.AgentDrawSize)
            If Agent.Delayer.IsWaiting Then
                gr.DrawEllipse(New Pen(Brush), Rectangle)
                gr.FillPie(Brush, Rectangle, 0, CSng(Agent.Delayer.GetPercentage * 360))
            Else
                gr.FillEllipse(Brush, Rectangle)
            End If


            If Agent.Plan.RoutePosition.RoadDelay Then
                Rectangle.Location = New Point(Rectangle.X + Rectangle.Width / 4, Rectangle.Y + Rectangle.Height / 4)
                Rectangle.Width /= 2
                Rectangle.Height /= 2
                Dim NegativeColourBrush As Brush = New SolidBrush(Color.FromArgb(Agent.Color.ToArgb Xor &HFFFFFF))
                gr.FillRectangle(NegativeColourBrush, Rectangle)
            End If

            If ConfigDrawAgentLines Then
                Dim TargetPoint As Point = CC.GetPoint(Agent.Plan.RoutePosition.GetEndPoint)
                Dim Pen As New Pen(Agent.Color)
                gr.DrawLine(Pen, CurrentPoint, TargetPoint)
            End If
        End If
    End Sub

    Private Sub DrawNodeRectangle(ByVal Point As Point, ByVal gr As Graphics)
        Dim Len2 As Integer = LANDMARK_NODE_DRAW_SIZE \ 2
        gr.DrawRectangle(Pens.Red, Point.X - Len2, Point.Y - Len2, _
                         LANDMARK_NODE_DRAW_SIZE, _
                         LANDMARK_NODE_DRAW_SIZE)
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

    Sub DrawTextOnBox(ByVal Text As String, ByVal Point As Point, ByVal gr As Graphics)
        Dim SizeF As SizeF = gr.MeasureString(Text, OVERLAY_FONT)
        Dim XStart As Integer = If(Point.X + SizeF.Width > Width, Width - SizeF.Width, Point.X)
        gr.FillRectangle(Brushes.White, XStart, Point.Y, SizeF.Width, SizeF.Height)
        gr.DrawRectangle(Pens.Black, XStart, Point.Y, SizeF.Width, SizeF.Height)
        gr.DrawString(Text, OVERLAY_FONT, Brushes.Black, XStart, Point.Y)
    End Sub

    Function DrawRouteStart(ByVal Point As IPoint) As Image
        Return DrawRoute(New Route(Point), Nothing)
    End Function
    Function DrawRoute(ByVal Route As Route) As Image
        Return DrawRoute(Route, Nothing)
    End Function

    Function DrawRoute(ByVal Route As Route, ByVal NodesSearched As List(Of Node)) As Image
        Dim grOverlay As Graphics = Graphics.FromImage(MapBitmapOverlay)

        Dim RouteFromPoint As IPoint = Route.GetStartPoint
        Dim FromNodePoint As Point = CC.GetPoint(RouteFromPoint)
        grOverlay.Clear(Color.Transparent)

        DrawNodeRectangle(FromNodePoint, grOverlay)
        DrawTextOnBox("FROM", FromNodePoint, grOverlay)

        If Route.HopCount > 1 Then
            If NodesSearched IsNot Nothing Then
                For i = 0 To NodesSearched.Count - 1
                    Dim Point As Point = CC.GetPoint(NodesSearched(i))
                    Dim Color As Color = GetRainbowColour(i / NodesSearched.Count)
                    grOverlay.FillRectangle(New SolidBrush(Color), Point.X - 1, Point.Y - 1, 2, 2)
                Next
            End If

            Dim RouteToNode = Route.GetEndPoint
            Dim ToNodePoint = CC.GetPoint(RouteToNode)
            DrawNodeRectangle(ToNodePoint, grOverlay)

            For Each Hop As Hop In Route.GetHopList()
                Dim FromPoint As Point = CC.GetPoint(Hop.FromPoint)
                Dim ToPoint As Point = CC.GetPoint(Hop.ToPoint)
                grOverlay.DrawLine(ROUTE_PEN_OUTER, FromPoint, ToPoint)
                grOverlay.DrawLine(ROUTE_PEN_INNER, FromPoint, ToPoint)
            Next

            Dim Len2 As Integer = LANDMARK_NODE_DRAW_SIZE \ 2
            grOverlay.FillRectangle(Brushes.Black, FromNodePoint.X - Len2, _
                                         FromNodePoint.Y - Len2, _
                                         LANDMARK_NODE_DRAW_SIZE, LANDMARK_NODE_DRAW_SIZE)
            grOverlay.FillRectangle(Brushes.Black, ToNodePoint.X - Len2, _
                                         ToNodePoint.Y - Len2, _
                                         LANDMARK_NODE_DRAW_SIZE, LANDMARK_NODE_DRAW_SIZE)

            DrawTextOnBox("FROM", FromNodePoint, grOverlay)

            Dim ToLabel As String = String.Format(ROUTE_TO_LABEL_FORMAT, Route.HopCount, _
                    Math.Round(Route.GetKM, 1), Math.Round(Route.GetHoursWithoutTraffic * 60, 1), _
                    Math.Round(Route.GetEstimatedHours() * 60, 1), _
                    Math.Round(Route.GetOptimalFuelUsageWithoutTraffic(), 2), _
                    Math.Round(Route.GetOptimalFuelUsageWithTraffic(), 2))
            DrawTextOnBox(ToLabel, ToNodePoint, grOverlay)

        End If

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(MapBitmapOverlay, 0, 0)

        grOverlay.Dispose()
        Return MapBitmapCopy
    End Function

    Function DrawOverlay(ByVal Agents As List(Of Agent), ByVal Map As StreetMap) As Image
        If MapRedrawRequired Then
            DrawMap(Map)
            MapRedrawRequired = False
        End If

        Dim OverlayBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)

        If ConfigDrawTrafficLayer Then
            Dim CC As New CoordinateConverter(Map.Bounds, Width, Height)
            For Each Way As Way In Map.WaysWithTraffic
                Dim TrafficIntensity As Double = Way.GetSpeedDifferenceAtTime(NoticeBoard.Time)
                Dim TrafficPen As New Pen(Color.FromArgb(Math.Min(TrafficIntensity * SimulationParameters.TrafficDisplayAlpha, 255), Color.Red), TRAFFIC_WAY_THICKNESS)
                Dim LastPoint As Point = CC.GetPoint(Way.Nodes(0))

                For i = 1 To Way.Nodes.Length - 1
                    Dim CurrentPoint As Point = CC.GetPoint(Way.Nodes(i))
                    grOverlay.DrawLine(TrafficPen, LastPoint, CurrentPoint)
                    LastPoint = CurrentPoint
                Next
            Next
        End If

        If ConfigDrawRoadDelayNodes Then
            Dim NodeDrawSize As Integer = If(ConfigDrawRoads = 2, SPECIAL_NODE_DRAW_SIZE_THICK, SPECIAL_NODE_DRAW_SIZE_THIN)
            Dim Len2 As Integer = NodeDrawSize \ 2
            For Each Node As Node In Map.Nodes
                If Node.RoadDelay > RoadDelay.UNEXPECTED Then
                    If IsDelayedAtTime(Node, Nothing, NoticeBoard.Time) Then
                        Dim Point As Point = CC.GetPoint(Node)
                        grOverlay.FillRectangle(NODE_ROAD_DELAY_BRUSH, Point.X - Len2, Point.Y - Len2, NodeDrawSize, NodeDrawSize)
                    End If
                End If
            Next
        End If

        For Each Agent As Agent In Agents
            DrawAgent(Agent, grOverlay)

            If Agent.Plan.Routes.Count = 0 Then
                Continue For
            End If

            If ConfigDrawAgentRoutes = 0 Then ' Lines for jobs, not for driving route
                Dim Brush As New SolidBrush(Agent.Color)
                Dim RoutePen As New Pen(New SolidBrush(Agent.Color), 3)
                Dim HalfRoutePen As New Pen(New SolidBrush(Agent.Color), 3)
                RoutePen.EndCap = Drawing2D.LineCap.ArrowAnchor
                HalfRoutePen.DashStyle = Drawing2D.DashStyle.Dash
                HalfRoutePen.DashCap = Drawing2D.DashCap.Triangle
                For Each WP As WayPoint In Agent.Plan.WayPoints
                    Dim Point As Point = CC.GetPoint(WP.Position)
                    DrawTriangle(grOverlay, Point, WP.VolumeDelta < 0)
                    If WP.Predecessor IsNot Nothing Then
                        Dim FromPoint As Point = CC.GetPoint(WP.Predecessor.Position)
                        If WP.Job.Status = JobStatus.PENDING_PICKUP Then
                            grOverlay.DrawLine(RoutePen, FromPoint, Point)
                        ElseIf WP.Job.Status = JobStatus.PENDING_DELIVERY Then
                            grOverlay.DrawLine(HalfRoutePen, FromPoint, Point)
                        End If
                    ElseIf Not WP.Job.OriginalDeliveryPosition.Equals(WP.Job.DeliveryPosition) Then
                        'If rerouted to depot
                        Dim FailedPoint As Point = CC.GetPoint(WP.Job.OriginalDeliveryPosition)
                        grOverlay.DrawLine(DELIVERY_FAIL_CROSS_PEN, FailedPoint.X - 5, FailedPoint.Y - 5, FailedPoint.X + 5, FailedPoint.Y + 5)
                        grOverlay.DrawLine(DELIVERY_FAIL_CROSS_PEN, FailedPoint.X - 5, FailedPoint.Y + 5, FailedPoint.X + 5, FailedPoint.Y - 5)
                        grOverlay.DrawLine(RoutePen, FailedPoint, Point)
                    End If
                Next
            ElseIf ConfigDrawAgentRoutes = 1 Then 'Simple straight lines for whole plan
                Dim RoutePen As New Pen(New SolidBrush(Agent.Color), 3)
                Dim LastPoint As Point = CC.GetPoint(Agent.Plan.RoutePosition.GetPoint)
                For Each WP As WayPoint In Agent.Plan.WayPoints
                    Dim Point As Point = CC.GetPoint(WP.Position)
                    grOverlay.DrawLine(RoutePen, LastPoint, Point)
                    LastPoint = Point
                Next
            ElseIf ConfigDrawAgentRoutes = 2 Then 'Complex, hop by hop routes for whole plan
                Dim PointRoute As New List(Of Point)
                For Each Route As Route In Agent.Plan.Routes
                    For Each Hop As Hop In Route.GetHopList
                        PointRoute.Add(CC.GetPoint(Hop.ToPoint))
                    Next
                Next
                Dim RoutePen As New Pen(New SolidBrush(Agent.Color), 3)
                grOverlay.DrawLines(RoutePen, PointRoute.ToArray)
            End If
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
