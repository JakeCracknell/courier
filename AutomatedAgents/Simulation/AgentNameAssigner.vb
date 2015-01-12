Module AgentNameAssigner
    Private AllNames As String() = My.Resources.ResourceManager.GetString("AgentNames").Split(vbNewLine)
    Private NamesToAssign As New List(Of String)

    Function AssignAgentName()
        If NamesToAssign.Count = 0 Then
            NamesToAssign = AllNames.ToList
        End If

        Dim Index As Integer = Int(Rnd() * NamesToAssign.Count)
        Dim Name As String = NamesToAssign(Index).Trim
        NamesToAssign.RemoveAt(Index)
        Return Name
    End Function
End Module
