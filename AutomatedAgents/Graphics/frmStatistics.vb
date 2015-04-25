Imports System.Windows.Forms.DataVisualization.Charting

Public Class frmStatistics
    Private Const SERIES_LIST_ITEM_FORMAT As String = "{0} ({1})"

    Private AASimulation As AASimulation

    Public Sub SetAASimulation(ByVal AASimulation As AASimulation)
        Me.AASimulation = AASimulation
        clbDataSeries.Items.Clear()
        InitialiseSeriesList()
    End Sub

    Sub InitialiseSeriesList()
        For Each DC As DataColumn In AASimulation.Statistics.Table.Columns
            If Not DC.Unique Then 'Don't include "Time"
                clbDataSeries.Items.Add(DC.ColumnName)
            End If
        Next
    End Sub

    Sub RefreshSeriesList()
        Dim Table As DataTable = AASimulation.Statistics.Table
        If clbDataSeries.Items.Count = 0 Then
            For Each DC As DataColumn In Table.Columns
                clbDataSeries.Items.Add(DC.ToString)
            Next
        End If
        If Table.Rows.Count > 0 Then
            Dim DR As DataRow = Table.Rows(Table.Rows.Count - 1)
            For i = 0 To Table.Columns.Count - 2 'Don't include "Time"
                Dim DC As DataColumn = Table.Columns(i + 1)
                clbDataSeries.Items(i) = String.Format(SERIES_LIST_ITEM_FORMAT, DC.ColumnName, DR(DC))
            Next
        End If
    End Sub

    Sub RefreshChart()
        Dim Table As DataTable = AASimulation.Statistics.Table

        Dim DataColumns As New List(Of DataColumn)
        For Each ColumnIndex In clbDataSeries.CheckedIndices
            DataColumns.Add(Table.Columns(ColumnIndex + 1))
        Next

        chartSimulationStatistics.Series.Clear()
        chartSimulationStatistics.DataSource = Table

        'Hourly interval if < 3 days, then daily.
        chartSimulationStatistics.ChartAreas(0).AxisX.MajorGrid.Interval = 86400
        chartSimulationStatistics.ChartAreas(0).AxisX.Interval = If(NoticeBoard.Time.TotalSeconds > 260000, 86400, 3600)

        For Each DC As DataColumn In DataColumns
            Dim Series As New Series(DC.ColumnName)
            Series.XValueMember = "Time"
            Series.YValueMembers = DC.ColumnName
            Series.ChartType = SeriesChartType.FastLine
            chartSimulationStatistics.Series.Add(Series)
        Next

        SyncLock Table
            chartSimulationStatistics.DataBind()
        End SyncLock
    End Sub

    Private Sub controlRefreshData_Event(sender As Object, e As EventArgs) Handles btnRefreshData.Click, clbDataSeries.Click, tmrStatisticsRefresh.Tick
        If AASimulation IsNot Nothing Then
            RefreshSeriesList()
            RefreshChart()
        End If
    End Sub

    Private Sub controlRefreshTimerChange(sender As Object, e As EventArgs) Handles cbDataRefreshTimer.CheckedChanged, txtDataRefreshTimerInterval.TextChanged
        txtDataRefreshTimerInterval.Enabled = cbDataRefreshTimer.Checked
        tmrStatisticsRefresh.Enabled = cbDataRefreshTimer.Checked
        If IsNumeric(txtDataRefreshTimerInterval.Text) Then
            tmrStatisticsRefresh.Interval = CInt(txtDataRefreshTimerInterval.Text)
        End If
    End Sub

    Private Sub btnSaveImage_Click(sender As Object, e As EventArgs) Handles btnSaveImage.Click
        If chartSimulationStatistics.Series.Count > 0 Then
            Dim PlottedColumns As String = ""
            For Each Serie In chartSimulationStatistics.Series
                PlottedColumns &= Serie.Name & ","
            Next
            PlottedColumns = PlottedColumns.TrimEnd(",")
            Dim FileName As String = AASimulation.Statistics.StatsDirectoryPath & PlottedColumns & ".png"
            Try
                IO.File.Delete(FileName)
                chartSimulationStatistics.SaveImage(FileName, Drawing.Imaging.ImageFormat.Png)
            Catch ex As Exception
                MsgBox("Operation failed: " & ex.Message, MsgBoxStyle.Exclamation)
            End Try
        End If

    End Sub

    Private Sub btnSaveDataAsXML_Click(sender As Object, e As EventArgs) Handles btnSaveDataAsXML.Click
        If AASimulation IsNot Nothing Then
            AASimulation.Statistics.SaveToXML()
        End If
    End Sub
End Class