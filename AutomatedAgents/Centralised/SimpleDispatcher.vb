Class SimpleDispatcher
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    Sub Tick()
        If Rnd() < 0.01 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, Map.NodesAdjacencyList.GetRandomPoint)
            NoticeBoard.WaitingJobs.Add(Job)
        End If
    End Sub

End Class
