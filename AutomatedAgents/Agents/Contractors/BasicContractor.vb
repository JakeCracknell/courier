Public Class BasicContractor
    Implements IContractor

    Private JobToCollect As CourierJob = Nothing
    Private ID As Integer

    Sub New(ByVal Agent As Agent)
        Me.ID = Agent.AgentID
    End Sub

    Public Sub AllocateJob(ByVal Job As CourierJob)
        JobToCollect = Job
    End Sub

    Public Function CollectJob() As CourierJob Implements IContractor.CollectJob
        Dim Temp As CourierJob = JobToCollect
        JobToCollect = Nothing
        Return Temp
    End Function

    Public Function GetID() As Integer Implements IContractor.GetID
        Return ID
    End Function
End Class
