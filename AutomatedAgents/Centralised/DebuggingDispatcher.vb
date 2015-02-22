Public Class DebuggingDispatcher
    Implements IDispatcher
    Private Map As StreetMap

    Sub New(Map As StreetMap)
        Me.Map = Map


    End Sub

    Sub Tick() Implements IDispatcher.Tick

        If NoticeBoard.IncompleteJobs.Count = 0 Then
            Dim RosslynAve As HopPosition = Map.NodesAdjacencyList.GetPointByNodeID(2525720829)
            Dim AvenueGardens As HopPosition = Map.NodesAdjacencyList.GetPointByNodeID(32045458)
            Dim Woodlands As HopPosition = Map.NodesAdjacencyList.GetPointByNodeID(21095820)
            Dim Railwayside As HopPosition = Map.NodesAdjacencyList.GetPointByNodeID(30947897)


            Dim Job As New CourierJob(RosslynAve, Railwayside, 0.01)
            NoticeBoard.AddJob(Job)
        End If
    End Sub
End Class