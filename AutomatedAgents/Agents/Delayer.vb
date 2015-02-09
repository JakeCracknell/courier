Public Class Delayer
    Private _TimeLeft As Integer
    Public ReadOnly Property TimeLeft() As Integer
        Get
            Return _TimeLeft
        End Get
    End Property

    Public ReadOnly TotalTime As Integer

    Sub New()
        Me.New(0)
    End Sub

    Sub New(ByVal TimeToWait As Integer)
        _TimeLeft = TimeToWait
        TotalTime = TimeToWait
    End Sub

    'Returns true if waiting time has elapsed
    Function Tick() As Boolean
        _TimeLeft -= 1
        Return TimeLeft <= 0
    End Function

    Function GetPercentage() As Double
        If TotalTime > 0 Then
            Return (TotalTime - _TimeLeft) / TotalTime
        Else
            Return 1
        End If
    End Function

    Function IsWaiting() As Boolean
        Return TimeLeft <= 0
    End Function
End Class
