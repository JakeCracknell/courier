Public Class Form1
    Private Map As StreetMap
    Private MapBitmapOriginal As Bitmap
    Private MapBitmapOverlay As Bitmap
    Private BG_COLOR As Color = Color.White
    Private Const AGENT_DRAW_SIZE As Integer = 10
    Private CC As CoordinateConverter
    Private Time As TimeSpan
    Private TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)
    Private Const OSM_FOLDER As String = "C:\Users\Jake\Downloads\"
    Private Const LOAD_TSMI_TEXT_PREFIX As String = "Load "
    Private Const AGENT_TSMI_TEXT_PREFIX As String = "Add "
    Private AGENT_TSMI_AMOUNTS() As Integer = {1, 5, 10, 50, 100, 500, 1000, 5000, 10000}
    Private Const DRAW_NODES As Boolean = True
    Private Const DRAW_ROADS As Boolean = True
    Private OverlayFont As New Font("TimesNewRoman", 12)

    Private Agents As New List(Of Agent)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim FolderToScan As New IO.DirectoryInfo(OSM_FOLDER)
        Dim OSMFiles As IO.FileInfo() = FolderToScan.GetFiles("*.osm")

        For Each OSMFile As IO.FileInfo In OSMFiles
            Dim Filename As String = OSMFile.Name
            Dim LoadMenuItem As New ToolStripMenuItem(LOAD_TSMI_TEXT_PREFIX & Filename)
            AddHandler LoadMenuItem.Click, AddressOf LoadToolStripMenuItem_Click
            FileToolStripMenuItem.DropDownItems.Add(LoadMenuItem)
        Next

        For Each AgentAmount As Integer In AGENT_TSMI_AMOUNTS
            Dim AgentMenuItem As New ToolStripMenuItem(AGENT_TSMI_TEXT_PREFIX & AgentAmount)
            AddHandler AgentMenuItem.Click, AddressOf AgentToolStripMenuItem_Click
            AgentsToolStripMenuItem.DropDownItems.Add(AgentMenuItem)
        Next

    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Map IsNot Nothing Then
            CC = New CoordinateConverter(Map.Bounds)
            CC.SetPanel(picMap.Width, picMap.Height)
            DrawMap()
        End If
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Randomize()
        'gr = pMap.CreateGraphics
        'gr.Clear(BG_COLOR)
    End Sub

    Private Sub LoadToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim File As String = OSM_FOLDER & CType(sender, ToolStripMenuItem).Text.Replace(LOAD_TSMI_TEXT_PREFIX, "")
        Dim Loader As OSMLoader = New OSMLoader(File)
        Map = Loader.CreateMap()
        Agents.Clear()
        CC = New CoordinateConverter(Map.Bounds)
        CC.SetPanel(picMap.Width, picMap.Height)
        DrawMap()
        tmrAgents.Start()
    End Sub

    Sub DrawMap()
        MapBitmapOverlay = New Bitmap(picMap.Width, picMap.Height)
        Dim gr As Graphics = Graphics.FromImage(MapBitmapOverlay)
        gr.Clear(Color.Transparent)

        MapBitmapOriginal = New Bitmap(picMap.Width, picMap.Height)
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
                pbLoad.Value = 100 * Progress / ProgressMax
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
                pbLoad.Value = 100 * Progress / ProgressMax
            Next
        End If

        MapBitmapOriginal.Save("out.bmp")
        picMap.Image = MapBitmapOriginal.Clone
    End Sub

    Sub DrawAgent(ByVal Agent As Agent, ByVal gr As Graphics)
        Dim Point As Point = CC.GetPoint(Agent.CurrentNode)
        Dim Brush As New SolidBrush(Agent.Color)
        gr.FillEllipse(Brush, CInt(Point.X - AGENT_DRAW_SIZE / 2), _
                       CInt(Point.Y - AGENT_DRAW_SIZE / 2), _
                       AGENT_DRAW_SIZE, AGENT_DRAW_SIZE)
    End Sub

    Sub DrawRoute(ByVal Hops As List(Of Hop), ByVal gr As Graphics)
        For Each Hop As Hop In Hops
            Dim FromPoint As Point = CC.GetPoint(Hop.FromNode)
            Dim ToPoint As Point = CC.GetPoint(Hop.ToNode)
            Dim Pen As New Pen(Brushes.Gold, 3)
            gr.DrawLine(Pen, FromPoint, ToPoint)
        Next
    End Sub

    Sub ShowMemoryUsage()
        Dim Bytes As Long = Process.GetCurrentProcess.WorkingSet64
        Dim KB As Long = Bytes / 1024
        lblLoadStatus.Text = KB & " K"
    End Sub
    Private Sub ShowTime()
        lblTime.Text = Time.ToString
    End Sub

    Sub AddAgent()
        Agents.Add(New Agent(Map, GetRandomColor))
    End Sub

    Private Sub AgentToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim Amount As Integer = CInt(CType(sender, ToolStripMenuItem).Text.Replace(AGENT_TSMI_TEXT_PREFIX, ""))
        For i = 1 To Amount
            AddAgent()
        Next
    End Sub


    Private Sub tmrAgents_Tick(sender As Object, e As EventArgs) Handles tmrAgents.Tick
        Time = Time.Add(TIME_INCREMENT)
        If Agents.Count = 0 Then
            Exit Sub
        End If

        Dim OverlayBitmapCopy As Bitmap = MapBitmapOriginal.Clone

        Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)
        For Each Agent As Agent In Agents
            Agent.MoveRandomly()
            DrawAgent(Agent, grOverlay)
        Next

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(OverlayBitmapCopy, 0, 0)
        OverlayBitmapCopy.Dispose()
        grMap.Dispose()
        grOverlay.Dispose()
        picMap.Image.Dispose()
        picMap.Image = MapBitmapCopy

    End Sub
    Private Sub tmrStatus_Tick(sender As Object, e As EventArgs) Handles tmrStatus.Tick
        ShowMemoryUsage()
        ShowTime()
    End Sub

    Enum MapSelectionMode
        NONE
        ROUTE_FROM
        ROUTE_TO
    End Enum
    Dim SelectionMode As MapSelectionMode
    Dim RouteFromNode As Node
    Dim RouteToNode As Node
    Dim MapMousePosition As Point
    Private Sub RouteFromToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RouteFromToolStripMenuItem.Click
        SelectionMode = MapSelectionMode.ROUTE_FROM
        RouteFromNode = Nothing
        RouteToNode = Nothing
    End Sub

    Private Sub picMap_Click(sender As Object, e As EventArgs) Handles picMap.Click
        Dim grOverlay As Graphics = Graphics.FromImage(MapBitmapOverlay)

        Select Case SelectionMode
            Case MapSelectionMode.NONE
                Exit Sub
            Case MapSelectionMode.ROUTE_FROM
                RouteFromNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
                Dim NodePoint As Point = CC.GetPoint(RouteFromNode)
                grOverlay.Clear(Color.Transparent)
                grOverlay.DrawRectangle(Pens.Red, NodePoint.X - 5, NodePoint.Y - 5, 10, 10)
                grOverlay.DrawString("FROM", OverlayFont, Brushes.Black, NodePoint)
                SelectionMode = MapSelectionMode.ROUTE_TO
            Case MapSelectionMode.ROUTE_TO
                RouteToNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
                Dim NodePoint As Point = CC.GetPoint(RouteToNode)
                grOverlay.DrawRectangle(Pens.Green, NodePoint.X - 5, NodePoint.Y - 5, 10, 10)

                Dim RouteFinder As RouteFinder = New BreadthFirstSearch(RouteFromNode, RouteToNode, Map.NodesAdjacencyList)
                DrawRoute(RouteFinder.GetRoute, grOverlay)
                grOverlay.DrawString("TO (" & RouteFinder.GetCost & ")", OverlayFont, Brushes.Black, NodePoint)

                SelectionMode = MapSelectionMode.NONE
        End Select

        Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
        Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
        grMap.DrawImage(MapBitmapOverlay, 0, 0)

        grOverlay.Dispose()
        picMap.Image.Dispose()
        picMap.Image = MapBitmapCopy
    End Sub

    'Highlights nodes on mouse over, if selection mode is on
    Private Sub picMap_MouseMove(sender As Object, e As MouseEventArgs) Handles picMap.MouseMove
        If SelectionMode <> MapSelectionMode.NONE Then
            MapMousePosition = e.Location

            Dim OverlayBitmapCopy As Bitmap = MapBitmapOverlay.Clone
            Dim grOverlay As Graphics = Graphics.FromImage(OverlayBitmapCopy)
            Dim DrawNode As Node = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
            lblLoadStatus.Text = DrawNode.Latitude & "," & DrawNode.Longitude
            If DrawNode IsNot Nothing Then
                Dim NodePoint As Point = CC.GetPoint(DrawNode)
                grOverlay.DrawRectangle(Pens.Red, NodePoint.X - 5, NodePoint.Y - 5, 10, 10)
            End If

            Dim MapBitmapCopy As Bitmap = MapBitmapOriginal.Clone
            Dim grMap As Graphics = Graphics.FromImage(MapBitmapCopy)
            grMap.DrawImage(OverlayBitmapCopy, 0, 0)
            OverlayBitmapCopy.Dispose()
            grMap.Dispose()
            grOverlay.Dispose()
            picMap.Image.Dispose()
            picMap.Image = MapBitmapCopy
        End If
    End Sub
End Class
