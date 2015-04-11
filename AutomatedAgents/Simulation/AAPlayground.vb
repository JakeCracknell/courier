Public Class AAPlayground
    Inherits AASimulation

    Public Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        NoticeBoard.Clear()
        InitialiseLoggingModules()
    End Sub

    Public Overrides Sub AddAgent()
        Dim Agent As New AgentAsync(Agents.Count, Map, GetSequentialColor)
        Agents.Add(Agent)
    End Sub

End Class
