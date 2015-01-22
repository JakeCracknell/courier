Public Class CoordinateConverter
    Private Bounds As Bounds
    Private PanelWidth As Integer
    Private PanelHeight As Integer
    Private ZoomAmount As Decimal = 1

    Private LongLength As Double
    Private LatHeight As Double

    Public Sub New(ByVal Bounds As Bounds, ByVal PanelWidth As Integer, ByVal PanelHeight As Integer)
        Me.Bounds = Bounds
        Me.PanelHeight = PanelHeight
        Me.PanelWidth = PanelWidth
        LongLength = Bounds.MaxLongitude - Bounds.MinLongitude
        LatHeight = Bounds.MaxLatitude - Bounds.MinLatitude
    End Sub

    Public Sub SetZoom(ByVal ZoomAmount As Decimal)
        Me.ZoomAmount = ZoomAmount
        Throw New NotImplementedException
    End Sub

    Public Function GetPoint(ByVal RoutePosition As RoutePosition) As Point
        Dim Lat, Lon As Double
        Dim X As Node = RoutePosition.GetOldNode
        Dim Y As Node = RoutePosition.GetNextNode

        If X.Latitude < Y.Latitude Then
            Lat = X.Latitude + (Y.Latitude - X.Latitude) * RoutePosition.PercentageTravelled
        Else
            Lat = Y.Latitude + (X.Latitude - Y.Latitude) * (1 - RoutePosition.PercentageTravelled)
        End If

        If X.Longitude < Y.Longitude Then
            Lon = X.Longitude + (Y.Longitude - X.Longitude) * RoutePosition.PercentageTravelled
        Else
            Lon = Y.Longitude + (X.Longitude - Y.Longitude) * (1 - RoutePosition.PercentageTravelled)
        End If

        Return GetPoint(Lat, Lon)
    End Function

    Public Function GetPoint(ByVal RoutingPoint As IPoint) As Point
        Return GetPoint(RoutingPoint.GetLatitude, RoutingPoint.GetLongitude)
    End Function

    Public Function GetPoint(ByVal Latitude As Double, ByVal Longitude As Double) As Point
        Dim X As Integer = (1 - ((Bounds.MaxLongitude - Longitude) / LongLength)) * PanelWidth
        Dim Y As Integer = ((Bounds.MaxLatitude - Latitude) / LatHeight) * PanelHeight
        Return New Point(X, Y)
    End Function

    Public Function GetNearestNodeFromPoint(ByVal MousePosition As Point, ByVal AdjacencyList As NodesAdjacencyList)
        Dim Longitude As Double = Bounds.MaxLongitude + LongLength * ((MousePosition.X / PanelWidth) - 1)
        Dim Latitiude As Double = Bounds.MaxLatitude - (MousePosition.Y * LatHeight / PanelHeight)
        Return AdjacencyList.GetNearestNode(Latitiude, Longitude)
    End Function
End Class
