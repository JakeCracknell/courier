Class SimpleDispatcher
    Implements IDispatcher
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    Sub Tick() Implements IDispatcher.Tick
        If Rnd() < SimulationParameters.DispatchRateCoefficient / 3600 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                      Map.NodesAdjacencyList.GetRandomPoint)
            NoticeBoard.AddJob(Job)
        End If
    End Sub

End Class
