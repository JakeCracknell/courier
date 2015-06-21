Public Class frmAgentStatus
    Private LastStateHash As Integer
    Private StrikeOutFont As Font
    Private frmStreetView As Form
    Private picStreetView As PictureBox

    Private Sub frmAgentStatus_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetDoubleBuffered(lvAgentList)
        SetDoubleBuffered(lvJobList)
        SetDoubleBuffered(lvLog)
        StrikeOutFont = New Font(lvJobList.Font.FontFamily, lvJobList.Font.Size, FontStyle.Strikeout)

        picStreetView = New PictureBox
        picStreetView.Dock = DockStyle.Fill
        frmStreetView = New Form()
        frmStreetView.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        frmStreetView.TopMost = True
        frmStreetView.ShowInTaskbar = False
        frmStreetView.Size = New Size(600, 200)
        frmStreetView.Controls.Add(picStreetView)
    End Sub

    Sub RefreshLists()
        Dim State = SimulationState.GetAASimulationStatus()
        If State.GetHashCode = LastStateHash Then
            Exit Sub
        End If

        lvAgentList.BeginUpdate()
        lvJobList.BeginUpdate()
        lvAgentList.Items.Clear()
        lvJobList.Items.Clear()
        For Each AgentList As List(Of String) In State.Item1
            Dim AgentID As Integer = Integer.Parse(AgentList(0))
            Dim LVI As New ListViewItem(AgentList(0))
            For i = 1 To AgentList.Count - 1
                LVI.SubItems.Add(AgentList(i))
            Next
            LVI.UseItemStyleForSubItems = False
            LVI.BackColor = GetSequentialColor(AgentID)

            lvAgentList.Items.Add(LVI)
        Next
        For Each JobList As List(Of String) In State.Item2
            Dim AgentID As Integer = Integer.Parse(JobList(1))
            Dim LVI As New ListViewItem(JobList(0))
            For i = 1 To JobList.Count - 1
                LVI.SubItems.Add(JobList(i))
            Next
            LVI.UseItemStyleForSubItems = False
            LVI.BackColor = GetSequentialColor(AgentID)
            If JobList(7).Contains("DEL") Then
                LVI.SubItems(2).Font = StrikeOutFont
            End If

            lvJobList.Items.Add(LVI)
        Next
        lvAgentList.EndUpdate()
        lvJobList.EndUpdate()

        PopulateEventsList()
    End Sub

    Private Sub PopulateEventsList()
        Dim Events = SimulationState.GetEvents
        If Events.Length = 0 Then
            Exit Sub
        End If

        lvLog.BeginUpdate()
        For Each E As SimulationState.LogEvent In Events
            Dim LVI As New ListViewItem(E.Time.ToString)
            LVI.SubItems.Add(If(E.AgentID >= 0, E.AgentID.ToString, "HQ"))
            LVI.SubItems.Add(E.Description)
            lvLog.Items.Insert(0, LVI)
        Next
        lvLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent)
        lvLog.EndUpdate()
    End Sub

    Private Sub lvAgentList_MouseDown(sender As Object, e As MouseEventArgs) Handles lvAgentList.MouseDown
        Try
            Dim MousePoint As Point = lvAgentList.PointToClient(Control.MousePosition)
            Dim HitTest As ListViewHitTestInfo = lvAgentList.HitTest(MousePoint)
            Dim LVI As ListViewItem = lvAgentList.GetItemAt(MousePoint.X, MousePoint.Y)
            If LVI IsNot Nothing Then
                Dim AgentID As Integer = Integer.Parse(LVI.Text)
                Dim Hop As Hop = SimulationState.GetHopByAgentID(AgentID)
                Dim Bearing As Double = GetBearing(Hop.FromPoint, Hop.ToPoint)
                picStreetView.ImageLocation = "http://maps.googleapis.com/maps/api/streetview?size=600x200&location=" & Hop.FromPoint.GetLatitude & ",%20" & Hop.FromPoint.GetLongitude & "&fov=110&heading=" & Bearing & "&pitch=9.66"
                frmStreetView.Location = Cursor.Position
                frmStreetView.Visible = True
            Else
                frmStreetView.Hide()
            End If
        Catch ex As Exception
            'Possible race condition. GUI bug is acceptable
        End Try
    End Sub

    Private Sub lvAgentList_MouseUp(sender As Object, e As MouseEventArgs) Handles lvAgentList.MouseUp
        frmStreetView.Hide()
    End Sub
End Class