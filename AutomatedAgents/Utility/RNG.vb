Namespace RNG
    Module RNG
        Private RandomNumberGenerators As Dictionary(Of String, Random)

        Sub Initialise()
            RandomNumberGenerators = New Dictionary(Of String, Random)
        End Sub

        Function R(ByVal Key As String) As Random
            If Not RandomNumberGenerators.ContainsKey(Key) Then
                RandomNumberGenerators.Add(Key, New Random(Key.GetHashCode))
            End If
            Return RandomNumberGenerators(Key)
        End Function

    End Module
End Namespace

