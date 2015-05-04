Public Class frmMain
    Private Map As StreetMap
 
    Private Const LOAD_TSMI_TEXT_PREFIX As String = "Load "
    Private Const AGENT_TSMI_TEXT_PREFIX As String = "Add "
    Private AGENT_TSMI_AMOUNTS() As Integer = {1, 2, 3, 4, 5, 10, 50, 100, 500, 1000, 5000, 10000}

    Private AASimulation As AASimulation

    'On form load, add menu items to load each som file, spawn agents.
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        RNG.Initialise()
        Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.Highest
        bwSimulator.RunWorkerAsync()
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

        'Open instances of other forms.
        frmAgentStatus.Show()
        frmAgentStatus.Location = New Point(Me.Location.X, Me.Location.Y + Me.Height)
        frmParameters.Show()
        frmParameters.Location = New Point(Me.Location.X + Me.Width, Me.Location.Y)

    End Sub

    Private Sub frmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Map IsNot Nothing Then
            MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
            SetPictureBox(MapGraphics.DrawMap(Map))
        End If
    End Sub

    Private Sub LoadToolStripMenuItem_Click(sender As Object, e As EventArgs)
        CancelSimulation()

        Dim FileName As String = CType(sender, ToolStripMenuItem).Text.Replace(LOAD_TSMI_TEXT_PREFIX, "")
        FileName = OSMFileSystemManager.GetFilePathFromName(FileName)
        Dim Loader As OSMLoader = New OSMLoader(FileName)
        Map = Loader.CreateMap()
        MapGraphics.Resize(picMap.Width, picMap.Height, Map.Bounds)
        SetPictureBox(MapGraphics.DrawMap(Map))
        tmrRedraw.Start()
    End Sub

    Private Sub NodesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NodesToolStripMenuItem.Click
        MapGraphics.ConfigDrawBusinessNodes = NodesToolStripMenuItem.Checked
        If Map IsNot Nothing Then
            SetPictureBox(DrawMap(Map))
        End If
    End Sub

    Private Sub AgentRoutesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgentRoutesToolStripMenuItem.Click
        MapGraphics.ConfigDrawAgentLines = AgentRoutesToolStripMenuItem.Checked
    End Sub


    Private Sub AgentToolStripMenuItem_Click(sender As Object, e As EventArgs)
        If Map IsNot Nothing AndAlso AASimulation IsNot Nothing Then
            Dim Amount As Integer = CInt(CType(sender, ToolStripMenuItem).Text.Replace(AGENT_TSMI_TEXT_PREFIX, ""))
            SyncLock AASimulation
                For i = 1 To Amount
                    AASimulation.AddAgent()
                Next
            End SyncLock
        Else
            MsgBox("Map is not loaded or simulation has not started")
        End If
    End Sub

    Private Sub bwSimulator_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles bwSimulator.DoWork
        While True
            If AASimulation IsNot Nothing AndAlso AASimulation.IsRunning Then
                Dim StatisticsLogCounter As Integer = 0
                SelectionMode = MapSelectionMode.AGENTS_ALL_ROUTE_TO

                If AASimulation IsNot Nothing AndAlso AASimulation.IsRunning Then
                    Dim SimulationStateChanged As Boolean = False
                    Dim TickCounter As Integer = 0
                    Do
                        If AASimulation IsNot Nothing AndAlso AASimulation.IsRunning Then
                            SyncLock AASimulation
                                If AASimulation IsNot Nothing AndAlso AASimulation.IsRunning Then

                                    SimulationStateChanged = SimulationStateChanged Or AASimulation.Tick()
                                    If StatisticsLogCounter Mod SimulationParameters.StatisticsTickInterval = 0 Then
                                        AASimulation.LogStatistics()
                                    End If

                                    If TickCounter Mod SimulationParameters.DisplayRefreshSpeed = 0 AndAlso _
                                        Not PauseDisplayToolStripMenuItem.Checked Then
                                        If SimulationStateChanged Then
                                            SetPictureBox(MapGraphics.DrawOverlay(AASimulation.Agents, AASimulation.Map))
                                            SimulationState.CacheAASimulationStatus(AASimulation)
                                        End If
                                    End If
                                    TickCounter += 1
                                    StatisticsLogCounter += 1
                                End If
                            End SyncLock
                        End If
                    Loop Until TickCounter >= SimulationParameters.SimulationSpeed

                End If

            End If
            Dim SleepTime As Integer = 1000 / SimulationParameters.SimulationSpeed
            If SleepTime > 0 AndAlso Not PauseDisplayToolStripMenuItem.Checked Then
                Threading.Thread.Sleep(SleepTime)
            End If
        End While
    End Sub

    Private Sub tmrStatus_Tick(sender As Object, e As EventArgs) Handles tmrStatus.Tick
        ShowMemoryUsage()
        ShowTime()
        ShowDebugVariable()

        If AASimulation IsNot Nothing AndAlso AASimulation.Agents.Count > 0 Then
            frmAgentStatus.RefreshLists()
        End If

        If KeepRefreshingRoute AndAlso LastAStarEpsilon <> SimulationParameters.AStarAccelerator Then
            FindAndDisplayRoute()
        End If
    End Sub

    Private Sub CancelSimulation()
        KeepRefreshingRoute = False
        KeepRefreshingRouteToolStripMenuItem.Checked = False

        If AASimulation IsNot Nothing Then
            SyncLock AASimulation
                AASimulation = Nothing 'Cancels previous sims/playgrounds if any.
            End SyncLock
        End If
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
    Dim KeepRefreshingRoute As Boolean = False
    Dim LastAStarEpsilon As Double
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
                    RouteFromNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.ConnectedNodesGrid)
                    If RouteFromNode IsNot Nothing Then
                        SetPictureBox(MapGraphics.DrawRouteStart(RouteFromNode))
                        SelectionMode = MapSelectionMode.ROUTE_TO
                    End If
                Case MapSelectionMode.ROUTE_TO
                    FindAndDisplayRoute()
                Case MapSelectionMode.AGENTS_ALL_ROUTE_TO
                    If AASimulation IsNot Nothing AndAlso AASimulation.Agents IsNot Nothing Then
                        SyncLock AASimulation
                            RouteToNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.ConnectedNodesGrid)
                            For Each Agent In AASimulation.Agents
                                Agent.SetRouteTo(Map.NodesAdjacencyList.GetHopPositionFromNode(RouteToNode.ID))
                            Next
                        End SyncLock
                    End If
            End Select
        End If
    End Sub
    Sub FindAndDisplayRoute()
        If RouteFromNode IsNot Nothing Then
            LastAStarEpsilon = SimulationParameters.AStarAccelerator
            Dim CC As New CoordinateConverter(Map.Bounds, picMap.Width, picMap.Height)
            RouteToNode = CC.GetNearestNodeFromPoint(MapMousePosition, Map.ConnectedNodesGrid)
            If Not RouteFromNode.Equals(RouteToNode) Then
                Dim RouteFinder As IRouteFinder = New AStarSearch(RouteFromNode, RouteToNode, _
                                                                  Map.NodesAdjacencyList, _
                                                                  SimulationParameters.RouteFindingMinimiser, _
                                                                  TimeSpan.FromHours(12))
                If RouteFinder.GetRoute() IsNot Nothing Then
                    SetPictureBox(MapGraphics.DrawRoute(RouteFinder.GetRoute, RouteFinder.GetNodesSearched))
                Else
                    SetPictureBox(MapGraphics.DrawQuestionMark(MapMousePosition))
                End If
                SelectionMode = MapSelectionMode.NONE
            End If
        End If

    End Sub
    'Highlights nodes on mouse over, if selection mode is on
    Private Sub picMap_MouseMove(sender As Object, e As MouseEventArgs) Handles picMap.MouseMove
        If SelectionMode = MapSelectionMode.ROUTE_FROM Or SelectionMode = MapSelectionMode.ROUTE_TO Then
            MapMousePosition = e.Location
            Dim CC As New CoordinateConverter(Map.Bounds, picMap.Width, picMap.Height)
            Dim Node As Node = CC.GetNearestNodeFromPoint(MapMousePosition, Map.ConnectedNodesGrid)
            If Node IsNot Nothing Then
                SetPictureBox(DrawHighlightedNode(Node, MapMousePosition))
            End If
        End If
    End Sub

    Private Sub SetPictureBox(ByVal NewImage As Image)
        Try
            'Without this the garbage collector has too much garbage. :(
            If picMap.Image IsNot Nothing Then
                picMap.Image.Dispose()
            End If
            picMap.Image = NewImage.Clone
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try
    End Sub

    Private Sub ShowDebugVariable()
        If SimulationParameters.DisplayedDebugVariable IsNot Nothing Then
            lblDebugVariable.Text = "Variable: " & SimulationParameters.DisplayedDebugVariable.ToString
        End If
    End Sub
    Private Sub ShowMemoryUsage()
        Dim Bytes As Long = Process.GetCurrentProcess.WorkingSet64
        Dim KB As Long = Bytes / 1024
        lblLoadStatus.Text = FormatNumber(KB, 0) & " K"
    End Sub
    Private Sub ShowTime()
        lblTime.Text = "Time: " & If(AASimulation IsNot Nothing, AASimulation.GetTimeString, "n/a")
    End Sub

    Private Sub StartToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StartSimulationToolStripMenuItem.Click
        If Map IsNot Nothing Then
            If AASimulation Is Nothing Then
                AASimulation = New AACourierSimulation(Map)
            ElseIf AASimulation.GetType <> GetType(AACourierSimulation) Then
                CancelSimulation()
                AASimulation = New AACourierSimulation(Map)
            End If
            AASimulation.Start()
            frmStatistics.SetAASimulation(AASimulation)
        End If
    End Sub

    Private Sub StartPlaygroundToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StartPlaygroundToolStripMenuItem.Click
        If Map IsNot Nothing Then
            If AASimulation Is Nothing Then
                AASimulation = New AAPlayground(Map)
            ElseIf AASimulation.GetType <> GetType(AAPlayground) Then
                CancelSimulation()
                AASimulation = New AAPlayground(Map)
            End If
            AASimulation.Start()
            frmStatistics.SetAASimulation(AASimulation)
        End If
    End Sub

    Private Sub StopToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StopToolStripMenuItem.Click
        If AASimulation IsNot Nothing Then
            SyncLock AASimulation
                AASimulation.Pause()
            End SyncLock
        End If
    End Sub

    Private Sub ResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem.Click
        CancelSimulation()
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
        frmParameters.Show()
    End Sub

    Private Sub BenchmarkToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BenchmarkToolStripMenuItem.Click
        If Map IsNot Nothing AndAlso Map.NodesAdjacencyList IsNot Nothing Then
            RouteFinderBenchmark.RunRouteFinderBenchmark(Map.NodesAdjacencyList)
        End If
    End Sub

    Private Sub ViewConsoleToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ViewConsoleToolStripMenuItem.Click
        frmAgentStatus.Show()
    End Sub
  
    Private Sub AgentPlansToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AgentPlansToolStripMenuItem.Click
        MapGraphics.ConfigDrawAgentRoutes = -1
        JobViewToolStripMenuItem.Checked = False
        LineViewToolStripMenuItem.Checked = False
        RouteFromToolStripMenuItem.Checked = False
    End Sub
    Private Sub JobViewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles JobViewToolStripMenuItem.Click
        MapGraphics.ConfigDrawAgentRoutes = If(JobViewToolStripMenuItem.Checked, 0, -1)
        LineViewToolStripMenuItem.Checked = False
        RouteViewToolStripMenuItem.Checked = False
    End Sub
    Private Sub LineViewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LineViewToolStripMenuItem.Click
        MapGraphics.ConfigDrawAgentRoutes = If(LineViewToolStripMenuItem.Checked, 1, -1)
        JobViewToolStripMenuItem.Checked = False
        RouteViewToolStripMenuItem.Checked = False
    End Sub
    Private Sub RouteViewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RouteViewToolStripMenuItem.Click
        MapGraphics.ConfigDrawAgentRoutes = If(RouteViewToolStripMenuItem.Checked, 2, -1)
        JobViewToolStripMenuItem.Checked = False
        LineViewToolStripMenuItem.Checked = False
    End Sub

    Private Sub LandmarksToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LandmarksToolStripMenuItem.Click
        MapGraphics.ConfigDrawLandmarks = LandmarksToolStripMenuItem.Checked
        SetPictureBox(MapGraphics.DrawMap(Map))
    End Sub

    Private Sub ViewStatisticsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ViewStatisticsToolStripMenuItem.Click
        frmStatistics.Show()
        If AASimulation IsNot Nothing Then
            frmStatistics.SetAASimulation(AASimulation)
        End If
    End Sub
    Private Sub GridToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GridToolStripMenuItem.Click
        MapGraphics.ConfigDrawGrid = GridToolStripMenuItem.Checked
        SetPictureBox(MapGraphics.DrawMap(Map))
    End Sub
    Private Sub RoadDelayNodesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RoadDelayNodesToolStripMenuItem.Click
        MapGraphics.ConfigDrawRoadDelayNodes = RoadDelayNodesToolStripMenuItem.Checked
        SetPictureBox(MapGraphics.DrawMap(Map))
    End Sub
    Private Sub TrafficToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TrafficToolStripMenuItem.Click
        MapGraphics.ConfigDrawTrafficLayer = TrafficToolStripMenuItem.Checked
    End Sub

    Private Sub CNP1ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CNP1ToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 0
        SimulationParameters.CNPVersion = ContractNetPolicy.CNP1
    End Sub
    Private Sub CNP2ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CNP2ToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 0
        SimulationParameters.CNPVersion = ContractNetPolicy.CNP2
    End Sub
    Private Sub CNP3ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CNP3ToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 0
        SimulationParameters.CNPVersion = ContractNetPolicy.CNP3
    End Sub
    Private Sub CNP4ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CNP4ToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 0
        SimulationParameters.CNPVersion = ContractNetPolicy.CNP4
    End Sub
    Private Sub CNP5ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CNP5ToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 0
        SimulationParameters.CNPVersion = ContractNetPolicy.CNP5
    End Sub
    Private Sub FreeforallToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FreeforallToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 1
    End Sub
    Private Sub RoundRobinToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RoundRobinToolStripMenuItem.Click
        SimulationParameters.RoutingStrategy = 2
    End Sub
    Private Sub NoneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NoneToolStripMenuItem.Click
        SimulationParameters.IdleStrategy = 0
    End Sub
    Private Sub SleepToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SleepToolStripMenuItem.Click
        SimulationParameters.IdleStrategy = 1
    End Sub
    Private Sub PredictiveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PredictiveToolStripMenuItem.Click
        SimulationParameters.IdleStrategy = 2
    End Sub
    Private Sub ScatterToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScatterToolStripMenuItem.Click
        SimulationParameters.IdleStrategy = 3
    End Sub
    Private Sub CarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CarToolStripMenuItem.Click
        SimulationParameters.VehicleType = Vehicles.Type.CAR
    End Sub
    Private Sub SmallCommercialVanToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SmallCommercialVanToolStripMenuItem.Click
        SimulationParameters.VehicleType = Vehicles.Type.VAN
    End Sub
    Private Sub Lorry75TonneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Lorry75TonneToolStripMenuItem.Click
        SimulationParameters.VehicleType = Vehicles.Type.TRUCK
    End Sub
    Private Sub DeliverToDepotToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeliverToDepotToolStripMenuItem.Click
        SimulationParameters.FailToDepot = True
    End Sub
    Private Sub DeliverToPickupToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeliverToPickupToolStripMenuItem.Click
        SimulationParameters.FailToDepot = False
    End Sub
    Private Sub GeneralToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GeneralToolStripMenuItem.Click
        SimulationParameters.Dispatcher = 0
    End Sub
    Private Sub SingleBusinessToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SingleBusinessToolStripMenuItem.Click
        SimulationParameters.Dispatcher = 1
    End Sub

    Private Sub DistanceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DistanceToolStripMenuItem.Click
        SimulationParameters.RouteFindingMinimiser = RouteFindingMinimiser.DISTANCE
        FindAndDisplayRoute()
    End Sub

    Private Sub TimeWith8AMTrafficToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TimeWith8AMTrafficToolStripMenuItem.Click
        SimulationParameters.RouteFindingMinimiser = RouteFindingMinimiser.TIME_WITH_TRAFFIC
        FindAndDisplayRoute()
    End Sub

    Private Sub TimeWithoutTrafficToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TimeWithoutTrafficToolStripMenuItem.Click
        SimulationParameters.RouteFindingMinimiser = RouteFindingMinimiser.TIME_NO_TRAFFIC
        FindAndDisplayRoute()
    End Sub

    Private Sub FuelWith8AMTrafficToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FuelWith8AMTrafficToolStripMenuItem.Click
        SimulationParameters.RouteFindingMinimiser = RouteFindingMinimiser.FUEL_WITH_TRAFFIC
        FindAndDisplayRoute()
    End Sub

    Private Sub FuelWithoutTrafficToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FuelWithoutTrafficToolStripMenuItem.Click
        SimulationParameters.RouteFindingMinimiser = RouteFindingMinimiser.FUEL_NO_TRAFFIC
        FindAndDisplayRoute()
    End Sub

    Private Sub KeepRefreshingRouteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles KeepRefreshingRouteToolStripMenuItem.Click
        KeepRefreshingRoute = KeepRefreshingRouteToolStripMenuItem.Checked
    End Sub
End Class
