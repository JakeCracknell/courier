Namespace SimulationState
    Module SimulationState

        Structure LogEvent
            Implements IComparable(Of LogEvent)

            Dim Time As TimeSpan
            Dim AgentID As Integer
            Dim Description As String
            Sub New(Time As TimeSpan, AgentID As Integer, Description As String)
                Me.Time = Time
                Me.AgentID = AgentID
                Me.Description = Description
            End Sub

            Public Function CompareTo(other As LogEvent) As Integer Implements IComparable(Of LogEvent).CompareTo
                Return Me.Time.CompareTo(other.Time)
            End Function
        End Structure

        Private Events As List(Of LogEvent)
        Private SimulationStateSyncObject As New Object
        Private AgentStatuses As List(Of List(Of String))
        Private JobStatuses As List(Of List(Of String))

        Public Sub Initialise()
            Events = New List(Of LogEvent)
            AgentStatuses = New List(Of List(Of String))
            JobStatuses = New List(Of List(Of String))
        End Sub

        Public Sub NewEvent(ByVal Description As String)
            NewEvent(-1, Description)
        End Sub

        Public Sub NewEvent(ByVal AgentID As Integer, ByVal Description As String)
            SyncLock Events
                Events.Add(New LogEvent(NoticeBoard.Time, AgentID, Description))
            End SyncLock
        End Sub

        Public Function GetEvents() As LogEvent()
            Dim EventsArray As LogEvent()
            SyncLock Events
                EventsArray = Events.ToArray
                Events.Clear()
            End SyncLock
            Return EventsArray
        End Function

        Public Sub CacheAASimulationStatus(ByVal AASimulation As AASimulation)
            SyncLock SimulationStateSyncObject
                AgentStatuses = New List(Of List(Of String))
                JobStatuses = New List(Of List(Of String))

                For Each Agent As Agent In AASimulation.Agents
                    Dim AgentList As New List(Of String)(13)
                    AgentList.Add(Agent.AgentID)
                    AgentList.Add(Agent.GetVehicleString())
                    AgentList.Add(Agent.Plan.RoutePosition.GetPoint.ToString)
                    AgentList.Add(If(Agent.Plan.WayPoints.Count > 0, Agent.Plan.WayPoints(0).ToString, "idle"))
                    AgentList.Add(Agent.Plan.WayPoints.Count)
                    AgentList.Add(Agent.Plan.GetCurrentJobs.Count)
                    AgentList.Add(Agent.CurrentSpeedKMH)
                    AgentList.Add(Math.Round(Agent.FuelLitres, 2))
                    AgentList.Add(Math.Round(Agent.TotalKMTravelled, 1))
                    AgentList.Add(FormatCurrency(Agent.FuelCosts))
                    AgentList.Add(Math.Round(Agent.GetVehicleCapacityPercentage, 1) & "%")
                    AgentList.Add(Agent.TotalCompletedJobs)
                    AgentStatuses.Add(AgentList)

                    For Each J As CourierJob In Agent.Plan.GetCurrentJobs
                        Dim JobList As New List(Of String)
                        JobList.Add(J.JobID)
                        JobList.Add(Agent.AgentID)
                        JobList.Add(J.PickupName)
                        JobList.Add(If(J.IsGoingToDepot(), "[D] <-", "") & J.DeliveryName)
                        JobList.Add(Math.Round(J.GetDirectRoute.GetKM, 3) & " km, " & J.GetDirectRoute.GetEstimatedTime.ToString("h\:mm\:ss"))
                        JobList.Add(Math.Round(J.CubicMetres, 3))
                        JobList.Add(J.Deadline.ToString("d\:hh\:mm\:ss"))
                        JobList.Add(J.Status.ToString("G"))
                        Dim TimeLeft As TimeSpan = J.Deadline - NoticeBoard.Time
                        JobList.Add(TimeLeft.ToString("h\:mm\:ss") & If(TimeLeft < TimeSpan.Zero, " LATE", ""))
                        JobList.Add(FormatCurrency(J.CustomerFee))
                        'LVI.BackColor = Agent.Color
                        JobStatuses.Add(JobList)
                    Next
                Next
            End SyncLock
        End Sub

        Public Function GetAASimulationStatus() As Tuple(Of List(Of List(Of String)), List(Of List(Of String)))
            SyncLock SimulationStateSyncObject
                Return New Tuple(Of List(Of List(Of String)), List(Of List(Of String)))(AgentStatuses, JobStatuses)
            End SyncLock
        End Function

    End Module
End Namespace
