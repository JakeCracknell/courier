
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Xml

Namespace osm2mssql.Library.OsmReader
    Public Class XmlOsmReader
       
        '  Public Function ReadWays(fileName As String) As IEnumerable(Of Way)
        '      Using file__1 = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        '          Using reader = XmlReader.Create(file__1)

        '              reader.MoveToContent()
        '              reader.ReadStartElement("osm")

        '              Dim foundFirstElement As Boolean = False
        '              While reader.IsStartElement()
        '                  If reader.Name <> "way" Then
        '                      reader.Skip()
        '                      If foundFirstElement Then
        '		yield Exit While
        '                      End If
        '                      Continue While
        '                  End If

        '                  foundFirstElement = True
        '                  Dim wayId = Long.Parse(reader.GetAttribute("id"))
        'Dim way = New Way() With { _
        '	Key .WayId = wayId _
        '}
        '                  reader.ReadStartElement()
        '                  ' node
        '                  reader.Skip()

        '                  While reader.Name = "nd"
        '                      Way.NodeRefs.Add(Long.Parse(reader.GetAttribute("ref")))
        '                      reader.ReadStartElement()
        '                      reader.Skip()
        '                  End While

        '                  Dim usedTagTypes = New HashSet(Of Integer)()
        '                  While reader.Name = "tag"
        '                      Dim tagType = attributeRegistry.GetAttributeValueId(OsmAttribute.TagType, reader.GetAttribute("k"))

        '                      If Not usedTagTypes.Contains(tagType) Then
        '		way.Tags.Add(New Tag() With { _
        '			Key .Value = reader.GetAttribute("v"), _
        '			Key .Typ = tagType _
        '		})
        '                          usedTagTypes.Add(tagType)
        '                      End If

        '                      reader.ReadStartElement()
        '                      reader.Skip()
        '                  End While

        '                  If reader.NodeType = XmlNodeType.EndElement Then
        '                      reader.ReadEndElement()
        '                  End If
        'yield Return way
        '              End While
        '          End Using
        '      End Using
        '  End Function

        'Public Function ReadNodes(fileName As String) As List(Of Node)
        'Dim Nodes As New List(Of Node)
        'Using file__1 = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        '    Using reader = XmlReader.Create(file__1)

        '        reader.MoveToContent()
        '        reader.ReadStartElement("osm")

        '        Dim foundFirstElement As Boolean = False
        '        While reader.IsStartElement()
        '            If reader.Name <> "node" Then
        '                reader.Skip()
        '                If foundFirstElement Then
        '                    Exit While
        '                End If
        '                Continue While
        '            End If
        '            foundFirstElement = True
        '            Dim Node As New Node(CLng(reader.GetAttribute("id")), CDbl(reader.GetAttribute("lat")), CDbl(reader.GetAttribute("lon")))

        '            If Bounds.Encloses(Node) Then
        '                reader.ReadStartElement()
        '                reader.Skip()

        '                Dim NodeName As String = Nothing
        '                Dim IsBusiness As Boolean = False
        '                While reader.Name = "tag"
        '                    Dim AttributeName As String = reader.GetAttribute("k")
        '                    If AttributeName = "shop" OrElse AttributeName = "office" Then
        '                        IsBusiness = True
        '                        NodeName = If(NodeName, reader.GetAttribute("v").Replace("_", " ") & " " & AttributeName)
        '                    ElseIf AttributeName = "name" Then
        '                        NodeName = reader.GetAttribute("v")
        '                    End If

        '                    reader.ReadStartElement()
        '                    reader.Skip()
        '                End While

        '                If IsBusiness Then
        '                    Node.Description = NodeName 'might be named something like 'music shop' or 'lawyer office'
        '                    Map.Businesses.Add(Node)
        '                End If

        '                If reader.NodeType = XmlNodeType.EndElement Then
        '                    reader.ReadEndElement()
        '                End If

        '                Map.Nodes.Add(Node)
        '                NodeHashMap.Add(Node.ID, Node)
        '            End If

        '        End While
        '    End Using
        'End Using
        'End Function
    End Class
End Namespace
