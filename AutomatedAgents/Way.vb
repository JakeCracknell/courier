Public Class Way
    Implements IEquatable(Of Way)
    Public ID As Long
    Public Nodes As Node()
    Public Type As WayType

    Public OneWay As Boolean

    Public Sub New(ByVal ID As Integer, ByVal Nodes As Node(), ByVal Type As WayType)
        Me.ID = ID
        Me.Nodes = Nodes
        Me.Type = Type
    End Sub

    Public Shared Operator =(ByVal Way1 As Way, ByVal Way2 As Way) As Boolean
        Return Way1.Equals(Way2)
    End Operator

    Public Shared Operator <>(ByVal Way1 As Way, ByVal Way2 As Way) As Boolean
        Return Not Way1.Equals(Way2)
    End Operator

    Public Overloads Function Equals(ByVal Way1 As Way) As Boolean _
        Implements System.IEquatable(Of Way).Equals
        Return Me.ID = Way1.ID
    End Function

    Public Sub SetOneWay(ByVal Value As String)
        Select Case Value
            Case "yes", "true", "1"
                'yes, Most common tag
                OneWay = True
            Case "no", "false", "0"
                'Superfluous tag, sometimes used to confirm a bidriectional street, against mapping errors.
                OneWay = False
            Case "-1", "reverse"
                'One way in opposite direction of node list
                Nodes.Reverse()
                OneWay = True
            Case Else
                Debug.WriteLine("I cannot categorise this oneway tag: " & Value)
        End Select
    End Sub
End Class
