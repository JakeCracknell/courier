Module UIDAssigner
    Private Const START_ID As Integer = 1000
    Private IDs As New Dictionary(Of String, Integer)
    Function NewID(ByVal type As String, Optional ByVal MinID As Integer = START_ID) As Integer
        If Not IDs.ContainsKey(type) Then
            IDs.Add(type, MinID)
        End If
        Dim ID As Integer = IDs(type)
        IDs(type) += 1
        Return ID
    End Function
End Module
