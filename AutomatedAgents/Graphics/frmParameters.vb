Public Class frmParameters

    Private Sub frmParameters_Load(sender As Object, e As EventArgs) Handles Me.Load
        tbGraphicsRefreshRate.Value = SimulationParameters.DisplayRefreshSpeed
        txtGraphicsRefreshRate.Text = SimulationParameters.DisplayRefreshSpeed
        tbSimSpeed.Value = SimulationParameters.SimulationSpeed
        txtSimSpeed.Text = SimulationParameters.SimulationSpeed
        txtDispatchRate.Text = SimulationParameters.DispatchRateCoefficient
        tbDispatchRate.Value = SimulationParameters.DispatchRateCoefficient
        txtCubicMetres.Text = SimulationParameters.PackageSizeLambda
        tbCubicMetres.Value = SimulationParameters.PackageSizeLambda * 100
        txtDeadlines.Text = SimulationParameters.DeadlineAverage
        tbDeadlines.Value = SimulationParameters.DeadlineAverage
        txtFailedPickup.Text = SimulationParameters.ProbPickupFail
        tbFailedPickup.Value = SimulationParameters.ProbPickupFail * 100
        txtFailedDelivery.Text = SimulationParameters.ProbDeliveryFail
        tbFailedDelivery.Value = SimulationParameters.ProbDeliveryFail * 100
        txtFeeBase.Text = SimulationParameters.FeeBasePrice
        tbFeeBase.Value = SimulationParameters.FeeBasePrice * 100
        txtFeeHourly.Text = SimulationParameters.FeeHourlyPrice
        tbFeeHourly.Value = SimulationParameters.FeeHourlyPrice * 100
        txtAStarAccelerator.Text = SimulationParameters.AStarAccelerator
        tbAStarAccelerator.Value = SimulationParameters.AStarAccelerator * 100

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
    Private Sub tbDispatchRate_Scroll(sender As Object, e As EventArgs) Handles tbDispatchRate.Scroll
        txtDispatchRate.Text = tbDispatchRate.Value
        SimulationParameters.DispatchRateCoefficient = tbDispatchRate.Value
    End Sub

    Private Sub tbDeadlines_Scroll(sender As Object, e As EventArgs) Handles tbDeadlines.Scroll
        txtDeadlines.Text = tbDeadlines.Value
        SimulationParameters.DeadlineAverage = tbDeadlines.Value
    End Sub
    Private Sub tbCubicMetres_Scroll(sender As Object, e As EventArgs) Handles tbCubicMetres.Scroll
        txtCubicMetres.Text = tbCubicMetres.Value / 100
        SimulationParameters.PackageSizeLambda = tbCubicMetres.Value / 100
    End Sub
    Private Sub tbFailedPickup_Scroll(sender As Object, e As EventArgs) Handles tbFailedPickup.Scroll
        txtFailedPickup.Text = tbFailedPickup.Value / 100
        SimulationParameters.ProbPickupFail = tbFailedPickup.Value / 100
    End Sub
    Private Sub tbFailedDelivery_Scroll(sender As Object, e As EventArgs) Handles tbFailedDelivery.Scroll
        txtFailedDelivery.Text = tbFailedDelivery.Value / 100
        SimulationParameters.ProbDeliveryFail = tbFailedDelivery.Value / 100
    End Sub
    Private Sub tbAStarAccelerator_Scroll(sender As Object, e As EventArgs) Handles tbAStarAccelerator.Scroll
        txtAStarAccelerator.Text = tbAStarAccelerator.Value / 100
        SimulationParameters.AStarAccelerator = tbAStarAccelerator.Value / 100
    End Sub
    Private Sub tbFeeBase_Scroll(sender As Object, e As EventArgs) Handles tbFeeBase.Scroll
        txtFeeBase.Text = tbFeeBase.Value / 100
        SimulationParameters.FeeBasePrice = tbFeeBase.Value / 100
    End Sub
    Private Sub tbFeeHourly_Scroll(sender As Object, e As EventArgs) Handles tbFeeHourly.Scroll
        txtFeeHourly.Text = tbFeeHourly.Value / 100
        SimulationParameters.FeeHourlyPrice = tbFeeHourly.Value / 100
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
                SimulationParameters.DispatchRateCoefficient = txtValue
            End If
        End If
    End Sub
    Private Sub txtDeadlines_TextChanged(sender As Object, e As EventArgs) Handles txtDeadlines.TextChanged
        If IsNumeric(txtDeadlines.Text) Then
            Dim txtValue As Integer = CInt(txtDeadlines.Text)
            If txtValue >= tbDeadlines.Minimum And txtValue <= tbDeadlines.Maximum Then
                tbDeadlines.Value = txtValue
                SimulationParameters.DeadlineAverage = txtValue
            End If
        End If
    End Sub
    Private Sub txtCubicMetres_TextChanged(sender As Object, e As EventArgs) Handles txtCubicMetres.TextChanged
        If IsNumeric(txtCubicMetres.Text) Then
            Dim txtValue As Integer = CDec(txtCubicMetres.Text) * 100
            If txtValue >= tbCubicMetres.Minimum And txtValue <= tbCubicMetres.Maximum Then
                tbCubicMetres.Value = txtValue
                SimulationParameters.PackageSizeLambda = txtValue / 100
            End If
        End If
    End Sub
    Private Sub txtFailedPickup_TextChanged(sender As Object, e As EventArgs) Handles txtFailedPickup.TextChanged
        If IsNumeric(txtFailedPickup.Text) Then
            Dim txtValue As Integer = CDec(txtFailedPickup.Text) * 100
            If txtValue >= tbFailedPickup.Minimum And txtValue <= tbFailedPickup.Maximum Then
                tbFailedPickup.Value = txtValue
                SimulationParameters.ProbPickupFail = txtValue / 100
            End If
        End If
    End Sub
    Private Sub txtFailedDelivery_TextChanged(sender As Object, e As EventArgs) Handles txtFailedDelivery.TextChanged
        If IsNumeric(txtFailedDelivery.Text) Then
            Dim txtValue As Integer = CDec(txtFailedDelivery.Text) * 100
            If txtValue >= tbFailedDelivery.Minimum And txtValue <= tbFailedDelivery.Maximum Then
                tbFailedDelivery.Value = txtValue
                SimulationParameters.ProbDeliveryFail = txtValue / 100
            End If
        End If
    End Sub
    Private Sub txtAStarAccelerator_TextChanged(sender As Object, e As EventArgs) Handles txtAStarAccelerator.TextChanged
        If IsNumeric(txtAStarAccelerator.Text) Then
            Dim txtValue As Integer = CDec(txtAStarAccelerator.Text) * 100
            If txtValue >= tbAStarAccelerator.Minimum And txtValue <= tbAStarAccelerator.Maximum Then
                tbAStarAccelerator.Value = txtValue
                SimulationParameters.AStarAccelerator = txtValue / 100
            End If
        End If
    End Sub
End Class