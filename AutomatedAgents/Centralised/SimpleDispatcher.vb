Class SimpleDispatcher
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    Sub Tick()
        If Rnd() < SimulationParameters.DispatchRatePerHour / 3600 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                      Map.NodesAdjacencyList.GetRandomPoint, _
                                      Rnd() * 0.5)
            NoticeBoard.AddJob(Job)
        End If
    End Sub

End Class
