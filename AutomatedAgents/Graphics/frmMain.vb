Public Class frmMain
    Private Map As StreetMap
 
    Private Const LOAD_TSMI_TEXT_PREFIX As String = "Load "
    Private Const AGENT_TSMI_TEXT_PREFIX As String = "Add "
    Private AGENT_TSMI_AMOUNTS() As Integer = {1, 5, 10, 50, 100, 500, 1000, 5000, 10000}

    Private AASimulation As New AASimulation

    'On form load, add menu items to load each som file, spawn agents.
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        Randomize()
        For Each File As String In OSMFileSystemManager.GetAllFilenames()
            Dim LoadMenuItem As New ToolStripMenuItem(LOAD_TSMI_TEXT_PREFIX & File)
            AddHandler LoadMenuItem.Click, AddressOf LoadToolStripMenuItem_Click
            FileToolStripMenuItem.DropDownItems.Add(LoadMenuItem)
        Next

        For Each AgentAmount As Integer In AGENT_TSMI_AMOUNTS
            Dim AgentMenuItem As New ToolStripMenuItem(AGENT_TSMI_TEXT_PREFIX & AgentAmount)
            AddHandler AgentMenuItem.Click, AddressOf AgentToolStripMenuItem_Click
            AgentsToolStripMenuItem.DropDownItems.Add(AgentMenuItem)
        Next
    End Sub

    Private Sub frmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Map IsNot Nothing Then
            MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
            SetPictureBox(MapGraphics.DrawMap(Map))
        End If
    End Sub

    Private Sub LoadToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim FileName As String = CType(sender, ToolStripMenuItem).Text.Replace(LOAD_TSMI_TEXT_PREFIX, "")
        FileName = OSMFileSystemManager.GetFilePathFromName(FileName)
        Dim Loader As OSMLoader = New OSMLoader(FileName)
        Map = Loader.CreateMap()
        AASimulation = New AASimulation()
        MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
        SetPictureBox(MapGraphics.DrawMap(Map))
        tmrAgents.Start()
    End Sub

    Private Sub NodesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NodesToolStripMenuItem.Click
        MapGraphics.ConfigDrawNodes = NodesToolStripMenuItem.Checked
        If Map IsNot Nothing Then
            SetPictureBox(DrawMap(Map))
        End If
    End Sub

    Private Sub AgentRoutesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgentRoutesToolStripMenuItem.Click
        MapGraphics.ConfigDrawAgentLines = AgentRoutesToolStripMenuItem.Checked
    End Sub


    Private Sub AgentToolStripMenuItem_Click(sender As Object, e As EventArgs)
        If Map IsNot Nothing Then
            Dim Amount As Integer = CInt(CType(sender, ToolStripMenuItem).Text.Replace(AGENT_TSMI_TEXT_PREFIX, ""))
            For i = 1 To Amount
                AASimulation.AddAgent(Map)
            Next
        Else
            MsgBox("Map is not loaded")
        End If
    End Sub


    Private Sub tmrAgents_Tick(sender As Object, e As EventArgs) Handles tmrAgents.Tick
        If AASimulation.IsRunning Then
            SelectionMode = MapSelectionMode.AGENTS_ALL_ROUTE_TO
            Dim SimulationStateChanged As Boolean = False
            For i = 1 To Math.Max(1, SimulationParameters.SimulationSpeed / (1000 / SimulationParameters.DisplayRefreshSpeed))
                SimulationStateChanged = SimulationStateChanged Or AASimulation.Tick()
            Next
            SetPictureBox(MapGraphics.DrawAgents(AASimulation.Agents))
        End If
        tmrAgents.Interval = SimulationParameters.DisplayRefreshSpeed
    End Sub
    Private Sub tmrStatus_Tick(sender As Object, e As EventArgs) Handles tmrStatus.Tick
        ShowMemoryUsage()
        ShowTime()
        ShowDebugVariable()
    End Sub

    Enum MapSelectionMode
        NONE
        ROUTE_FROM
        ROUTE_TO
        AGENTS_ALL_ROUTE_TO
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
        If Map IsNot Nothing Then
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
                    Dim RouteFinder As RouteFinder = New AStarSearch(RouteFromNode, RouteToNode, Map.NodesAdjacencyList)
                    If RouteFinder.GetRoute() IsNot Nothing Then
                        SetPictureBox(MapGraphics.DrawRoute(RouteFinder.GetRoute, RouteFinder.GetNodesSearched))
                    Else
                        SetPictureBox(MapGraphics.DrawQuestionMark(MapMousePosition))
                    End If
                    SelectionMode = MapSelectionMode.NONE
                Case MapSelectionMode.AGENTS_ALL_ROUTE_TO
                    RouteToNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.NodesAdjacencyList)
                    For Each Agent In AASimulation.Agents
                        Agent.SetRouteTo(RouteToNode)
                    Next
            End Select
        End If
    End Sub

    'Highlights nodes on mouse over, if selection mode is on
    Private Sub picMap_MouseMove(sender As Object, e As MouseEventArgs) Handles picMap.MouseMove
        MapMousePosition = e.Location
        If SelectionMode = MapSelectionMode.ROUTE_FROM Or SelectionMode = MapSelectionMode.ROUTE_TO Then
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

    Private Sub ShowDebugVariable()
        If dEbUgVaRiAbLe IsNot Nothing Then
            lblDebugVariable.Text = "Variable: " & dEbUgVaRiAbLe.ToString
        End If
    End Sub
    Private Sub ShowMemoryUsage()
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

    Private Sub ThinRoadsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ThinRoadsToolStripMenuItem.Click
        If ThinRoadsToolStripMenuItem.Checked Then
            MapGraphics.ConfigDrawRoads = 1
            ThickRoadsToolStripMenuItem.Checked = False
        ElseIf Not ThickRoadsToolStripMenuItem.Checked Then
            MapGraphics.ConfigDrawRoads = 0
        End If

        If Map IsNot Nothing Then
            SetPictureBox(DrawMap(Map))
        End If
    End Sub

    Private Sub ThickRoadsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ThickRoadsToolStripMenuItem.Click
        If ThickRoadsToolStripMenuItem.Checked Then
            MapGraphics.ConfigDrawRoads = 2
            ThinRoadsToolStripMenuItem.Checked = False
        ElseIf Not ThinRoadsToolStripMenuItem.Checked Then
            MapGraphics.ConfigDrawRoads = 0
        End If

        If Map IsNot Nothing Then
            SetPictureBox(DrawMap(Map))
        End If
    End Sub

    Private Sub SpeedToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SpeedToolStripMenuItem.Click
        frmSimulationSpeed.Show()
    End Sub

    Private Sub BenchmarkToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BenchmarkToolStripMenuItem.Click
        If Map.NodesAdjacencyList IsNot Nothing Then
            RouteFinderBenchmark.RunRouteFinderBenchmark(Map.NodesAdjacencyList)
        End If

    End Sub
End Class
