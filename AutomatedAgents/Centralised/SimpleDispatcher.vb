Class SimpleDispatcher
    Private Map As StreetMap
    Private WaitingJobs As List(Of CourierJob)

    Sub New(ByVal Map As StreetMap)
        Map = Map
    End Sub

    Sub Tick()
        If Rnd() < 0.01 Then
            'Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomNode, Map.NodesAdjacencyList.GetRandomNode)
            'WaitingJobs
        End If
    End Sub

    Function GetWaitingNodes() As List(Of Node)
        Return Nothing
    End Function
End Class
