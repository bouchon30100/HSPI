Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes
Imports iCloudLib

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    ' Dim ic As New iCloud1


    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Try


            Log("page """ & PageName & """ post: " & data, MessageType.Debug)
            Dim parts As Collections.Specialized.NameValueCollection
            parts = HttpUtility.ParseQueryString(data)


            Select Case parts("id")
                Case "submitNewAccount"
                    Dim ic As New iCloud
                    If ic.Connect(New iCloud.iCloudLoginCredentials(parts("account"), parts("password"), True)) Then
                        ic.StartFindMyiPhone(hs.GetINISetting("param", "refresh", 5, INIFILE) * 60 * 1000)
                        AddHandler ic.FindMyiPhoneUpdate, AddressOf FMIUpdated
                        AppleIDs.Add(ic, ic.UserInformation.FirstName & ic.UserInformation.LastName) ' .Add(ic)
                        hs.SaveINISetting("comptes", ic.UserInformation.FirstName & ic.UserInformation.LastName, " ", INIFILE)
                        hs.SaveINISetting(ic.UserInformation.FirstName & ic.UserInformation.LastName, "appleID", parts("account"), INIFILE)
                        hs.SaveINISetting(ic.UserInformation.FirstName & ic.UserInformation.LastName, "password", parts("password"), INIFILE)
                        For Each device In ic.Devices
                            ' hs.SaveINISetting("devices_" & ic.UserInformation.FirstName & "_" & ic.UserInformation.LastName, device.DeviceName, " ", INIFILE)
                            hs.SaveINISetting(device.DeviceName, "ref", 0, INIFILE)
                        Next
                    Else
                        Throw New Exception("AppleId / Mot de passe invalide")
                    End If
                    divToUpdate.Add("div_comptes", builbTableComptes)

                Case "delai"
                    hs.SaveINISetting("param", "refresh", parts("delai"), INIFILE)
                    For Each appleId In AppleIDs
                        appleId.Value.StartFindMyiPhone(parts("delai") * 60 * 1000)
                    Next

                Case "LogLevel"
                    hs.SaveINISetting("param", "logLevel", parts("LogLevel"), INIFILE)

                'si ELEMENT A UPDATER est alimenté
                Case "ElementToUpdate"

                    hs.SaveINISetting("param", "ElementToUpdate", parts(parts("id")), INIFILE)


                Case Else


                    If (parts("id") <> Nothing) Then

                        'si SUPPR est appuyé
                        If parts("id").StartsWith("Supp") Then
                            Dim compte = parts("id").Split("_")(1)

                            For Each device In AppleIDs(compte).Devices 'hs.GetINISectionEx(compte, INIFILE)
                                'device = device.Split("=")(0)
                                hs.ClearINISection(device.DeviceName, INIFILE)
                            Next
                            hs.ClearINISection(compte, INIFILE)

                            hs.SaveINISetting("comptes", compte, "", INIFILE)
                            divToUpdate.Add("div_comptes", builbTableComptes)
                        End If

                        'si PSWD est alimenté
                        If parts("id").StartsWith("PSWD_") Then
                            Dim compte = parts("id").Split("_")(1)
                            Dim pswd = parts(parts("id"))
                            If pswd = "" Then pswd = " "
                            hs.SaveINISetting(compte, "password", pswd, INIFILE)
                            divToUpdate.Add("div_comptes", builbTableComptes)
                        End If

                        'Si ref est alimenté
                        If parts("id").StartsWith("ref_") Then
                            Dim device = parts("id").Replace("__", " ").Split("_")(1)
                            hs.SaveINISetting(device, "ref", parts(parts("id")), INIFILE)
                            divToUpdate.Add("div_comptes", builbTableComptes)
                        End If



                        'si refresh est appuyé
                        If parts("id").StartsWith("Refesh_") Then



                            'Dim compte = parts("id").Replace("_at_", "@").Replace("_dot_", ".").Split("_")(1)
                            'Dim devices() = ic.getsDevicesFromAppleID(compte, hs.GetINISetting("comptes", compte, "", INIFILE))

                            'For Each device In hs.GetINISectionEx(compte, INIFILE)
                            '    device = device.Split("=")(0)
                            '    hs.SaveINISetting(compte, device, 0, INIFILE)
                            'Next

                            'For Each device In devices
                            '    hs.SaveINISetting(compte, device, "1", INIFILE)
                            '    hs.SaveINISetting(device, "ElementToUpdate", hs.GetINISetting(device, "ElementToUpdate", " ", INIFILE), INIFILE)
                            '    hs.SaveINISetting(device, "ref", hs.GetINISetting(device, "ref", " ", INIFILE), INIFILE)
                            'Next

                            'Dim oldDevices = hs.GetINISectionEx(compte, INIFILE)
                            'hs.ClearINISection(compte, INIFILE)

                            'For Each device In oldDevices

                            '    Dim actu = device.Split("=")(1)
                            '    device = device.Split("=")(0)
                            '    If (actu = 1) Then
                            '        hs.SaveINISetting(compte, device, "0", INIFILE)
                            '    Else
                            '        hs.ClearINISection(device, INIFILE)
                            '    End If
                            'Next
                            'Me.pageCommands.Add("closedialog", "div_wait")
                            'divToUpdate.Add("div_comptes", builbTableComptes)
                        End If
                    End If

            End Select
        Catch ex As Exception
            Log(ex.Message, MessageType.Error_)
        End Try
        Return MyBase.postBackProc(page, data, user, userRights)
    End Function

    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        Dim stb As New StringBuilder
        Dim instancetext As String = ""
        Try
            '  ic = New iCloud1()
            Me.reset()

            CurrentPage = Me



            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)
            End If
            If Instance <> "" Then instancetext = " - " & Instance
            Me.AddHeader(hs.GetPageHeader(pageName, "Find My iPhone" & " Configuration", "", "", True, False, True))
            stb.Append(hs.GetPageHeader(pageName, "Find My iPhone" & " Configuration", "", "", False, True, False, True))



            stb.Append(clsPageBuilder.DivStart("configuration", ""))
            stb.Append(DivStart("div_param", "style=""text-align: center;display: inline-block;"""))

            stb.Append("Elément à mettre à jour")
            Dim selectStatus As New clsJQuery.jqDropList("ElementToUpdate", pageName, False)
            Dim selected As Boolean = False

            Dim ElementsToUpdate = {"location", "location2", "value", "string"}
            For Each Str As String In ElementsToUpdate
                selected = hs.GetINISetting("param", "ElementToUpdate", 0, INIFILE) = Str
                selectStatus.AddItem(Str, Str, selected)
            Next

            stb.Append(selectStatus.Build)

            stb.Append("  ")

            stb.Append(" Délai de MAJ (en minutes) : ")

            Dim tb As New clsJQuery.jqTextBox("delai", "text", hs.GetINISetting("param", "refresh", 5, INIFILE), pageName, 4, False)
            tb.id = "delai"
            stb.Append(tb.Build)

            stb.Append("  ")
            stb.Append(getLogHTMLConfig(pageName))

            stb.Append(DivEnd())
            stb.Append("<br><br>")

            stb.Append(DivStart("div_addAccount", "style=""text-align: center;display: inline-block;"""))
            stb.Append(FormStart("new_account", "new_account", "post"))
            stb.Append("AppleId : ")
            tb = New clsJQuery.jqTextBox("account", "text", "", pageName, 50, False)
            stb.Append(tb.Build)
            stb.Append(" Mot de passe : ")
            tb = New clsJQuery.jqTextBox("password", "text", "", pageName, 20, False)
            stb.Append(tb.Build)
            stb.Append(" ")
            Dim butAdd As New clsJQuery.jqButton("submitNewAccount", "Ajouter", pageName, True)
            butAdd.id = "submitNewAccount"
            stb.Append(butAdd.Build)
            stb.Append(FormEnd())
            stb.Append(DivEnd())
            stb.Append("<br><br>")



            stb.Append(DivStart("div_comptes", "style=""text-align: center;display: inline-block;"""))
            stb.Append(builbTableComptes)
            stb.Append(DivEnd())

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
    ' construction des bloc comptes
    '**********************************************************
    Private Function builbTableComptes() As String

        Dim stb As New StringBuilder()


        Dim comptes() = hs.GetINISectionEx("comptes", INIFILE)
        For Each compte In comptes

            compte = compte.Split("=")(0)
            Dim appleID = hs.GetINISetting(compte, "appleID", "", INIFILE)
            Dim password = hs.GetINISetting(compte, "password", "", INIFILE)

            ' Dim cpt = compte.Replace("@", "_at_").Replace(".", "_dot_")
            Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
            tab.addRow()


            Dim buttonDelete As New clsJQuery.jqButton("Supp_" & compte, "Supprimer", PageName, False)
            buttonDelete.imagePathNormal = "../images/HomeSeer/ui/Delete.png"
            buttonDelete.AddStyle("float:right")
            Dim buttonRefresh As New clsJQuery.jqButton("Refesh_" & compte, "Rafraichir", PageName, False)
            buttonRefresh.width = 20
            buttonRefresh.height = 20
            buttonRefresh.imagePathNormal = "../images/HomeSeer/contemporary/refresh.png"
            buttonRefresh.AddStyle("float:right")
            buttonRefresh.visible = True
            buttonRefresh.functionToCallOnClick = "$.blockUI({ message: '<h2><img  src=""/images/HomeSeer/ui/spinner.gif"" /> Wait...</h2>' });"
            '  If (hs.GetINISetting("comptes", compte, " ", INIFILE) = " ") Then
            '  buttonRefresh.visible = False
            '  End If

            tab.addCell("<div style='float:left;'>" & compte.Replace("_", " ") & "</div>" & buttonRefresh.Build & " " & buttonDelete.Build, "", 2)

            tab.addRow()
            tab.addCell("Apple ID : ", "", 1)
            Dim tb As New clsJQuery.jqTextBox("appleID_" & compte, "text", appleID, PageName, 30, False)
            tb.id = "appleID_" & compte
            tab.addCell(tb.Build, "", 2)

            tab.addRow()
            tab.addCell("Mot de passe : ", "", 1)
            tb = New clsJQuery.jqTextBox("PSWD_" & compte, "text", password, PageName, 30, False)
            tb.id = "PSWD_" & compte
            tab.addCell(tb.Build, "", 2)

            tab.addEmptyRow(5, "../images/Default/c.png")
            Dim devices() = AppleIDs(compte).Devices  ' hs.GetINISectionEx(compte, INIFILE)
            For Each device In devices
                'device = device.Split("=")(0)
                tab.addRow()

                tab.addCell(buildTableIDevice(device.DeviceName), "", 1)

                ' tab.addCell(getSelectorStatus(compte & "_" & Str.Split("=")(0)).Build, "", 1)

            Next


            stb.Append(tab.GetHTML)
        Next

        '  Dim tab1 As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        ' tab1.addRow()
        '  Dim tb1 As New clsJQuery.jqTextBox("adresse_new", "text", "Nouveau_compte", PageName, 30, False)
        '  tb1.id = "adresse_new"

        '  tab1.addCell(tb1.Build, "", 2)

        ' stb.Append(tab1.GetHTML)

        Return stb.ToString
    End Function

    '**********************************************************
    ' construction des bloc iDevice
    '**********************************************************
    Private Function buildTableIDevice(device As String) As String

        Dim stb As New StringBuilder()
        Dim ref = hs.GetINISetting(device, "ref", 0, INIFILE)
        Dim dev = device.Replace(" ", "__")
        Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab.addRow()

        Dim str1 = "<div style='"
        If (ref = 0) Then
            str1 = str1 & "color: lightgray;"
        End If
        str1 = str1 & "'>" & device & "</div>"
        tab.addCell(str1, "", 2)

        tab.addRow()
        tab.addCell(" Référence du Device : ", "", 1)
        tab.addEmptyCell(2)
        Dim tb As New clsJQuery.jqTextBox("ref_" & dev, "text", ref, PageName, 10, False)
        tb.id = "ref_" & dev
        tab.addCell(tb.Build, "", 1)
        If ref > 0 Then

            tab.addEmptyRow(5, "../images/Default/c.png")


        End If
        stb.Append(tab.GetHTML)


        Return stb.ToString
    End Function
End Class

