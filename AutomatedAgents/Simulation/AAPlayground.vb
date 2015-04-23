Public Class AAPlayground
    Inherits AASimulation

    Public Sub New(ByVal Map As StreetMap)
        Me.Map = Map
        NoticeBoard.Initialise()
        InitialiseLoggingModules()
    End Sub

    Public Overrides Sub AddAgent()
        Dim ID As Integer = UIDAssigner.NewID("playground", 1000)
        Dim Agent As New AgentAsync(ID, Map, GetSequentialColor(ID))
        Agents.Add(Agent)
    End Sub

End Class
