
Class SingleDispatcher
    Implements IDispatcher
    Private Map As StreetMap

    Sub New(Map As StreetMap)
        Me.Map = Map
    End Sub

    Sub Tick() Implements IDispatcher.Tick
        If NoticeBoard.IncompleteJobs.Count = 0 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                   Map.NodesAdjacencyList.GetRandomPoint, _
                                   Rnd() * 0.5)
            NoticeBoard.AddJob(Job)
        End If
    End Sub
End Class
