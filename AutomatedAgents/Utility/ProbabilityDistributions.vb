Namespace ProbabilityDistributions
    Module ProbabilityDistributions

        Public Function Gaussian(ByVal RNG As Random, Optional mu As Double = 0, Optional sigma As Double = 1) As Double
            Dim u1 = RNG.NextDouble()
            Dim u2 = RNG.NextDouble()

            Dim rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)

            Dim rand_normal = mu + sigma * rand_std_normal

            Return rand_normal
        End Function

        Public Function Exponential(ByVal lambda As Double) As Double
            Return Exponential(lambda, RNG.R("exp").NextDouble)
        End Function
        Public Function Exponential(ByVal lambda As Double, ByVal UniformlyDistributedRV As Double) As Double
            Return (Math.Log(1 - UniformlyDistributedRV)) / (-lambda) 'The inversion method
        End Function

        'Adapted from http://www.codeproject.com/Articles/15102/NET-random-number-generators-and-distributions
        Public Function Gamma(ByVal RNG As Random, ByVal alpha As Double, ByVal theta As Double) As Double
            Dim helper1 As Double = alpha - Math.Floor(alpha)
            Dim helper2 As Double = Math.E / (Math.E + helper1)

            Dim xi As Double, eta As Double, gen1 As Double, gen2 As Double
            Do
                gen1 = 1.0 - RNG.NextDouble()
                gen2 = 1.0 - RNG.NextDouble()
                If gen1 <= helper2 Then
                    xi = Math.Pow(gen1 / helper2, 1.0 / helper1)
                    eta = gen2 * Math.Pow(xi, helper1 - 1.0)
                Else
                    xi = 1.0 - Math.Log((gen1 - helper2) / (1.0 - helper2))
                    eta = gen2 * Math.Pow(Math.E, -xi)
                End If
            Loop While eta > Math.Pow(xi, helper1 - 1.0) * Math.Pow(Math.E, -xi)

            For i As Integer = 1 To alpha
                xi -= Math.Log(RNG.NextDouble())
            Next

            Return xi * theta
        End Function

    End Module
End Namespace
