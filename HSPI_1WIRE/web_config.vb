Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim listLocations(2) As List(Of String)
    Dim liste1Wire As List(Of Module1Wire)

    ' Dim listeClient As List(Of SarahClient) = New List(Of SarahClient)

    Public Sub New(ByVal pagename As String, lm As List(Of Module1Wire))
        MyBase.New(pagename)
        liste1Wire = lm
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        '  Console.WriteLine("page post: " & data)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)


        Select Case parts("id")

            Case "LogLevel"
                hs.SaveINISetting("param", "logLevel", parts("LogLevel"), INIFILE)

            Case Else
                If (parts("id") <> Nothing) Then
                    Dim idModule As String = parts("id").Split("_")(1)
                    Dim Canal As String = parts("id").Split("_")(2)
                    '  If (listLocations Is Nothing) Then listLocations = getListLocations()

                    Dim m As Module1Wire = New Module1Wire()
                    For Each modul In liste1Wire
                        If (modul.OneWireAdress = idModule) AndAlso (modul.Channel = Canal) Then
                            m = modul
                        End If
                    Next

                    If (parts("id").Contains("dblCoef_")) Then
                        m.dblCoef = parts("dblCoef_" & idModule & "_" & Canal)
                        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "COEFFICIENT", m.dblCoef, INIFILE)

                    ElseIf (parts("id").Contains("dblOffset_")) Then
                        m.dblOffset = parts("dblOffset_" & idModule & "_" & Canal)
                        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "OFFSET", m.dblOffset, INIFILE)

                    ElseIf (parts("id").Contains("sFormat_")) Then
                        m.sFormat = parts("sFormat_" & idModule & "_" & Canal)
                        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "FORMAT", m.sFormat, INIFILE)

                    ElseIf (parts("id").Contains("ARRONDI_")) Then
                        m.coeffArrondi = parts("ARRONDI_" & idModule & "_" & Canal).Replace(".", ",")
                        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "COEFFICIENT_ARRONDI", m.coeffArrondi, INIFILE)

                    ElseIf (parts("id").Contains("BTN")) Then



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
            Dim refresh As Boolean = True

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)
                If parts("getModules") = "false" Then
                    refresh = False
                End If
            End If


            If Instance <> "" Then instancetext = " - " & Instance

            stb.Append(clsPageBuilder.DivStart("configuration", ""))


            Me.AddHeader(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", True, False, True))
            stb.Append(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", False, True, False, True))





            '  listLocations = getListLocations()
            '    If refresh Then liste1Wire = getModules()


            stb.Append(getLogHTMLConfig(pageName))


            stb.Append(clsPageBuilder.DivStart("listeClients", ""))
            Dim table As New HTMLTable(0, 0, -1, "", "", HTML_TableAlign.INHERIT, 0, -1)
            table.addRow()
            table.addCell("Configuration générale", "", 1, "", HTML_Align.CENTER)


            For Each m As Module1Wire In liste1Wire
                table.addRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE)
                'row IP Client SARAH
                '  Dim IP1 = hs.GetINISetting("CONFIG" & Instance, "IP", "127.0.0.1", INIFILE)

                Dim BTN As clsJQuery.jqButton = New clsJQuery.jqButton("BTN_" & m.REF, m.ETAGE & " - " & m.PIECE & " - " & m.NAME & " (" & m.REF & ")", Me.PageName, False)
                BTN.id = "BTN_" & m.REF
                BTN.hyperlink = True

                BTN.url = "deviceutility?ref=" & m.REF & "&edit=1"
                BTN.urlNewWindow = True
                table.addCell(BTN.Build, "", 1, "../images/default/c.png")
                table.addEmptyCell(10)
                
                Dim dblCoef As New clsJQuery.jqTextBox("dblCoef_" & m.OneWireAdress & "_" & m.Channel, "text", m.dblCoef, Me.PageName, 5, False)
                dblCoef.id = "dblCoef_" & m.OneWireAdress & "_" & m.Channel
                dblCoef.promptText = "Coefficient multiplicateur : "
                ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
                table.addCell("Coefficient : <br>" & dblCoef.Build, "", 1, "../images/default/c.png")
                
                table.addEmptyCell(10)

                Dim dblOffset As New clsJQuery.jqTextBox("dblOffset_" & m.OneWireAdress & "_" & m.Channel, "text", m.dblOffset, Me.PageName, 5, False)
                dblOffset.id = "dblOffset_" & m.OneWireAdress & "_" & m.Channel
                dblOffset.promptText = "Offset : "
                ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
                table.addCell("Offset : <br>" & dblOffset.Build, "", 1, "../images/default/c.png")
                table.addEmptyCell(10)

                Dim sFormat As New clsJQuery.jqTextBox("sFormat_" & m.OneWireAdress & "_" & m.Channel, "text", m.sFormat, Me.PageName, 5, False)
                sFormat.id = "sFormat_" & m.OneWireAdress & "_" & m.Channel
                sFormat.promptText = "Format : "
                ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
                table.addCell("Format : <br>" & sFormat.Build, "", 1, "../images/default/c.png")

                table.addEmptyCell(10)

                Dim coefArrondi As New clsJQuery.jqTextBox("ARRONDI_" & m.OneWireAdress & "_" & m.Channel, "text", m.coeffArrondi, Me.PageName, 5, False)
                coefArrondi.id = "ARRONDI_" & m.OneWireAdress & "_" & m.Channel
                coefArrondi.promptText = "Arrondi : "
                ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
                table.addCell("Arrondi : <br>" & coefArrondi.Build, "", 1, "../images/default/c.png")
                ' stb.Append(getSTModule1Wire(m))

                '  stb.Append(m & "<br><br>")
            Next
            stb.Append(table.GetHTML())
            stb.Append(DivEnd())

            'stb.Append(clsPageBuilder.FormStart("FORMPORT", "FORMADD", "post"))
            'stb.Append("<input width='100%' id='TB_NAME' type='text' name='TB_NAME' ></input>")

            'stb.Append(clsPageBuilder.FormEnd)
            'stb.Append("<br><br>")

            'stb.Append(clsPageBuilder.DivStart("listeClients", ""))
            'stb.Append(constructListeClients())
            'stb.Append(DivEnd())

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
    ' construction d'un slideTab Module 1Wire
    '**********************************************************
    Public Function getSTModule1Wire(m As Module1Wire) As String


        Dim stb As New StringBuilder
        Dim stC As New clsJQuery.jqSlidingTab("STModule_" & m.OneWireAdress & "_" & m.Channel, Me.PageName, False)
        stC.initiallyOpen = False
        stC.toolTip = "Configuration du Client SARAH : " & m.OneWireAdress & "_" & m.Channel
        'st.tab.AddContent("the content")
        stC.tab.name = "STModule_" & m.OneWireAdress & "_" & m.Channel

        Dim strTitle As String = "Configuration du Module : "
        If (m.REF <> "") Then
            strTitle += HTML_StartFont(COLOR_GREEN)
        Else : strTitle += HTML_StartFont(COLOR_RED)
        End If
        strTitle += m.OneWireAdress & " / " & m.Channel
        strTitle += HTML_EndFont

        stC.tab.tabName.Unselected = strTitle
        stC.id = "STModule_" & m.OneWireAdress & "_" & m.Channel
        stC.tab.tabName.Selected = strTitle
        Dim stb2 As New StringBuilder
        stb2.Append(getTableConfig(m))
        stb2.Append(getSTConfig(m))

 
        stC.tab.tabContent = stb2.ToString()
        Return stC.Build
    End Function


    '**********************************************************
    ' construction d'un slideTab configuration
    '**********************************************************
    Function getSTConfig(m As Module1Wire) As String



        Dim stb2 As New StringBuilder
        Dim st As clsJQuery.jqSlidingTab
        st = New clsJQuery.jqSlidingTab("STCONFIG_" & m.OneWireAdress & "_" & m.Channel, Me.PageName, False)
        st.initiallyOpen = False
        st.toolTip = "Module Homeseer associé"
        'st.tab.AddContent("the content")
        st.tab.name = "STCONFIG_" & m.OneWireAdress & "_" & m.Channel
        st.tab.tabName.Unselected = "Module Homeseer associé : "
        st.id = "STCONFIG_" & m.OneWireAdress & "_" & m.Channel
        st.tab.tabName.Selected = "Module Homeseer associé :"

        stb2.Append(getTablemoduleHS(m))

        Dim BTNSAVE As clsJQuery.jqButton = New clsJQuery.jqButton("BTNSAVE_" & m.OneWireAdress & "_" & m.Channel, "Enregistrer les modifications", Me.PageName, False)
        BTNSAVE.id = "BTNSAVE_" & m.OneWireAdress & "_" & m.Channel
        'BTNSAVE.enabled = (m.REF <> "")
        stb2.Append(BTNSAVE.Build)


        Dim BTNSUPP As clsJQuery.jqButton = New clsJQuery.jqButton("BTNSUPP_" & m.OneWireAdress & "_" & m.Channel, "Supprimer le Module", Me.PageName, False)
        BTNSUPP.id = "BTNSUPP_" & m.OneWireAdress & "_" & m.Channel
        BTNSUPP.enabled = (m.REF <> "")
        stb2.Append(BTNSUPP.Build)

        st.tab.tabContent = stb2.ToString
        Return st.Build
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

    Function getTableConfig(m As Module1Wire) As String
        Dim stb As New StringBuilder

        stb.Append(HTML_StartTable(1))

        'row IP Client SARAH
        '  Dim IP1 = hs.GetINISetting("CONFIG" & Instance, "IP", "127.0.0.1", INIFILE)
        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1))
        stb.Append("adresse :")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb As New clsJQuery.jqTextBox("TBmoduleAdresse_" & m.OneWireAdress & "_" & m.Channel, "text", m.OneWireAdress, Me.PageName, 20, False)
        tb.id = "TBmoduleAdresse_" & m.OneWireAdress & "_" & m.Channel
        tb.promptText = "Adresse : "
        ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
        tb.dialogWidth = 200
        tb.enabled = False
        stb.Append(tb.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)

        

        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1))
        stb.Append("Type  :")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb1 As New clsJQuery.jqTextBox("TBTYPE_" & m.OneWireAdress & "_" & m.Channel, "text", m.OneWireType, Me.PageName, 20, False)
        tb1.id = "TBTYPE_" & m.OneWireAdress & "_" & m.Channel
        tb1.promptText = "Type "
        tb1.toolTip = "Type"
        tb1.enabled = False
        'tb.dialogWidth = 200
        stb.Append(tb1.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)

        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1))
        stb.Append("Canal du Module One Wire")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb2 As New clsJQuery.jqTextBox("TBcanal_" & m.OneWireAdress & "_" & m.Channel, "text", m.Channel, Me.PageName, 20, False)
        tb2.id = "TBcanal_" & m.OneWireAdress & "_" & m.Channel
        tb2.promptText = "Canal "
        tb2.toolTip = "Canal"
        tb2.enabled = False
        'tb.dialogWidth = 200
        stb.Append(tb2.Build)
        stb.Append(HTML_EndCell)
        stb.Append(HTML_EndRow)

        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1))
        stb.Append("Réf. Module HS :")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb3 As New clsJQuery.jqTextBox("TBREF_" & m.OneWireAdress & "_" & m.Channel, "text", m.REF, Me.PageName, 20, False)
        tb3.id = "TBREF_" & m.OneWireAdress & "_" & m.Channel
        tb3.promptText = "Référence HS"
        tb3.enabled = False
        stb.Append(tb3.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)


        stb.Append(HTML_EndTable)
        Return stb.ToString
    End Function

    Private Function getTablemoduleHS(m As Module1Wire) As String
        Dim stb As New StringBuilder
       

        stb.Append(HTML_StartTable(1))

        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1, HTML_Align.RIGHT))
        stb.Append("Nom :")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb4 As New clsJQuery.jqTextBox("TBNAME_" & m.OneWireAdress & "_" & m.Channel, "text", m.NAME, Me.PageName, 20, False)
        tb4.id = "TBNAME_" & m.OneWireAdress & "_" & m.Channel
        tb4.promptText = "Nom : "
        tb4.enabled = True
        stb.Append(tb4.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)

        'row HC - MC
        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))


        stb.Append(HTML_StartCell("", 1, HTML_Align.RIGHT))
        stb.Append("HouseCode :")
        stb.Append(HTML_EndCell)
        stb.Append(HTML_StartCell("", 1))
        Dim LBHC As New clsJQuery.jqDropList("LBHC_" & m.OneWireAdress & "_" & m.Channel, Me.PageName, False)
        LBHC.id = "LBHC_" & m.OneWireAdress & "_" & m.Channel
        LBHC.AddItem("", "", (m.HC = ""))
        For Each l In "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            LBHC.AddItem(l, l, l = m.HC)
        Next
        stb.Append(LBHC.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1, HTML_Align.RIGHT))
        stb.Append("Module Code :")
        stb.Append(HTML_EndCell)
        stb.Append(HTML_StartCell("", 1))
        Dim LBMC As New clsJQuery.jqDropList("LBMC_" & m.OneWireAdress & "_" & m.Channel, Me.PageName, False)
        LBMC.id = "LBMC_" & m.OneWireAdress & "_" & m.Channel
        LBMC.height = 100
        LBMC.AddItem("", "", (m.HC = ""))
        For i = 1 To 200
            LBMC.AddItem(i.ToString, i.ToString, i.ToString = m.MC)
        Next
        stb.Append(LBMC.Build)
        stb.Append(HTML_EndCell)
        stb.Append(HTML_EndRow)


        'row pièce - Etage
        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))


        stb.Append(HTML_StartCell("", 1, HTML_Align.RIGHT))
        stb.Append("Pièce :")
        stb.Append(HTML_EndCell)
        stb.Append(HTML_StartCell("", 1))
        Dim lb As New clsJQuery.jqDropList("LBPIECE_" & m.OneWireAdress & "_" & m.Channel, Me.PageName, False)
        lb.id = "LBPIECE_" & m.OneWireAdress & "_" & m.Channel
        lb.AddItem("", "", (m.PIECE = ""))
        For Each p In listLocations(0)
            lb.AddItem(p, p, (m.PIECE = p))
        Next
        stb.Append(lb.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1, HTML_Align.RIGHT))
        stb.Append("Etage :")
        stb.Append(HTML_EndCell)
        stb.Append(HTML_StartCell("", 1))
        Dim lb1 As New clsJQuery.jqDropList("LBETAGE_" & m.OneWireAdress & "_" & m.Channel, Me.PageName, False)
        lb1.id = "LBETAGE_" & m.OneWireAdress & "_" & m.Channel
        lb1.AddItem("", "", (m.ETAGE = ""))
        For Each p In listLocations(1)
            lb1.AddItem(p, p, (m.ETAGE = p))
        Next
        stb.Append(lb1.Build)
        stb.Append(HTML_EndCell)
        stb.Append(HTML_EndRow)

        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1, HTML_Align.RIGHT))
        stb.Append("Type :")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb3 As New clsJQuery.jqTextBox("TBHSTYPE_" & m.OneWireAdress & "_" & m.Channel, "text", m.hsType, Me.PageName, 20, False)
        tb3.id = "TBHSTYPE_" & m.OneWireAdress & "_" & m.Channel
        tb3.promptText = "Type : "
        tb3.enabled = True
        stb.Append(tb3.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)
        stb.Append(HTML_EndTable)
        Return stb.ToString()
    End Function

    

End Class

