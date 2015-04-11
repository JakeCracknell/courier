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

        Public Sub Initialise()
            Events = New List(Of LogEvent)
        End Sub

        Public Sub NewEvent(ByVal Description As String)
            NewEvent(-1, Description)
        End Sub

        Public Sub NewEvent(ByVal AgentID As Integer, ByVal Description As String)
            SyncLock Events
                Events.Add(New LogEvent(NoticeBoard.CurrentTime, AgentID, Description))
            End SyncLock
        End Sub

        Public Function GetEvents() As LogEvent()
            Dim EventsArray As LogEvent() = Events.ToArray
            SyncLock Events
                Events.Clear()
            End SyncLock
            Return EventsArray
        End Function
    End Module
End Namespace
