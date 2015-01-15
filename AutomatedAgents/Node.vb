Public Class Node
    Implements IEquatable(Of Node)
    Implements RoutingPoint
    Public ID As Long
    Public Latitude As Double
    Public Longitude As Double

    Public Connected As Boolean = True

    Private AgentTraffic As Integer = 0
    Public Shared TotalNodesTraffic As Integer = 1

    Public Sub New(ByVal ID As Long, ByVal Latitiude As Double, ByVal Longitude As Double)
        Me.ID = ID
        Me.Latitude = Latitiude
        Me.Longitude = Longitude
    End Sub

    Public Shared Operator =(ByVal Node1 As Node, ByVal Node2 As Node) As Boolean
        Return Node1.Equals(Node2)
    End Operator

    Public Shared Operator <>(ByVal Node1 As Node, ByVal Node2 As Node) As Boolean
        Return Not Node1.Equals(Node2)
    End Operator

    Public Overloads Function Equals(ByVal OtherNode As Node) As Boolean _
        Implements System.IEquatable(Of Node).Equals
        Return OtherNode IsNot Nothing AndAlso Me.ID = OtherNode.ID
    End Function

    Public Overloads Function GetLongitude() As Double Implements RoutingPoint.GetLongitude
        Return Longitude
    End Function
    Public Overloads Function GetLatitude() As Double Implements RoutingPoint.GetLatitude
        Return Latitude
    End Function

    Public Overloads Function ToString() As String
        Return ID
    End Function

    Public Sub VisitNode()
        AgentTraffic += 1
        TotalNodesTraffic += 1
    End Sub

    Public Function GetAgentTraffic() As Integer
        Return AgentTraffic
    End Function
End Class
