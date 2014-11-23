Public Class frmMain
    Private Map As StreetMap
 
    Private Const OSM_FOLDER As String = "C:\Users\Jake\Downloads\"
    Private Const LOAD_TSMI_TEXT_PREFIX As String = "Load "
    Private Const AGENT_TSMI_TEXT_PREFIX As String = "Add "
    Private AGENT_TSMI_AMOUNTS() As Integer = {1, 5, 10, 50, 100, 500, 1000, 5000, 10000}

    Private AASimulation As New AASimulation

    'On form load, add menu items to load each som file, spawn agents.
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
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

        Randomize()
    End Sub

    Private Sub frmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Map IsNot Nothing Then
            MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
            SetPictureBox(MapGraphics.DrawMap(Map))
        End If
    End Sub

    Private Sub LoadToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim FileName As String = OSM_FOLDER & CType(sender, ToolStripMenuItem).Text.Replace(LOAD_TSMI_TEXT_PREFIX, "")
        Dim Loader As OSMLoader = New OSMLoader(FileName)
        Map = Loader.CreateMap()
        AASimulation = New AASimulation()
        MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
        SetPictureBox(MapGraphics.DrawMap(Map))
        tmrAgents.Start()
    End Sub

    Private Sub NodesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NodesToolStripMenuItem.Click
        MapGraphics.DRAW_NODES = NodesToolStripMenuItem.Checked
        SetPictureBox(DrawMap(Map))
    End Sub

    Private Sub RoadsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RoadsToolStripMenuItem.Click
        MapGraphics.DRAW_ROADS = RoadsToolStripMenuItem.Checked
        SetPictureBox(DrawMap(Map))
    End Sub


    Private Sub AgentToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim Amount As Integer = CInt(CType(sender, ToolStripMenuItem).Text.Replace(AGENT_TSMI_TEXT_PREFIX, ""))
        For i = 1 To Amount
            AASimulation.AddAgent(Map)
        Next
    End Sub


    Private Sub tmrAgents_Tick(sender As Object, e As EventArgs) Handles tmrAgents.Tick
        If AASimulation.IsRunning Then
            If AASimulation.Tick() Then
                SetPictureBox(MapGraphics.DrawAgents(AASimulation.Agents))
            End If
        End If
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
        Dim CC As New CoordinateConverter(Map.Bounds, picMap.Width, picMap.Height)
        Select Case SelectionMode
            Case MapSelectionMode.NONE
                Exit Sub
            Case MapSelectionMode.ROUTE_FROM
                RouteFromNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
                SetPictureBox(MapGraphics.DrawRouteStart(RouteFromNode))
                SelectionMode = MapSelectionMode.ROUTE_TO
            Case MapSelectionMode.ROUTE_TO
                RouteToNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
                Dim RouteFinder As RouteFinder = New BreadthFirstSearch(RouteFromNode, RouteToNode, Map.NodesAdjacencyList)
                SetPictureBox(MapGraphics.DrawRoute(RouteFinder.GetRoute))
                SelectionMode = MapSelectionMode.NONE
        End Select
    End Sub

    'Highlights nodes on mouse over, if selection mode is on
    Private Sub picMap_MouseMove(sender As Object, e As MouseEventArgs) Handles picMap.MouseMove
        If SelectionMode <> MapSelectionMode.NONE Then
            MapMousePosition = e.Location
            Dim CC As New CoordinateConverter(Map.Bounds, picMap.Width, picMap.Height)
            Dim Node As Node = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
            SetPictureBox(DrawHighlightedNode(Node, MapMousePosition))
        End If
    End Sub

    'Without this the garbage collector has too much garbage. :(
    Private Sub SetPictureBox(ByVal NewImage As Image)
        If picMap.Image IsNot Nothing Then
            picMap.Image.Dispose()
        End If
        picMap.Image = NewImage
    End Sub

    Sub ShowMemoryUsage()
        Dim Bytes As Long = Process.GetCurrentProcess.WorkingSet64
        Dim KB As Long = Bytes / 1024
        lblLoadStatus.Text = FormatNumber(KB, 0) & " K"
    End Sub
    Private Sub ShowTime()
        lblTime.Text = "Time: " & AASimulation.GetSimulationTime
    End Sub

    Private Sub StartToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StartToolStripMenuItem.Click
        AASimulation.StartSimulation()
    End Sub

    Private Sub StopToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StopToolStripMenuItem.Click
        AASimulation.StopSimulation()
    End Sub

    Private Sub ResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem.Click
        AASimulation = New AASimulation()
        SetPictureBox(MapGraphics.DrawMap(Map))
    End Sub
End Class
