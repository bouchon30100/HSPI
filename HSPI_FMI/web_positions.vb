Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes
Imports System.Web.Script.Serialization
Imports System.Net

Public Class web_positions
    Inherits clsPageBuilder

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Log("page """ & PageName & """ post: " & data, MessageType.Debug)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)


        Select Case parts("id")
            Case "adresse_new"
                Dim location = parts("adresse_new")
                hs.SaveINISetting("locations", location, " ", INIFILE)
                hs.SaveINISetting(location, "formatted_address", " ", INIFILE)
                hs.SaveINISetting(location, "lat", " ", INIFILE)
                hs.SaveINISetting(location, "lng", " ", INIFILE)
                hs.SaveINISetting(location, "value", " ", INIFILE)
                hs.SaveINISetting(location, "perimetre", " ", INIFILE)
                divToUpdate.Add("div_adresse", builbTableLocation)
            Case Else
                If (parts("id") IsNot Nothing) Then
                    'si Adersse modifiée
                    If (parts("id").StartsWith("formattedaddress_")) Then
                        Dim location = parts("id").Split("_")(1)
                        Dim wc As WebClient = New WebClient()
                        Dim adresse As String = Extensions.PostDataToWebsite(wc, "https://maps.googleapis.com/maps/api/geocode/json?address=" & parts(parts("id")) & " & key = AIzaSyD6wVi9GPtsx25DPIo2sOLy1SEZ1WQh6rc", "") '"http: //maps.googleapis.com/maps/api/geocode/json?latlng=44.129830,4.095377", "")
                        Dim js = New JavaScriptSerializer()
                        Dim response = js.Deserialize(Of Object)(adresse)
                        Dim content = response("results")
                        hs.SaveINISetting(location, "formatted_address", content(0)("formatted_address"), INIFILE)
                        hs.SaveINISetting(location, "lat", content(0)("geometry")("location")("lat"), INIFILE)
                        hs.SaveINISetting(location, "lng", content(0)("geometry")("location")("lng"), INIFILE)
                        divToUpdate.Add("div_adresse", builbTableLocation)
                    End If

                    'si perimetre modifiée
                    If (parts("id").StartsWith("perimetre_")) Then
                        Dim location = parts("id").Split("_")(1)

                        hs.SaveINISetting(location, "perimetre", parts(parts("id")), INIFILE)
                        divToUpdate.Add("div_adresse", builbTableLocation)
                    End If

                    'si value modifiée
                    If (parts("id").StartsWith("value_")) Then
                        Dim location = parts("id").Split("_")(1)

                        hs.SaveINISetting(location, "value", parts(parts("id")), INIFILE)
                        divToUpdate.Add("div_adresse", builbTableLocation)
                    End If

                    'Si longitude modifiée
                    If (parts("id").StartsWith("lng_")) Then
                        Dim location = parts("id").Split("_")(1)
                        Dim lng = parts(parts("id")).Replace(",", ".")
                        hs.SaveINISetting(location, "lng", lng, INIFILE)
                        Dim lat = hs.GetINISetting(location, "lat", " ", INIFILE).Replace(",", ".")

                        If (lat <> " ") Then
                            Dim wc As WebClient = New WebClient()
                            Dim adresse As String = Extensions.PostDataToWebsite(wc, "http://maps.googleapis.com/maps/api/geocode/json?latlng=" & lat & "," & lng, "")
                            Dim js = New JavaScriptSerializer()
                            Dim response = js.Deserialize(Of Object)(adresse)
                            Dim content = response("results")
                            hs.SaveINISetting(location, "formatted_address", content(0)("formatted_address"), INIFILE)
                            hs.SaveINISetting(location, "lat", content(0)("geometry")("location")("lat"), INIFILE)
                            hs.SaveINISetting(location, "lng", content(0)("geometry")("location")("lng"), INIFILE)
                            divToUpdate.Add("div_adresse", builbTableLocation)
                        End If
                    End If

                    'si Latitiude modifiée
                    If (parts("id").StartsWith("lat_")) Then
                        Dim location = parts("id").Split("_")(1)
                        Dim lat = parts(parts("id")).Replace(",", ".")
                        hs.SaveINISetting(location, "lat", lat, INIFILE)
                        Dim lng = hs.GetINISetting(location, "lng", " ", INIFILE).Replace(",", ".")

                        If (lng <> " ") Then
                            Dim wc As WebClient = New WebClient()
                            Dim adresse As String = Extensions.PostDataToWebsite(wc, "http://maps.googleapis.com/maps/api/geocode/json?latlng=" & lat & "," & lng, "")
                            Dim js = New JavaScriptSerializer()
                            Dim response = js.Deserialize(Of Object)(adresse)
                            Dim content = response("results")
                            hs.SaveINISetting(location, "formatted_address", content(0)("formatted_address"), INIFILE)
                            hs.SaveINISetting(location, "lat", content(0)("geometry")("location")("lat"), INIFILE)
                            hs.SaveINISetting(location, "lng", content(0)("geometry")("location")("lng"), INIFILE)
                            divToUpdate.Add("div_adresse", builbTableLocation)
                        End If
                    End If

                    'si supp
                    If (parts("id").StartsWith("Supp_")) Then
                        Dim location = parts("id").Split("_")(1)
                        'Dim locations() = hs.GetINISectionEx("locations", INIFILE)
                        'hs.ClearINISection("locations", INIFILE)

                        'For Each loc As String In locations
                        '    If (loc.Split("=")(0) <> location) Then
                        '        hs.SaveINISetting("locations", loc.Split("=")(0), " ", INIFILE)
                        '    End If
                        'Next
                        hs.SaveINISetting("locations", location, "", INIFILE)
                        hs.ClearINISection(location, INIFILE)

                        divToUpdate.Add("div_adresse", builbTableLocation)
                    End If

                    'si Location modifiée
                    If (parts("id").StartsWith("location_")) Then
                        Dim location = parts("id").Split("_")(1)
                        hs.SaveINISetting(location, "location", parts(parts("id")), INIFILE)
                    End If

                    'si Location2 modifiée
                    If (parts("id").StartsWith("location2_")) Then
                        Dim location = parts("id").Split("_")(1)
                        hs.SaveINISetting(location, "location2", parts(parts("id")), INIFILE)
                        divToUpdate.Add("div_adresse", builbTableLocation)
                    End If

                    'si string modifiée
                    If (parts("id").StartsWith("string_")) Then
                        Dim location = parts("id").Split("_")(1)

                        hs.SaveINISetting(location, "string", parts(parts("id")), INIFILE)
                        divToUpdate.Add("div_adresse", builbTableLocation)
                    End If
                End If

        End Select

        Return MyBase.postBackProc(page, data, user, userRights)
    End Function

    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        Dim stb As New StringBuilder
        Dim instancetext As String = ""
        Try

            Me.reset()

            CurrentPage = Me

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)
            End If
            If Instance <> "" Then instancetext = " - " & Instance
            Me.AddHeader(hs.GetPageHeader(pageName, "Find my iPhone" & " Configuration", "", "", True, False, True))
            stb.Append(hs.GetPageHeader(pageName, "Find my iPhone" & " Configuration", "", "", False, True, False, True))

            stb.Append(clsPageBuilder.DivStart("div_adresse", ""))

            stb.Append(builbTableLocation())
            'stb.Append(DivStart("div_comptes", "style=""text-align: center;display: inline-block;"""))
            '   stb.Append(builbTableComptes)
            '  stb.Append(DivEnd())

            stb.Append(DivEnd())

            Me.AddBody(stb.ToString)
            Me.AddFooter(hs.GetPageFooter)
            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function

    '**********************************************************
    ' construction du bloc location
    '**********************************************************
    Private Function builbTableLocation() As String

        Dim stb As New StringBuilder()
        Dim locations() = hs.GetINISectionEx("locations", INIFILE)
        For Each location In locations

            location = location.Split("=")(0)
            Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
            tab.addRow()


            Dim buttonDelete As New clsJQuery.jqButton("Supp_" & location, "Supprimer", PageName, False)
            buttonDelete.imagePathNormal = "../images/HomeSeer/ui/Delete.png"
            buttonDelete.AddStyle("float:right")
            tab.addCell("<div style='float:left;'>" & location & "</div>" & buttonDelete.Build, "", 2)
            tab.addEmptyRow(5, "../images/Default/c.png")

            For Each paramètre As String In hs.GetINISectionEx(location, INIFILE)
                paramètre = paramètre.Split("=")(0)
                tab.addRow()
                tab.addCell(traduire(paramètre.Split("=")(0)), "", 1)
                tab.addEmptyCell(2)
                Dim tb As New clsJQuery.jqTextBox(paramètre.Replace("_", "") & "_" & location, "text", hs.GetINISetting(location, paramètre, "", INIFILE), PageName, 30, False)
                tb.id = paramètre.Replace("_", "") & "_" & location
                tab.addCell(tb.Build, "", 1)


                ' tab.addCell(getSelectorStatus(compte & "_" & Str.Split("=")(0)).Build, "", 1)

            Next


            stb.Append(tab.GetHTML)
        Next

        Dim tab1 As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab1.addRow()
        Dim tb1 As New clsJQuery.jqTextBox("adresse_new", "text", "Nouvelle adresse", PageName, 30, False)
        tb1.id = "adresse_new"

        tab1.addCell(tb1.Build, "", 2)

        stb.Append(tab1.GetHTML)
        Return stb.ToString
    End Function

    Function traduire(texte As String) As String
        Select Case texte
            Case "formatted_address"
                texte = "Adresse"
            Case "lat"
                texte = "Latitude"
            Case "lng"
                texte = "Longitude"
            Case "value"
                texte = "Valeur du status"
            Case "perimetre"
                texte = "Périmètre"


        End Select
        Return texte



    End Function


End Class

