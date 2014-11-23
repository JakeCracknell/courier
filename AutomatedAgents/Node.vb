Public Class Node
    Public ID As Long
    Public Latitude As Double
    Public Longitude As Double


    Public Sub New(ByVal ID As Long, ByVal Latitiude As Double, ByVal Longitude As Double)
        Me.ID = ID
        Me.Latitude = Latitiude
        Me.Longitude = Longitude
    End Sub

End Class
