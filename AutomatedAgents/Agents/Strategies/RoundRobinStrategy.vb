Public Class RoundRobinStrategy
    Inherits AgentStrategy

    Private Contractor As BasicContractor

    Public Sub New(ByVal Agent As Agent)
        MyBase.New(Agent)
        Contractor = New BasicContractor(Agent)
        NoticeBoard.Broadcaster.RegisterContractor(Contractor)
    End Sub

    Overrides Sub Run()
        CollectJobs()

        If Agent.Plan.IsIdle() Then
            Exit Sub
        End If

        'No need to recompute A* route to first waypoint.
        Agent.Plan.Update(False)

        'Periodically, check for changed traffic conditions that might warrant a replan.
        PeriodicReplan(ContractNetPolicy.CNP4)

        'If a route somewhere has just been completed...
        RouteCompletion(ContractNetPolicy.CNP4)
    End Sub

    Public Overrides Sub CollectJobs()
        Dim NewJob As CourierJob = Contractor.CollectJob
        If NewJob IsNot Nothing Then
            Agent.Plan.Update(True)
            Dim Planner As New NNGAPlanner(Agent, True, NewJob)
            Agent.Plan = Planner.GetPlan
            Agent.PickupPoints.Add(NewJob.PickupPosition)
        End If
    End Sub
End Class

