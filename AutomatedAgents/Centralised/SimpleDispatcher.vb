Class SimpleDispatcher
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    Sub Tick()
        If Rnd() < DispatchRatePerHour / 3600 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, Map.NodesAdjacencyList.GetRandomPoint)
            NoticeBoard.AddJob(Job)
        End If
    End Sub

End Class
