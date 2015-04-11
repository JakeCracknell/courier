﻿Public Class frmAgentStatus
    Private AASimulation As AASimulation
    Private LastStateHash As Integer

    Private Sub frmAgentStatus_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetDoubleBuffered(lvAgentList)
        SetDoubleBuffered(lvJobList)
        SetDoubleBuffered(lvLog)
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
            Dim AgentID As Integer = CInt(AgentList(0))
            Dim LVI As New ListViewItem(AgentList(0))
            For i = 1 To AgentList.Count - 1
                LVI.SubItems.Add(AgentList(i))
            Next
            LVI.UseItemStyleForSubItems = False            LVI.BackColor = GetSequentialColor(AgentID)
            lvAgentList.Items.Add(LVI)
        Next
        For Each JobList As List(Of String) In State.Item2
            Dim AgentID As Integer = CInt(JobList(1))
            Dim LVI As New ListViewItem(JobList(0))
            For i = 1 To JobList.Count - 1
                LVI.SubItems.Add(JobList(i))
            Next            LVI.UseItemStyleForSubItems = False
            LVI.BackColor = GetSequentialColor(AgentID)
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
            LVI.SubItems.Add(If(E.AgentID >= 0, E.AgentID, "HQ"))
            LVI.SubItems.Add(E.Description)
            lvLog.Items.Insert(0, LVI)
        Next
        lvLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent)
        lvLog.EndUpdate()
    End Sub

End Class