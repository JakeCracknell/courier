Module Utility
    Private SequentialColours() As Color = {Color.Blue, Color.Green, Color.Red, Color.Yellow, Color.Pink, Color.Purple, Color.Orange, Color.Brown, Color.Black, Color.Beige, Color.Violet}
    Private SequentialColoursDictionary As New Dictionary(Of Integer, Color)
    Function GetSequentialColor(ByVal ID As Integer) As Color
        If ID < SequentialColours.Length Then
            Return SequentialColours(ID)
        ElseIf SequentialColoursDictionary.ContainsKey(ID) Then
            Return SequentialColoursDictionary(ID)
        Else
            Dim RandomColour As Color = GetRandomColour()
            SequentialColoursDictionary.Add(ID, RandomColour)
            Return RandomColour
        End If
    End Function
    Function GetRandomColour() As Color
        Return Color.FromArgb(Rnd() * 255, Rnd() * 255, Rnd() * 255)
    End Function

    Function GetTimeIndex(ByVal Time As TimeSpan) As Integer
        Dim DayIndex As Integer = Int(Time.TotalDays) Mod 7
        Dim FiveMinuteIndex As Integer = TimeSpan.FromSeconds(Time.TotalSeconds Mod TimeSpan.FromDays(1).TotalSeconds).TotalMinutes \ 5
        Dim TimeIndex As Integer = DayIndex * 288 + FiveMinuteIndex
        Return TimeIndex
    End Function




    Public Sub SetDoubleBuffered(ByVal c As System.Windows.Forms.Control)
        If System.Windows.Forms.SystemInformation.TerminalServerSession Then
            Return
        End If
        Dim aProp As System.Reflection.PropertyInfo = GetType(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
        aProp.SetValue(c, True, Nothing)
    End Sub

    Declare Function GetKeyState Lib "user32" Alias "GetKeyState" (ByVal ByValnVirtKey As Int32) As Int16
    Private Const VK_CAPSLOCK = &H14
    Private CapsLockOriginalState As Boolean = GetKeyState(VK_CAPSLOCK)
    Public Function IsInDebugMode()
        Return CapsLockOriginalState <> GetKeyState(VK_CAPSLOCK)
    End Function
End Module
