Public Class frmSimulationSpeed

    Private Sub frmSimulationSpeed_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        tbGraphicsRefreshRate.Value = SimulationParameters.DisplayRefreshSpeed
        txtGraphicsRefreshRate.Text = SimulationParameters.DisplayRefreshSpeed
        tbSimSpeed.Value = SimulationParameters.SimulationSpeed
        txtSimSpeed.Text = SimulationParameters.SimulationSpeed
        Label1.Focus()
    End Sub

    Private Sub tbSimSpeed_Scroll(sender As Object, e As EventArgs) Handles tbSimSpeed.Scroll
        txtSimSpeed.Text = tbSimSpeed.Value
        SimulationParameters.SimulationSpeed = tbSimSpeed.Value
    End Sub

    Private Sub tbGraphicsRefreshRate_Scroll(sender As Object, e As EventArgs) Handles tbGraphicsRefreshRate.Scroll
        txtGraphicsRefreshRate.Text = tbGraphicsRefreshRate.Value
        SimulationParameters.DisplayRefreshSpeed = tbGraphicsRefreshRate.Value
    End Sub

    Private Sub txtSimSpeed_TextChanged(sender As Object, e As EventArgs) Handles txtSimSpeed.TextChanged
        If IsNumeric(txtSimSpeed.Text) Then
            Dim txtValue As Integer = CInt(txtSimSpeed.Text)
            If txtValue >= tbSimSpeed.Minimum And txtValue <= tbSimSpeed.Maximum Then
                tbSimSpeed.Value = txtSimSpeed.Text
            Else
                Exit Sub
            End If
        End If
        SimulationParameters.SimulationSpeed = tbSimSpeed.Value
    End Sub

    Private Sub txtGraphicsRefreshRate_TextChanged(sender As Object, e As EventArgs) Handles txtGraphicsRefreshRate.TextChanged
        If IsNumeric(txtGraphicsRefreshRate.Text) Then
            Dim txtValue As Integer = CInt(txtGraphicsRefreshRate.Text)
            If txtValue >= tbGraphicsRefreshRate.Minimum And txtValue <= tbGraphicsRefreshRate.Maximum Then
                tbGraphicsRefreshRate.Value = txtGraphicsRefreshRate.Text
            Else
                Exit Sub
            End If
        End If
        SimulationParameters.DisplayRefreshSpeed = tbGraphicsRefreshRate.Value
    End Sub

End Class