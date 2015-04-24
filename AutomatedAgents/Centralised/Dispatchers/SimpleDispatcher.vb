Class SimpleDispatcher
    Implements IDispatcher
    Private Map As StreetMap

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    Public Function Tick() As Boolean Implements IDispatcher.Tick
        If RNG.R("dispatcher").NextDouble < SimulationParameters.DispatchRateCoefficient / 3600 Then
            Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                      Map.NodesAdjacencyList.GetRandomPoint)
            NoticeBoard.PostJob(Job)
            Return True
        End If
        Return False
    End Function

End Class
