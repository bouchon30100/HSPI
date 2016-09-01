Imports System.Text
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports HomeSeerAPI
Imports Scheduler
Imports Scheduler.Classes


Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim listLocations(2) As List(Of String)
    Dim js As clsJQuery.jqMultiSelect

    ' Dim listeClient As List(Of SarahClient) = New List(Of SarahClient)

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)

    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Log("page post: " & data, MessageType.Debug)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)

        Dim idImage As String = ""
        Dim saison As String = ""
        If data.StartsWith("img_") Then
            idImage = data.Split("=")(0)
            saison = idImage.Split("_")(1)
        End If
        Dim inifileDevice = INIFILE_DEVICE.Replace("####", saison)

        'gestion de la sélection d'une image --> MAJ de la vignette
        If (parts(idImage) <> Nothing) Then

            Dim appPath = hs.GetAppPath().Replace("\", "/") & "/html/"
            Dim src As String = "../" & parts(idImage).Replace(appPath, "")
            hs.SaveINISetting("PARAMS", "IMG", src, inifileDevice)
            divToUpdate.Add("div_saisons", getHTMLOngletSAISONS)
            divToUpdate.Add("list", getListAssociation)
            'divToUpdate.Add("div_img_" & key, "<img id='imgsel" & key & "' src='" & src & "' height=50px width=50px>")
            UpdateModule(hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE))
        End If

        If (parts("id") IsNot Nothing) Then

            Select Case parts("id")

                Case "LogLevel"
                    hs.SaveINISetting("param", "logLevel", parts("LogLevel"), INIFILE)

                Case "Actualiser"
                    For Each device In GetListDEVICES()
                        Dim dv As DeviceClass = hs.GetDeviceByRef(device)
                        Dim value = dv.devValue(Nothing)
                        UpdateModuleTHERMOSTAT(device, value, value)
                    Next
                    Me.pageCommands.Add("closedialog", "div_wait")
                '   divToUpdate.Add("div_saisons", getHTMLOngletSAISONS)
                '   divToUpdate.Add("list", getListAssociation)


                Case "newSaison"
                    AddSaison(parts(parts("id")))
                    divToUpdate.Add("div_saisons", getHTMLOngletSAISONS)
                    divToUpdate.Add("list", getListAssociation)
                    UpdateModule(hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE))

                Case "moduleSaison"
                    hs.SaveINISetting("PARAMS", "MODULE_SAISON", parts(parts("id")), INIFILE)
                    ' divToUpdate.Add("div_saisons", getHTMLOngletSAISONS)
                    'divToUpdate.Add("list", getListAssociation)
                    UpdateModule(hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE))

                Case "NewThermostat"
                    If (parts(parts("id")) > 0) Then
                        inifileDevice = INIFILE_DEVICE.Replace("####", parts(parts("id")))
                        hs.SaveINISetting("ASSOCIATIONS", parts(parts("id")), "0", INIFILE)

                        For Each status In GetListSTATUS()
                            hs.SaveINISetting("IMG", status, " ", inifileDevice)
                        Next

                        For Each saison In GetListSAISONS()
                            hs.SaveINISetting(saison, "min", "18", inifileDevice)
                            hs.SaveINISetting(saison, "max", "22", inifileDevice)
                        Next
                        UpdateModuleTHERMOSTAT(parts(parts("id")), 99, 99)
                        divToUpdate.Add("list", getListAssociation)
                        divToUpdate.Add("Actions", getListAssociationForOutput)
                    End If
                Case Else
                    If (parts("id").StartsWith("Thermostat")) Then
                        Dim oldAsso = parts("id").Split("_")(1)
                        Dim newAsso = parts(parts("id"))

                        If (newAsso = "0") Then
                            DeleteDevices(oldAsso)
                            hs.SaveINISetting("ASSOCIATIONS", oldAsso, "", INIFILE)
                            FileIO.FileSystem.DeleteFile(hs.GetAppPath & "/Config/" & "HSPI_" & IFACE_NAME & "/" & oldAsso & ".ini")

                        Else
                            Dim valueAsso = hs.GetINISetting("ASSOCIATIONS", oldAsso, "0", INIFILE)
                            hs.SaveINISetting("ASSOCIATIONS", newAsso, valueAsso, INIFILE)
                            hs.SaveINISetting("ASSOCIATIONS", oldAsso, "", INIFILE)
                            FileIO.FileSystem.RenameFile(hs.GetAppPath & "/Config/" & "HSPI_" & IFACE_NAME & "/" & oldAsso & ".ini", newAsso & ".ini")

                        End If
                        divToUpdate.Add("Actions", getListAssociationForOutput)
                        divToUpdate.Add("list", getListAssociation)
                    End If

                    ' gestiondes saisons

                    If (parts("id").StartsWith("saison")) Then

                        Dim Oldsaison = parts("id").Split("_")(1)
                        Dim NewSaison = parts(parts("id"))
                        If (NewSaison = "") Or ((NewSaison = " ")) Then
                            deleteSaison(Oldsaison)

                        Else
                            updateSaisonName(Oldsaison, NewSaison)

                        End If
                        divToUpdate.Add("div_saisons", getHTMLOngletSAISONS)
                        divToUpdate.Add("list", getListAssociation)
                        UpdateModule(hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE))
                    End If

                    If (parts("id").StartsWith("SaisonValue_")) Then

                        saison = parts("id").Split("_")(1)
                        Dim value = parts(parts("id"))
                        inifileDevice = INIFILE_DEVICE.Replace("####", saison)
                        hs.SaveINISetting("PARAMS", "VALUE", value, inifileDevice)
                        UpdateModule(hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE))
                    End If

                    If (parts("id").StartsWith("debut")) Then

                        saison = parts("id").Split("_")(1)
                        Dim value = parts(parts("id"))
                        inifileDevice = INIFILE_DEVICE.Replace("####", saison)
                        hs.SaveINISetting("PARAMS", "debut", value, inifileDevice)

                    End If

                    If (parts("id").StartsWith("fin")) Then

                        saison = parts("id").Split("_")(1)
                        Dim value = parts(parts("id"))
                        inifileDevice = INIFILE_DEVICE.Replace("####", saison)
                        hs.SaveINISetting("PARAMS", "fin", value, inifileDevice)

                    End If

                    'gestion de la zone de confort
                    If (parts("id").StartsWith("min")) Then
                        Dim asso = parts("id").Split("_")(2)
                        saison = parts("id").Split("_")(1)
                        Dim value = parts(parts("id"))
                        inifileDevice = INIFILE_DEVICE.Replace("####", asso)
                        hs.SaveINISetting(saison, "min", value, inifileDevice)
                        Dim dv As DeviceClass = hs.GetDeviceByRef(asso)
                        Dim value1 = dv.devValue(Nothing)
                        UpdateModuleTHERMOSTAT(asso, value, value)

                        divToUpdate.Add("list", getListAssociation)
                    End If

                    If (parts("id").StartsWith("max")) Then
                        Dim asso = parts("id").Split("_")(2)
                        saison = parts("id").Split("_")(1)
                        Dim value = parts(parts("id"))
                        inifileDevice = INIFILE_DEVICE.Replace("####", asso)
                        hs.SaveINISetting(saison, "max", value, inifileDevice)
                        Dim dv As DeviceClass = hs.GetDeviceByRef(asso)
                        Dim value1 = dv.devValue(Nothing)
                        UpdateModuleTHERMOSTAT(asso, value, value)

                        divToUpdate.Add("list", getListAssociation)
                    End If

                    If (parts("id").StartsWith("Chauffage")) Then
                        Dim asso = parts("id").Split("_")(1)
                        Dim valueAsso = parts(parts("id"))
                        hs.SaveINISetting("ASSOCIATIONS", asso, valueAsso, INIFILE)
                        divToUpdate.Add("list", getListAssociation)
                    End If
                    '*********************************************************************************************
                    '**         Gestion de l'onglet OUTPUT
                    '*********************************************************************************************

                    If (parts("id").StartsWith("OutputType_")) Then
                        Dim asso = parts("id").Split("_")(1)
                        Dim ini = INIFILE_DEVICE.Replace("####", asso)
                        Dim valueType = parts(parts("id"))
                        If valueType = " " Then
                            hs.SaveINISetting("ASSOCIATIONS", asso, " ", INIFILE)
                        Else
                            hs.SaveINISetting("ASSOCIATIONS", asso, valueType, INIFILE)
                        End If
                        hs.SaveINISetting("LINKS", "OUTPUTTYPE", valueType, ini)
                        hs.SaveINISetting("LINKS", "OUTPUT", " ", ini)
                            hs.SaveINISetting("ACTIONS", "COLD", " ", ini)
                            hs.SaveINISetting("ACTIONS", "STOP", " ", ini)
                            hs.SaveINISetting("ACTIONS", "HOT", " ", ini)
                            divToUpdate.Add("Actions", getListAssociationForOutput)
                        End If

                        If (parts("id").StartsWith("Output_")) Then
                        Dim asso = parts("id").Split("_")(1)
                        Dim ini = INIFILE_DEVICE.Replace("####", asso)
                        Dim valueOutput = parts(parts("id"))
                        hs.SaveINISetting("ASSOCIATIONS", asso, valueOutput, INIFILE)
                        hs.SaveINISetting("LINKS", "OUTPUT", valueOutput, ini)
                        hs.SaveINISetting("ACTIONS", "COLD", " ", ini)
                        hs.SaveINISetting("ACTIONS", "STOP", " ", ini)
                        hs.SaveINISetting("ACTIONS", "HOT", " ", ini)

                        divToUpdate.Add("Actions", getListAssociationForOutput)
                    End If

                    If (parts("id").StartsWith("Actions_")) Then
                        Dim action = parts("id").Split("_")(1)
                        Dim asso = parts("id").Split("_")(2)
                        Dim ini = INIFILE_DEVICE.Replace("####", asso)
                        Dim valueOutput = parts(parts("id"))
                        hs.SaveINISetting("ACTIONS", action, valueOutput, ini)
                        divToUpdate.Add("Actions", getListAssociationForOutput)
                    End If


            End Select
        Else
            If (parts("filtreType.close") IsNot Nothing) Then
                Dim filtre = parts("filtreType.close").Split("|")
                divToUpdate.Add("ajout", getModuleHSHTMLSelector("NewThermostat", PageName, "0", filtre))

            End If
        End If
        Return MyBase.postBackProc(page, data, user, userRights)
    End Function


    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        Dim stb As New StringBuilder
        Me.AddTitleBar(pageName & " Configuration", user, False, "", False, False, False, False)

        Dim instancetext As String = ""
        Try

            Me.reset()
            GetSaison()
            CurrentPage = Me
            Dim refresh As Boolean = True

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)

            End If


            If Instance <> "" Then instancetext = " - " & Instance


            Me.AddHeader(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", True, False, True))
            stb.Append(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", False, True, False, True))


            Dim onglets As New clsJQuery.jqTabs("onglets", pageName)


            Dim TAB = New clsJQuery.Tab
            TAB.tabTitle = "Thermostats"
            TAB.tabContent = getHTMLOngletThermostat()
            onglets.tabs.Add(TAB)
            onglets.defaultTab = TAB.tabTitle

            TAB = New clsJQuery.Tab
            TAB.tabTitle = "Périodes"
            TAB.tabContent = "<div id=""div_saisons"">" & getHTMLOngletSAISONS() & "</div>"
            onglets.tabs.Add(TAB)

            TAB = New clsJQuery.Tab
            TAB.tabTitle = "OUPUT"
            TAB.tabContent = getHTMLOngletOutput()
            onglets.tabs.Add(TAB)
            stb.Append(onglets.Build)

            Me.AddBody(stb.ToString)

            Me.AddFooter(hs.GetPageFooter())

            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function

    Function getHTMLOngletSAISONS()
        Dim stb As New StringBuilder()
        stb.Append(" <br>Module HS pour la saison : ")
        Dim moduleSaison = hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE)
        stb.Append(getModuleHSHTMLSelector("moduleSaison", PageName, moduleSaison, {}))
        stb.Append(" <br>")
        stb.Append(" <br>")

        Dim tb As New clsJQuery.jqTextBox("newSaison", "text", "", PageName, 10, False)
        tb.id = "newSaison"
        stb.Append("Ajouter une nouvelle période : " & tb.Build)
        stb.Append(" <br>")
        stb.Append(" <br>")

        Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab.addRow()
        tab.addCell("Périodes", "", 8)

        For Each saison In GetListSAISONS()

            tab.addRow()
            Dim inifilesaison = INIFILE_DEVICE.Replace("####", saison)
            '  stb.Append("<br>")
            tb = New clsJQuery.jqTextBox("saison_" & saison, "text", saison, PageName, 10, False)
            tb.id = "saison_" & saison
            tab.addCell(tb.Build & "", "", 1)
            tab.addEmptyCell(30)
            '    stb.Append(tb.Build & " : ")
            tb = New clsJQuery.jqTextBox("debut_" & saison, "text", hs.GetINISetting("PARAMS", "debut", "", inifilesaison), PageName, 10, False)
            tb.id = "debut_" & saison
            tab.addCell("Début : " & tb.Build, "", 1)
            tab.addEmptyCell(10)
            '  stb.Append("Début : " & tb.Build)
            tb = New clsJQuery.jqTextBox("fin_" & saison, "text", hs.GetINISetting("PARAMS", "fin", "", inifilesaison), PageName, 10, False)
            tb.id = "fin_" & saison
            tab.addCell("Fin : " & tb.Build, "", 1)
            '  stb.Append("Fin : " & tb.Build)
            tab.addEmptyCell(30)
            tb = New clsJQuery.jqTextBox("SaisonValue_" & saison, "text", hs.GetINISetting("PARAMS", "VALUE", "", inifilesaison), PageName, 10, False)
            tb.id = "SaisonValue_" & saison
            tab.addCell("Status Value : " & tb.Build, "", 1)
            '  stb.Append("Status Value : " & tb.Build)

            Dim img = hs.GetINISetting("PARAMS", "IMG", "", inifilesaison)
            Dim htmlImg = " <div id='div_img_" & saison & "'><img id='imgsel_" & saison & "' src='" & img & "' height=50px width=50px></div>"
            tab.addCell(htmlImg, "", 1)
            '  stb.Append(htmlImg)
            Dim jImage1 As New clsJQuery.jqLocalFileSelector("img_" & saison, PageName, False)
            jImage1.id = "img_" & saison
            jImage1.AddExtension("*.jpg")
            jImage1.AddExtension("*.png")
            jImage1.AddExtension("*.gif")
            jImage1.dialogCaption = "Sélectionner une image"
            jImage1.label = "..."
            jImage1.path = hs.GetAppPath() & "/html/images/"
            tab.addCell(jImage1.Build, "", 1)
            '  stb.Append(jImage1.Build)

        Next

        stb.Append(tab.GetHTML)
        stb.Append(DivStart("note", "style='color:red';"))
        stb.Append("<b>Pour supprimer une période, videz le nom.</b>")
        stb.Append(DivEnd)
        Return stb.ToString
    End Function

    Function getHTMLOngletThermostat()
        Dim stb As New StringBuilder()
        stb.Append("<br>")
        stb.Append(getLogHTMLConfig(PageName))
        stb.Append("<br>")
        js = New clsJQuery.jqMultiSelect("filtreType", PageName, False)
        js.id = "filtreType"
        For Each type In getListTypes()
            js.AddItem(type, type, False)
        Next
        stb.Append(js.Build)
        stb.Append("    ")

        Dim bt As New clsJQuery.jqButton("Actualiser", "Mettre à jour Tous les thermostats", PageName, False)
        bt.id = "Actualiser"
        bt.functionToCallOnClick = "$.blockUI({ message: '<h2><img  src=""/images/HomeSeer/ui/spinner.gif"" /> Wait...</h2>' });"
        stb.Append(bt.Build)
        stb.Append("    ")


        stb.Append(DivStart("ajout", ""))
        stb.Append(getModuleHSHTMLSelector("NewThermostat", PageName, "0", {}))
        stb.Append(DivEnd)
        stb.Append("<br>")
        stb.Append("<br>")

        stb.Append(DivStart("list", "style=""text-align: center;display: inline-block;"""))

        stb.Append(getListAssociation())

        stb.Append(DivEnd)
        Return stb.ToString
    End Function


    Function getHTMLOngletOutput()
        Dim stb As New StringBuilder()

        stb.Append(DivStart("Actions", "style=""text-align: center;display: inline-block;"""))

        stb.Append(getListAssociationForOutput())

        stb.Append(DivEnd)
        Return stb.ToString
    End Function


    Public Function getListAssociationForOutput()

        Dim OutputType = {" ", "Module", "Script", "Commande"}
        Dim stb As New StringBuilder
        For Each association In hs.GetINISectionEx("ASSOCIATIONS", INIFILE)
            Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
            tab.addRow()

            Dim asso = association.Split("=")(0)
            '     stb.Append(DivStart("div_" & asso, "style='border : solid'"))
            Dim dv As DeviceClass = hs.GetDeviceByRef(asso)
            Dim str = dv.Location2(Nothing) & " - " & dv.Location(Nothing) & " - " & dv.Name(Nothing)
            tab.addCell(str, "", 3)
            tab.addRow()

            tab.addCell("Type de Sortie : ", "", 1)
            tab.addEmptyCell(10)
            Dim jsType As New clsJQuery.jqDropList("OutputType_" & asso, PageName, False)
            jsType.id = "OutputType_" & asso

            Dim ini = INIFILE_DEVICE.Replace("####", asso)

            For Each typeOutput In OutputType
                Dim selected As Boolean = typeOutput = hs.GetINISetting("LINKS", "OUTPUTTYPE", " ", ini)
                jsType.AddItem(typeOutput, typeOutput, selected)
            Next
            tab.addCell(jsType.Build, "", 1)

            tab.addRow()

            '********* Construction de la sélection de la sortie MODULES ou SCRIPT
            Select Case hs.GetINISetting("LINKS", "OUTPUTTYPE", " ", ini)
                Case "Script"

                    tab.addCell("Action de sortie : ", "", 1)
                    tab.addEmptyCell(10)

                    Dim SelectorAction As New clsJQuery.jqDropList("Output_" & asso, PageName, False)
                    SelectorAction.id = "Output_" & asso
                    SelectorAction.AddItem(" ", " ", True)
                    Dim path = hs.GetAppPath & "\scripts\"
                    For Each f In FileIO.FileSystem.GetFiles(path)
                        f = f.Replace(path, "")
                        Dim selected As Boolean = f = hs.GetINISetting("LINKS", "OUTPUT", " ", ini)
                        SelectorAction.AddItem(f, f, selected)
                    Next
                    tab.addCell(SelectorAction.Build, "", 1)

                    Dim tb As New clsJQuery.jqTextBox("Actions_COLD_" & asso, "text", hs.GetINISetting("ACTIONS", "COLD", " ", ini), PageName, 50, False)
                    tb.id = "Actions_COLD_" & asso
                    tab.addRow()
                    tab.addCell("Raffrachissement : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(tb.Build, "", 1)

                    tb = New clsJQuery.jqTextBox("Actions_STOP_" & asso, "text", hs.GetINISetting("ACTIONS", "STOP", " ", ini), PageName, 50, False)
                    tb.id = "Actions_STOP_" & asso
                    tab.addRow()
                    tab.addCell("Stop : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(tb.Build, "", 1)

                    tb = New clsJQuery.jqTextBox("Actions_HOT_" & asso, "text", hs.GetINISetting("ACTIONS", "HOT", " ", ini), PageName, 50, False)
                    tb.id = "Actions_HOT_" & asso
                    tab.addRow()
                    tab.addCell("Chauffage : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(tb.Build, "", 1)

                Case "Commande"
                    Dim tb As New clsJQuery.jqTextBox("Actions_COLD_" & asso, "text", hs.GetINISetting("ACTIONS", "COLD", " ", ini), PageName, 50, False)
                    tb.id = "Actions_COLD_" & asso
                    tab.addRow()
                    tab.addCell("Raffrachissement : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(tb.Build, "", 1)

                    tb = New clsJQuery.jqTextBox("Actions_STOP_" & asso, "text", hs.GetINISetting("ACTIONS", "STOP", " ", ini), PageName, 50, False)
                    tb.id = "Actions_STOP_" & asso
                    tab.addRow()
                    tab.addCell("Stop : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(tb.Build, "", 1)

                    tb = New clsJQuery.jqTextBox("Actions_HOT_" & asso, "text", hs.GetINISetting("ACTIONS", "HOT", " ", ini), PageName, 50, False)
                    tb.id = "Actions_HOT_" & asso
                    tab.addRow()
                    tab.addCell("Chauffage : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(tb.Build, "", 1)


                Case "Module"

                    tab.addCell("Action de sortie : ", "", 1)
                    tab.addEmptyCell(10)

                    Dim ValueSelected = hs.GetINISetting("LINKS", "OUTPUT", "0", ini)
                    tab.addCell(getModuleHSHTMLSelector("Output_" & asso, PageName, ValueSelected, {}), "", 1)
                    tab.addRow()
                    tab.addCell("Raffrachissement : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(getActionsModule(asso, "COLD"), "", 1)

                    tab.addRow()
                    tab.addCell("Stop : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(getActionsModule(asso, "STOP"), "", 1)

                    tab.addRow()
                    tab.addCell("Chauffage : ", "", 1)
                    tab.addEmptyCell(10)
                    tab.addCell(getActionsModule(asso, "HOT"), "", 1)

                Case Else
                    tab.addCell("Sélectionner un Type de Sortie.", "", 1)
            End Select

            stb.Append(tab.GetHTML)
        Next
        Return stb.ToString
    End Function

    Public Function getActionsModule(refINPUT As String, Action As String)
        Dim SelectorAction As New clsJQuery.jqDropList("Actions_" & Action & "_" & refINPUT, PageName, False)
        SelectorAction.id = "Actions_" & Action & "_" & refINPUT

        Dim ini = INIFILE_DEVICE.Replace("####", refINPUT)
        Dim refOUTPUT = hs.GetINISetting("LINKS", "OUTPUT", "0", ini)
        If (refOUTPUT > 0) Then


            Dim pairs As VSPair() = hs.DeviceVSP_GetAllStatus(refOUTPUT)

            SelectorAction.AddItem("", "99", True)
            For Each pair In pairs
                Dim StrStatus = hs.DeviceVSP_GetStatus(refOUTPUT, hs.GetINISetting("ACTIONS", Action, "99", ini), ePairStatusControl.Status)
                Dim StrCurrentStatus = hs.DeviceVSP_GetStatus(refOUTPUT, pair.Value, ePairStatusControl.Status)
                Dim selected As Boolean = StrCurrentStatus = StrStatus
                SelectorAction.AddItem(StrCurrentStatus, pair.Value, selected)
            Next
        End If
        Return SelectorAction.Build
    End Function

    Public Function getListAssociation()
        Dim stb As New StringBuilder



        For Each association In hs.GetINISectionEx("ASSOCIATIONS", INIFILE)

            Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
            tab.addRow()

            Dim asso = association.Split("=")(0)
            '     stb.Append(DivStart("div_" & asso, "style='border : solid'"))
            Dim str = getModuleHSHTMLSelector("Thermostat_" & asso, PageName, asso, {})
            '   str += " associé à "
            '  str += getModuleHSHTMLSelector("Chauffage_" & asso, PageName, value, {})
            tab.addCell(str, "", 5)


            Dim inifileDevice = INIFILE_DEVICE.Replace("####", asso)
            Dim Saisons = GetListSAISONS()

            For Each saison In Saisons
                tab.addRow()

                tab.addCell(saison & " ", "", 1)
                tab.addEmptyCell(20)
                Dim tb As New clsJQuery.jqTextBox("min_" & saison & "_" & asso, "text", hs.GetINISetting(saison, "min", 18, inifileDevice), PageName, 5, False)
                tb.id = "min_" & saison & "_" & asso
                tab.addCell("Min : " & tb.Build, "", 1)
                tab.addEmptyCell(10)
                tb = New clsJQuery.jqTextBox("max_" & saison & "_" & asso, "text", hs.GetINISetting(saison, "max", 22, inifileDevice), PageName, 5, False)
                tb.id = "max_" & saison & "_" & asso
                tab.addCell("Max : " & tb.Build, "", 1)

            Next
            stb.Append(tab.GetHTML)
            '    stb.Append(DivEnd)

        Next
        Return stb.ToString
    End Function




    Public Function getListModuleHS() As SortedList(Of String, DeviceClass)
        Dim list As New SortedList(Of String, DeviceClass)
        '    list(0) = New List(Of String)
        '    list(1) = New List(Of String)
        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            '    Dim i = 0
            Do While Not en.Finished
                dv = en.GetNext

                If dv IsNot Nothing Then
                    '        i += 1

                    Dim str = dv.Location2(Nothing) & " " & dv.Location(Nothing) & " " & dv.Name(Nothing) & "_" & dv.Ref(Nothing)
                    list.Add(str, dv)
                End If
            Loop
            '  Console.WriteLine(i)
        Catch ex As Exception
            Log(ex.Message, MessageType.Error_)
        End Try
        Return list
    End Function

    Public Function getListTypes() As List(Of String)
        Dim list As New List(Of String)
        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            Do While Not en.Finished
                dv = en.GetNext
                If dv IsNot Nothing Then
                    If Not list.Contains(dv.Device_Type_String(Nothing)) Then
                        list.Add(dv.Device_Type_String(Nothing))
                    End If
                End If
            Loop
            list.Sort()

        Catch ex As Exception
        End Try
        Return list
    End Function

    Public Function getListLocations() As List(Of String)()
        Dim list(1) As List(Of String)
        list(0) = New List(Of String)
        list(1) = New List(Of String)
        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            Do While Not en.Finished
                dv = en.GetNext
                If dv IsNot Nothing Then
                    If Not list(0).Contains(dv.Location(hs)) Then
                        list(0).Add(dv.Location(hs))
                    End If
                    If Not list(1).Contains(dv.Location2(hs)) Then
                        list(1).Add(dv.Location2(hs))
                    End If
                End If
            Loop
            list(0).Sort()
            list(1).Sort()
        Catch ex As Exception
        End Try
        Return list
    End Function



    Public Function getModuleHSHTMLSelector(name As String, pageName As String, valueSelected As String, filtre As String()) As String
        Dim stb1 As New StringBuilder()

        Dim selectModuleHS As New clsJQuery.jqDropList(name, pageName, False)
        selectModuleHS.AddItem(" ", "0", True)

        Dim selected As Boolean = False

        For Each element In getListModuleHS()

            Dim dv As DeviceClass = element.Value

            If (filtre.Contains(dv.Device_Type_String(Nothing))) Or (filtre.Count = 0) Then
                selected = dv.Ref(Nothing) = valueSelected
                Dim str = dv.Location2(Nothing) & " - " & dv.Location(Nothing) & " - " & dv.Name(Nothing)
                selectModuleHS.AddItem(str, dv.Ref(Nothing), selected)
            End If

        Next

        stb1.Append(selectModuleHS.Build)
        Return stb1.ToString()
    End Function




End Class

