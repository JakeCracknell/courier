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
        Me.components = New System.ComponentModel.Container()
        Me.tmrAgentListView = New System.Windows.Forms.Timer(Me.components)
        Me.lvAgentList = New System.Windows.Forms.ListView()
        Me.cID = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cVehicle = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cAName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cAt = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cDestination = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cJobs = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cKMH = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cLitres = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cTotalKM = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cFuelCost = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cCapacity = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cCompletedJobs = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SuspendLayout()
        '
        'tmrAgentListView
        '
        Me.tmrAgentListView.Enabled = True
        '
        'lvAgentList
        '
        Me.lvAgentList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.cID, Me.cVehicle, Me.cAName, Me.cAt, Me.cDestination, Me.cJobs, Me.cKMH, Me.cLitres, Me.cTotalKM, Me.cFuelCost, Me.cCapacity, Me.cCompletedJobs})
        Me.lvAgentList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvAgentList.FullRowSelect = True
        Me.lvAgentList.Location = New System.Drawing.Point(0, 0)
        Me.lvAgentList.Name = "lvAgentList"
        Me.lvAgentList.Size = New System.Drawing.Size(1034, 131)
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
        'cAName
        '
        Me.cAName.Text = "Name"
        Me.cAName.Width = 78
        '
        'cAt
        '
        Me.cAt.Text = "Route Position"
        Me.cAt.Width = 116
        '
        'cDestination
        '
        Me.cDestination.Text = "Next Destination"
        Me.cDestination.Width = 110
        '
        'cJobs
        '
        Me.cJobs.Text = "Jobs"
        Me.cJobs.Width = 35
        '
        'cKMH
        '
        Me.cKMH.DisplayIndex = 7
        Me.cKMH.Text = "Speed (KM/h)"
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
        Me.cTotalKM.Text = "Total Distance (KM)"
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
        Me.cCompletedJobs.Text = "Completed Jobs"
        Me.cCompletedJobs.Width = 89
        '
        'frmAgentStatus
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1034, 131)
        Me.Controls.Add(Me.lvAgentList)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "frmAgentStatus"
        Me.Text = "Agent Status"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tmrAgentListView As System.Windows.Forms.Timer
    Friend WithEvents lvAgentList As System.Windows.Forms.ListView
    Friend WithEvents cID As System.Windows.Forms.ColumnHeader
    Friend WithEvents cAt As System.Windows.Forms.ColumnHeader
    Friend WithEvents cDestination As System.Windows.Forms.ColumnHeader
    Friend WithEvents cKMH As System.Windows.Forms.ColumnHeader
    Friend WithEvents cAName As System.Windows.Forms.ColumnHeader
    Friend WithEvents cLitres As System.Windows.Forms.ColumnHeader
    Friend WithEvents cTotalKM As System.Windows.Forms.ColumnHeader
    Friend WithEvents cFuelCost As System.Windows.Forms.ColumnHeader
    Friend WithEvents cVehicle As System.Windows.Forms.ColumnHeader
    Friend WithEvents cJobs As System.Windows.Forms.ColumnHeader
    Friend WithEvents cCapacity As System.Windows.Forms.ColumnHeader
    Friend WithEvents cCompletedJobs As System.Windows.Forms.ColumnHeader
End Class
