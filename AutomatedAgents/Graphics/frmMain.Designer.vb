<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.pbLoad = New System.Windows.Forms.ToolStripProgressBar()
        Me.lblLoadStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblTime = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblDebugVariable = New System.Windows.Forms.ToolStripStatusLabel()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AgentsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectStrategyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CNP1ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CNP2ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CNP3ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CNP4ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CNP5ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FreeforallToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RoundRobinToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectIdleStrategyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NoneToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SleepToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PredictiveToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ScatterToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectVehicleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SmallCommercialVanToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Lorry75TonneToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectFailurePointToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeliverToDepotToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeliverToPickupToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectDispatcherToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GeneralToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SingleBusinessToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RouteTestingToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RouteFromToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BenchmarkToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NodesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RoadDelayNodesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RoadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ThinRoadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ThickRoadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TrafficToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AgentRoutesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AgentPlansToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.JobViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LineViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RouteViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LandmarksToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GridToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PauseDisplayToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SimulationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StartSimulationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StartPlaygroundToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SpeedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ViewStatisticsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ViewConsoleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.tmrRedraw = New System.Windows.Forms.Timer(Me.components)
        Me.picMap = New System.Windows.Forms.PictureBox()
        Me.tmrStatus = New System.Windows.Forms.Timer(Me.components)
        Me.bwSimulator = New System.ComponentModel.BackgroundWorker()
        Me.MinimiserToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DistanceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TimeWith8AMTrafficToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TimeWithoutTrafficToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FuelWith8AMTrafficToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FuelWithoutTrafficToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KeepRefreshingRouteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StatusStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.picMap, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.pbLoad, Me.lblLoadStatus, Me.lblTime, Me.lblDebugVariable})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 438)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1099, 22)
        Me.StatusStrip1.TabIndex = 0
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'pbLoad
        '
        Me.pbLoad.Name = "pbLoad"
        Me.pbLoad.Size = New System.Drawing.Size(100, 16)
        '
        'lblLoadStatus
        '
        Me.lblLoadStatus.Name = "lblLoadStatus"
        Me.lblLoadStatus.Size = New System.Drawing.Size(39, 17)
        Me.lblLoadStatus.Text = "Ready"
        '
        'lblTime
        '
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(46, 17)
        Me.lblTime.Text = "Time: 0"
        '
        'lblDebugVariable
        '
        Me.lblDebugVariable.Name = "lblDebugVariable"
        Me.lblDebugVariable.Size = New System.Drawing.Size(55, 17)
        Me.lblDebugVariable.Text = "Variable: "
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.AgentsToolStripMenuItem, Me.RouteTestingToolStripMenuItem, Me.ViewToolStripMenuItem, Me.SimulationToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(1099, 24)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'AgentsToolStripMenuItem
        '
        Me.AgentsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SelectStrategyToolStripMenuItem, Me.SelectIdleStrategyToolStripMenuItem, Me.SelectVehicleToolStripMenuItem, Me.SelectFailurePointToolStripMenuItem, Me.SelectDispatcherToolStripMenuItem})
        Me.AgentsToolStripMenuItem.Name = "AgentsToolStripMenuItem"
        Me.AgentsToolStripMenuItem.Size = New System.Drawing.Size(56, 20)
        Me.AgentsToolStripMenuItem.Text = "Agents"
        '
        'SelectStrategyToolStripMenuItem
        '
        Me.SelectStrategyToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CNP1ToolStripMenuItem, Me.CNP2ToolStripMenuItem, Me.CNP3ToolStripMenuItem, Me.CNP4ToolStripMenuItem, Me.CNP5ToolStripMenuItem, Me.FreeforallToolStripMenuItem, Me.RoundRobinToolStripMenuItem})
        Me.SelectStrategyToolStripMenuItem.Name = "SelectStrategyToolStripMenuItem"
        Me.SelectStrategyToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.SelectStrategyToolStripMenuItem.Text = "Select Strategy"
        '
        'CNP1ToolStripMenuItem
        '
        Me.CNP1ToolStripMenuItem.Name = "CNP1ToolStripMenuItem"
        Me.CNP1ToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.CNP1ToolStripMenuItem.Text = "CNP1"
        '
        'CNP2ToolStripMenuItem
        '
        Me.CNP2ToolStripMenuItem.Name = "CNP2ToolStripMenuItem"
        Me.CNP2ToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.CNP2ToolStripMenuItem.Text = "CNP2"
        '
        'CNP3ToolStripMenuItem
        '
        Me.CNP3ToolStripMenuItem.Name = "CNP3ToolStripMenuItem"
        Me.CNP3ToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.CNP3ToolStripMenuItem.Text = "CNP3"
        '
        'CNP4ToolStripMenuItem
        '
        Me.CNP4ToolStripMenuItem.Name = "CNP4ToolStripMenuItem"
        Me.CNP4ToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.CNP4ToolStripMenuItem.Text = "CNP4"
        '
        'CNP5ToolStripMenuItem
        '
        Me.CNP5ToolStripMenuItem.Name = "CNP5ToolStripMenuItem"
        Me.CNP5ToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.CNP5ToolStripMenuItem.Text = "CNP5"
        '
        'FreeforallToolStripMenuItem
        '
        Me.FreeforallToolStripMenuItem.Name = "FreeforallToolStripMenuItem"
        Me.FreeforallToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.FreeforallToolStripMenuItem.Text = "Free-for-all"
        '
        'RoundRobinToolStripMenuItem
        '
        Me.RoundRobinToolStripMenuItem.Name = "RoundRobinToolStripMenuItem"
        Me.RoundRobinToolStripMenuItem.Size = New System.Drawing.Size(143, 22)
        Me.RoundRobinToolStripMenuItem.Text = "Round Robin"
        '
        'SelectIdleStrategyToolStripMenuItem
        '
        Me.SelectIdleStrategyToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NoneToolStripMenuItem, Me.SleepToolStripMenuItem, Me.PredictiveToolStripMenuItem, Me.ScatterToolStripMenuItem})
        Me.SelectIdleStrategyToolStripMenuItem.Name = "SelectIdleStrategyToolStripMenuItem"
        Me.SelectIdleStrategyToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.SelectIdleStrategyToolStripMenuItem.Text = "Select Idle Strategy"
        '
        'NoneToolStripMenuItem
        '
        Me.NoneToolStripMenuItem.Name = "NoneToolStripMenuItem"
        Me.NoneToolStripMenuItem.Size = New System.Drawing.Size(126, 22)
        Me.NoneToolStripMenuItem.Text = "None"
        '
        'SleepToolStripMenuItem
        '
        Me.SleepToolStripMenuItem.Name = "SleepToolStripMenuItem"
        Me.SleepToolStripMenuItem.Size = New System.Drawing.Size(126, 22)
        Me.SleepToolStripMenuItem.Text = "Sleep"
        '
        'PredictiveToolStripMenuItem
        '
        Me.PredictiveToolStripMenuItem.Name = "PredictiveToolStripMenuItem"
        Me.PredictiveToolStripMenuItem.Size = New System.Drawing.Size(126, 22)
        Me.PredictiveToolStripMenuItem.Text = "Predictive"
        '
        'ScatterToolStripMenuItem
        '
        Me.ScatterToolStripMenuItem.Name = "ScatterToolStripMenuItem"
        Me.ScatterToolStripMenuItem.Size = New System.Drawing.Size(126, 22)
        Me.ScatterToolStripMenuItem.Text = "Scatter"
        '
        'SelectVehicleToolStripMenuItem
        '
        Me.SelectVehicleToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CarToolStripMenuItem, Me.SmallCommercialVanToolStripMenuItem, Me.Lorry75TonneToolStripMenuItem})
        Me.SelectVehicleToolStripMenuItem.Name = "SelectVehicleToolStripMenuItem"
        Me.SelectVehicleToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.SelectVehicleToolStripMenuItem.Text = "Select Vehicle"
        '
        'CarToolStripMenuItem
        '
        Me.CarToolStripMenuItem.Name = "CarToolStripMenuItem"
        Me.CarToolStripMenuItem.Size = New System.Drawing.Size(194, 22)
        Me.CarToolStripMenuItem.Text = "Car"
        '
        'SmallCommercialVanToolStripMenuItem
        '
        Me.SmallCommercialVanToolStripMenuItem.Name = "SmallCommercialVanToolStripMenuItem"
        Me.SmallCommercialVanToolStripMenuItem.Size = New System.Drawing.Size(194, 22)
        Me.SmallCommercialVanToolStripMenuItem.Text = "Small Commercial Van"
        '
        'Lorry75TonneToolStripMenuItem
        '
        Me.Lorry75TonneToolStripMenuItem.Name = "Lorry75TonneToolStripMenuItem"
        Me.Lorry75TonneToolStripMenuItem.Size = New System.Drawing.Size(194, 22)
        Me.Lorry75TonneToolStripMenuItem.Text = "Lorry (7.5 Tonne)"
        '
        'SelectFailurePointToolStripMenuItem
        '
        Me.SelectFailurePointToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DeliverToDepotToolStripMenuItem, Me.DeliverToPickupToolStripMenuItem})
        Me.SelectFailurePointToolStripMenuItem.Name = "SelectFailurePointToolStripMenuItem"
        Me.SelectFailurePointToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.SelectFailurePointToolStripMenuItem.Text = "Select Failure Point"
        '
        'DeliverToDepotToolStripMenuItem
        '
        Me.DeliverToDepotToolStripMenuItem.Name = "DeliverToDepotToolStripMenuItem"
        Me.DeliverToDepotToolStripMenuItem.Size = New System.Drawing.Size(163, 22)
        Me.DeliverToDepotToolStripMenuItem.Text = "Deliver to Depot"
        '
        'DeliverToPickupToolStripMenuItem
        '
        Me.DeliverToPickupToolStripMenuItem.Name = "DeliverToPickupToolStripMenuItem"
        Me.DeliverToPickupToolStripMenuItem.Size = New System.Drawing.Size(163, 22)
        Me.DeliverToPickupToolStripMenuItem.Text = "Deliver to Pickup"
        '
        'SelectDispatcherToolStripMenuItem
        '
        Me.SelectDispatcherToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.GeneralToolStripMenuItem, Me.SingleBusinessToolStripMenuItem})
        Me.SelectDispatcherToolStripMenuItem.Name = "SelectDispatcherToolStripMenuItem"
        Me.SelectDispatcherToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.SelectDispatcherToolStripMenuItem.Text = "Select Dispatcher"
        '
        'GeneralToolStripMenuItem
        '
        Me.GeneralToolStripMenuItem.Name = "GeneralToolStripMenuItem"
        Me.GeneralToolStripMenuItem.Size = New System.Drawing.Size(154, 22)
        Me.GeneralToolStripMenuItem.Text = "General"
        '
        'SingleBusinessToolStripMenuItem
        '
        Me.SingleBusinessToolStripMenuItem.Name = "SingleBusinessToolStripMenuItem"
        Me.SingleBusinessToolStripMenuItem.Size = New System.Drawing.Size(154, 22)
        Me.SingleBusinessToolStripMenuItem.Text = "Single Business"
        '
        'RouteTestingToolStripMenuItem
        '
        Me.RouteTestingToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RouteFromToolStripMenuItem, Me.BenchmarkToolStripMenuItem, Me.MinimiserToolStripMenuItem, Me.KeepRefreshingRouteToolStripMenuItem})
        Me.RouteTestingToolStripMenuItem.Name = "RouteTestingToolStripMenuItem"
        Me.RouteTestingToolStripMenuItem.Size = New System.Drawing.Size(92, 20)
        Me.RouteTestingToolStripMenuItem.Text = "Route Testing"
        '
        'RouteFromToolStripMenuItem
        '
        Me.RouteFromToolStripMenuItem.Name = "RouteFromToolStripMenuItem"
        Me.RouteFromToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.RouteFromToolStripMenuItem.Text = "Route From..."
        '
        'BenchmarkToolStripMenuItem
        '
        Me.BenchmarkToolStripMenuItem.Name = "BenchmarkToolStripMenuItem"
        Me.BenchmarkToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.BenchmarkToolStripMenuItem.Text = "Benchmark"
        '
        'ViewToolStripMenuItem
        '
        Me.ViewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NodesToolStripMenuItem, Me.RoadDelayNodesToolStripMenuItem, Me.RoadsToolStripMenuItem, Me.TrafficToolStripMenuItem, Me.AgentRoutesToolStripMenuItem, Me.AgentPlansToolStripMenuItem, Me.LandmarksToolStripMenuItem, Me.GridToolStripMenuItem, Me.PauseDisplayToolStripMenuItem})
        Me.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem"
        Me.ViewToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.ViewToolStripMenuItem.Text = "View"
        '
        'NodesToolStripMenuItem
        '
        Me.NodesToolStripMenuItem.CheckOnClick = True
        Me.NodesToolStripMenuItem.Name = "NodesToolStripMenuItem"
        Me.NodesToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.NodesToolStripMenuItem.Text = "Business Nodes"
        '
        'RoadDelayNodesToolStripMenuItem
        '
        Me.RoadDelayNodesToolStripMenuItem.CheckOnClick = True
        Me.RoadDelayNodesToolStripMenuItem.Name = "RoadDelayNodesToolStripMenuItem"
        Me.RoadDelayNodesToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.RoadDelayNodesToolStripMenuItem.Text = "Road Delay Nodes"
        '
        'RoadsToolStripMenuItem
        '
        Me.RoadsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ThinRoadsToolStripMenuItem, Me.ThickRoadsToolStripMenuItem})
        Me.RoadsToolStripMenuItem.Name = "RoadsToolStripMenuItem"
        Me.RoadsToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.RoadsToolStripMenuItem.Text = "Roads"
        '
        'ThinRoadsToolStripMenuItem
        '
        Me.ThinRoadsToolStripMenuItem.Checked = True
        Me.ThinRoadsToolStripMenuItem.CheckOnClick = True
        Me.ThinRoadsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ThinRoadsToolStripMenuItem.Name = "ThinRoadsToolStripMenuItem"
        Me.ThinRoadsToolStripMenuItem.Size = New System.Drawing.Size(138, 22)
        Me.ThinRoadsToolStripMenuItem.Text = "Thin Roads"
        '
        'ThickRoadsToolStripMenuItem
        '
        Me.ThickRoadsToolStripMenuItem.CheckOnClick = True
        Me.ThickRoadsToolStripMenuItem.Name = "ThickRoadsToolStripMenuItem"
        Me.ThickRoadsToolStripMenuItem.Size = New System.Drawing.Size(138, 22)
        Me.ThickRoadsToolStripMenuItem.Text = "Thick Roads"
        '
        'TrafficToolStripMenuItem
        '
        Me.TrafficToolStripMenuItem.CheckOnClick = True
        Me.TrafficToolStripMenuItem.Name = "TrafficToolStripMenuItem"
        Me.TrafficToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.TrafficToolStripMenuItem.Text = "Traffic"
        '
        'AgentRoutesToolStripMenuItem
        '
        Me.AgentRoutesToolStripMenuItem.Checked = True
        Me.AgentRoutesToolStripMenuItem.CheckOnClick = True
        Me.AgentRoutesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AgentRoutesToolStripMenuItem.Name = "AgentRoutesToolStripMenuItem"
        Me.AgentRoutesToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.AgentRoutesToolStripMenuItem.Text = "Agent Route Lines"
        '
        'AgentPlansToolStripMenuItem
        '
        Me.AgentPlansToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.JobViewToolStripMenuItem, Me.LineViewToolStripMenuItem, Me.RouteViewToolStripMenuItem})
        Me.AgentPlansToolStripMenuItem.Name = "AgentPlansToolStripMenuItem"
        Me.AgentPlansToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.AgentPlansToolStripMenuItem.Text = "Agent Plans"
        '
        'JobViewToolStripMenuItem
        '
        Me.JobViewToolStripMenuItem.Checked = True
        Me.JobViewToolStripMenuItem.CheckOnClick = True
        Me.JobViewToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.JobViewToolStripMenuItem.Name = "JobViewToolStripMenuItem"
        Me.JobViewToolStripMenuItem.Size = New System.Drawing.Size(133, 22)
        Me.JobViewToolStripMenuItem.Text = "Job View"
        '
        'LineViewToolStripMenuItem
        '
        Me.LineViewToolStripMenuItem.CheckOnClick = True
        Me.LineViewToolStripMenuItem.Name = "LineViewToolStripMenuItem"
        Me.LineViewToolStripMenuItem.Size = New System.Drawing.Size(133, 22)
        Me.LineViewToolStripMenuItem.Text = "Line View"
        '
        'RouteViewToolStripMenuItem
        '
        Me.RouteViewToolStripMenuItem.CheckOnClick = True
        Me.RouteViewToolStripMenuItem.Name = "RouteViewToolStripMenuItem"
        Me.RouteViewToolStripMenuItem.Size = New System.Drawing.Size(133, 22)
        Me.RouteViewToolStripMenuItem.Text = "Route View"
        '
        'LandmarksToolStripMenuItem
        '
        Me.LandmarksToolStripMenuItem.Checked = True
        Me.LandmarksToolStripMenuItem.CheckOnClick = True
        Me.LandmarksToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.LandmarksToolStripMenuItem.Name = "LandmarksToolStripMenuItem"
        Me.LandmarksToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.LandmarksToolStripMenuItem.Text = "Depots and Fuel Points"
        '
        'GridToolStripMenuItem
        '
        Me.GridToolStripMenuItem.CheckOnClick = True
        Me.GridToolStripMenuItem.Name = "GridToolStripMenuItem"
        Me.GridToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.GridToolStripMenuItem.Text = "Grid"
        '
        'PauseDisplayToolStripMenuItem
        '
        Me.PauseDisplayToolStripMenuItem.CheckOnClick = True
        Me.PauseDisplayToolStripMenuItem.Name = "PauseDisplayToolStripMenuItem"
        Me.PauseDisplayToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.PauseDisplayToolStripMenuItem.Text = "Pause Display"
        '
        'SimulationToolStripMenuItem
        '
        Me.SimulationToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StartSimulationToolStripMenuItem, Me.StartPlaygroundToolStripMenuItem, Me.StopToolStripMenuItem, Me.ResetToolStripMenuItem, Me.SpeedToolStripMenuItem, Me.ViewStatisticsToolStripMenuItem, Me.ViewConsoleToolStripMenuItem})
        Me.SimulationToolStripMenuItem.Name = "SimulationToolStripMenuItem"
        Me.SimulationToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
        Me.SimulationToolStripMenuItem.Text = "Simulation"
        '
        'StartSimulationToolStripMenuItem
        '
        Me.StartSimulationToolStripMenuItem.Name = "StartSimulationToolStripMenuItem"
        Me.StartSimulationToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.StartSimulationToolStripMenuItem.Text = "Start Simulation"
        '
        'StartPlaygroundToolStripMenuItem
        '
        Me.StartPlaygroundToolStripMenuItem.Name = "StartPlaygroundToolStripMenuItem"
        Me.StartPlaygroundToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.StartPlaygroundToolStripMenuItem.Text = "Start Playground"
        '
        'StopToolStripMenuItem
        '
        Me.StopToolStripMenuItem.Name = "StopToolStripMenuItem"
        Me.StopToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.StopToolStripMenuItem.Text = "Stop"
        '
        'ResetToolStripMenuItem
        '
        Me.ResetToolStripMenuItem.Name = "ResetToolStripMenuItem"
        Me.ResetToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.ResetToolStripMenuItem.Text = "Reset"
        '
        'SpeedToolStripMenuItem
        '
        Me.SpeedToolStripMenuItem.Name = "SpeedToolStripMenuItem"
        Me.SpeedToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.SpeedToolStripMenuItem.Text = "Set Parameters..."
        '
        'ViewStatisticsToolStripMenuItem
        '
        Me.ViewStatisticsToolStripMenuItem.Name = "ViewStatisticsToolStripMenuItem"
        Me.ViewStatisticsToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.ViewStatisticsToolStripMenuItem.Text = "View Statistics..."
        '
        'ViewConsoleToolStripMenuItem
        '
        Me.ViewConsoleToolStripMenuItem.Name = "ViewConsoleToolStripMenuItem"
        Me.ViewConsoleToolStripMenuItem.Size = New System.Drawing.Size(162, 22)
        Me.ViewConsoleToolStripMenuItem.Text = "View Console..."
        '
        'tmrRedraw
        '
        Me.tmrRedraw.Enabled = True
        Me.tmrRedraw.Interval = 10
        '
        'picMap
        '
        Me.picMap.Dock = System.Windows.Forms.DockStyle.Fill
        Me.picMap.Location = New System.Drawing.Point(0, 24)
        Me.picMap.Name = "picMap"
        Me.picMap.Size = New System.Drawing.Size(1099, 414)
        Me.picMap.TabIndex = 2
        Me.picMap.TabStop = False
        '
        'tmrStatus
        '
        Me.tmrStatus.Enabled = True
        '
        'bwSimulator
        '
        '
        'MinimiserToolStripMenuItem
        '
        Me.MinimiserToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DistanceToolStripMenuItem, Me.TimeWith8AMTrafficToolStripMenuItem, Me.TimeWithoutTrafficToolStripMenuItem, Me.FuelWith8AMTrafficToolStripMenuItem, Me.FuelWithoutTrafficToolStripMenuItem})
        Me.MinimiserToolStripMenuItem.Name = "MinimiserToolStripMenuItem"
        Me.MinimiserToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.MinimiserToolStripMenuItem.Text = "Minimiser"
        '
        'DistanceToolStripMenuItem
        '
        Me.DistanceToolStripMenuItem.Name = "DistanceToolStripMenuItem"
        Me.DistanceToolStripMenuItem.Size = New System.Drawing.Size(189, 22)
        Me.DistanceToolStripMenuItem.Text = "Distance"
        '
        'TimeWith8AMTrafficToolStripMenuItem
        '
        Me.TimeWith8AMTrafficToolStripMenuItem.Name = "TimeWith8AMTrafficToolStripMenuItem"
        Me.TimeWith8AMTrafficToolStripMenuItem.Size = New System.Drawing.Size(189, 22)
        Me.TimeWith8AMTrafficToolStripMenuItem.Text = "Time with 8AM traffic"
        '
        'TimeWithoutTrafficToolStripMenuItem
        '
        Me.TimeWithoutTrafficToolStripMenuItem.Name = "TimeWithoutTrafficToolStripMenuItem"
        Me.TimeWithoutTrafficToolStripMenuItem.Size = New System.Drawing.Size(189, 22)
        Me.TimeWithoutTrafficToolStripMenuItem.Text = "Time without traffic"
        '
        'FuelWith8AMTrafficToolStripMenuItem
        '
        Me.FuelWith8AMTrafficToolStripMenuItem.Name = "FuelWith8AMTrafficToolStripMenuItem"
        Me.FuelWith8AMTrafficToolStripMenuItem.Size = New System.Drawing.Size(189, 22)
        Me.FuelWith8AMTrafficToolStripMenuItem.Text = "Fuel with 8AM traffic"
        '
        'FuelWithoutTrafficToolStripMenuItem
        '
        Me.FuelWithoutTrafficToolStripMenuItem.Name = "FuelWithoutTrafficToolStripMenuItem"
        Me.FuelWithoutTrafficToolStripMenuItem.Size = New System.Drawing.Size(189, 22)
        Me.FuelWithoutTrafficToolStripMenuItem.Text = "Fuel without traffic"
        '
        'KeepRefreshingRouteToolStripMenuItem
        '
        Me.KeepRefreshingRouteToolStripMenuItem.CheckOnClick = True
        Me.KeepRefreshingRouteToolStripMenuItem.Name = "KeepRefreshingRouteToolStripMenuItem"
        Me.KeepRefreshingRouteToolStripMenuItem.Size = New System.Drawing.Size(193, 22)
        Me.KeepRefreshingRouteToolStripMenuItem.Text = "Keep Refreshing Route"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1099, 460)
        Me.Controls.Add(Me.picMap)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmMain"
        Me.Text = "Autonomous Agents Simulator"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.picMap, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents pbLoad As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents lblLoadStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AgentsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tmrRedraw As System.Windows.Forms.Timer
    Friend WithEvents picMap As System.Windows.Forms.PictureBox
    Friend WithEvents lblTime As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents tmrStatus As System.Windows.Forms.Timer
    Friend WithEvents RouteTestingToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RouteFromToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblDebugVariable As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NodesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RoadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SimulationToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StartSimulationToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StopToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ResetToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AgentRoutesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ThinRoadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ThickRoadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SpeedToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents BenchmarkToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StartPlaygroundToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AgentPlansToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents JobViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LineViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RouteViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LandmarksToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents bwSimulator As System.ComponentModel.BackgroundWorker
    Friend WithEvents ViewStatisticsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectStrategyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CNP1ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CNP2ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CNP3ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CNP4ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CNP5ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectVehicleToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CarToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SmallCommercialVanToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Lorry75TonneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GridToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PauseDisplayToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RoadDelayNodesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TrafficToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FreeforallToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RoundRobinToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectIdleStrategyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents NoneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SleepToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PredictiveToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ScatterToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectFailurePointToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeliverToDepotToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeliverToPickupToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ViewConsoleToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SelectDispatcherToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GeneralToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SingleBusinessToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MinimiserToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DistanceToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TimeWith8AMTrafficToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TimeWithoutTrafficToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FuelWith8AMTrafficToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FuelWithoutTrafficToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents KeepRefreshingRouteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
