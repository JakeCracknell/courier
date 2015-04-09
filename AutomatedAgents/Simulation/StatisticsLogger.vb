﻿Public Class StatisticsLogger
    Private Const STATS_DIRECTORY_FORMAT As String = "stats\{0}-{1}\"
    Private Const CSV_FILENAME As String = "data.csv"
    Private Const CSV_SAVE_MESSAGE_FORMAT As String = "Statistics Saved to {0}. File size: {1} KB. Row count: {2}"


    Public ReadOnly SimulationStartTime As Date
    Public ReadOnly Map As StreetMap
    Public ReadOnly Table As DataTable
    Public ReadOnly StatsDirectoryPath As String

    Sub New(ByVal SimulationStartTime As Date, ByVal Map As StreetMap)
        Me.SimulationStartTime = SimulationStartTime
        Me.Map = Map

        StatsDirectoryPath = String.Format(STATS_DIRECTORY_FORMAT, SimulationStartTime.Ticks, Map.Name)
        IO.Directory.CreateDirectory(StatsDirectoryPath)

        Table = New DataTable("CourierSimulation")

        Dim TimeColumn As New DataColumn("Time", GetType(Integer))
        Table.Columns.Add(TimeColumn)
        Table.PrimaryKey = New DataColumn() {TimeColumn} 'Must be specified as array

        Table.Columns.Add("CompletedJobsCount", GetType(Integer))
        Table.Columns.Add("IncompleteJobsCount", GetType(Integer))
        Table.Columns.Add("RefusedJobsCount", GetType(Integer))

        Table.Columns.Add("LateJobsCount", GetType(Integer))
        Table.Columns.Add("CumulativeTimeEarly", GetType(Integer))
        Table.Columns.Add("CumulativeTimeLate", GetType(Integer))

        Table.Columns.Add("CurrentActiveVehicles", GetType(Integer))
        Table.Columns.Add("CurrentAverageVehicleSpeed", GetType(Double))
        Table.Columns.Add("CurrentAverageVehicleSaturation", GetType(Double))
        Table.Columns.Add("CurrentTotalSaturation", GetType(Double))

        Table.Columns.Add("CumulativeDrivingDistance", GetType(Double))
        Table.Columns.Add("CumulativeDrivingHours", GetType(Double))

        Table.Columns.Add("CumulativeCost", GetType(Decimal))
        Table.Columns.Add("CumulativeRevenue", GetType(Decimal))
        Table.Columns.Add("CumulativeProfit", GetType(Decimal))
    End Sub

    Sub Log(ByVal Agents As List(Of Agent))
        Dim Row As DataRow = Table.NewRow
        Row("Time") = NoticeBoard.CurrentTime.TotalSeconds

        Row("CompletedJobsCount") = NoticeBoard.CompletedJobs.Count
        Row("IncompleteJobsCount") = NoticeBoard.IncompleteJobs.Count
        Row("RefusedJobsCount") = NoticeBoard.RefusedJobs.Count

        Row("LateJobsCount") = NoticeBoard.LateJobs
        Row("CumulativeTimeEarly") = NoticeBoard.TotalTimeEarly
        Row("CumulativeTimeLate") = NoticeBoard.TotalTimeLate

        If Agents.Count > 0 Then
            Row("CurrentActiveVehicles") = Agents.Sum(Function(x)
                                                          Return Math.Min(x.Plan.WayPoints.Count, 1)
                                                      End Function)
            Row("CurrentAverageVehicleSpeed") = Agents.Average(Function(x)
                                                                   Return x.CurrentSpeedKMH
                                                               End Function)
            Row("CurrentAverageVehicleSaturation") = Agents.Average(Function(x)
                                                                        Return x.Plan.CapacityLeft
                                                                    End Function)
            Row("CurrentTotalSaturation") = Agents.Sum(Function(x)
                                                           Return x.GetVehicleMaxCapacity - x.GetVehicleCapacityLeft
                                                       End Function)
        End If


        Row("CumulativeCost") = NoticeBoard.FuelBill
        Row("CumulativeRevenue") = NoticeBoard.JobRevenue
        Row("CumulativeProfit") = NoticeBoard.JobRevenue - NoticeBoard.FuelBill

        SyncLock Table
            Table.Rows.Add(Row)
        End SyncLock
    End Sub

    Sub SaveToCSV()
        SyncLock Table
            Try
                Dim FileName As String = StatsDirectoryPath & CSV_FILENAME
                IO.File.Delete(FileName)
                Table.WriteXml(FileName)

                MsgBox(String.Format(CSV_SAVE_MESSAGE_FORMAT, FileName, CInt(0), 0), MsgBoxStyle.OkOnly)
            Catch ex As Exception
                MsgBox("Operation failed: " & ex.Message, MsgBoxStyle.Exclamation)
            End Try
        End SyncLock


    End Sub

End Class
