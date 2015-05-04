Public Class CoordinateConverter
    Private Bounds As Bounds
    Private PanelWidth As Integer
    Private PanelHeight As Integer

    Private LongLength As Double
    Private LatHeight As Double

    Private MaxLat As Double
    Private MinLat As Double

    Public Sub New(ByVal Bounds As Bounds, ByVal PanelWidth As Integer, ByVal PanelHeight As Integer)
        Me.Bounds = Bounds
        Me.PanelHeight = PanelHeight
        Me.PanelWidth = PanelWidth
        LongLength = Bounds.MaxLongitude - Bounds.MinLongitude
        MinLat = 180.0 / Math.PI * Math.Log(Math.Tan(Math.PI / 4.0 + Bounds.MinLatitude * (System.Math.PI / 180.0) / 2))
        MaxLat = 180.0 / Math.PI * Math.Log(Math.Tan(Math.PI / 4.0 + Bounds.MaxLatitude * (System.Math.PI / 180.0) / 2))
        LatHeight = MaxLat - MinLat
    End Sub

    Public Function GetPoint(ByVal RoutingPoint As IPoint) As Point
        Return GetPoint(RoutingPoint.GetLatitude, RoutingPoint.GetLongitude)
    End Function

    Public Function GetPoint(ByVal Latitude As Double, ByVal Longitude As Double) As Point
        Dim TransformedLong As Double = Longitude - Bounds.MinLongitude
        Dim X As Integer = (TransformedLong / LongLength) * PanelWidth
        Dim TransformedLat As Double = 180.0 / Math.PI * Math.Log(Math.Tan(Math.PI / 4.0 + Latitude * (Math.PI / 180.0) / 2))
        Dim Y As Integer = (1 - (TransformedLat - MinLat) / LatHeight) * PanelHeight

        Return New Point(X, Y)
    End Function

    Public Function GetNearestNodeFromPoint(ByVal MousePosition As Point, ByVal Grid As NodesGrid)
        Dim Longitude As Double = Bounds.MaxLongitude + LongLength * ((MousePosition.X / PanelWidth) - 1)
        Dim Latitiude As Double = Bounds.MaxLatitude - (MousePosition.Y * (Bounds.MaxLatitude - Bounds.MinLatitude) / PanelHeight)
        Grid.FitCoordinates(Latitiude, Longitude)
        Return Grid.GetNearestNode(Latitiude, Longitude)
    End Function
End Class
