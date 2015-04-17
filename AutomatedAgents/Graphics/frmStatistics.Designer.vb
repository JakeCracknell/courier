<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStatistics
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
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer5 = New System.Windows.Forms.SplitContainer()
        Me.clbDataSeries = New System.Windows.Forms.CheckedListBox()
        Me.chartSimulationStatistics = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer3 = New System.Windows.Forms.SplitContainer()
        Me.btnRefreshData = New System.Windows.Forms.Button()
        Me.txtDataRefreshTimerInterval = New System.Windows.Forms.TextBox()
        Me.cbDataRefreshTimer = New System.Windows.Forms.CheckBox()
        Me.SplitContainer4 = New System.Windows.Forms.SplitContainer()
        Me.btnSaveImage = New System.Windows.Forms.Button()
        Me.btnSaveDataAsXML = New System.Windows.Forms.Button()
        Me.tmrStatisticsRefresh = New System.Windows.Forms.Timer(Me.components)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.SplitContainer5, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer5.Panel1.SuspendLayout()
        Me.SplitContainer5.Panel2.SuspendLayout()
        Me.SplitContainer5.SuspendLayout()
        CType(Me.chartSimulationStatistics, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer3.Panel1.SuspendLayout()
        Me.SplitContainer3.Panel2.SuspendLayout()
        Me.SplitContainer3.SuspendLayout()
        CType(Me.SplitContainer4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer4.Panel1.SuspendLayout()
        Me.SplitContainer4.Panel2.SuspendLayout()
        Me.SplitContainer4.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.IsSplitterFixed = True
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.SplitContainer5)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.SplitContainer2)
        Me.SplitContainer1.Size = New System.Drawing.Size(745, 481)
        Me.SplitContainer1.SplitterDistance = 434
        Me.SplitContainer1.TabIndex = 0
        '
        'SplitContainer5
        '
        Me.SplitContainer5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer5.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer5.Name = "SplitContainer5"
        '
        'SplitContainer5.Panel1
        '
        Me.SplitContainer5.Panel1.Controls.Add(Me.clbDataSeries)
        '
        'SplitContainer5.Panel2
        '
        Me.SplitContainer5.Panel2.Controls.Add(Me.chartSimulationStatistics)
        Me.SplitContainer5.Size = New System.Drawing.Size(745, 434)
        Me.SplitContainer5.SplitterDistance = 158
        Me.SplitContainer5.TabIndex = 0
        '
        'clbDataSeries
        '
        Me.clbDataSeries.CheckOnClick = True
        Me.clbDataSeries.Dock = System.Windows.Forms.DockStyle.Fill
        Me.clbDataSeries.FormattingEnabled = True
        Me.clbDataSeries.Location = New System.Drawing.Point(0, 0)
        Me.clbDataSeries.Name = "clbDataSeries"
        Me.clbDataSeries.Size = New System.Drawing.Size(158, 434)
        Me.clbDataSeries.TabIndex = 0
        '
        'chartSimulationStatistics
        '
        ChartArea1.AxisX.Minimum = 0.0R
        ChartArea1.Name = "ChartArea1"
        Me.chartSimulationStatistics.ChartAreas.Add(ChartArea1)
        Me.chartSimulationStatistics.Dock = System.Windows.Forms.DockStyle.Fill
        Legend1.Name = "Legend1"
        Me.chartSimulationStatistics.Legends.Add(Legend1)
        Me.chartSimulationStatistics.Location = New System.Drawing.Point(0, 0)
        Me.chartSimulationStatistics.Name = "chartSimulationStatistics"
        Me.chartSimulationStatistics.Size = New System.Drawing.Size(583, 434)
        Me.chartSimulationStatistics.TabIndex = 0
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.IsSplitterFixed = True
        Me.SplitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer2.Name = "SplitContainer2"
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.SplitContainer3)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.SplitContainer4)
        Me.SplitContainer2.Size = New System.Drawing.Size(745, 43)
        Me.SplitContainer2.SplitterDistance = 368
        Me.SplitContainer2.TabIndex = 0
        '
        'SplitContainer3
        '
        Me.SplitContainer3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer3.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer3.Name = "SplitContainer3"
        '
        'SplitContainer3.Panel1
        '
        Me.SplitContainer3.Panel1.Controls.Add(Me.btnRefreshData)
        '
        'SplitContainer3.Panel2
        '
        Me.SplitContainer3.Panel2.Controls.Add(Me.txtDataRefreshTimerInterval)
        Me.SplitContainer3.Panel2.Controls.Add(Me.cbDataRefreshTimer)
        Me.SplitContainer3.Size = New System.Drawing.Size(368, 43)
        Me.SplitContainer3.SplitterDistance = 179
        Me.SplitContainer3.TabIndex = 0
        '
        'btnRefreshData
        '
        Me.btnRefreshData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnRefreshData.Font = New System.Drawing.Font("Microsoft Sans Serif", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnRefreshData.Location = New System.Drawing.Point(0, 0)
        Me.btnRefreshData.Name = "btnRefreshData"
        Me.btnRefreshData.Size = New System.Drawing.Size(179, 43)
        Me.btnRefreshData.TabIndex = 0
        Me.btnRefreshData.Text = "Refresh"
        Me.btnRefreshData.UseVisualStyleBackColor = True
        '
        'txtDataRefreshTimerInterval
        '
        Me.txtDataRefreshTimerInterval.Dock = System.Windows.Forms.DockStyle.Top
        Me.txtDataRefreshTimerInterval.Enabled = False
        Me.txtDataRefreshTimerInterval.Location = New System.Drawing.Point(0, 17)
        Me.txtDataRefreshTimerInterval.Name = "txtDataRefreshTimerInterval"
        Me.txtDataRefreshTimerInterval.Size = New System.Drawing.Size(185, 20)
        Me.txtDataRefreshTimerInterval.TabIndex = 1
        Me.txtDataRefreshTimerInterval.Text = "100"
        '
        'cbDataRefreshTimer
        '
        Me.cbDataRefreshTimer.AutoSize = True
        Me.cbDataRefreshTimer.Dock = System.Windows.Forms.DockStyle.Top
        Me.cbDataRefreshTimer.Location = New System.Drawing.Point(0, 0)
        Me.cbDataRefreshTimer.Name = "cbDataRefreshTimer"
        Me.cbDataRefreshTimer.Size = New System.Drawing.Size(185, 17)
        Me.cbDataRefreshTimer.TabIndex = 0
        Me.cbDataRefreshTimer.Text = "Timer Refresh Interval:"
        Me.cbDataRefreshTimer.UseVisualStyleBackColor = True
        '
        'SplitContainer4
        '
        Me.SplitContainer4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer4.IsSplitterFixed = True
        Me.SplitContainer4.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer4.Name = "SplitContainer4"
        '
        'SplitContainer4.Panel1
        '
        Me.SplitContainer4.Panel1.Controls.Add(Me.btnSaveImage)
        '
        'SplitContainer4.Panel2
        '
        Me.SplitContainer4.Panel2.Controls.Add(Me.btnSaveDataAsXML)
        Me.SplitContainer4.Size = New System.Drawing.Size(373, 43)
        Me.SplitContainer4.SplitterDistance = 173
        Me.SplitContainer4.TabIndex = 0
        '
        'btnSaveImage
        '
        Me.btnSaveImage.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnSaveImage.Font = New System.Drawing.Font("Microsoft Sans Serif", 26.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSaveImage.Location = New System.Drawing.Point(0, 0)
        Me.btnSaveImage.Name = "btnSaveImage"
        Me.btnSaveImage.Size = New System.Drawing.Size(173, 43)
        Me.btnSaveImage.TabIndex = 0
        Me.btnSaveImage.Text = ".PNG"
        Me.btnSaveImage.UseVisualStyleBackColor = True
        '
        'btnSaveDataAsXML
        '
        Me.btnSaveDataAsXML.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnSaveDataAsXML.Font = New System.Drawing.Font("Microsoft Sans Serif", 26.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSaveDataAsXML.Location = New System.Drawing.Point(0, 0)
        Me.btnSaveDataAsXML.Name = "btnSaveDataAsXML"
        Me.btnSaveDataAsXML.Size = New System.Drawing.Size(196, 43)
        Me.btnSaveDataAsXML.TabIndex = 1
        Me.btnSaveDataAsXML.Text = ".XML"
        Me.btnSaveDataAsXML.UseVisualStyleBackColor = True
        '
        'tmrStatisticsRefresh
        '
        '
        'frmStatistics
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(745, 481)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStatistics"
        Me.Text = "Simulation Statistics"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.SplitContainer5.Panel1.ResumeLayout(False)
        Me.SplitContainer5.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer5, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer5.ResumeLayout(False)
        CType(Me.chartSimulationStatistics, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        Me.SplitContainer3.Panel1.ResumeLayout(False)
        Me.SplitContainer3.Panel2.ResumeLayout(False)
        Me.SplitContainer3.Panel2.PerformLayout()
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer3.ResumeLayout(False)
        Me.SplitContainer4.Panel1.ResumeLayout(False)
        Me.SplitContainer4.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer4.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents SplitContainer5 As System.Windows.Forms.SplitContainer
    Friend WithEvents clbDataSeries As System.Windows.Forms.CheckedListBox
    Friend WithEvents chartSimulationStatistics As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents SplitContainer2 As System.Windows.Forms.SplitContainer
    Friend WithEvents SplitContainer3 As System.Windows.Forms.SplitContainer
    Friend WithEvents btnRefreshData As System.Windows.Forms.Button
    Friend WithEvents txtDataRefreshTimerInterval As System.Windows.Forms.TextBox
    Friend WithEvents cbDataRefreshTimer As System.Windows.Forms.CheckBox
    Friend WithEvents SplitContainer4 As System.Windows.Forms.SplitContainer
    Friend WithEvents btnSaveImage As System.Windows.Forms.Button
    Friend WithEvents btnSaveDataAsXML As System.Windows.Forms.Button
    Friend WithEvents tmrStatisticsRefresh As System.Windows.Forms.Timer
End Class
