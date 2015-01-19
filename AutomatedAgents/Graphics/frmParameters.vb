Public Class frmParameters

    Private Sub frmParameters_Load(sender As Object, e As EventArgs) Handles Me.Load
        tbGraphicsRefreshRate.Value = SimulationParameters.DisplayRefreshSpeed
        txtGraphicsRefreshRate.Text = SimulationParameters.DisplayRefreshSpeed
        tbSimSpeed.Value = SimulationParameters.SimulationSpeed
        txtSimSpeed.Text = SimulationParameters.SimulationSpeed
        txtDispatchRate.Text = SimulationParameters.DispatchRatePerHour
        tbDispatchRate.Value = SimulationParameters.DispatchRatePerHour
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
                tbSimSpeed.Value = txtValue
                SimulationParameters.SimulationSpeed = txtValue
            End If
        End If
    End Sub

    Private Sub txtGraphicsRefreshRate_TextChanged(sender As Object, e As EventArgs) Handles txtGraphicsRefreshRate.TextChanged
        If IsNumeric(txtGraphicsRefreshRate.Text) Then
            Dim txtValue As Integer = CInt(txtGraphicsRefreshRate.Text)
            If txtValue >= tbGraphicsRefreshRate.Minimum And txtValue <= tbGraphicsRefreshRate.Maximum Then
                tbGraphicsRefreshRate.Value = txtValue
                SimulationParameters.DisplayRefreshSpeed = txtValue
            End If
        End If
    End Sub

    Private Sub txtDispatchRate_TextChanged(sender As Object, e As EventArgs) Handles txtDispatchRate.TextChanged
        If IsNumeric(txtDispatchRate.Text) Then
            Dim txtValue As Integer = CInt(txtDispatchRate.Text)
            If txtValue >= tbDispatchRate.Minimum And txtValue <= tbDispatchRate.Maximum Then
                tbDispatchRate.Value = txtValue
                SimulationParameters.DispatchRatePerHour = txtValue
            End If
        End If
    End Sub

    Private Sub tbDispatchRate_Scroll(sender As Object, e As EventArgs) Handles tbDispatchRate.Scroll
        txtDispatchRate.Text = tbDispatchRate.Value
        SimulationParameters.DispatchRatePerHour = tbDispatchRate.Value
    End Sub
End Class