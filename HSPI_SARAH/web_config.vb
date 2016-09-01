Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
   
    Dim listeClient As List(Of SarahClient) = New List(Of SarahClient)

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Console.WriteLine("page post: " & data)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)

       
        Select Case parts("id")

            Case "BTN_ADD"
                Dim strListe As String = hs.GetINISetting("CONFIG", "LISTE_CLIENTS", "", INIFILE) & "|" & parts("TB_NAME")
                hs.SaveINISetting("CONFIG", "LISTE_CLIENTS", strListe, INIFILE)
                listeClient.Add(New SarahClient(parts("TB_NAME")))
                listeClient.Last().Save()
                Me.divToUpdate.Add("listeClients", constructListeClients())

            Case Else
                If (parts("id") <> Nothing) Then

                    If (parts("id").Contains("TB_IpClient")) Then
                        Dim cl As SarahClient = New SarahClient(parts("id").Remove(0, 12))
                        cl.IP = parts("TB_IpClient_" & cl.Name)
                        cl.Save()
                        Me.divToUpdate.Add("listeClients", constructListeClients())

                    ElseIf (parts("id").Contains("TB_PortClient")) Then
                        Dim cl As SarahClient = New SarahClient(parts("id").Remove(0, 14))
                        cl.IP = parts("TB_PortClient_" & cl.Name)
                        cl.Save()
                        Me.divToUpdate.Add("listeClients", constructListeClients())
                    ElseIf (parts("id").Contains("BTN_envoyer")) Then
                        Dim cl As SarahClient = New SarahClient(parts("id").Remove(0, 12))
                    
                        Dim url = "http://" & cl.IP & ":" & cl.PORT & "/?tts=" & parts("TB_text_" & cl.Name) & "."
                        Me.divToUpdate.Add("result_" & cl.Name, url)
                        hs.GetURL(cl.IP, "/?tts=" & parts("TB_text_" & cl.Name) & ".", False, cl.PORT)

                    ElseIf (parts("id").Contains("BTN_SUPP_")) Then
                        Dim strListe As String = hs.GetINISetting("CONFIG", "LISTE_CLIENTS", "", INIFILE)
                        strListe = strListe.Replace("|" & parts("id").Remove(0, 9), "")
                        hs.SaveINISetting("CONFIG", "LISTE_CLIENTS", strListe, INIFILE)
                        hs.ClearINISection(parts("id").Remove(0, 9), INIFILE)
                        Me.divToUpdate.Add("listeClients", constructListeClients())
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
            stb.Append(hs.GetPageHeader(pageName, "SARAH" & instancetext, "", "", False, True))

            stb.Append(clsPageBuilder.DivStart("configuration", ""))



            stb.Append(clsPageBuilder.FormStart("FORMADD", "FORMADD", "post"))
            stb.Append("<input width='100%' id='TB_NAME' type='text' name='TB_NAME' ></input>")

            Dim BTN_ADD As clsJQuery.jqButton = New clsJQuery.jqButton("BTN_ADD", "Ajouter", Me.PageName, True)
            stb.Append(BTN_ADD.Build)

            stb.Append(clsPageBuilder.FormEnd)
            stb.Append("<br><br>")

            stb.Append(clsPageBuilder.DivStart("listeClients", ""))
            stb.Append(constructListeClients())
            stb.Append(DivEnd())




            stb.Append(DivEnd())

            Me.AddBody(stb.ToString)

            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function


    '**********************************************************
    ' construction de la liste de clients
    '**********************************************************
    Public Function constructListeClients() As String
        Dim stb As New StringBuilder
        For Each nClient As String In hs.GetINISetting("CONFIG", "LISTE_CLIENTS", "", INIFILE).Split("|")
            If (nClient <> "") Then
                stb.Append(getSTClient(New SarahClient(nClient)))
            End If
        Next
        Return stb.ToString()
    End Function

    '**********************************************************
    ' construction d'un slideTab Client
    '**********************************************************
    Public Function getSTClient(client As SarahClient) As String

        Dim stb As New StringBuilder
        Dim stC As New clsJQuery.jqSlidingTab("ST_client_" & client.Name, Me.PageName, False)
        stC.initiallyOpen = False
        stC.toolTip = "Configuration du Client SARAH : " & client.Name
        'st.tab.AddContent("the content")
        stC.tab.name = "ST_client_" & client.Name
        stC.tab.tabName.Unselected = "Configuration du Client SARAH : " & client.Name
        stC.id = "ST_client_" & client.Name
        stC.tab.tabName.Selected = "Configuration du Client SARAH : " & client.Name
        Dim stb2 As New StringBuilder
        stb2.Append(getSTConfig(client) & getSTtest(client))
        Dim BTN_SUPP As clsJQuery.jqButton = New clsJQuery.jqButton("BTN_SUPP_" & client.Name, "Supprimer", Me.PageName, False)
        stb2.Append(BTN_SUPP.Build)
        stC.tab.tabContent = stb2.ToString()
        Return stC.Build
    End Function

    '**********************************************************
    ' construction d'un slideTab Test
    '**********************************************************
    Function getSTtest(client As SarahClient) As String

        Dim stTest = New clsJQuery.jqSlidingTab("stTest_" & client.Name, Me.PageName, False)
        stTest.initiallyOpen = False
        stTest.toolTip = "Tester la connexion"
        'st.tab.AddContent("the content")
        stTest.tab.name = "stTest_" & client.Name
        stTest.tab.tabName.Unselected = "Tester la connexion au client"
        stTest.id = "stTest_" & client.Name
        stTest.tab.tabName.Selected = "Tester la connexion au client"

        Dim stb2 As New StringBuilder
        stb2.Append(clsPageBuilder.FormStart("FORMTEST_" & client.Name, "testpage", "post"))
        stb2.Append("<input width='100%' id='TB_TEXT_" & client.Name & "' type='text' name='TB_TEXT_" & client.Name & "' ></input><br><br>")

        Dim BTN_envoyer As clsJQuery.jqButton = New clsJQuery.jqButton("BTN_envoyer_" & client.Name, "Envoyer", Me.PageName, True)
        stb2.Append(BTN_envoyer.Build)

        stb2.Append(clsPageBuilder.FormEnd)
        stb2.Append("<br><br>")
        stb2.Append(DivStart("result_" & client.Name, ""))
        stb2.Append(DivEnd())

        stTest.tab.tabContent = stb2.ToString()
        Return stTest.Build
    End Function

    '**********************************************************
    ' construction d'un slideTab configuration
    '**********************************************************
    Function getSTConfig(client As SarahClient) As String
        Dim stb As New StringBuilder
        Dim st As clsJQuery.jqSlidingTab
        st = New clsJQuery.jqSlidingTab("ST_CONFIG_" & client.Name, Me.PageName, False)
        st.initiallyOpen = False
        st.toolTip = "Configuration du Client SARAH"
        'st.tab.AddContent("the content")
        st.tab.name = "ST_CONFIG_" & client.Name
        st.tab.tabName.Unselected = "Adresse de connection au client : " & AddNBSP(5) & client.IP & ":" & client.PORT
        st.id = "ST_CONFIG_" & client.Name
        st.tab.tabName.Selected = "Adresse de connection au client : "
        st.tab.tabContent = getTableConfig(client)
        Return st.Build
    End Function

    Function getTableConfig(client As SarahClient) As String
        Dim stb As New StringBuilder




        stb.Append(HTML_StartTable(1))

        'row IP Client SARAH
        '  Dim IP1 = hs.GetINISetting("CONFIG" & Instance, "IP", "127.0.0.1", INIFILE)
        stb.Append(HTML_StartRow("", "", HTML_Align.CENTER, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1))
        stb.Append("IP Client SARAH")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb As New clsJQuery.jqTextBox("TB_IpClient_" & client.Name, "text", client.IP, Me.PageName, 20, False)
        tb.id = "TB_IpClient_" & client.Name
        tb.promptText = "Adresse IP du poste sur lequel SARAH doit s'exprimer : "
        tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
        tb.dialogWidth = 200
        stb.Append(tb.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)

        'row Port SERVEUR

        ' Dim port = hs.GetINISetting("CONFIG" & Instance, "Port", "8888", INIFILE)

        stb.Append(HTML_StartRow("", "", HTML_Align.CENTER, "", HTML_VertAlign.MIDDLE))

        stb.Append(HTML_StartCell("", 1))
        stb.Append("Port Client SARAH")
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim tb1 As New clsJQuery.jqTextBox("TB_PortClient_" & client.Name, "text", client.PORT, Me.PageName, 10, False)
        tb1.id = "TB_PortClient_" & client.Name
        tb1.promptText = "Port du poste sur lequel SARAH doit s'exprimer : "
        tb1.toolTip = "Port IP du poste sur lequel SARAH doit s'exprimer"
        'tb.dialogWidth = 200
        stb.Append(tb1.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)
        stb.Append(HTML_EndTable)
        Return stb.ToString
    End Function

End Class

