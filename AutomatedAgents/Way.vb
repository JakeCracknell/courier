Public Class Way
    Public ID As Long
    Public Nodes As Node()
    Public Type As WayType

    Public Sub New(ByVal ID As Integer, ByVal Nodes As Node(), ByVal Type As WayType)
        Me.ID = ID
        Me.Nodes = Nodes
        Me.Type = Type
    End Sub
End Class
