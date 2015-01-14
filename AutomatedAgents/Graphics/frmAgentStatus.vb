Public Class frmAgentStatus
    Private AASimulation As AASimulation
    Private Sub tmrAgentListView_Tick(sender As Object, e As EventArgs) Handles tmrAgentListView.Tick
        If AASimulation.Agents.Count <> lvAgentList.Items.Count Then
            SetAASimulation(AASimulation)
        End If

        lvAgentList.BeginUpdate()
        For i = 0 To AASimulation.Agents.Count - 1
            Dim Position As RoutePosition = AASimulation.Agents(i).Position
            If Position IsNot Nothing Then
                Dim Way As Way = Position.GetCurrentWayPosition.Hop.Way
                DisplayLVCell(i, cAt, IIf(Way Is Nothing, "", Way.Name))
                DisplayLVCell(i, cDestination, Position.GetEndNode.ID)
                DisplayLVCell(i, cKMH, AASimulation.Agents(i).CurrentSpeedKMH)
            End If

            DisplayLVCell(i, cVehicle, AASimulation.Agents(i).GetVehicleString())
            DisplayLVCell(i, cAName, AASimulation.Agents(i).AgentName)
            DisplayLVCell(i, cLitres, Math.Round(AASimulation.Agents(i).PetroleumLitres, 2))
            DisplayLVCell(i, cTotalKM, Math.Round(AASimulation.Agents(i).TotalKMTravelled, 1))
            DisplayLVCell(i, cFuelCost, FormatCurrency(AASimulation.Agents(i).FuelCosts))
        Next
        lvAgentList.EndUpdate()
    End Sub

    Private Sub DisplayLVCell(ByVal Row As Integer, ByVal Column As ColumnHeader, ByVal Value As String)
        If lvAgentList.Items(Row).SubItems(Column.Index).Text <> Value Then
            lvAgentList.Items(Row).SubItems(Column.Index).Text = Value
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