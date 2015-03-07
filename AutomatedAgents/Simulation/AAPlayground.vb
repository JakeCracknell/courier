Public Class AAPlayground
    Inherits AASimulation

    Public Overrides Sub AddAgent(ByVal Map As StreetMap)
        Dim Agent As New AgentAsync(Map, GetSequentialColor)
        Agents.Add(Agent)
    End Sub

End Class
