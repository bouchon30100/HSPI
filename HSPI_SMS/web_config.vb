Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String

        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)

        Select Case parts("id")
            Case "Ajouter_telephoneName"
                Dim name As String = parts(parts("id"))
                hs.SaveINISetting("TELEPHONES", name, " ", INIFILE)
                divToUpdate.Add("list", getListTelephones)

            Case "IP"
                Dim value As String = parts(parts("id"))
                hs.SaveINISetting("param", "IP", value, INIFILE)
                divToUpdate.Add("config", getConfig)

            Case "PORT"
                Dim value As String = parts(parts("id"))
                hs.SaveINISetting("param", "PORT", value, INIFILE)
                divToUpdate.Add("config", getConfig)

            Case "LogLevel"
                Dim value As String = parts(parts("id"))
                hs.SaveINISetting("param", "LogLevel", value, INIFILE)
                divToUpdate.Add("config", getConfig)

            Case Else
                'gestion de modification PhoneNumber
                If (parts("id").StartsWith("telephoneNumber_")) Then
                    Dim name = parts("id").Split("_")(1)
                    Dim number = parts(parts("id"))
                    hs.SaveINISetting("TELEPHONES", name, number, INIFILE)
                    divToUpdate.Add("list", getListTelephones)
                End If

                'gestion de modification PhoneName
                If (parts("id").StartsWith("telephoneName_")) Then
                    Dim name = parts("id").Split("_")(1)
                    Dim NewName = parts(parts("id"))
                    Dim number = hs.GetINISetting("TELEPHONES", name, " ", INIFILE)
                    hs.SaveINISetting("TELEPHONES", name, "", INIFILE)
                    hs.SaveINISetting("TELEPHONES", NewName, number, INIFILE)
                    'TODO: penser à supprimer les liens dans les autres sections
                    divToUpdate.Add("list", getListTelephones)
                End If

                'gestion de suppression d'un téléphone
                If (parts("id").StartsWith("Supp_")) Then
                    Dim name = parts("id").Split("_")(1)
                    hs.SaveINISetting("TELEPHONES", name, "", INIFILE)
                    'TODO: penser à supprimer les liens dans les autres sections
                    divToUpdate.Add("list", getListTelephones)
                End If

                'gestion des abonnements
                If (parts("id").StartsWith("abonnement_")) Then

                    Dim action = parts("id").Split("_")(1)
                    Dim abonnement = ""
                    If (action <> "ajouter") Then
                        abonnement = parts("id").Split("_")(2)
                    Else
                        abonnement = parts(parts("id"))
                    End If

                    Select Case action

                        Case "ajouter"          'ajout d'un abonnement
                            hs.SaveINISetting("ABONNEMENTS", abonnement, " ", INIFILE)

                        Case "phrase"           'modification de la phrase d'un abonnement
                            Dim val = URLDecode(parts(parts("id")))
                            If val = "" Then val = " "
                            hs.SaveINISetting(abonnement, "PHRASE", val, INIFILE)

                        Case "actif"            'actif/inactif
                            Dim val = parts(parts("id"))
                            If val = "" Then val = " "
                            If val = "unchecked" Then
                                val = "FALSE"
                            ElseIf val = "checked" Then
                                val = "TRUE"
                            End If
                            hs.SaveINISetting(abonnement, "ACTIF", val, INIFILE)

                        Case "value"            'modification de la value
                            Dim val = parts(parts("id"))
                            If val = "" Then val = "TRUE"
                            hs.SaveINISetting(abonnement, "VALUE", val, INIFILE)

                        Case "abonne"           'modification des abonnés
                            Dim abonné = parts("id").Split("_")(3)
                            Dim val = parts(parts("id"))
                            If val = "unchecked" Then
                                val = "FALSE"
                            ElseIf val = "checked" Then
                                val = "TRUE"
                            End If
                            Dim myList As New ArrayList
                            myList.AddRange(hs.GetINISetting(abonnement, "ABONNES", "", INIFILE).Split(","))
                            If val Then
                                myList.Add(abonné)
                            Else
                                myList.Remove(abonné)
                            End If
                            Dim str = String.Join(",", myList.ToArray())
                            hs.SaveINISetting(abonnement, "ABONNES", str, INIFILE)

                        Case "supp"             'suppresion d'un abonnement
                            hs.SaveINISetting("ABONNEMENTS", abonnement, "", INIFILE)
                            hs.ClearINISection(abonnement, INIFILE)

                    End Select
                    divToUpdate.Add("listAbonnements", getListAbonnements)

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
            stb.Append(hs.GetPageHeader(pageName, "Sample" & instancetext, "", "", True, False))

            stb.Append(clsPageBuilder.DivStart("pluginpage", ""))




            ' a message area for error messages from jquery ajax postback (optional, only needed if using AJAX calls to get data)
            stb.Append(clsPageBuilder.DivStart("errormessage", "class='errormessage'"))
            stb.Append(clsPageBuilder.DivEnd)

            Dim onglets As New clsJQuery.jqTabs("onglets", pageName)

            Dim stb4 As New StringBuilder()
            stb4.Append(clsPageBuilder.DivStart("config", "style=""text-align: center;display: inline-block;"""))
            stb4.Append(getConfig())
            stb4.Append(DivEnd)
            Dim TAB2 = New clsJQuery.Tab
            TAB2.tabTitle = "CONFIGURATION"
            TAB2.tabContent = stb4.ToString
            onglets.tabs.Add(TAB2)

            Dim stb2 As New StringBuilder()
            stb2.Append(clsPageBuilder.DivStart("list", "style=""text-align: center;display: inline-block;"""))
            stb2.Append(getListTelephones())
            stb2.Append(DivEnd)
            Dim TAB = New clsJQuery.Tab
            TAB.tabTitle = "TELEPHONES"
            TAB.tabContent = stb2.ToString
            onglets.tabs.Add(TAB)


            Dim stb3 As New StringBuilder()
            stb3.Append(clsPageBuilder.DivStart("listAbonnements", "style=""text-align: center;display: inline-block;"""))
            stb3.Append(getListAbonnements())
            stb3.Append(DivEnd)
            Dim TAB1 = New clsJQuery.Tab
            TAB1.tabTitle = "ABONNEMENTS"
            TAB1.tabContent = stb3.ToString
            onglets.tabs.Add(TAB1)

            stb.Append(onglets.Build)

            stb.Append(clsPageBuilder.DivEnd)

            ' add the body html to the page
            Me.AddBody(stb.ToString)

            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function

    Private Function getConfig() As String

        Dim str = "<br> "
        str += getLogHTMLConfig(PageName)
        str += "<br>"

        str += "IP de SMS GATEWAY : "
        Dim tb = New clsJQuery.jqTextBox("IP", "text", hs.GetINISetting("param", "IP", "", INIFILE), PageName, 20, False)
        tb.toolTip = "Saisissez le nom..."
        tb.id = tb.name
        str += tb.Build
        str += "<br>"

        str += "Port de SMS GATEWAY : "
        tb = New clsJQuery.jqTextBox("PORT", "text", hs.GetINISetting("param", "PORT", "", INIFILE), PageName, 10, False)
        tb.id = tb.name
        str += tb.Build

        Return str
    End Function

    Private Function getTableauAjouter(tab As HTMLTable) As HTMLTable
        '    Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab.addRow()

        Dim tb = New clsJQuery.jqTextBox("Ajouter_telephoneName", "text", "", PageName, 10, False)
        tb.promptText = "Saisissez le nom..."
        tb.toolTip = "Saisissez le nom..."

        tb.id = "Ajouter_telephoneName"
        tab.addCell("Ajouter un téléphone : " + tb.Build, "", 3)

        Return tab
    End Function

    Private Function getListAbonnements() As String

        Dim stb As New StringBuilder
        Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        'Entête du tableau
        tab.addRow()
        tab.addCell("Actif", "", 1)
        tab.addEmptyCell(5)
        tab.addCell("Module", "", 1)
        tab.addEmptyCell(5)
        tab.addCell("Phrase", "", 1)
        tab.addEmptyCell(5)
        tab.addCell("Valeur à envoyer", "", 1)
        tab.addEmptyCell(5)
        tab.addEmptyCell(5)

        For Each telephone In hs.GetINISectionEx("TELEPHONES", INIFILE)
            Dim name = telephone.Split("=")(0)
            tab.addCell(name, "", 1)
            tab.addEmptyCell(5)
        Next
        tab.addEmptyCell(5)
        tab.addCell("Supp", "", 1)

        'lignes du tableau

        For Each abonnement In hs.GetINISectionEx("ABONNEMENTS", INIFILE)
            tab.addRow()
            abonnement = abonnement.Split("=")(0)
            Dim Actif As Boolean = CBool(abonnement.Split("=")(0))

            Dim chbox As New clsJQuery.jqCheckBox("abonnement_actif_" + abonnement, "", PageName, True, False)
            chbox.id = "abonnement_actif_" + abonnement
            chbox.checked = CBool(hs.GetINISetting(abonnement, "ACTIF", "TRUE", INIFILE))
            tab.addCell(chbox.Build, "", 1)
            tab.addEmptyCell(5)

            tab.addCell(getModuleNameComplet(CInt(abonnement)), "", 1)
            tab.addEmptyCell(5)

            Dim phrase = hs.GetINISetting(abonnement, "PHRASE", "La valeur de |MODULE| est passée à |VALUE|.", INIFILE)
            Dim tb = New clsJQuery.jqTextBox("abonnement_phrase_" + abonnement, "text", phrase, PageName, 80, False)
            '  tb.promptText = "Saisissez la phrase à envoyer. Mettre %VALUE% à la place de la valeur à envoyer."
            tb.toolTip = "Saisissez la phrase à envoyer. Mettre %VALUE% à la place de la valeur à envoyer."
            tb.id = "abonnement_phrase_" + abonnement
            tab.addCell(tb.Build, "", 1)
            tab.addEmptyCell(5)

            Dim value = hs.GetINISetting(abonnement, "VALUE", " ", INIFILE)
            Dim selectStatus As New clsJQuery.jqDropList("abonnement_value_" + abonnement, PageName, False)
            Dim selected As Boolean = False
            Dim ElementsToUpdate = {" ", "string", "value", "status", "location", "location2"}
            For Each Str As String In ElementsToUpdate
                selected = (value = Str)
                selectStatus.AddItem(Str, Str, selected)
            Next
            tab.addCell(selectStatus.Build, "", 1)
            tab.addEmptyCell(5)
            tab.addEmptyCell(5)

            For Each telephone In hs.GetINISectionEx("TELEPHONES", INIFILE)
                Dim name = telephone.Split("=")(0)
                Dim abonnés As String() = hs.GetINISetting(abonnement, "ABONNES", " ", INIFILE).Split(",")

                Dim chbox1 As New clsJQuery.jqCheckBox("abonnement_abonne_" + abonnement + "_" + name, "", PageName, True, False)
                chbox1.id = chbox1.name
                chbox1.checked = abonnés.Contains(name)
                tab.addCell(chbox1.Build, "", 1)
                tab.addEmptyCell(5)
            Next

            tab.addEmptyCell(5)
            Dim buttonDelete As New clsJQuery.jqButton("abonnement_supp_" & abonnement, "Supprimer", PageName, False)
            buttonDelete.imagePathNormal = "../images/HomeSeer/ui/Delete.png"
            ' buttonDelete.AddStyle("float:right")
            tab.addCell(buttonDelete.Build, "", 1)
            tab.addEmptyCell(5)

        Next

        'ligne pour ajouter un abonnement 
        tab.addRow()
        tab.addCell("Ajouter un abonnement pour le module : " + getallModuleHSHTMLSelector("abonnement_ajouter", PageName, "", {}), "", 9)

        stb.Append(tab.GetHTML)
        Return stb.ToString
    End Function

    Public Function getListTelephones()
        Dim stb As New StringBuilder
        Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab.addRow()
        tab.addCell("Nom", "", 1)
        tab.addEmptyCell(5)
        tab.addCell("Numéro", "", 1)
        tab.addEmptyCell(5)
        tab.addCell("Supp", "", 1)


        For Each telephone In hs.GetINISectionEx("TELEPHONES", INIFILE)

            Dim name = telephone.Split("=")(0)
            Dim number = telephone.Split("=")(1)

            tab.addRow()

            Dim tb = New clsJQuery.jqTextBox("telephoneName_" + name, "text", name, PageName, 10, False)
            tb.id = "telephoneName_" + name
            tab.addCell(tb.Build, "", 1)
            tab.addEmptyCell(5)

            tb = New clsJQuery.jqTextBox("telephoneNumber_" + name, "text", number, PageName, 10, False)
            tb.id = "telephoneNumber_" + name
            tab.addCell(tb.Build, "", 1)
            tab.addEmptyCell(5)

            Dim buttonDelete As New clsJQuery.jqButton("Supp_" & name, "Supprimer", PageName, False)
            buttonDelete.imagePathNormal = "../images/HomeSeer/ui/Delete.png"
            ' buttonDelete.AddStyle("float:right")
            tab.addCell(buttonDelete.Build, "", 1)
        Next

        stb.Append(getTableauAjouter(tab).GetHTML)
        Return stb.ToString
    End Function

    Public Function getModuleNameComplet(refSelected As Integer)
        For Each element In getListModuleHS()

            Dim dv As DeviceClass = element.Value

            If (dv.Ref(Nothing) = refSelected) Then
                Return dv.Location2(Nothing) & " - " & dv.Location(Nothing) & " - " & dv.Name(Nothing)
            End If
        Next
        Return ""
    End Function

    Public Function getModuleCOMMANDEtoSupp(name As String, pageName As String, refSelected As String)
        Dim stb1 As New StringBuilder()
        Dim selectModuleHS As New clsJQuery.jqDropList(name, pageName, False)
        selectModuleHS.AddItem(" ", refSelected, False)
        Dim selected As Boolean = False
        For Each element In getListModuleHS()

            Dim dv As DeviceClass = element.Value

            If (dv.Ref(Nothing) = refSelected) Then
                Dim str = dv.Location2(Nothing) & " - " & dv.Location(Nothing) & " - " & dv.Name(Nothing)
                selectModuleHS.AddItem(str, dv.Ref(Nothing), True)
            End If
        Next

        stb1.Append(selectModuleHS.Build)
        Return stb1.ToString()
    End Function

    Public Function getallModuleHSHTMLSelector(name As String, pageName As String, valueSelected As String, filtre As String()) As String
        Dim stb1 As New StringBuilder()

        Dim selectModuleHS As New clsJQuery.jqDropList(name, pageName, False)
        selectModuleHS.AddItem(" ", "0", True)

        Dim selected As Boolean = False

        For Each element In getListModuleHS()

            Dim dv As DeviceClass = element.Value

            If (filtre.Contains(dv.Device_Type_String(Nothing))) Or (filtre.Count = 0) Then
                If (hs.GetINISection("ABONNEMENTS", INIFILE).Contains(dv.Ref(Nothing))) Then
                Else
                    selected = (CStr(dv.Ref(Nothing)) = valueSelected)
                    Dim str = dv.Location2(Nothing) & " - " & dv.Location(Nothing) & " - " & dv.Name(Nothing)
                    selectModuleHS.AddItem(str, dv.Ref(Nothing), selected)
                End If
            End If

        Next

        stb1.Append(selectModuleHS.Build)
        Return stb1.ToString()
    End Function



    Function BuildContent() As String
        Dim stb As New StringBuilder
        stb.Append(" <table border='0' cellpadding='0' cellspacing='0' width='1000'>")
        stb.Append(" <tr><td width='1000' align='center' style='color:#FF0000; font-size:14pt; height:30px;'><strong><div id='message'>&nbsp;</div></strong></tr>")

        stb.Append(" </table>")
        Return stb.ToString
    End Function

    Sub PostMessage(ByVal sMessage As String)
        Me.divToUpdate.Add("message", sMessage)
        Me.pageCommands.Add("starttimer", "")
        TimerEnabled = True
    End Sub

End Class

