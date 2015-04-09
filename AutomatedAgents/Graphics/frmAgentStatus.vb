Public Class frmAgentStatus
    Private AASimulation As AASimulation

    Private Sub frmAgentStatus_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetDoubleBuffered(lvAgentList)
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

            lvAgentList.BeginUpdate()
            For i = 0 To AASimulation.Agents.Count - 1
                Dim Agent As Agent = AASimulation.Agents(i)
                Dim Position As RoutePosition = Agent.Plan.RoutePosition
                If Position IsNot Nothing Then
                    Dim Way As Way = Position.GetCurrentWay
                    DisplayLVCell(i, cAt, If(Way IsNot Nothing, Way.Name, ""))
                    DisplayLVCell(i, cDestination, Position.GetEndPoint.ToString)
                    DisplayLVCell(i, cKMH, Agent.CurrentSpeedKMH)
                End If

                DisplayLVCell(i, cJobs, Agent.Plan.WayPoints.Count)
                DisplayLVCell(i, cVehicle, Agent.GetVehicleString())
                DisplayLVCell(i, cAName, Agent.AgentName)
                DisplayLVCell(i, cLitres, Math.Round(Agent.PetroleumLitres, 2))
                DisplayLVCell(i, cTotalKM, Math.Round(Agent.TotalKMTravelled, 1))
                DisplayLVCell(i, cFuelCost, FormatCurrency(Agent.FuelCosts))
                DisplayLVCell(i, cCapacity, Math.Round(100 * Agent.GetVehicleCapacityPercentage, 1) & "%")
                DisplayLVCell(i, cCompletedJobs, Agent.TotalCompletedJobs)
            Next
            lvAgentList.EndUpdate()
        Catch ex As Exception
            Debug.WriteLine("Do not ignore this exception!!!:   " & ex.ToString)
            'TODO SOME SORT OF NULLPOINTER CAN HAPPEN HERE?
        End Try

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