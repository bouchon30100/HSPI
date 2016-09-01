Imports System.Text
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports HomeSeerAPI
Imports Scheduler
Imports Scheduler.Classes
Imports Scheduler.clsJQuery

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim listLocations(2) As List(Of String)
    Public Shared divPrincipal As String = ""
    Public Shared divSecondaire As String = ""
    Public Shared AffichageSecondaire As Boolean = False

    ' Dim listeClient As List(Of SarahClient) = New List(Of SarahClient)

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)

    End Sub



    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Console.WriteLine("page post1: " & data)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)



        divToUpdate.Add("current_time", DateTime.Now.ToString)
        Dim id As String = parts("id")
        If (id IsNot Nothing) Then
            If id.Contains("BTN_") Then

                Dim refDevice = id.Split("_")(1)
                Dim action As String = id.Split("_")(2)
                If action = "TOGGLE" Then
                    Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                    Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                    Me.divToUpdate.Add("DIV_DETAIL", "")
                    divSecondaire = divPrincipal = ""
                    toogleDevice(refDevice)
                ElseIf action = "AFFICHE"
                    If (divPrincipal = "") Then
                        Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=visible")
                        divPrincipal = buildPrincipal(refDevice)
                        Me.divToUpdate.Add("DIV_DETAIL", divPrincipal)
                    Else
                        Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=visible")
                        divSecondaire = buildDETAIL(refDevice)
                        Me.divToUpdate.Add("DIV_DETAIL", divSecondaire)
                    End If

                Else
                    hs.SetDeviceValueByRef(refDevice, action, True)
                    Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                    Me.divToUpdate.Add("DIV_DETAIL", "")
                    divSecondaire = ""
                    divPrincipal = ""
                End If

            ElseIf id.Contains("SEL_") Then
                Dim refDevice = id.Split("_")(1)
                Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                Me.divToUpdate.Add("DIV_DETAIL", "")
                divSecondaire = ""
                divPrincipal = ""
                hs.SetDeviceValueByRef(refDevice, Convert.ToInt32(parts(id)), True)

            ElseIf id.StartsWith("SLI_") Then
                Dim refDevice = id.Split("_")(1)
                Dim action As String = parts(id)
                hs.SetDeviceValueByRef(refDevice, action, True)
                Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                Me.divToUpdate.Add("DIV_DETAIL", "")
                divSecondaire = ""
                divPrincipal = ""

            ElseIf id.StartsWith("ACTUALISE") Then
                HSPI.styleFilter.Clear()
                HSPI.styleFilter.AddRange(hs.GetINISetting("POSITION", "TYPES", "", INIFILE).Trim().Split(","))
                HSPI.maison.ContruireMaison(HSPI.styleFilter)
                Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                Me.divToUpdate.Add("DIV_DETAIL", "")
                divSecondaire = ""
                divPrincipal = ""
                Me.divToUpdate.Add("plugin", Me.GetPagePlugin(PageName, user, userRights, data, HSPI.maison))
            ElseIf id.StartsWith("CLOSE") Then
                Dim refDevice = id.Split("_")(1)
                If divSecondaire <> "" Then

                    Me.divToUpdate.Add("DIV_DETAIL", divPrincipal)
                    divSecondaire = ""
                Else
                    Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                    Me.divToUpdate.Add("DIV_DETAIL", "")
                    divSecondaire = ""
                    divPrincipal = ""
                End If
            End If
        End If

        Return MyBase.postBackProc(page, data, user, userRights)
    End Function


    Dim Taille_Img As Integer = 140
    Structure ParametresClim
        Const HOUSECODE_MASTER = 0
        Const HOUSECODE_TEMP = 1
        Const HOUSECODE_FAN = 2
        Const HOUSECODE_PWFL = 3
        Const HOUSECODE_SWING = 4
    End Structure

    Function buildPrincipal(refDev As Integer) As String
        Dim stb As New StringBuilder


        stb.Append(clsPageBuilder.DivStart("tableClim", "style = ""text-align:center; width:100%;height:100%;  background-color:white;z-index: 999; position: relative;"""))
        Dim jbut As New jqButton("CLOSE_" & refDev, "Fermer", PageName, False)
        jbut.AddStyle("float: right;")
        stb.Append(jbut.Build)
        Dim tabl As New HTMLTable("tab_MODE", 0, False, -1, -1, "display inline-block;height:100%; width:100%;")

        Dim i = 1

        Dim dv As DeviceClass = hs.GetDeviceByRef(refDev)
        If dv.AssociatedDevices_Count(Nothing) = 0 Then
            Return buildDETAIL(refDev)
        Else

            tabl.addRow()
            Dim m As New HsModule(dv)

            ' tabl.addCell("<div id=""DIV_IMG_" & refDev & """>" & HsModule.getImageStatus(refDev), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)
            tabl.addCell(m.construireModuleHTML(), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)

            For Each ref As String In dv.AssociatedDevices_List(Nothing).Split(",")
                If (i > 1) Then
                    tabl.addRow()
                    i = 0
                End If
                'tabl.addCell("<div id=""DIV_IMG_" & ref & """>" & HsModule.getImageStatus(ref), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)
                m = New HsModule(hs.GetDeviceByRef(ref))
                tabl.addCell(m.construireModuleHTML(), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)
                i += 1
            Next


        End If


        'Dim HC_Master = dv.Code(Nothing)
        'Dim lettercodeEnfant As String = Left(HC_Master, 1)
        'Dim NumbercodeEnfant As Integer = Convert.ToInt16(HC_Master.Replace(lettercodeEnfant, ""))
        'Dim HC_FAN = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_FAN).ToString()
        'Dim HC_TEMP = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_TEMP).ToString()
        'Dim HC_PWRFL = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_PWFL).ToString()
        'Dim HC_SWING = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_SWING).ToString()



        'refDevice = hs.DeviceExistsCode(HC_FAN)
        'tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & HsModule.getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)

        'tabl.addRow()
        'refDevice = hs.DeviceExistsCode(HC_TEMP)
        'Dim str As String = HsModule.getValue(refDevice, "°")


        'Dim jslider As jqSlider = New jqSlider("SLI_" & refDevice, 18, 35, hs.DeviceValue(refDevice), jqSlider.jqSliderOrientation.horizontal, 150, IFACE_NAME, False)
        'str &= jslider.build()
        '' str &= "<input type="" range"" id="" SLI_" & refDevice & " name="" SLI_" & refDevice & " min="" 18"" max="" 35"" value=" & hs.DeviceValue(refDevice) & """>"
        'tabl.addCell(str, "", 2, "", "", HTML_Align.CENTER, False, Taille_Img, 0, "", HTML_VertAlign.MIDDLE)
        'tabl.addRow()

        'refDevice = hs.DeviceExistsCode(HC_PWRFL)
        'tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & HsModule.getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)

        'refDevice = hs.DeviceExistsCode(HC_SWING)
        'tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & HsModule.getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)


        stb.Append(tabl.GetHTML)
        stb.Append(DivEnd)
        Return stb.ToString

    End Function

    Function Odd(ByVal value As Integer) As Boolean
        Return (value And 1) = 1
    End Function

    Function buildDETAIL(refDevice) As String
        Dim stb As New StringBuilder
        '   Dim refDevice = hs.DeviceExistsCode(HC_Master)
        Dim jbut As New jqButton("CLOSE_" & refDevice, "Revenir", PageName, False)
        jbut.AddStyle("float: right;")
        stb.Append(jbut.Build)

        Dim last As Boolean = False
        Dim nb As Integer = hs.DeviceVSP_GetAllStatus(refDevice).Length

        Dim memTaille = Taille_Img
        Taille_Img = 100
        Dim tabl As New HTMLTable("tab_MODE", 0, False, -1, -1, "height:" + (nb / 2 * Taille_Img).ToString + "px; width:100%;")

        Dim nbLigne As Integer = 0
        Dim reste = 0
        If Odd(nb) Then
            reste = 1
        End If
        reste = 1

        Dim VsPairs As VSPair() = hs.DeviceVSP_GetAllStatus(refDevice)

        For Each vsPair As HomeSeerAPI.VSPair In VsPairs


            If nb = hs.DeviceVSP_GetAllStatus(refDevice).Length Then
                tabl.addRow()

                If (vsPair.PairType = VSVGPairType.Range) Then
                    tabl.addCell(getSelector(refDevice, vsPair.RangeStart, vsPair.RangeEnd), "", 2, "", "", HTML_Align.CENTER, False, 100, 100, "", HTML_VertAlign.MIDDLE)
                Else
                    tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & HsModule.getImageStatus(refDevice, vsPair.Value), "", 2, "", "", HTML_Align.CENTER, False, 100, 100, "", HTML_VertAlign.MIDDLE)

                End If


            Else
                If (last = False) Then
                    tabl.addRow()
                End If
                Dim colspan = "1"
                If nb = reste Then
                    colspan = "2"
                End If
                If (vsPair.PairType = VSVGPairType.Range) Then
                    tabl.addCell(getSelector(refDevice, vsPair.RangeStart, vsPair.RangeEnd), "", 2, "", "", HTML_Align.CENTER, False, 100, 100, "", HTML_VertAlign.MIDDLE)
                Else
                    tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & HsModule.getImageStatus(refDevice, vsPair.Value), "", colspan, "", "", HTML_Align.CENTER, False, 100, 100, "", HTML_VertAlign.MIDDLE)
                End If


                last = Not (last)
            End If
            nb = nb - 1

        Next
        Taille_Img = memTaille

        stb.Append(tabl.GetHTML())
        Return stb.ToString

    End Function

    Private Function getSelector(refDevice As Object, rangeStart As Double, rangeEnd As Double) As String
        'TODO : retourner le html d'un slecteur
        Dim stb As New StringBuilder()
        Dim sel As New jqDropList("SEL_" & refDevice, PageName, False)
        For i = rangeStart To rangeEnd
            sel.AddItem(i, i, i = hs.DeviceValue(refDevice))
        Next

        stb.Append(sel.Build)
        stb.Append("<br>")
        stb.Append(hs.GetDeviceByRef(refDevice).Name(Nothing))
        Return stb.ToString
    End Function

    Sub toogleDevice(refDevice As Integer)

        Dim VSPs As VSPair() = hs.DeviceVSP_GetAllStatus(refDevice)
        For Each VSP As VSPair In VSPs

            If VSP.Value <> hs.DeviceValue(refDevice) Then
                hs.SetDeviceValueByRef(refDevice, VSP.Value, True)
                Return
            End If

        Next

    End Sub


    Public Sub updateModule(id As String, ByRef hsModule As HsModule)

        Me.divToUpdate.Add(id, hsModule.getHtml(Me.PageName))

    End Sub


    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String, ByRef maison As Maison) As String
        Dim stb As New StringBuilder
        AffichageSecondaire = False

        divSecondaire = ""
        divPrincipal = ""
        Me.RefreshIntervalMilliSeconds = 2000
        '  Me.pageCommands.Add("starttimer", "")

        Dim instancetext As String = ""
        Try

            Me.reset()
            Me.AddStyleSheet("css/homeplan.css")
            CurrentPage = Me
            Dim refresh As Boolean = True

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)

            End If
            '  Me.AddHeader(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", True, False, True))
            '    stb.Append(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", False, True, False, True))


            If Instance <> "" Then instancetext = " - " & Instance

            stb.Append(DivStart("plugin", "style = ""height:100%; width:100%;z-index: 500;position: absolute;"""))

            stb.Append(maison.getHtml(Me.PageName))
            stb.Append("<div id='current_time'>" & DateTime.Now.ToString & "</div>" & vbCrLf)
            Dim button As New jqButton("ACTUALISE", "Acutaliser", pageName, False)
            stb.Append(button.Build)


            stb.Append(DivEnd())


            stb.Append(clsPageBuilder.DivStart("DIV_DETAIL", "style = "" width: 100%;visibility: hidden; height: 100%; width: 280px; z-index: 999; border-style: solid; top: 50px;margin-left: auto; margin-right: auto;position: relative;background-color: white;"""))
            stb.Append(DivEnd)
            stb.Append(Me.AddAjaxHandlerPost("action=updatetime", pageName))
            Me.AddBody(stb.ToString)
            '     Me.AddFooter(hs.GetPageFooter)
            AffichageSecondaire = True
            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
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



End Class

