Public Class CityDispatcher
    Implements IDispatcher

    Private Map As StreetMap
    Private ReadOnly WeekdayProbabilities As Double() = {1, 1, 1, 1, 1, 1, 1, 10, 15, 20, 20, 20, 20, 20, 20, 20, 18, 14, 10, 6, 4, 3, 2, 1}
    Private ReadOnly WeekendProbabilities As Double() = {1, 1, 1, 1, 1, 1, 1, 5, 7, 10, 10, 10, 10, 10, 10, 10, 9, 7, 5, 3, 2, 1, 1, 1}
    Private Const SECONDS_IN_HOUR As Integer = 3600
    Private RandomNumberGenerator As New Random(44)

    Sub New(ByVal Map As StreetMap)
        Me.Map = Map
    End Sub

    'Simulation time starts on a MONDAY at midnight.
    Public Sub Tick() Implements IDispatcher.Tick
        Dim DayOfWeek As DayOfWeek = (Int(NoticeBoard.CurrentTime.TotalDays) + 1) Mod 7
        Dim TimeOfDay As TimeSpan = TimeSpan.FromSeconds(NoticeBoard.CurrentTime.TotalSeconds Mod TimeSpan.FromDays(1).TotalSeconds)
        SimulationParameters.DisplayedDebugVariable = DayOfWeek.ToString("G") & " " & TimeOfDay.ToString

        Dim ProbabilityOfDispatch As Double
        Select Case DayOfWeek
            Case DayOfWeek.Saturday, DayOfWeek.Sunday
                ProbabilityOfDispatch = WeekendProbabilities(TimeOfDay.Hours)
            Case Else
                ProbabilityOfDispatch = WeekdayProbabilities(TimeOfDay.Hours)
        End Select
        ProbabilityOfDispatch = ProbabilityOfDispatch * SimulationParameters.DispatchRateCoefficient / SECONDS_IN_HOUR

        'A Bernoulli(P) distribution, where P varies by day and hour.
        If RandomNumberGenerator.NextDouble < ProbabilityOfDispatch Then
            Select Case TimeOfDay.Hours
                Case Is <= 6, Is >= 18
                    'Order generated outside of business hours - location is random, deadline more scattered
                    Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                          Map.NodesAdjacencyList.GetRandomPoint)
                    NoticeBoard.AddJob(Job)
                Case Else
                    'Order generated inside of business hours - location is ?????, deadline likely to be end of business day
                    Dim Job As New CourierJob(Map.NodesAdjacencyList.GetRandomPoint, _
                                          Map.NodesAdjacencyList.GetRandomPoint)
                    NoticeBoard.AddJob(Job)
            End Select
        Else
            'No job is dispatched.
        End If




    End Sub
End Class
