Module Utility
    Private SequentialColours() As Color = {Color.Blue, Color.Green, Color.Red, Color.Yellow, Color.Pink, Color.Purple, Color.Orange, Color.Brown, Color.Black, Color.Beige, Color.Violet}
    Private SequentialColoursIndex As Integer = 0

    Function GetSequentialColor() As Color
        If SequentialColoursIndex < SequentialColours.Length Then
            Dim Color As Color = SequentialColours(SequentialColoursIndex)
            SequentialColoursIndex += 1
            Return Color
        Else
            Return GetRandomColor()
        End If
    End Function
    Function GetRandomColor() As Color
        Return Color.FromArgb(Rnd() * 255, Rnd() * 255, Rnd() * 255)
    End Function

    Function HaversineDistance(ByVal Node1 As IPoint, ByVal Node2 As IPoint) As Double
        Return HaversineDistance(Node1.GetLatitude, Node1.GetLongitude, Node2.GetLatitude, Node2.GetLongitude)
    End Function
    Function HaversineDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Dim dDistance As Double
        Dim dLat1InRad As Double = lat1 * (Math.PI / 180.0)
        Dim dLong1InRad As Double = lon1 * (Math.PI / 180.0)
        Dim dLat2InRad As Double = lat2 * (Math.PI / 180.0)
        Dim dLong2InRad As Double = lon2 * (Math.PI / 180.0)

        Dim dLongitude As Double = dLong2InRad - dLong1InRad
        Dim dLatitude As Double = dLat2InRad - dLat1InRad

        ' Intermediate result a.
        Dim a As Double = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2.0), 2.0)

        ' Intermediate result c (great circle distance in Radians).
        Dim c As Double = 2.0 * Math.Asin(Math.Sqrt(a))

        ' Distance.
        Const kEarthRadiusKms As Double = 6371
        dDistance = kEarthRadiusKms * c

        Return dDistance
    End Function

    'Inaccurate. Can be up to 50m above actual sometimes
    Function GetFuzzyDistance(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Return Math.Sqrt((lat1 - lat2) ^ 2 + (lon1 - lon2) ^ 2) * 111.2
    End Function

    Public Function Gaussian(Optional mu As Double = 0, Optional sigma As Double = 1) As Double
        Dim r As New Random
        Dim u1 = r.NextDouble()
        Dim u2 = r.NextDouble()

        Dim rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)

        Dim rand_normal = mu + sigma * rand_std_normal

        Return rand_normal
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
