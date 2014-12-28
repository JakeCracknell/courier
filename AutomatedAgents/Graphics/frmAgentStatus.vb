Public Class frmAgentStatus
    Private AASimulation As AASimulation
    Private Sub tmrAgentListView_Tick(sender As Object, e As EventArgs) Handles tmrAgentListView.Tick
        If AASimulation.Agents.Count <> lvAgentList.Items.Count Then
            SetAASimulation(AASimulation)
        End If
        For i = 0 To AASimulation.Agents.Count - 1
            Dim Position As RoutePosition = AASimulation.Agents(i).Position
            If Position IsNot Nothing Then
                Dim Way As Way = Position.GetCurrentWayPosition.Hop.Way
                If Way IsNot Nothing AndAlso lvAgentList.Items(i).SubItems(1).Text <> Way.Name Then
                    lvAgentList.Items(i).SubItems(1).Text = Way.Name
                End If

                If lvAgentList.Items(i).SubItems(2).Text <> Position.GetEndNode.ID.ToString Then
                    lvAgentList.Items(i).SubItems(2).Text = Position.GetEndNode.ID
                End If

                If lvAgentList.Items(i).SubItems(3).Text <> AASimulation.Agents(i).CurrentSpeedKMH.ToString Then
                    lvAgentList.Items(i).SubItems(3).Text = AASimulation.Agents(i).CurrentSpeedKMH
                End If

            End If

        Next
    End Sub
    Public Sub SetAASimulation(ByVal AASimulation As Object)
        Me.AASimulation = AASimulation
        lvAgentList.Items.Clear()
        For i = 0 To Me.AASimulation.Agents.Count - 1
            lvAgentList.Items.Add(i)
            lvAgentList.Items(i).SubItems.Add("")
            lvAgentList.Items(i).SubItems.Add("")
            lvAgentList.Items(i).SubItems.Add("")
        Next
    End Sub
End Class