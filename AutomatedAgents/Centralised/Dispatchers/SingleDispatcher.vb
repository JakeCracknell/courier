
Class SingleDispatcher
    Implements IDispatcher
    Private Map As StreetMap

    Sub New(Map As StreetMap)
        Me.Map = Map
    End Sub

    Public Function Tick() As Boolean Implements IDispatcher.Tick
        If NoticeBoard.IncompleteJobs.Count = 0 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                   Map.NodesAdjacencyList.GetRandomPoint)
            NoticeBoard.PostJob(Job)
            Return True
        End If
        Return False
    End Function
End Class
