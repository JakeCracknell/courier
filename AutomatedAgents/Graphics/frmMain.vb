Public Class frmMain
    Private Map As StreetMap
 
    Private Time As TimeSpan
    Private TIME_INCREMENT As TimeSpan = TimeSpan.FromSeconds(1)
    Private Const OSM_FOLDER As String = "C:\Users\Jake\Downloads\"
    Private Const LOAD_TSMI_TEXT_PREFIX As String = "Load "
    Private Const AGENT_TSMI_TEXT_PREFIX As String = "Add "
    Private AGENT_TSMI_AMOUNTS() As Integer = {1, 5, 10, 50, 100, 500, 1000, 5000, 10000}


    Private Agents As New List(Of Agent)

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
            picMap.Image = MapGraphics.DrawMap(Map)
        End If
    End Sub

    Private Sub LoadToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim File As String = OSM_FOLDER & CType(sender, ToolStripMenuItem).Text.Replace(LOAD_TSMI_TEXT_PREFIX, "")
        Dim Loader As OSMLoader = New OSMLoader(File)
        Map = Loader.CreateMap()
        Agents.Clear()
        MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
        picMap.Image = MapGraphics.DrawMap(Map)
        tmrAgents.Start()
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
        For Each Agent As Agent In Agents
            Agent.MoveRandomly()
        Next

        picMap.Image = MapGraphics.DrawAgents(Agents)
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
                picMap.Image = MapGraphics.DrawRouteStart(RouteFromNode)
                SelectionMode = MapSelectionMode.ROUTE_TO
            Case MapSelectionMode.ROUTE_TO
                RouteToNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
                Dim RouteFinder As RouteFinder = New BreadthFirstSearch(RouteFromNode, RouteToNode, Map.NodesAdjacencyList)
                picMap.Image = MapGraphics.DrawRoute(RouteFinder.GetRoute)
                SelectionMode = MapSelectionMode.NONE
        End Select
    End Sub

    'Highlights nodes on mouse over, if selection mode is on
    Private Sub picMap_MouseMove(sender As Object, e As MouseEventArgs) Handles picMap.MouseMove
        If SelectionMode <> MapSelectionMode.NONE Then
            MapMousePosition = e.Location
            Dim CC As New CoordinateConverter(Map.Bounds, picMap.Width, picMap.Height)
            Dim DrawNode As Node = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
            lblLoadStatus.Text = DrawNode.Latitude & "," & DrawNode.Longitude
            picMap.Image = DrawHighlightedNode(DrawNode)
        End If
    End Sub
End Class
