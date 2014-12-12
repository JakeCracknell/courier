<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSimulationSpeed
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
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.txtSimSpeed = New System.Windows.Forms.TextBox()
        Me.txtGraphicsRefreshRate = New System.Windows.Forms.TextBox()
        Me.tbSimSpeed = New System.Windows.Forms.TrackBar()
        Me.tbGraphicsRefreshRate = New System.Windows.Forms.TrackBar()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.tbSimSpeed, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbGraphicsRefreshRate, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 3
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 71.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.txtSimSpeed, 2, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.txtGraphicsRefreshRate, 2, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.tbSimSpeed, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.tbGraphicsRefreshRate, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(369, 59)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'txtSimSpeed
        '
        Me.txtSimSpeed.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtSimSpeed.Location = New System.Drawing.Point(301, 3)
        Me.txtSimSpeed.Name = "txtSimSpeed"
        Me.txtSimSpeed.Size = New System.Drawing.Size(65, 20)
        Me.txtSimSpeed.TabIndex = 0
        Me.txtSimSpeed.Text = "1"
        '
        'txtGraphicsRefreshRate
        '
        Me.txtGraphicsRefreshRate.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtGraphicsRefreshRate.Location = New System.Drawing.Point(301, 32)
        Me.txtGraphicsRefreshRate.Name = "txtGraphicsRefreshRate"
        Me.txtGraphicsRefreshRate.Size = New System.Drawing.Size(65, 20)
        Me.txtGraphicsRefreshRate.TabIndex = 1
        Me.txtGraphicsRefreshRate.Text = "50"
        '
        'tbSimSpeed
        '
        Me.tbSimSpeed.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tbSimSpeed.Location = New System.Drawing.Point(152, 3)
        Me.tbSimSpeed.Maximum = 1000
        Me.tbSimSpeed.Minimum = 1
        Me.tbSimSpeed.Name = "tbSimSpeed"
        Me.tbSimSpeed.Size = New System.Drawing.Size(143, 23)
        Me.tbSimSpeed.TabIndex = 2
        Me.tbSimSpeed.TickFrequency = 0
        Me.tbSimSpeed.TickStyle = System.Windows.Forms.TickStyle.None
        Me.tbSimSpeed.Value = 1
        '
        'tbGraphicsRefreshRate
        '
        Me.tbGraphicsRefreshRate.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tbGraphicsRefreshRate.Location = New System.Drawing.Point(152, 32)
        Me.tbGraphicsRefreshRate.Maximum = 5000
        Me.tbGraphicsRefreshRate.Minimum = 10
        Me.tbGraphicsRefreshRate.Name = "tbGraphicsRefreshRate"
        Me.tbGraphicsRefreshRate.Size = New System.Drawing.Size(143, 24)
        Me.tbGraphicsRefreshRate.TabIndex = 3
        Me.tbGraphicsRefreshRate.TickFrequency = 0
        Me.tbGraphicsRefreshRate.TickStyle = System.Windows.Forms.TickStyle.None
        Me.tbGraphicsRefreshRate.Value = 50
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label1.Location = New System.Drawing.Point(3, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(143, 29)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Simulation Speed (x)"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label2.Location = New System.Drawing.Point(3, 29)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(143, 30)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Graphics Refresh Rate (ms)"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'frmSimulationSpeed
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(369, 59)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "frmSimulationSpeed"
        Me.Text = "Set Speed"
        Me.TopMost = True
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        CType(Me.tbSimSpeed, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbGraphicsRefreshRate, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents txtSimSpeed As System.Windows.Forms.TextBox
    Friend WithEvents txtGraphicsRefreshRate As System.Windows.Forms.TextBox
    Friend WithEvents tbSimSpeed As System.Windows.Forms.TrackBar
    Friend WithEvents tbGraphicsRefreshRate As System.Windows.Forms.TrackBar
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
End Class
