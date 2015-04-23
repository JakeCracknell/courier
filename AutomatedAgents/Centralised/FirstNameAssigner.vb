Module FirstNameAssigner
    'Source: http://deron.meranda.us/data/census-derived-all-first.txt
    Private AllNames As String() = My.Resources.ResourceManager.GetString("AgentNames").Split(vbNewLine)
    Private NamesToAssign As New List(Of String)

    Function AssignName() As String
        If NamesToAssign.Count = 0 Then
            NamesToAssign = AllNames.ToList
        End If

        Dim Index As Integer = Int(RNG.R("first_name").NextDouble * NamesToAssign.Count)
        Dim Name As String = NamesToAssign(Index).Trim
        NamesToAssign.RemoveAt(Index)
        Return Name
    End Function
End Module
