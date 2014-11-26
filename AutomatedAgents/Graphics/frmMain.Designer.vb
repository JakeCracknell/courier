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
        Me.RouteTestingToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RouteFromToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NodesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RoadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AgentRoutesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SimulationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.tmrAgents = New System.Windows.Forms.Timer(Me.components)
        Me.picMap = New System.Windows.Forms.PictureBox()
        Me.tmrStatus = New System.Windows.Forms.Timer(Me.components)
        Me.ThinRoadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ThickRoadsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
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
        Me.AgentsToolStripMenuItem.Name = "AgentsToolStripMenuItem"
        Me.AgentsToolStripMenuItem.Size = New System.Drawing.Size(56, 20)
        Me.AgentsToolStripMenuItem.Text = "Agents"
        '
        'RouteTestingToolStripMenuItem
        '
        Me.RouteTestingToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RouteFromToolStripMenuItem})
        Me.RouteTestingToolStripMenuItem.Name = "RouteTestingToolStripMenuItem"
        Me.RouteTestingToolStripMenuItem.Size = New System.Drawing.Size(92, 20)
        Me.RouteTestingToolStripMenuItem.Text = "Route Testing"
        '
        'RouteFromToolStripMenuItem
        '
        Me.RouteFromToolStripMenuItem.Name = "RouteFromToolStripMenuItem"
        Me.RouteFromToolStripMenuItem.Size = New System.Drawing.Size(145, 22)
        Me.RouteFromToolStripMenuItem.Text = "Route From..."
        '
        'ViewToolStripMenuItem
        '
        Me.ViewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RoadsToolStripMenuItem, Me.NodesToolStripMenuItem, Me.AgentRoutesToolStripMenuItem})
        Me.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem"
        Me.ViewToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.ViewToolStripMenuItem.Text = "View"
        '
        'NodesToolStripMenuItem
        '
        Me.NodesToolStripMenuItem.Checked = True
        Me.NodesToolStripMenuItem.CheckOnClick = True
        Me.NodesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.NodesToolStripMenuItem.Name = "NodesToolStripMenuItem"
        Me.NodesToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.NodesToolStripMenuItem.Text = "Nodes"
        '
        'RoadsToolStripMenuItem
        '
        Me.RoadsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ThinRoadsToolStripMenuItem, Me.ThickRoadsToolStripMenuItem})
        Me.RoadsToolStripMenuItem.Name = "RoadsToolStripMenuItem"
        Me.RoadsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.RoadsToolStripMenuItem.Text = "Roads"
        '
        'AgentRoutesToolStripMenuItem
        '
        Me.AgentRoutesToolStripMenuItem.Checked = True
        Me.AgentRoutesToolStripMenuItem.CheckOnClick = True
        Me.AgentRoutesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AgentRoutesToolStripMenuItem.Name = "AgentRoutesToolStripMenuItem"
        Me.AgentRoutesToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.AgentRoutesToolStripMenuItem.Text = "Agent Routes"
        '
        'SimulationToolStripMenuItem
        '
        Me.SimulationToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StartToolStripMenuItem, Me.StopToolStripMenuItem, Me.ResetToolStripMenuItem})
        Me.SimulationToolStripMenuItem.Name = "SimulationToolStripMenuItem"
        Me.SimulationToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
        Me.SimulationToolStripMenuItem.Text = "Simulation"
        '
        'StartToolStripMenuItem
        '
        Me.StartToolStripMenuItem.Name = "StartToolStripMenuItem"
        Me.StartToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.StartToolStripMenuItem.Text = "Start"
        '
        'StopToolStripMenuItem
        '
        Me.StopToolStripMenuItem.Name = "StopToolStripMenuItem"
        Me.StopToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.StopToolStripMenuItem.Text = "Stop"
        '
        'ResetToolStripMenuItem
        '
        Me.ResetToolStripMenuItem.Name = "ResetToolStripMenuItem"
        Me.ResetToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ResetToolStripMenuItem.Text = "Reset"
        '
        'tmrAgents
        '
        Me.tmrAgents.Interval = 20
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
        'ThinRoadsToolStripMenuItem
        '
        Me.ThinRoadsToolStripMenuItem.Checked = True
        Me.ThinRoadsToolStripMenuItem.CheckOnClick = True
        Me.ThinRoadsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ThinRoadsToolStripMenuItem.Name = "ThinRoadsToolStripMenuItem"
        Me.ThinRoadsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ThinRoadsToolStripMenuItem.Text = "Thin Roads"
        '
        'ThickRoadsToolStripMenuItem
        '
        Me.ThickRoadsToolStripMenuItem.CheckOnClick = True
        Me.ThickRoadsToolStripMenuItem.Name = "ThickRoadsToolStripMenuItem"
        Me.ThickRoadsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ThickRoadsToolStripMenuItem.Text = "Thick Roads"
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
    Friend WithEvents tmrAgents As System.Windows.Forms.Timer
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
    Friend WithEvents StartToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StopToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ResetToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AgentRoutesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ThinRoadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ThickRoadsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
