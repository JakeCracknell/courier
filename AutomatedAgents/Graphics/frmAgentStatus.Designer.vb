<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAgentStatus
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
        Me.lvAgentList = New System.Windows.Forms.ListView()
        Me.cID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cVehicle = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cAt = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cNextWayPoint = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cWayPoints = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cAllocatedJobs = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cKMH = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cLitres = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cTotalKM = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cFuelCost = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cCapacity = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cCompletedJobs = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.lvJobList = New System.Windows.Forms.ListView()
        Me.cJobID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cAgent = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cFrom = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cTo = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cDirect = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cVolume = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cDeadline = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cStatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cTimeLeft = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cRevenue = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lvLog = New System.Windows.Forms.ListView()
        Me.cEventTime = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cAgentID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cEventDescription = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        Me.SuspendLayout()
        '
        'lvAgentList
        '
        Me.lvAgentList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.cID, Me.cVehicle, Me.cAt, Me.cNextWayPoint, Me.cWayPoints, Me.cAllocatedJobs, Me.cKMH, Me.cLitres, Me.cTotalKM, Me.cFuelCost, Me.cCapacity, Me.cCompletedJobs})
        Me.lvAgentList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvAgentList.FullRowSelect = True
        Me.lvAgentList.Location = New System.Drawing.Point(0, 0)
        Me.lvAgentList.Name = "lvAgentList"
        Me.lvAgentList.Size = New System.Drawing.Size(915, 119)
        Me.lvAgentList.TabIndex = 0
        Me.lvAgentList.UseCompatibleStateImageBehavior = False
        Me.lvAgentList.View = System.Windows.Forms.View.Details
        '
        'cID
        '
        Me.cID.Text = "ID"
        Me.cID.Width = 36
        '
        'cVehicle
        '
        Me.cVehicle.Text = "Vehicle"
        '
        'cAt
        '
        Me.cAt.Text = "Route Position"
        Me.cAt.Width = 90
        '
        'cNextWayPoint
        '
        Me.cNextWayPoint.Text = "Next WayPoint"
        Me.cNextWayPoint.Width = 160
        '
        'cWayPoints
        '
        Me.cWayPoints.Text = "WayPoints"
        Me.cWayPoints.Width = 66
        '
        'cAllocatedJobs
        '
        Me.cAllocatedJobs.Text = "Jobs"
        Me.cAllocatedJobs.Width = 39
        '
        'cKMH
        '
        Me.cKMH.DisplayIndex = 7
        Me.cKMH.Text = "Speed (km/h)"
        Me.cKMH.Width = 82
        '
        'cLitres
        '
        Me.cLitres.DisplayIndex = 8
        Me.cLitres.Text = "Fuel (L)"
        '
        'cTotalKM
        '
        Me.cTotalKM.DisplayIndex = 9
        Me.cTotalKM.Text = "Total Distance (km)"
        Me.cTotalKM.Width = 111
        '
        'cFuelCost
        '
        Me.cFuelCost.DisplayIndex = 11
        Me.cFuelCost.Text = "Fuel Costs"
        Me.cFuelCost.Width = 63
        '
        'cCapacity
        '
        Me.cCapacity.DisplayIndex = 6
        Me.cCapacity.Text = "Capacity"
        Me.cCapacity.Width = 53
        '
        'cCompletedJobs
        '
        Me.cCompletedJobs.DisplayIndex = 10
        Me.cCompletedJobs.Text = "Completed"
        Me.cCompletedJobs.Width = 67
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.lvAgentList)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.SplitContainer2)
        Me.SplitContainer1.Size = New System.Drawing.Size(915, 413)
        Me.SplitContainer1.SplitterDistance = 119
        Me.SplitContainer1.TabIndex = 1
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer2.Name = "SplitContainer2"
        Me.SplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.lvJobList)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.lvLog)
        Me.SplitContainer2.Size = New System.Drawing.Size(915, 290)
        Me.SplitContainer2.SplitterDistance = 145
        Me.SplitContainer2.TabIndex = 2
        '
        'lvJobList
        '
        Me.lvJobList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.cJobID, Me.cAgent, Me.cFrom, Me.cTo, Me.cDirect, Me.cVolume, Me.cDeadline, Me.cStatus, Me.cTimeLeft, Me.cRevenue})
        Me.lvJobList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvJobList.FullRowSelect = True
        Me.lvJobList.Location = New System.Drawing.Point(0, 0)
        Me.lvJobList.Name = "lvJobList"
        Me.lvJobList.Size = New System.Drawing.Size(915, 145)
        Me.lvJobList.TabIndex = 1
        Me.lvJobList.UseCompatibleStateImageBehavior = False
        Me.lvJobList.View = System.Windows.Forms.View.Details
        '
        'cJobID
        '
        Me.cJobID.Text = "ID"
        Me.cJobID.Width = 36
        '
        'cAgent
        '
        Me.cAgent.Text = "Agent"
        '
        'cFrom
        '
        Me.cFrom.Text = "From ↑"
        Me.cFrom.Width = 156
        '
        'cTo
        '
        Me.cTo.Text = "To ↓"
        Me.cTo.Width = 166
        '
        'cDirect
        '
        Me.cDirect.Text = "Direct Route"
        Me.cDirect.Width = 114
        '
        'cVolume
        '
        Me.cVolume.Text = "m³"
        Me.cVolume.Width = 47
        '
        'cDeadline
        '
        Me.cDeadline.Text = "Deadline"
        Me.cDeadline.Width = 83
        '
        'cStatus
        '
        Me.cStatus.Text = "Status"
        Me.cStatus.Width = 85
        '
        'cTimeLeft
        '
        Me.cTimeLeft.Text = "Time Left"
        Me.cTimeLeft.Width = 71
        '
        'cRevenue
        '
        Me.cRevenue.Text = "Revenue"
        '
        'lvLog
        '
        Me.lvLog.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.cEventTime, Me.cAgentID, Me.cEventDescription})
        Me.lvLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvLog.Font = New System.Drawing.Font("Consolas", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lvLog.FullRowSelect = True
        Me.lvLog.Location = New System.Drawing.Point(0, 0)
        Me.lvLog.Name = "lvLog"
        Me.lvLog.Size = New System.Drawing.Size(915, 141)
        Me.lvLog.TabIndex = 2
        Me.lvLog.UseCompatibleStateImageBehavior = False
        Me.lvLog.View = System.Windows.Forms.View.Details
        '
        'cEventTime
        '
        Me.cEventTime.Text = "Time"
        '
        'cAgentID
        '
        Me.cAgentID.Text = "Agent"
        Me.cAgentID.Width = 46
        '
        'cEventDescription
        '
        Me.cEventDescription.Text = "Description"
        Me.cEventDescription.Width = 104
        '
        'frmAgentStatus
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(915, 413)
        Me.Controls.Add(Me.SplitContainer1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "frmAgentStatus"
        Me.Text = "Agent Status"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lvAgentList As System.Windows.Forms.ListView
    Friend WithEvents cID As System.Windows.Forms.ColumnHeader
    Friend WithEvents cAt As System.Windows.Forms.ColumnHeader
    Friend WithEvents cNextWayPoint As System.Windows.Forms.ColumnHeader
    Friend WithEvents cKMH As System.Windows.Forms.ColumnHeader
    Friend WithEvents cLitres As System.Windows.Forms.ColumnHeader
    Friend WithEvents cTotalKM As System.Windows.Forms.ColumnHeader
    Friend WithEvents cFuelCost As System.Windows.Forms.ColumnHeader
    Friend WithEvents cVehicle As System.Windows.Forms.ColumnHeader
    Friend WithEvents cWayPoints As System.Windows.Forms.ColumnHeader
    Friend WithEvents cCapacity As System.Windows.Forms.ColumnHeader
    Friend WithEvents cCompletedJobs As System.Windows.Forms.ColumnHeader
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents lvJobList As System.Windows.Forms.ListView
    Friend WithEvents cJobID As System.Windows.Forms.ColumnHeader
    Friend WithEvents cAgent As System.Windows.Forms.ColumnHeader
    Friend WithEvents cFrom As System.Windows.Forms.ColumnHeader
    Friend WithEvents cTo As System.Windows.Forms.ColumnHeader
    Friend WithEvents cDirect As System.Windows.Forms.ColumnHeader
    Friend WithEvents cVolume As System.Windows.Forms.ColumnHeader
    Friend WithEvents cDeadline As System.Windows.Forms.ColumnHeader
    Friend WithEvents cStatus As System.Windows.Forms.ColumnHeader
    Friend WithEvents cTimeLeft As System.Windows.Forms.ColumnHeader
    Friend WithEvents cRevenue As System.Windows.Forms.ColumnHeader
    Friend WithEvents cAllocatedJobs As System.Windows.Forms.ColumnHeader
    Friend WithEvents SplitContainer2 As System.Windows.Forms.SplitContainer
    Friend WithEvents lvLog As System.Windows.Forms.ListView
    Friend WithEvents cEventTime As System.Windows.Forms.ColumnHeader
    Friend WithEvents cAgentID As System.Windows.Forms.ColumnHeader
    Friend WithEvents cEventDescription As System.Windows.Forms.ColumnHeader
End Class
