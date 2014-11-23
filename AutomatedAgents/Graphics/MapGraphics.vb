﻿Module MapGraphics
    Private MapBitmapOriginal As Bitmap
    Private MapBitmapOverlay As Bitmap

    Private BG_COLOR As Color = Color.White
    Private Const AGENT_DRAW_SIZE As Integer = 10
    Private CC As CoordinateConverter
    Private Const DRAW_NODES As Boolean = True
    Private Const DRAW_ROADS As Boolean = True
    Private OverlayFont As New Font("TimesNewRoman", 12)

    Private Width As Integer
    Private Height As Integer

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
        Dim Progress, ProgressMax As Integer

        If DRAW_NODES Then
            Progress = 0
            ProgressMax = Map.Nodes.Count
            For Each N As Node In Map.Nodes
                Dim Point As Point = CC.GetPoint(N)
                gr.DrawRectangle(Pens.Blue, Point.X, Point.Y, 1, 1)

                Progress += 1
                'pbLoad.Value = 100 * Progress / ProgressMax
            Next
        End If

        If DRAW_ROADS Then
            Progress = 0
            ProgressMax = Map.Ways.Count
            For Each W As Way In Map.Ways
                If W.Nodes.Length >= 2 Then
                    Dim LastPoint As Point = CC.GetPoint(W.Nodes(0))

                    For i = 1 To W.Nodes.Length - 1
                        Dim CurrentPoint As Point = CC.GetPoint(W.Nodes(i))
                        gr.DrawLine(Pens.Black, LastPoint, CurrentPoint)
                        LastPoint = CurrentPoint
                    Next
                End If
                Progress += 1
                'pbLoad.Value = 100 * Progress / ProgressMax
            Next
        End If

        MapBitmapOriginal.Save("out.bmp")
        Return MapBitmapOriginal.Clone
    End Function

    Sub DrawAgent(ByVal Agent As Agent, ByVal gr As Graphics)
        Dim Point As Point = CC.GetPoint(Agent.CurrentNode)
        Dim Brush As New SolidBrush(Agent.Color)
        gr.FillEllipse(Brush, CInt(Point.X - AGENT_DRAW_SIZE / 2), _
                       CInt(Point.Y - AGENT_DRAW_SIZE / 2), _
                       AGENT_DRAW_SIZE, AGENT_DRAW_SIZE)
    End Sub

    Function DrawHighlightedNode(ByVal Node As Node) As Image
        Dim NodePoint As Point = CC.GetPoint(Node)

        Dim OverlayBitmapCopy As Bitmap = MapBitmapOverlay.Clone
        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)
        grOverlay.DrawRectangle(Pens.Red, NodePoint.X - 5, NodePoint.Y - 5, 10, 10)
        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(OverlayBitmapCopy, 0, 0)
        OverlayBitmapCopy.Dispose()
        grMap.Dispose()
        grOverlay.Dispose()
        'picMap.Image.Dispose()
        Return MapBitmapCopy
    End Function

    Function DrawRouteStart(ByVal Node As Node) As Image
        Dim OneHop As New List(Of Hop)
        OneHop.Add(New Hop(Node, Node, Nothing))
        Return DrawRoute(OneHop)
    End Function

    Function DrawRoute(ByVal RouteHops As List(Of Hop)) As Image
        Dim grOverlay As Graphics = Graphics.FromImage(MapBitmapOverlay)

        Dim RouteFromNode As Node = RouteHops(0).FromNode
        Dim NodePoint As Point = CC.GetPoint(RouteFromNode)
        grOverlay.Clear(Color.Transparent)
        grOverlay.DrawRectangle(Pens.Red, NodePoint.X - 5, NodePoint.Y - 5, 10, 10)
        grOverlay.DrawString("FROM", OverlayFont, Brushes.Black, NodePoint)

        If RouteHops.Count > 1 Then
            Dim RouteToNode = RouteHops(RouteHops.Count - 1).ToNode
            NodePoint = CC.GetPoint(RouteToNode)
            grOverlay.DrawRectangle(Pens.Green, NodePoint.X - 5, NodePoint.Y - 5, 10, 10)

            For Each Hop As Hop In RouteHops
                Dim FromPoint As Point = CC.GetPoint(Hop.FromNode)
                Dim ToPoint As Point = CC.GetPoint(Hop.ToNode)
                Dim Pen As New Pen(Brushes.Gold, 3)
                grOverlay.DrawLine(Pen, FromPoint, ToPoint)
            Next
            grOverlay.DrawString("TO (" & RouteHops.Count & ")", OverlayFont, Brushes.Black, NodePoint)
        End If

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(MapBitmapOverlay, 0, 0)

        grOverlay.Dispose()
        'picMap.Image.Dispose()
        Return MapBitmapCopy

    End Function

    Function DrawAgents(ByVal Agents As List(Of Agent)) As Image
        Dim OverlayBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)
        For Each Agent As Agent In Agents
            DrawAgent(Agent, grOverlay)
        Next

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(OverlayBitmapCopy, 0, 0)
        OverlayBitmapCopy.Dispose()
        grMap.Dispose()
        grOverlay.Dispose()
        Return MapBitmapCopy
        'TODO: I removed pbmap.image.dispose, check for mem leak
    End Function

End Module
