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

    Function GetCentralPointInPlane(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As PointF
        Dim MinX As Double = Math.Min(X1, X2)
        Dim MaxX As Double = Math.Max(X1, X2)
        Dim MinY As Double = Math.Min(Y1, Y2)
        Dim MaxY As Double = Math.Max(Y1, Y2)

        Return New PointF(MinX + (MaxX - MinX) / 2, MinY + (MaxY - MinY) / 2)
    End Function

    Function GetMidpointOfTwoPoints(ByVal p1 As IPoint, ByVal p2 As IPoint) As PointF
        Return GetMidpointOfTwoPoints(p1.GetLatitude, p1.GetLatitude, p2.GetLongitude, p2.GetLongitude)
    End Function
    Function GetMidpointOfTwoPoints(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As PointF
        Return New PointF((X1 + X2) / 2, (Y1 + Y2) / 2)
    End Function

    Public Function Gaussian(Optional mu As Double = 0, Optional sigma As Double = 1) As Double
        Dim r As New Random
        Dim u1 = r.NextDouble()
        Dim u2 = r.NextDouble()

        Dim rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)

        Dim rand_normal = mu + sigma * rand_std_normal

        Return rand_normal
    End Function

    Public Function Exponential(ByVal lambda As Double) As Double
        Return Exponential(lambda, Rnd())
    End Function
    Public Function Exponential(ByVal lambda As Double, ByVal UniformlyDistributedRV As Double) As Double
        Return (Math.Log(1 - UniformlyDistributedRV)) / (-lambda) 'The inversion method
    End Function

    'http://www.codeproject.com/Articles/15102/NET-random-number-generators-and-distributions
    Private RNGForGamma As New Random 'TODO: central random number factory
    Public Function Gamma(ByVal alpha As Double, ByVal theta As Double) As Double
        Dim helper1 As Double = alpha - Math.Floor(alpha)
        Dim helper2 As Double = Math.E / (Math.E + helper1)

        Dim xi As Double, eta As Double, gen1 As Double, gen2 As Double
        Do
            gen1 = 1.0 - RNGForGamma.NextDouble()
            gen2 = 1.0 - RNGForGamma.NextDouble()
            If gen1 <= helper2 Then
                xi = Math.Pow(gen1 / helper2, 1.0 / helper1)
                eta = gen2 * Math.Pow(xi, helper1 - 1.0)
            Else
                xi = 1.0 - Math.Log((gen1 - helper2) / (1.0 - helper2))
                eta = gen2 * Math.Pow(Math.E, -xi)
            End If
        Loop While eta > Math.Pow(xi, helper1 - 1.0) * Math.Pow(Math.E, -xi)

        For i As Integer = 1 To alpha
            xi -= Math.Log(RNGForGamma.NextDouble())
        Next

        Return xi * theta
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
