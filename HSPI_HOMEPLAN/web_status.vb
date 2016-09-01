Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.clsJQuery
Imports Scheduler.Classes

Public Class web_status
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim lbList As New Collection
    Dim ddTable As DataTable = Nothing
    Dim HC_Master As String = ""
    Dim HC_FAN As String = ""
    Dim HC_TEMP As String = ""
    Dim HC_PWRFL As String = ""
    Dim HC_SWING As String = ""
    Dim Taille_Img As Integer = 140

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String

        Dim parts As Collections.Specialized.NameValueCollection
        Dim value As String = ""
        Dim name As String = ""
        parts = HttpUtility.ParseQueryString(data)
        '   Console.WriteLine("post: " & data)

        divToUpdate.Add("current_time", DateTime.Now.ToString)
        Dim id As String = parts("id")
        If (id IsNot Nothing) Then
            If id.StartsWith("BTN_") Then

                Dim refDevice = id.Split("_")(1)
                Dim action As String = id.Split("_")(2)
                If action = "TOGGLE" Then
                    toogleDevice(refDevice)
                ElseIf action = "AFFICHE" Then
                    Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=visible")
                    Me.divToUpdate.Add("DIV_DETAIL", buildDETAIL(refDevice))
                Else
                    hs.SetDeviceValueByRef(refDevice, action, True)
                    Me.propertySet.Add("DIV_DETAIL", "setcss=visibility=hidden")
                    Me.divToUpdate.Add("DIV_DETAIL", "")
                End If
            ElseIf id.StartsWith("SLI_") Then
                Dim refDevice = id.Split("_")(1)
                Dim action As String = parts(id)
                hs.SetDeviceValueByRef(refDevice, action, True)
            End If
        End If

        Return MyBase.postBackProc(page, data, user, userRights)
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


    Structure ParametresClim
        Const HOUSECODE_MASTER = 0
        Const HOUSECODE_TEMP = 1
        Const HOUSECODE_FAN = 2
        Const HOUSECODE_PWFL = 3
        Const HOUSECODE_SWING = 4
    End Structure

    Public Sub updateModule(id As String, ByRef hsModule As HsModule)
        Dim refDevice As Integer = Convert.ToInt32(id)
        Dim Image As String = "../" + hs.DeviceVGP_GetGraphic(id, hs.DeviceValue(Convert.ToInt32(id)))
        Dim strImg = "<img style = ""height:  " & Taille_Img & "px; width: " & Taille_Img & """ align=""absmiddle"" id=""" & hsModule.dv.Code(Nothing) & """ name=""" & hsModule.dv.Code(Nothing) & """ src=""" & Image & """ onclick='process(id,""MODE"")'>"
        Me.divToUpdate.Add("DIV_IMG_" & id, getImageStatus(refDevice))
        Me.divToUpdate.Add("DIV_VAL_" & id, getValue(refDevice, "°"))
        Me.divToUpdate.Add("DIV_STR_" & id, hs.DeviceString(Convert.ToInt32(id)))

    End Sub


    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String, ByRef maison As Maison) As String
        Dim stb As New StringBuilder
        Dim instancetext As String = ""
        Try

            Me.reset()

            Me.AddHeader("<!-- Display compability for iOS Devices -->")
            Me.AddHeader("<meta name=""apple-mobile-web-app-capable"" content=""yes"">")

            Me.AddHeader("<meta name=""viewport"" content=""width = device-width, initial-Scale = 1.0, user-scalable = no""/>")
            Me.AddHeader("<meta name=""apple-mobile-web-app-status-bar-style"" content=""default""/>")
            Me.AddHeader("<!---translucent-->")


            Me.AddHeader("<!-- Splashscreen -->")
            Me.AddHeader("<!-- iPhone standard resolution 320x460 (landscape Not needed because all web apps start portrait On iPhone) -->")
            Me.AddHeader("<Link rel=""apple-touch-startup-image"" href=""../images/ete.png"" media=""(device-width:      320px)""/>")
            Me.AddHeader("<!-- iPhone high resolution (retina) 640x920 pixels (landscape Not needed because all web apps start portrait On iPhone) -->")
            Me.AddHeader("<Link rel=""apple-touch-startup-image"" href=""../images/splash_RADIATEUR.png"" media=""(device-width:      320px) And (-webkit-device-pixel-ratio: 2)""/>")
            Me.AddHeader("<!-- iPad Portrait 768x1004 -->")
            Me.AddHeader("<Link rel=""apple-touch-startup-image"" href=""../images/ete.png"" media=""(device-width   768px) And (orientation: portrait)""/>")
            Me.AddHeader("<!-- iPad Landscape 748x1024 (yes, A portrait image but With content rotated 90 degrees - allows 20px For status bar) -->")
            Me.AddHeader("<Link rel=""apple-touch-startup-image"" href=""../images/ete.png"" media=""(device-width:    768px) And (orientation: landscape)""/>")
            Me.AddHeader("<!-- iPad retina Portrait 1536x2008 -->")
            Me.AddHeader(" <Link rel=""apple-touch-startup-image"" href=""../images/ete.png"" media=""(device-width:   768px) And (orientation: portrait) And (-webkit-device-pixel-ratio: 2)""/>")
            Me.AddHeader("<!-- iPad retina Landscape 1496x2048 (yes, A portrait image but With content rotated 90 degrees - allows 40px For status bar) -->")
            Me.AddHeader(" <Link rel=""apple-touch-startup-image"" href=""../images/ete.png"" media=""(device-width:   768px) And (orientation: landscape) And (-webkit-device-pixel-ratio: 2)""/>")

            Me.AddHeader(" <meta name=""apple-touch-fullscreen"" content=""yes"">")




            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)
                HC_Master = parts("device")
            End If

            Dim lettercodeEnfant As String = Left(HC_Master, 1)
            Dim NumbercodeEnfant As Integer = Convert.ToInt16(HC_Master.Replace(lettercodeEnfant, ""))
            HC_FAN = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_FAN).ToString()
            HC_TEMP = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_TEMP).ToString()
            HC_PWRFL = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_PWFL).ToString()
            HC_SWING = lettercodeEnfant + (NumbercodeEnfant + ParametresClim.HOUSECODE_SWING).ToString()


            Dim dv As Scheduler.Classes.DeviceClass
            dv = hs.GetDeviceByRef(hs.DeviceExistsCode(HC_Master))
            Me.AddTitleBar("Clim " + dv.Location(hs), user, True, "", True, True, True, False)





            If Instance <> "" Then instancetext = " - " & Instance
            ' stb.Append(hs.GetPageHeader(pageName, "Sample" & instancetext, "", "", True, False))

            stb.Append(clsPageBuilder.DivStart("pluginpage", ""))

            ' a message area for error messages from jquery ajax postback (optional, only needed if using AJAX calls to get data)
            stb.Append(clsPageBuilder.DivStart("errormessage", "class='errormessage'"))
            stb.Append(clsPageBuilder.DivEnd)

            Me.RefreshIntervalMilliSeconds = 2000
            ' Me.pageCommands.Add("starttimer", "")


            ' specific page starts here
            stb.Append(DivStart("DIV_DETAIL", "style = ""visibility:hidden;height:436px; width:280px;position:fixed; z-index: 999; background-color:lightgray; border-style:solid"""))
            stb.Append(DivEnd)
            stb.Append(Me.buildCLIM)



            'je mets un div juste pour mettre à jour la page ?!?!
            stb.Append(clsJQuery.DivStart("current_time", "", False, False, "", "", pageName))
            stb.Append(clsJQuery.DivEnd)
            stb.Append(clsPageBuilder.DivEnd)
            stb.Append(Me.AddAjaxHandlerPost("action=updatetime1", pageName))
            ' add the body html to the page
            Me.AddBody(stb.ToString)

            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function



    Function buildCLIM() As String
        Dim stb As New StringBuilder
        stb.Append(DivStart("tableClim", "style = ""text-align:center; width:100%;height:100%;  background-color:white;position:absolute;"""))
        Dim tabl As New HTMLTable("tab_MODE", 1, False, -1, -1, "display inline-block;")

        tabl.addRow()


        Dim refDevice = hs.DeviceExistsCode(HC_Master)
        tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)


        refDevice = hs.DeviceExistsCode(HC_FAN)
        tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)

        tabl.addRow()
        refDevice = hs.DeviceExistsCode(HC_TEMP)
        Dim str As String = getValue(refDevice, "°")


        Dim jslider As jqSlider = New jqSlider("SLI_" & refDevice, 18, 35, hs.DeviceValue(refDevice), jqSlider.jqSliderOrientation.horizontal, 150, Me.PageName, False)
        str &= jslider.build()
        ' str &= "<input type="" range"" id="" SLI_" & refDevice & " name="" SLI_" & refDevice & " min="" 18"" max="" 35"" value=" & hs.DeviceValue(refDevice) & """>"
        tabl.addCell(str, "", 2, "", "", HTML_Align.CENTER, False, Taille_Img, 0, "", HTML_VertAlign.MIDDLE)
        tabl.addRow()

        refDevice = hs.DeviceExistsCode(HC_PWRFL)
        tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)

        refDevice = hs.DeviceExistsCode(HC_SWING)
        tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & getImageStatus(refDevice), "", 1, "", "", HTML_Align.CENTER, False, 105, 0, "", HTML_VertAlign.MIDDLE)


        stb.Append(tabl.GetHTML)
        stb.Append(DivEnd)
        Return stb.ToString

    End Function

    Function getImageStatus(refDevice As Integer, Optional ByVal value As Integer = -1) As String
        Dim Image As String = ""
        If (value = -1) Then
            Image = "../" + hs.DeviceVGP_GetGraphic(refDevice, hs.DeviceValue(refDevice))
        Else
            Image = "../" + hs.DeviceVGP_GetGraphic(refDevice, value)
        End If
        Dim strImg = ""

        If hs.DeviceVSP_CountStatus(refDevice) < 0 Then
            'Si pas d'action possible alors je met une image simple
            strImg &= "<img style = ""height: " & Taille_Img & "px; width: " & Taille_Img & """ align=""absmiddle"" id=""IMG_" & refDevice & """ name=""IMG_" & refDevice & """ src=""" & Image & """'>"
        Else
            'sinon je mets un  bouton
            Dim button As jqButton = New jqButton("BTN_" & refDevice, "test", PageName, False)
            button.imagePathNormal = Image
            button.height = Taille_Img
            button.width = Taille_Img
            Dim dv As DeviceClass = hs.GetDeviceByRef(refDevice)
            If hs.DeviceVSP_CountStatus(refDevice) = 2 And dv.AssociatedDevices_Count(hs) = 0 Then
                'si je n'aiu que 2 action possble alors je toggle
                button.id = "BTN_" & refDevice & "_TOGGLE"
            Else
                If (value = -1) Then
                    'sinon j'affiche un nouveau panel avec toutes les actions possibles car c'est le value du device
                    button.id = "BTN_" & refDevice & "_AFFICHE"
                Else
                    'je met un id avec la valeur a effectuer sur Homeseer
                    button.id = "BTN_" & refDevice & "_" & value
                End If

            End If
            strImg &= button.Build()
        End If
        ' 

        strImg &= "</div>"
        Return strImg
    End Function


    Function getValue(refDevice As Integer, unité As String) As String
        Return "<div id=""DIV_VAL_" & refDevice & """ style=""font-size: xx-large; font-weight: bold;"">" & hs.DeviceValue(refDevice) & " " & unité & "</div>"
    End Function

    Function Odd(ByVal value As Integer) As Boolean
        Return (value And 1) = 1
    End Function

    Function buildDETAIL(refDevice) As String
        Dim stb As New StringBuilder
        '   Dim refDevice = hs.DeviceExistsCode(HC_Master)
        Dim tabl As New HTMLTable("tab_MODE", 1, False, -1, -1, "height:436px; width:280px;")



        Dim last As Boolean = False
        Dim nb As Integer = hs.DeviceVSP_GetAllStatus(refDevice).Length
        Dim nbLigne As Integer = 0
        Dim reste = 0
        If Odd(nb) Then
            reste = 1
        End If
        reste = 1
        Dim memTaille = Taille_Img
        Taille_Img = 100
        For Each vsPair As HomeSeerAPI.VSPair In hs.DeviceVSP_GetAllStatus(refDevice)

            If nb = hs.DeviceVSP_GetAllStatus(refDevice).Length Then
                tabl.addRow()

                tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & getImageStatus(refDevice, vsPair.Value), "", 2, "", "", HTML_Align.CENTER, False, 100, 100, "", HTML_VertAlign.MIDDLE)


            Else
                If (last = False) Then
                    tabl.addRow()
                End If
                Dim colspan = "1"
                If nb = reste Then
                    colspan = "2"
                End If

                tabl.addCell("<div id=""DIV_IMG_" & refDevice & """>" & getImageStatus(refDevice, vsPair.Value), "", colspan, "", "", HTML_Align.CENTER, False, 100, 100, "", HTML_VertAlign.MIDDLE)

                If (last) Then

                End If

                last = Not (last)
            End If
            nb = nb - 1

        Next
        Taille_Img = memTaille

        stb.Append(tabl.GetHTML())
        Return stb.ToString

    End Function


    Sub PostMessage(ByVal sMessage As String)
        Me.divToUpdate.Add("message", sMessage)
        Me.pageCommands.Add("starttimer", "")
        TimerEnabled = True
    End Sub

End Class
