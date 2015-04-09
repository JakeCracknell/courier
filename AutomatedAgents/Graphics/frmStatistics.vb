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
            clbDataSeries.Items.Add(DC.ColumnName)
        Next
    End Sub

    Sub RefreshSeriesList()
        Dim Table As DataTable = AASimulation.Statistics.Table
        If clbDataSeries.Items.Count = 0 Then
            For Each DC As DataColumn In Table.Columns
                clbDataSeries.Items.Add(DC.ToString)
            Next
        End If
        Dim DR As DataRow = Table.Rows(Table.Rows.Count - 1)
        For i = 0 To Table.Columns.Count - 1
            Dim DC As DataColumn = Table.Columns(i)
            clbDataSeries.Items(i) = String.Format(SERIES_LIST_ITEM_FORMAT, DC.ColumnName, DR(DC))
        Next
    End Sub

    Sub RefreshChart()
        Dim Table As DataTable = AASimulation.Statistics.Table

        Dim DataColumns As New List(Of DataColumn)
        For Each ColumnIndex In clbDataSeries.CheckedIndices
            DataColumns.Add(Table.Columns(ColumnIndex))
        Next

        chartSimulationStatistics.DataBindTable(Table)

    End Sub

    Private Sub btnRefreshData_Click(sender As Object, e As EventArgs) Handles btnRefreshData.Click
        RefreshSeriesList()
    End Sub
End Class