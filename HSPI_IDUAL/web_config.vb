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
            Case "NewVolet"
                If (parts(parts("id")) > 0) Then
                    Dim refCOMMAND As Integer = parts(parts("id"))
                    '    inifileDevice = INIFILE_DEVICE.Replace("####", parts(parts("id")))
                    hs.SaveINISetting("MODULES", refCOMMAND, "0", INIFILE)
                    Dim refVOLET As Integer = CreateVoletParent(hs.GetDeviceByRef(refCOMMAND))
                    Dim dv As DeviceClass = hs.GetDeviceByRef(refVOLET)
                    Dim ped As New clsPlugExtraData()
                    ped.AddNamed("1000", 99999)
                    ' ped.AddNamed("STOP", 99999)
                    ped.AddNamed("900", 99999)
                    ped.AddNamed("timer", 60)
                    dv.PlugExtraData_Set(hs) = ped
                    dv.Interface(hs) = IFACE_NAME
                    createRelationShip(hs.GetINISetting("GROUPS", "GENERAL", "", INIFILE), refVOLET, Enums.eRelationship.Indeterminate)
                    divToUpdate.Add("list", getListAssociation)
                End If
            Case Else
                If (parts("id").StartsWith("Actions")) Then
                    Dim refCOMMAND As Integer = parts("id").Split("_")(2)
                    Dim dv As DeviceClass = hs.GetDeviceByRef(refCOMMAND)
                    Dim refParent = findParent(hs.GetINISetting("GROUPS", "GENERAL", "", INIFILE), refCOMMAND)
                    dv = hs.GetDeviceByRef(refParent)
                    Dim action As String = parts("id").Split("_")(1)
                    Dim value As Integer = parts(parts("id"))
                    Dim ped As clsPlugExtraData = dv.PlugExtraData_Get(hs)
                    ped.RemoveNamed(action)
                    ped.AddNamed(action, value)
                    dv.PlugExtraData_Set(hs) = ped
                    divToUpdate.Add("list", getListAssociation)
                End If

                If (parts("id").StartsWith("timer_")) Then
                    Dim refCOMMAND As Integer = parts("id").Split("_")(1)
                    Dim dv As DeviceClass = hs.GetDeviceByRef(refCOMMAND)
                    Dim refParent = findParent(hs.GetINISetting("GROUPS", "GENERAL", "", INIFILE), refCOMMAND)
                    dv = hs.GetDeviceByRef(refParent)
                    Dim action As String = parts("id").Split("_")(1)
                    Dim value As Integer = parts(parts("id"))
                    Dim ped As clsPlugExtraData = dv.PlugExtraData_Get(hs)
                    ped.RemoveNamed("timer")
                    ped.AddNamed("timer", value)
                    dv.PlugExtraData_Set(hs) = ped
                    divToUpdate.Add("list", getListAssociation)
                End If

                If (parts("id").StartsWith("suppr")) Then
                    Dim ModuleCOMMANDE = parts(parts("id"))
                    deleteVOLET(ModuleCOMMANDE)
                    hs.SaveINISetting("MODULES", ModuleCOMMANDE, "", INIFILE)
                    divToUpdate.Add("list", getListAssociation)
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

            stb.Append(clsPageBuilder.DivStart("list", "style=""text-align: center;display: inline-block;"""))
            stb.Append(getListAssociation())
            stb.Append(DivEnd)

            ' specific page starts here

            stb.Append(BuildContent)

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

    Public Function getTableauAjouter()
        Dim stb As New StringBuilder

        Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
        tab.addRow()
        Dim str = getallModuleHSHTMLSelector("NewVolet", PageName, "0", {})
        tab.addCell(str, "", 1)
        tab.addRow()
        tab.addCell("Sélectionner un module à ajouter", "", 1)
        stb.Append(tab.GetHTML)
        Return stb.ToString
    End Function

    Public Function getListAssociation()
        Dim stb As New StringBuilder
        For Each association In hs.GetINISectionEx("MODULES", INIFILE)

            Dim tab As New HTMLTable(1, 0, -1, "display: inherit; vertical-align: top;")
            tab.addRow()

            Dim asso = association.Split("=")(0)

            Dim dv As DeviceClass = hs.GetDeviceByRef(asso)
            Dim refParent = findParent(hs.GetINISetting("GROUPS", "GENERAL", "", INIFILE), asso)

            dv = hs.GetDeviceByRef(refParent)

            Dim str = getModuleCOMMANDEtoSupp("suppr_" & asso, PageName, asso)
            '   str += " associé à "
            '  str += getModuleHSHTMLSelector("Chauffage_" & asso, PageName, value, {})
            tab.addCell(str, "", 5)

            tab.addRow()

            '  tab.addCell("OUVRI", "", 1)
            '  tab.addEmptyCell(20)
            tab.addCell("Ouvrir : " & getDeviceActions(asso, "1000", PageName, dv.PlugExtraData_Get(hs).GetNamed("1000")), "", 1)
            '    tab.addEmptyCell(10)
            '      tab.addRow()
            '   tb = New clsJQuery.jqTextBox("stop_" & asso, "text", "", PageName, 5, False)
            '  tb.id = "stop_" & asso
            '      tab.addCell("Arrêter : " & getDeviceActions(asso, "STOP", PageName, dv.PlugExtraData_Get(hs).GetNamed("STOP")), "", 1)

            tab.addRow()
            'tb = New clsJQuery.jqTextBox("close_" & asso, "text", "", PageName, 5, False)
            ' tb.id = "close_" & asso
            tab.addCell("Fermer : " & getDeviceActions(asso, "900", PageName, dv.PlugExtraData_Get(hs).GetNamed("900")), "", 1)
            tab.addRow()
            Dim tb As New clsJQuery.jqTextBox("timer_" & asso, "text", dv.PlugExtraData_Get(hs).GetNamed("timer"), PageName, 5, False)
            tb.id = "timer_" & asso
            tab.addCell("Timer O-->F : " & tb.Build, "", 1)

            stb.Append(tab.GetHTML)
            '    stb.Append(DivEnd)

        Next
        stb.Append(getTableauAjouter().ToString())
        Return stb.ToString
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
                If (hs.GetINISection("MODULES", INIFILE).Contains(dv.Ref(Nothing))) Then
                Else
                    selected = dv.Ref(Nothing) = valueSelected
                    Dim str = dv.Location2(Nothing) & " - " & dv.Location(Nothing) & " - " & dv.Name(Nothing)
                    selectModuleHS.AddItem(str, dv.Ref(Nothing), selected)
                End If
            End If

        Next

        stb1.Append(selectModuleHS.Build)
        Return stb1.ToString()
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

