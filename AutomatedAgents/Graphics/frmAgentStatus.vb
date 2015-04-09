Public Class frmAgentStatus
    Private AASimulation As AASimulation

    Private Sub frmAgentStatus_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetDoubleBuffered(lvAgentList)
        SetDoubleBuffered(lvJobList)
    End Sub
    'TODO update this within synclock on graphics tick of frmmain.

    Private Sub tmrAgentListView_Tick(sender As Object, e As EventArgs) Handles tmrAgentListView.Tick
        If AASimulation Is Nothing Then
            Exit Sub
        End If
        Try
            If AASimulation.Agents.Count <> lvAgentList.Items.Count Then
                SetAASimulation(AASimulation)
            End If
            PopulateAgentList()
            If IsInDebugMode() Then
                PopulateJobList()
            End If
        Catch ex As Exception
            Debug.WriteLine("Do not ignore this exception!!!:   " & ex.ToString)
            'TODO SOME SORT OF NULLPOINTER CAN HAPPEN HERE? Sync issue?
        End Try

    End Sub
    Private Sub PopulateJobList()
        lvJobList.BeginUpdate()
        lvJobList.Items.Clear()
        For AgentID = 0 To AASimulation.Agents.Count - 1
            For Each J As CourierJob In AASimulation.Agents(AgentID).Plan.GetCurrentJobs
                Dim LVI As New ListViewItem(J.JobID)
                LVI.SubItems.Add(AgentID)
                LVI.SubItems.Add(J.PickupPosition.ToString)
                LVI.SubItems.Add(J.OriginalDeliveryPosition.ToString & If(J.IsGoingToDepot(), " -> [D]", ""))
                LVI.SubItems.Add(Math.Round(J.GetDirectRoute.GetKM, 3))
                LVI.SubItems.Add(J.GetDirectRoute.GetEstimatedTime.ToString("h\:mm\:ss"))
                LVI.SubItems.Add(J.CubicMetres)
                LVI.SubItems.Add(J.Deadline.ToString)
                LVI.SubItems.Add(J.Status.ToString("G"))
                LVI.SubItems.Add((NoticeBoard.CurrentTime - J.Deadline).ToString("h\:mm\:ss"))
                LVI.SubItems.Add(FormatCurrency(J.CustomerFee))
                LVI.BackColor = AASimulation.Agents(AgentID).Color
                LVI.UseItemStyleForSubItems = False
                lvJobList.Items.Add(LVI)
            Next
        Next
        lvJobList.EndUpdate()
    End Sub

    Private Sub PopulateAgentList()
        lvAgentList.BeginUpdate()
        For i = 0 To AASimulation.Agents.Count - 1
            Dim Agent As Agent = AASimulation.Agents(i)
            Dim Position As RoutePosition = Agent.Plan.RoutePosition
            If Position IsNot Nothing Then
                Dim Way As Way = Position.GetCurrentWay
                DisplayLVCell(lvAgentList, i, cAt, If(Way IsNot Nothing, Way.Name, ""))
                DisplayLVCell(lvAgentList, i, cDestination, Position.GetEndPoint.ToString)
                DisplayLVCell(lvAgentList, i, cKMH, Agent.CurrentSpeedKMH)
            End If

            DisplayLVCell(lvAgentList, i, cJobs, Agent.Plan.WayPoints.Count)
            DisplayLVCell(lvAgentList, i, cVehicle, Agent.GetVehicleString())
            DisplayLVCell(lvAgentList, i, cAName, Agent.AgentName)
            DisplayLVCell(lvAgentList, i, cLitres, Math.Round(Agent.PetroleumLitres, 2))
            DisplayLVCell(lvAgentList, i, cTotalKM, Math.Round(Agent.TotalKMTravelled, 1))
            DisplayLVCell(lvAgentList, i, cFuelCost, FormatCurrency(Agent.FuelCosts))
            DisplayLVCell(lvAgentList, i, cCapacity, Math.Round(100 * Agent.GetVehicleCapacityPercentage, 1) & "%")
            DisplayLVCell(lvAgentList, i, cCompletedJobs, Agent.TotalCompletedJobs)
        Next
        lvAgentList.EndUpdate()
    End Sub

    Private Sub DisplayLVCell(ByVal lv As ListView, ByVal Row As Integer, ByVal Column As ColumnHeader, ByVal Value As String)
        If lv.Items(Row).SubItems(Column.Index).Text <> Value Then
            lv.Items(Row).SubItems(Column.Index).Text = Value
        End If
    End Sub

    Public Sub SetAASimulation(ByVal AASimulation As Object)
        Me.AASimulation = AASimulation
        lvAgentList.Items.Clear()
        For i = 0 To Me.AASimulation.Agents.Count - 1
            lvAgentList.Items.Add(i)
            lvAgentList.Items(i).UseItemStyleForSubItems = False
            lvAgentList.Items(i).SubItems(cID.Index).BackColor = Me.AASimulation.Agents(i).Color
            For j = 0 To lvAgentList.Columns.Count - 2
                lvAgentList.Items(i).SubItems.Add("")
            Next
        Next
    End Sub

End Class