﻿Public Class Node
    Implements IEquatable(Of Node)
    Public ID As Long
    Public Latitude As Double
    Public Longitude As Double


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
        Return Me.ID = OtherNode.ID
    End Function
End Class
