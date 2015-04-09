Public Class AAPlayground
    Inherits AASimulation

    Public Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        InitialiseLogger()
    End Sub

    Public Overrides Sub AddAgent()
        Dim Agent As New AgentAsync(Map, GetSequentialColor)
        Agents.Add(Agent)
    End Sub

End Class
