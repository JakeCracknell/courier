Module OSMFileSystemManager
    Private OSM_FOLDERS() As String = {"C:\Users\Jake\Downloads\", "maps\", "C:\Users\Jake\Documents\AAMaps"}

    Private Dictionary As New Dictionary(Of String, String)

    Public Function GetAllFilenames() As List(Of String)
        Dim FilenameList As New List(Of String)
        For Each Folder As String In OSM_FOLDERS
            If IO.Directory.Exists(Folder) Then
                Dim FolderToScan As New IO.DirectoryInfo(Folder)
                Dim OSMFiles As IO.FileInfo() = FolderToScan.GetFiles("*.osm")
                For Each OSMFile As IO.FileInfo In OSMFiles
                    Dim Filename As String = OSMFile.Name
                    If Not Dictionary.ContainsKey(OSMFile.Name) Then
                        Dictionary.Add(OSMFile.Name, OSMFile.FullName)
                    End If
                    FilenameList.Add(OSMFile.Name)
                Next
            End If
        Next
        Return FilenameList
    End Function

    Public Function GetFilePathFromName(ByVal Filename As String) As String
        Return Dictionary(Filename)
    End Function


End Module
