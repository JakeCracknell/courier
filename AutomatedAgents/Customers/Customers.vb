Namespace Customers
    Module Customers
        Public ReadOnly WaitTimeMaxSeconds As Integer = 120
        Public ReadOnly WaitTimeMaxHours As Double = WaitTimeMaxSeconds / 3600
        Public ReadOnly WaitTimeMax As TimeSpan = TimeSpan.FromSeconds(WaitTimeMaxSeconds)

        Public ReadOnly WaitTimeMinSeconds As Integer = 30
        Public ReadOnly WaitTimeMinHours As Double = WaitTimeMinHours / 3600
        Public ReadOnly WaitTimeMin As TimeSpan = TimeSpan.FromSeconds(WaitTimeMinSeconds)

        Public ReadOnly WaitTimeAvgSeconds As Integer = (WaitTimeMaxSeconds - WaitTimeMinSeconds) / 2
        Public ReadOnly WaitTimeAvgHours As Double = WaitTimeAvgSeconds / 3600
        Public ReadOnly WaitTimeAvg As TimeSpan = TimeSpan.FromSeconds(WaitTimeAvgSeconds)
    End Module
End Namespace
