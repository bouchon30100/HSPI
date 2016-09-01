Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes
Imports Google.Apis.Auth.OAuth2
Imports System.IO
Imports Google.Apis.Calendar.v3
Imports System.Threading
Imports Google.Apis.Util.Store

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim listLocations(2) As List(Of String)

    ' Dim listeClient As List(Of SarahClient) = New List(Of SarahClient)

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)

    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Console.WriteLine("page post: " & data)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)


        Dim idImage As String = ""
        Dim key As String = ""
        If data.StartsWith("img_") Then
            idImage = data.Split("=")(0)
            key = idImage.Split("_")(1)
        End If

        'gestion de la sélection d'une image --> MAJ de la vignette
        If (parts(idImage) <> Nothing) Then

            Dim appPath = hs.GetAppPath().Replace("\", "/") & "/html/"
            Dim src As String = "../" & parts(idImage).Replace(appPath, "")
            hs.SaveINISetting(key, "IMG", src, INIFILE)
            divToUpdate.Add("div_status", buildTableStatus)
            'divToUpdate.Add("div_img_" & key, "<img id='imgsel" & key & "' src='" & src & "' height=50px width=50px>")
        End If



        Select Case parts("id")
            Case "key_new"
                Dim clé = parts("key_new")

                hs.SaveINISetting("clés", clé, " ", INIFILE)
                hs.SaveINISetting(clé, "Value", " ", INIFILE)
                hs.SaveINISetting(clé, "STATUS", " ", INIFILE)
                hs.SaveINISetting(clé, "IMG", " ", INIFILE)

                divToUpdate.Add("div_status", buildTableStatus)

            Case "LogLevel"
                hs.SaveINISetting("param", "logLevel", parts("LogLevel"), INIFILE)

            Case "adresse_new"
                Dim cpt = parts("adresse_new").Split("@")(0)
                CreateAccount(cpt).Wait()

                hs.SaveINISetting("comptes", cpt, " ", INIFILE)
                'créer un Device pour l'utilisateur --> ref du device
                hs.SaveINISetting(cpt, "ref", "0", INIFILE)

                divToUpdate.Add("div_comptes", builbTableComptes)

            Case Else
                If (parts("id") <> Nothing) Then
                    'gestion de la modification d'un compte
                    If (parts("id").StartsWith("adresse_")) Then
                        Dim cpt = parts("id").Split("_")(1)
                        'Dim comptes() = hs.GetINISectionEx("comptes", INIFILE)
                        'hs.ClearINISection("comptes", INIFILE)
                        'For Each compte In comptes
                        '    If (compte.Split("=")(0) <> cpt) Then
                        '        hs.SaveINISetting("comptes", compte.Split("=")(0), " ", INIFILE)
                        '    Else
                        '        hs.SaveINISetting("comptes", parts(parts("id")), " ", INIFILE)
                        '    End If
                        'Next
                        hs.SaveINISetting("comptes", cpt, "", INIFILE)
                        hs.SaveINISetting("comptes", parts(parts("id")), " ", INIFILE)

                        ' Dim index = parts("id").Split("_")(2)
                        '  hs.SaveINISetting("comptes", index, parts(parts("id")), INIFILE)

                        Dim settings() = hs.GetINISectionEx(cpt, INIFILE)
                        hs.ClearINISection(parts("id").Split("_")(1), INIFILE)
                        For Each str As String In settings
                            hs.SaveINISetting(parts(parts("id")), str.Split("=")(0), str.Split("=")(1), INIFILE)
                        Next
                        DeleteAccount(cpt).Wait()
                        CreateAccount(parts(parts("id"))).Wait()
                        divToUpdate.Add("div_comptes", builbTableComptes)
                    End If

                    'gestion de la supression d'un compte
                    If (parts("id").StartsWith("Supp_")) Then
                        Dim cpt = parts("id").Split("_")(1)
                        '  Dim index = parts("id").Split("_")(2)
                        hs.ClearINISection(cpt, INIFILE)
                        'Dim comptes() = hs.GetINISectionEx("comptes", INIFILE)
                        'hs.ClearINISection("comptes", INIFILE)
                        'For Each compte In comptes
                        '    If (compte.Split("=")(0) <> cpt) Then
                        '        hs.SaveINISetting("comptes", compte.Split("=")(0), " ", INIFILE)
                        '    End If
                        'Next
                        hs.SaveINISetting("comptes", cpt, "", INIFILE)
                        'supression du fichier Credentials
                        DeleteAccount(cpt).Wait()

                        divToUpdate.Add("div_comptes", builbTableComptes)
                    End If

                    'gestion de la modification du Status par DEFAUT
                    If (parts("id").StartsWith("selectStatus_")) Then
                        Dim cpt = parts("id").Split("_")(1)
                        Dim clé = parts("id").Split("_")(2)
                        hs.SaveINISetting(cpt, clé, parts(parts("id")), INIFILE)
                        'divToUpdate.Add("div_comptes", builbTableComptes)
                    End If
                    'gestion de la modification du Device d'un CompteT
                    If (parts("id").StartsWith("ref_")) Then
                        Dim cpt = parts("id").Split("_")(1)
                        hs.SaveINISetting(cpt, "ref", parts(parts("id")), INIFILE)
                        divToUpdate.Add("div_comptes", builbTableComptes)
                    End If

                    'gestion de la modification du Clé elle même
                    If (parts("id").StartsWith("key_")) Then
                        Dim clé = parts("id").Split("_")(1)
                        Dim Newclé = parts(parts("id"))
                        'Dim clés() = hs.GetINISectionEx("clés", INIFILE)
                        'hs.ClearINISection("clés", INIFILE)

                        'For Each str As String In clés
                        '    If (str.Split("=")(0) <> clé) Then
                        '        hs.SaveINISetting("clés", str.Split("=")(0), " ", INIFILE)
                        '    Else
                        '        hs.SaveINISetting("clés", Newclé, " ", INIFILE)
                        '    End If
                        'Next
                        hs.SaveINISetting("clés", clé, "", INIFILE)
                        hs.SaveINISetting("clés", Newclé, " ", INIFILE)

                        hs.SaveINISetting(Newclé, "Value", hs.GetINISetting(clé, "Value", " ", INIFILE), INIFILE)
                        hs.SaveINISetting(Newclé, "STATUS", hs.GetINISetting(clé, "STATUS", " ", INIFILE), INIFILE)
                        hs.SaveINISetting(Newclé, "IMG", hs.GetINISetting(clé, "IMG", " ", INIFILE), INIFILE)
                        hs.ClearINISection(clé, INIFILE)

                        divToUpdate.Add("div_status", buildTableStatus)
                        divToUpdate.Add("div_comptes", builbTableComptes)
                    End If


                    'gestion de la modification de la VALUE d'un STATUS
                    If (parts("id").StartsWith("Value_")) Then
                        Dim ValueClé = parts(parts("id"))
                        Dim clé = parts("id").Split("_")(1)

                        hs.SaveINISetting(clé, "Value", ValueClé, INIFILE)
                        divToUpdate.Add("div_comptes", builbTableComptes)
                    End If

                    'gestion de la modification de la STATUS d'un STATUS
                    If (parts("id").StartsWith("Status_")) Then
                        Dim clé = parts("id").Split("_")(1)
                        Dim status = parts(parts("id"))
                        hs.SaveINISetting(clé, "STATUS", status, INIFILE)
                        divToUpdate.Add("div_comptes", builbTableComptes)
                    End If

                    'gestion de la suppression d'un clé
                    If (parts("id").StartsWith("SuppCle_")) Then

                        Dim clé = parts("id").Split("_")(1)
                        'Dim clés() = hs.GetINISectionEx("clés", INIFILE)
                        'hs.ClearINISection("clés", INIFILE)

                        'For Each str As String In clés
                        '    If (str.Split("=")(0) <> clé) Then
                        '        hs.SaveINISetting("clés", str.Split("=")(0), " ", INIFILE)
                        '    End If
                        'Next
                        hs.SaveINISetting("clés", clé, "", INIFILE)

                        hs.ClearINISection(clé, INIFILE)
                        divToUpdate.Add("div_status", buildTableStatus)
                        divToUpdate.Add("div_comptes", builbTableComptes)

                    End If

                End If
        End Select

        Return MyBase.postBackProc(page, data, user, userRights)
    End Function

    Function CreateAccount(compte As String) As Tasks.Task(Of Boolean)
        Dim Scopes = {CalendarService.Scope.CalendarReadonly}
        Dim ApplicationName As String = "HomeseerGoogleCalendarReader"

        Dim credential As UserCredential
        Dim credPath As String = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
        credPath = Path.Combine(credPath, ".credentials")

        Dim cs As New ClientSecrets()
        cs.ClientId = "834172357305-0p5djrrf1uc57lgl9eq2h07u2kk7779b.apps.googleusercontent.com"
        cs.ClientSecret = "EBIYNdtcWlTqgR6by7GWJdG-"

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
           cs,
            Scopes,
            compte, CancellationToken.None,
            New FileDataStore(credPath, True)).Result

        Return credential.RefreshTokenAsync(CancellationToken.None)
    End Function

    Function DeleteAccount(compte As String) As Tasks.Task(Of Boolean)
        Dim Scopes = {CalendarService.Scope.CalendarReadonly}
        Dim ApplicationName As String = "HomeseerGoogleCalendarReader"

        Dim credential As UserCredential
        Dim credPath As String = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
        credPath = Path.Combine(credPath, ".credentials")

        Dim cs As New ClientSecrets()
        cs.ClientId = "834172357305-0p5djrrf1uc57lgl9eq2h07u2kk7779b.apps.googleusercontent.com"
        cs.ClientSecret = "EBIYNdtcWlTqgR6by7GWJdG-"

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
   cs,
    Scopes,
    compte, CancellationToken.None,
    New FileDataStore(credPath, True)).Result

        Return credential.RevokeTokenAsync(CancellationToken.None)
    End Function




    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        '   Dim stb As New StringBuilder



        Try

            Me.reset()

            CurrentPage = Me

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)

            End If

            Dim stb1 As New StringBuilder()
            Me.AddHeader(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", True, False, True))
            stb1.Append(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", False, True, False, True))


            stb1.Append(getLogHTMLConfig(pageName))

            ' Dim comptes() As String = hs.GetINISetting("Parametres", "Comptes", "", INIFILE).Split(", ")

            stb1.Append(DivStart("div_comptes", "style=""text-align: center;display: inline-block;"""))
            stb1.Append(builbTableComptes)
            stb1.Append(DivEnd())

            Dim stb2 As New StringBuilder()
            stb2.Append(DivStart("div_status", "style=""text-align: center"""))
            stb2.Append(buildTableStatus)
            stb2.Append(DivEnd)

            Dim onglets As New clsJQuery.jqTabs("onglets", pageName)


            Dim TAB = New clsJQuery.Tab
            TAB.tabTitle = "Mots-Clés"
            tab.tabContent = stb2.ToString
            onglets.tabs.Add(tab)

            tab = New clsJQuery.Tab
            tab.tabTitle = "Comptes"
            tab.tabContent = stb1.ToString
            onglets.tabs.Add(tab)



            '  onglets.defaultTab = "Général"
            '  stb1.Append(onglets.Build)


            Me.AddBody(stb1.ToString)
            Me.AddFooter(hs.GetPageFooter)
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)

            Return "error - " & Err.Description & " ---> Détail : " & ex.Message
        End Try
    End Function

    Function getSelectorStatus(compte As String) As clsJQuery.jqDropList
        Dim selectStatus As New clsJQuery.jqDropList("selectStatus_" & compte, PageName, False)
        Dim clés() = hs.GetINISectionEx("clés", INIFILE)
        Dim cpt = compte.Split("_")(0)

        Dim dv As DeviceClass = hs.GetDeviceByRef(hs.GetINISetting(cpt, "ref", 0, INIFILE))
        Dim ref = hs.GetINISetting(cpt, "ref", 0, INIFILE)
        Dim pairs As VSPair()
        If dv IsNot Nothing Then
            Dim selected As Boolean = False


            pairs = hs.DeviceVSP_GetAllStatus(ref)
            For Each p In pairs
                selected = (p.Value = hs.GetINISetting(cpt, compte.Split("_")(1), 0, INIFILE))
                Dim status As String = hs.DeviceVSP_GetStatus(ref, p.Value, ePairStatusControl.Status)
                selectStatus.AddItem(status, p.Value, selected)
            Next
        End If

        '    For Each clé In clés

        '  Dim selected As Boolean = False

        '  selected = (clé.Split("=")(0) = hs.GetINISetting(cpt, compte.Split("_")(1), "", INIFILE))


        ' selectStatus.AddItem(clé.Split("=")(0), clé.Split("=")(0), selected)
        'selectStatus.AddItem(clé.Split("=")(1).Split("|")(0), clé.Split("=")(1).Split("|")(0), selected)
        '  Next
        Return selectStatus
    End Function

    Private Function builbTableComptes() As String

        Dim stb As New StringBuilder()






        'Dim tab As New HTMLTable(1)
        'tab.addRow()
        'tab.addCell("Adresse", "", 1)
        'tab.addCell("Device", "", 1)
        'tab.addCell("Status Defaut", "", 1)
        'tab.addCell("Actions", "", 1)

        Dim comptes() = hs.GetINISectionEx("comptes", INIFILE)
        For Each compte In comptes

            compte = compte.Split("=")(0)
            'TODO : gérer les général pour chaque compte !
            Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
            tab.addRow()


            Dim buttonDelete As New clsJQuery.jqButton("Supp_" & compte, "Supprimer", PageName, False)
            buttonDelete.imagePathNormal = "../images/HomeSeer/ui/Delete.png"
            buttonDelete.AddStyle("float:right")
            tab.addCell("<div style='float:left;'>" & compte & "</div>" & buttonDelete.Build, "", 2)



            tab.addRow()
            tab.addCell(" Référence du Device : ", "", 1)
            tab.addEmptyCell(2)
            Dim tb As New clsJQuery.jqTextBox("ref_" & compte, "text", hs.GetINISetting(compte, "ref", 0, INIFILE), PageName, 10, False)
            tb.id = "ref_" & compte
            tab.addCell(tb.Build, "", 1)
            If hs.GetINISetting(compte, "ref", 0, INIFILE) > 0 Then

                tab.addEmptyRow(5, "../images/Default/c.png")
                For Each Str As String In hs.GetINISectionEx("general", INIFILE)

                    tab.addRow()

                    tab.addCell(UCase(Str.Split("=")(0)), "", 1)
                    tab.addEmptyCell(2)
                    tab.addCell(getSelectorStatus(compte & "_" & Str.Split("=")(0)).Build, "", 1)

                Next

            End If
            stb.Append(tab.GetHTML)


            'compte = compte.Split("=")(0)

            'Dim cpt = compte.Split("@")(0)
            'Dim ref As Integer = hs.GetINISetting(compte, "ref", "", INIFILE)
            'tab.addRow()
            'Dim tb As New clsJQuery.jqTextBox("adresse_" & cpt, "text", compte, PageName, 50, False)
            'tb.id = "adresse_" & cpt
            'tab.addCell(tb.Build, "", 1)
            'tab.addCell(ref, "", 1)
            'tab.addCell(getSelectorStatus(cpt).Build, "", 1)
            '' Dim button As New clsJQuery.jqButton("Modifier_" & cpt & "_" & index, "Modifier", PageName, False)
            '' tab.addCell(button.Build, "", 1)
            'Dim buttonDelete As New clsJQuery.jqButton("Supp_" & cpt, "Supprimer", PageName, False)
            'tab.addCell(buttonDelete.Build, "", 1)
        Next

        Dim tab1 As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab1.addRow()
        Dim tb1 As New clsJQuery.jqTextBox("adresse_new", "text", "Nouveau_compte", PageName, 30, False)
        tb1.id = "adresse_new"

        tab1.addCell(tb1.Build, "", 2)

        stb.Append(tab1.GetHTML)

        Return stb.ToString
    End Function



    Private Function buildTableStatus() As String
        Dim stb As New StringBuilder()

        Dim clés() = hs.GetINISectionEx("clés", INIFILE)
        Dim tab As New HTMLTable(1)


        tab.addRow()
        tab.addCell("Mot Clé", "", 1)
        tab.addCell("Value", "", 1)
        '  tab.addCell("Status", "", 1)
        '  tab.addCell("Image Status", "", 2)
        tab.addCell("Actions", "", 1)


        For Each clé In clés
            If (clé.Split("=")(0) <> " ") Then


                Dim key = clé.Split("=")(0)
                Dim Value = hs.GetINISetting(key, "Value", "", INIFILE)
                Dim Status = hs.GetINISetting(key, "STATUS", "", INIFILE)
                Dim img = hs.GetINISetting(key, "IMG", "", INIFILE)
                tab.addRow()
                Dim tb As New clsJQuery.jqTextBox("key_" & key, "text", key, PageName, 30, False)
                tb.id = "key_" & key
                tab.addCell(tb.Build, "", 1)
                tb = New clsJQuery.jqTextBox("Value_" & key, "text", Value, PageName, 5, False)
                tb.id = "Value_" & key
                tab.addCell(tb.Build, "", 1)
                'tb = New clsJQuery.jqTextBox("Status_" & key, "text", Status, PageName, 30, False)
                'tb.id = "Status_" & key
                'tab.addCell(tb.Build, "", 1)
                'tab.addCell("<div id='div_img_" & key & "'><img id='imgsel_" & key & "' src='" & img & "' height=50px width=50px></div>", "", 1)
                'Dim jImage1 As New clsJQuery.jqLocalFileSelector("img_" & key, PageName, False)
                'jImage1.id = "img_" & key
                'jImage1.AddExtension("*.jpg")
                'jImage1.AddExtension("*.png")
                'jImage1.AddExtension("*.gif")
                'jImage1.dialogCaption = "Sélectionner une image"
                'jImage1.label = "..."
                'jImage1.path = hs.GetAppPath() & "/html/images/"
                'tab.addCell(jImage1.Build, "", 1)

                Dim buttonDelete As New clsJQuery.jqButton("SuppCle_" & key, "Supprimer", PageName, False)
                buttonDelete.imagePathNormal = "../images/HomeSeer/ui/Delete.png"
                tab.addCell(buttonDelete.Build, "", 1)
            End If
        Next

        tab.addRow()

        Dim tb1 As New clsJQuery.jqTextBox("key_new", "text", "Nouveau_mot_clé", PageName, 30, False)
        tb1.id = "key_new"
        tab.addCell("<br>" & tb1.Build, "", 1)
        tab.addCell("", "", 1)
        tab.addCell("", "", 1)

        stb.Append(tab.GetHTML)

        Return stb.ToString
    End Function

End Class

