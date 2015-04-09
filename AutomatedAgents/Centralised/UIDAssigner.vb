Module UIDAssigner
    Private Const START_ID As Integer = 1000
    Private IDs As New Dictionary(Of String, Integer)
    Function NewID(ByVal type As String)
        If Not IDs.ContainsKey(type) Then
            IDs.Add(type, START_ID)
        End If
        Dim ID As Integer = IDs(type)
        IDs(type) += 1
        Return ID
    End Function
End Module
