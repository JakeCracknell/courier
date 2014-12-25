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
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SuspendLayout()
        '
        'tmrAgentListView
        '
        Me.tmrAgentListView.Enabled = True
        '
        'lvAgentList
        '
        Me.lvAgentList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.lvAgentList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvAgentList.FullRowSelect = True
        Me.lvAgentList.Location = New System.Drawing.Point(0, 0)
        Me.lvAgentList.Name = "lvAgentList"
        Me.lvAgentList.Size = New System.Drawing.Size(509, 509)
        Me.lvAgentList.TabIndex = 0
        Me.lvAgentList.UseCompatibleStateImageBehavior = False
        Me.lvAgentList.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "ID"
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Route Position"
        Me.ColumnHeader2.Width = 116
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Destination"
        Me.ColumnHeader3.Width = 110
        '
        'frmAgentStatus
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(509, 509)
        Me.Controls.Add(Me.lvAgentList)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "frmAgentStatus"
        Me.Text = "frmAgentStatus"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tmrAgentListView As System.Windows.Forms.Timer
    Friend WithEvents lvAgentList As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
End Class
