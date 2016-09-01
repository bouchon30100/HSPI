Imports System
Imports Scheduler
Imports HomeSeerAPI
Imports HSCF.Communication.Scs.Communication.EndPoints.Tcp
Imports HSCF.Communication.ScsServices.Client
Imports HSCF.Communication.ScsServices.Service
Imports System.Reflection
Imports Scheduler.Classes
Imports System.Threading
Imports System.Net
Imports System.Text

Public Class HSPI
    Implements IPlugInAPI    ' this API is required for ALL plugins

    Dim sConfigPage As String = "SMS - Configuration"

    ' Dim sStatusPage As String = "Sample_Status"
    Dim ConfigPage As New web_config(sConfigPage)
    Dim ReceiveSMSWebPage As New SMSWebPage("SMS")
    ' Dim StatusPage As New web_status(sStatusPage)
    Dim WebPage As Object
    Dim actions As New hsCollection
    Dim action As New action

    Public Function PluginFunction(ByVal proc As String, ByVal parms() As Object) As Object Implements IPlugInAPI.PluginFunction
        Try
            Dim ty As Type = Me.GetType
            Dim mi As MethodInfo = ty.GetMethod(proc)
            If mi Is Nothing Then
                Log("Method " & proc & " does not exist in this plugin.", MessageType.Error_)
                Return Nothing
            End If
            Return (mi.Invoke(Me, parms))
        Catch ex As Exception
            Log("Error in PluginProc: " & ex.Message, MessageType.Error_)
        End Try

        Return Nothing
    End Function

    Public Function PluginPropertyGet(ByVal proc As String, parms() As Object) As Object Implements IPlugInAPI.PluginPropertyGet
        Try
            Dim ty As Type = Me.GetType
            Dim mi As PropertyInfo = ty.GetProperty(proc)
            If mi Is Nothing Then
                Log("Property " & proc & " does not exist in this plugin.", MessageType.Error_)
                Return Nothing
            End If
            Return mi.GetValue(Me, parms)
        Catch ex As Exception
            Log("Error in PluginPropertyGet: " & ex.Message, MessageType.Error_)
        End Try

        Return Nothing
    End Function

    Public Sub PluginPropertySet(ByVal proc As String, value As Object) Implements IPlugInAPI.PluginPropertySet
        Try
            Dim ty As Type = Me.GetType
            Dim mi As PropertyInfo = ty.GetProperty(proc)
            If mi Is Nothing Then
                Log("Property " & proc & " does not exist in this plugin.", MessageType.Error_)
            End If
            mi.SetValue(Me, value, Nothing)
        Catch ex As Exception
            Log("Error in PluginPropertySet: " & ex.Message, MessageType.Error_)
        End Try
    End Sub

    Public ReadOnly Property Name As String Implements HomeSeerAPI.IPlugInAPI.Name
        Get
            Return IFACE_NAME
        End Get
    End Property

    Public ReadOnly Property HSCOMPort As Boolean Implements HomeSeerAPI.IPlugInAPI.HSCOMPort
        Get
            Return False
        End Get
    End Property

    Public Function Capabilities() As Integer Implements HomeSeerAPI.IPlugInAPI.Capabilities
        Return HomeSeerAPI.Enums.eCapabilities.CA_IO
    End Function

    Public Function AccessLevel() As Integer Implements HomeSeerAPI.IPlugInAPI.AccessLevel
        Return 1
    End Function

    Public Function InterfaceStatus() As HomeSeerAPI.IPlugInAPI.strInterfaceStatus Implements HomeSeerAPI.IPlugInAPI.InterfaceStatus
        Dim es As New IPlugInAPI.strInterfaceStatus
        es.intStatus = IPlugInAPI.enumInterfaceStatus.OK
        Return es
    End Function

    Public Function SupportsMultipleInstances() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsMultipleInstances
        Return False
    End Function

    Public Function SupportsMultipleInstancesSingleEXE() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsMultipleInstancesSingleEXE
        Return False
    End Function

    Public Function InstanceFriendlyName() As String Implements HomeSeerAPI.IPlugInAPI.InstanceFriendlyName
        Return IFACE_NAME
    End Function

    'Public Function SendResponse(ByVal request As HttpListenerRequest) As String
    '    Return String.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now)
    'End Function

    'Dim Ws As WebServer

    Public Function InitIO(ByVal port As String) As String Implements HomeSeerAPI.IPlugInAPI.InitIO
        Try
            If hs.GetINISetting("param", "moduleRef", "", INIFILE) = "" Then
                Dim dv = CreateDevice("SMS recu", "MAISON", "MAISON", "SMS")
                dv.Interface(hs) = IFACE_NAME
                hs.SaveINISetting("param", "moduleRef", dv.Ref(Nothing), INIFILE)
            Else
                If hs.GetDeviceByRef(hs.GetINISetting("param", "moduleRef", "", INIFILE)) Is Nothing Then
                    Dim dv = CreateDevice("SMS recu", "MAISON", "MAISON", "SMS")
                    dv.Interface(hs) = IFACE_NAME
                    hs.SaveINISetting("param", "moduleRef", dv.Ref(Nothing), INIFILE)
                End If
            End If

            If hs.GetINISetting("param", "moduleSender", "", INIFILE) = "" Then
                Dim dv = CreateDevice("SMS Sender", "MAISON", "MAISON", "SMS")
                dv.Interface(hs) = IFACE_NAME
                hs.SaveINISetting("param", "moduleSender", dv.Ref(Nothing), INIFILE)
            Else
                If hs.GetDeviceByRef(hs.GetINISetting("param", "moduleSender", "", INIFILE)) Is Nothing Then
                    Dim dv = CreateDevice("SMS Sender", "MAISON", "MAISON", "SMS")
                    dv.Interface(hs) = IFACE_NAME
                    hs.SaveINISetting("param", "moduleSender", dv.Ref(Nothing), INIFILE)
                End If
            End If


            '  Ws = New WebServer({"http://192.168.0.232:9090/"}, AddressOf SendResponse)
            '  Ws.Start()


            RegisterWebPage(ConfigPage.PageName)
            RegisterWebPage(ReceiveSMSWebPage.PageName)
            callback.RegisterEventCB(Enums.HSEvent.VALUE_CHANGE, IFACE_NAME, "")

        Catch ex As Exception
            Return "Error on InitIO (" + IFACE_NAME + "): " & ex.Message
        End Try
        Return ""
    End Function

    Public Function RaisesGenericCallbacks() As Boolean Implements HomeSeerAPI.IPlugInAPI.RaisesGenericCallbacks
        Return True
    End Function

    Public Sub SetIOMulti(colSend As System.Collections.Generic.List(Of HomeSeerAPI.CAPI.CAPIControl)) Implements HomeSeerAPI.IPlugInAPI.SetIOMulti
        Dim dv As DeviceClass
        For Each element In colSend
            dv = hs.GetDeviceByRef(element.Ref)
            Dim str As String = " - module " & element.Ref & ": " & dv.Location(Nothing) & " - " & dv.Location2(Nothing) & " - " & dv.Name(Nothing) & " - " & dv.devValue(Nothing) & " --> " & element.ControlValue
            Console.WriteLine("SetIOMulti : " & str)

            hs.SetDeviceValueByRef(dv.Ref(Nothing), element.ControlValue, True)

            'Dim ValueToApply = dv.PlugExtraData_Get(hs).GetNamed(element.ControlValue.ToString)
            'If ValueToApply Is Nothing Then
            '    ValueToApply = element.ControlValue
            'End If

            'For Each refchild As Integer In dv.AssociatedDevices(Nothing)
            '    updateArbreDeDevicesVolets(refchild, ValueToApply)
            'Next


            'If (element.ControlValue < 900) Then



            '    ' Dim t As New Thread(AddressOf updateVolet)
            '    Dim Evaluator = New Thread(Sub() updateVolet(dv, element.ControlValue.ToString))
            '    Evaluator.Start()




            'End If
        Next



    End Sub



    Public Sub ShutdownIO() Implements HomeSeerAPI.IPlugInAPI.ShutdownIO
        Try
            Try
                'Ws.Stop()
                hs.SaveEventsDevices()
            Catch ex As Exception
                Log("could not save devices")
            End Try
            bShutDown = True
        Catch ex As Exception
            Log("Error ending " & IFACE_NAME & " Plug-In")
        End Try
    End Sub

    Public Sub HSEvent(ByVal EventType As Enums.HSEvent, ByVal parms() As Object) Implements IPlugInAPI.HSEvent
        '     Console.WriteLine("HSEvent: " & EventType.ToString)



        Select Case EventType
            Case Enums.HSEvent.VALUE_CHANGE
                If (hs.GetINISetting("param", "moduleRef", "", INIFILE) = parms(4)) Then
                    Dim dv As DeviceClass = hs.GetDeviceByRef(parms(4))
                    'hs.SetDeviceString(parms(4), "Quelle heure est-il ?", True)
                    If (dv.devString(Nothing) <> "") Then
                        '    hs.SetDeviceString(parms(4), "Quelle heure est-il ?", False)
                        Dim DvSender As DeviceClass = hs.GetDeviceByRef(hs.GetINISetting("param", "moduleSender", "", INIFILE))
                        Dim result As String = hs.PluginFunction("SARAH", "", "AskSARAH", New Object() {dv.devString(Nothing), DvSender.devString(Nothing), "pepito"})
                        result = replacesmileys(result)
                        result = DecodeUTF8(result)
                        Log(sendSMS(DvSender.devString(Nothing), "", result) & " : " & result, LogLevel.Debug)
                        hs.SetDeviceString(DvSender.Ref(Nothing), "", True)
                        hs.SetDeviceString(dv.Ref(Nothing), "", True)
                    End If
                End If



        End Select
    End Sub



    Public Function PollDevice(ByVal dvref As Integer) As IPlugInAPI.PollResultInfo Implements IPlugInAPI.PollDevice

    End Function

    Public Function GenPage(ByVal link As String) As String Implements HomeSeerAPI.IPlugInAPI.GenPage
        Return Nothing
    End Function

    Public Function PagePut(ByVal data As String) As String Implements HomeSeerAPI.IPlugInAPI.PagePut
        Return Nothing
    End Function

    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String Implements HomeSeerAPI.IPlugInAPI.GetPagePlugin
        ' build and return the actual page
        WebPage = SelectPage(pageName)
        Return WebPage.GetPagePlugin(pageName, user, userRights, queryString)
    End Function

    Public Function PostBackProc(ByVal pageName As String, ByVal data As String, ByVal user As String, ByVal userRights As Integer) As String Implements HomeSeerAPI.IPlugInAPI.PostBackProc
        WebPage = SelectPage(pageName)
        Return WebPage.postBackProc(pageName, data, user, userRights)
    End Function

    Private Function SelectPage(ByVal pageName As String) As Object
        SelectPage = Nothing
        Select Case pageName
            Case ConfigPage.PageName
                SelectPage = ConfigPage
            Case ReceiveSMSWebPage.PageName
                SelectPage = ReceiveSMSWebPage
            Case Else
                SelectPage = ConfigPage
        End Select
    End Function

    Private mvarActionAdvanced As Boolean
    Public Property ActionAdvancedMode As Boolean Implements HomeSeerAPI.IPlugInAPI.ActionAdvancedMode
        Set(ByVal value As Boolean)
            mvarActionAdvanced = value
        End Set
        Get
            Return mvarActionAdvanced
        End Get
    End Property

    Public Function ActionBuildUI(ByVal sUnique As String, ByVal ActInfo As IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.ActionBuildUI
        Dim UID As String
        UID = ActInfo.UID.ToString
        Dim Dest As String = ""
        Dim text As String = ""
        Dim IP As String = ""
        Dim port As String = ""

        If Not (ActInfo.DataIn Is Nothing) Then
            DeSerializeObject(ActInfo.DataIn, action)
        Else 'new event, so clean out the action object
            action = New action
        End If

        For Each sKey In action.Keys
            Select Case True
                Case InStr(sKey, "text_") > 0
                    text = action(sKey)
                Case InStr(sKey, "Dest_") > 0
                    Dest = action(sKey)
            End Select
        Next

        'ajout du Destinataire
        Dim stb As New StringBuilder
        Dim dd As New clsJQuery.jqDropList("Dest_" & UID & sUnique, "Events", True)
        dd.autoPostBack = True
        dd.AddItem("--Please Select--", "", True)
        Dim i = 0
        For Each C In hs.GetINISectionEx("TELEPHONES", INIFILE)
            If (C <> "") Then
                Dim destinataire As String = C.Split("=")(0)
                'Dim number As String = C.Split("=")(1)
                dd.AddItem(destinataire, destinataire, (destinataire = Dest))
                i += 1
            End If
        Next
        stb.Append("Destinataire : ")
        stb.Append(dd.Build)

        'ajout texte à envoyer 
        stb.Append(" Texte : ")
        Dim tb As New clsJQuery.jqTextBox("text_" & UID & sUnique, "text", text, "Events", 80, True)
        tb.id = "text_" & UID & sUnique
        stb.Append(tb.Build)

        Return stb.ToString()
    End Function

    Public Function ActionConfigured(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.ActionConfigured
        Dim Configured As Boolean = False

        Dim UID As String
        UID = ActInfo.UID.ToString

        Dim sKey As String
        Dim itemsConfigured As Integer = 0
        Dim itemsToConfigure As Integer = 2

        If Not (ActInfo.DataIn Is Nothing) Then
            DeSerializeObject(ActInfo.DataIn, action)
            For Each sKey In action.Keys
                Select Case True
                    Case InStr(sKey, "Dest_") > 0 AndAlso action(sKey) <> ""
                        itemsConfigured += 1
                    Case InStr(sKey, "text_") > 0 AndAlso action(sKey) <> ""
                        itemsConfigured += 1

                End Select
            Next
            If itemsConfigured = itemsToConfigure Then Configured = True
        End If
        Return Configured
    End Function

    Public Function ActionReferencesDevice(ByVal ActInfo As IPlugInAPI.strTrigActInfo, ByVal dvRef As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.ActionReferencesDevice
        Return False
    End Function

    Public Function ActionFormatUI(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.ActionFormatUI
        Dim stb As New StringBuilder
        Dim Dest As String = ""
        Dim text As String = ""
        If Not (ActInfo.DataIn Is Nothing) Then
            DeSerializeObject(ActInfo.DataIn, action)
        End If

        For Each sKey In action.Keys
            Select Case True
                Case InStr(sKey, "Dest_") > 0
                    Dest = action(sKey)
                Case InStr(sKey, "text_") > 0
                    text = action(sKey)

            End Select
        Next


        stb.Append(" Envoi du texte  """ & text & """ à " & Dest & ".")
        Return stb.ToString
    End Function

    Public ReadOnly Property ActionName(ByVal ActionNumber As Integer) As String Implements HomeSeerAPI.IPlugInAPI.ActionName
        Get
            SetActions()
            If ActionNumber > 0 AndAlso ActionNumber <= actions.Count Then
                Return IFACE_NAME & ": " & actions.Keys(ActionNumber - 1)
            Else
                Return ""
            End If
            'If Not ValidAct(ActionNumber) Then Return ""
            'Select Case ActionNumber
            '    Case 1
            '        Return IFACE_NAME & ": SARAH dit ..."
            'End Select
            'Return ""
        End Get
    End Property

    Public Function ActionProcessPostUI(ByVal PostData As Collections.Specialized.NameValueCollection, ByVal ActInfoIN As IPlugInAPI.strTrigActInfo) As IPlugInAPI.strMultiReturn Implements HomeSeerAPI.IPlugInAPI.ActionProcessPostUI
        Dim Ret As New HomeSeerAPI.IPlugInAPI.strMultiReturn

        Ret.sResult = ""
        ' We cannot be passed info ByRef from HomeSeer, so turn right around and return this same value so that if we want, 
        '   we can exit here by returning 'Ret', all ready to go.  If in this procedure we need to change DataOut or TrigInfo,
        '   we can still do that.
        Ret.DataOut = ActInfoIN.DataIn
        Ret.TrigActInfo = ActInfoIN

        If PostData Is Nothing Then Return Ret
        If PostData.Count < 1 Then Return Ret


        Dim UID As String
        UID = ActInfoIN.UID.ToString

        If Not (ActInfoIN.DataIn Is Nothing) Then
            DeSerializeObject(ActInfoIN.DataIn, action)
        End If

        Dim parts As Collections.Specialized.NameValueCollection

        Dim sKey As String

        parts = PostData

        Try
            For Each sKey In parts.Keys
                If sKey Is Nothing Then Continue For
                If String.IsNullOrEmpty(sKey.Trim) Then Continue For
                Select Case True
                    Case InStr(sKey, "Dest_" & UID) > 0
                        action.Add(CObj(parts(sKey)), sKey)
                    Case InStr(sKey, "text_" & UID) > 0
                        action.Add(CObj(parts(sKey)), sKey)

                End Select
            Next
            If Not SerializeObject(action, Ret.DataOut) Then
                Ret.sResult = IFACE_NAME & " Error, Serialization failed. Signal Action not added."
                Return Ret
            End If
        Catch ex As Exception
            Ret.sResult = "ERROR, Exception in Action UI of " & IFACE_NAME & ": " & ex.Message
            Return Ret
        End Try

        ' All OK
        Ret.sResult = ""
        Return Ret

    End Function

    Public Function ActionCount() As Integer Implements HomeSeerAPI.IPlugInAPI.ActionCount
        SetActions()
        Return actions.Count
    End Function

    Public Property Condition(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.Condition
        Set(ByVal value As Boolean)

        End Set
        Get
            Return False
        End Get
    End Property

    Public Function HandleAction(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.HandleAction
        Dim Dest As String = ""
        Dim text As String = ""
        Dim IP As String = hs.GetINISetting("param", "IP", "", INIFILE)
        Dim port As String = hs.GetINISetting("param", "PORT", "", INIFILE)


        If IP = "" Or port = "" Then
            Log("L'adresse IP ou le Port ne sont pas définis !", LogLevel.Normal)
        Else
            Try
                If Not (ActInfo.DataIn Is Nothing) Then
                    DeSerializeObject(ActInfo.DataIn, action)
                Else
                    Return False
                End If

                For Each sKey In action.Keys
                    Select Case True
                        Case InStr(sKey, "Dest_") > 0
                            Dest = action(sKey)
                        Case InStr(sKey, "text_") > 0
                            text = action(sKey)
                            text = hs.ReplaceVariables(text)
                    End Select
                Next
                '   Dim toto = hs.ReplaceVariables("$$DTR:4640:")
                '******************************************************
                '   send URL &hs.GetURL("192.168.0.49","/sendsms?phone=0603164024&text=Une Poule est dans le poulailler ",TRUE,9090)
                '******************************************************
                Dim arguments As String = ""
                Dim number As String = hs.GetINISetting("TELEPHONES", Dest, "", INIFILE)
                If (number = "") Then
                    Log("Le numéro de téléphone de " & Dest & " n'est pas défini.")
                End If
                arguments = "/sendsms?phone=" & number & "&text=" & text


                Log("Arguments == " & arguments, LogLevel.Debug)


                Log(hs.GetURL(IP, arguments, False, port), LogLevel.Debug)

            Catch ex As Exception
                hs.WriteLog(IFACE_NAME, "Error executing action: " & ex.Message)
            End Try
        End If
        Return True
    End Function

    Public ReadOnly Property HasConditions(ByVal TriggerNumber As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.HasConditions
        Get
            Return False
        End Get
    End Property

    Public Function TriggerTrue(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerTrue
        Return False
    End Function

    Public ReadOnly Property HasTriggers() As Boolean Implements HomeSeerAPI.IPlugInAPI.HasTriggers
        Get
            Return False
        End Get
    End Property

    Public ReadOnly Property SubTriggerCount(ByVal TriggerNumber As Integer) As Integer Implements HomeSeerAPI.IPlugInAPI.SubTriggerCount
        Get
            Return 0
        End Get
    End Property

    Public ReadOnly Property SubTriggerName(ByVal TriggerNumber As Integer, ByVal SubTriggerNumber As Integer) As String Implements HomeSeerAPI.IPlugInAPI.SubTriggerName
        Get
            Return ""
        End Get
    End Property

    Public Function TriggerBuildUI(ByVal sUnique As String, ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.TriggerBuildUI
        Return ""
    End Function

    Public ReadOnly Property TriggerConfigured(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerConfigured
        Get
            Return False
        End Get
    End Property

    Public Function TriggerReferencesDevice(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo, ByVal dvRef As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerReferencesDevice
        Return False
    End Function

    Public Function TriggerFormatUI(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.TriggerFormatUI
        Return ""
    End Function

    Public ReadOnly Property TriggerName(ByVal TriggerNumber As Integer) As String Implements HomeSeerAPI.IPlugInAPI.TriggerName
        Get
            Return ""
        End Get
    End Property

    Public Function TriggerProcessPostUI(ByVal PostData As System.Collections.Specialized.NameValueCollection, _
                                         ByVal TrigInfoIn As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As HomeSeerAPI.IPlugInAPI.strMultiReturn Implements HomeSeerAPI.IPlugInAPI.TriggerProcessPostUI

    End Function

    Public ReadOnly Property TriggerCount As Integer Implements HomeSeerAPI.IPlugInAPI.TriggerCount
        Get
            Return 0
        End Get
    End Property

    Public Function SupportsConfigDevice() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsConfigDevice
        Return True
    End Function

    Public Function SupportsConfigDeviceAll() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsConfigDeviceAll
        Return True
    End Function

    Public Function SupportsAddDevice() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsAddDevice
        Return True
    End Function

    Function ConfigDevicePost(ByVal ref As Integer, ByVal data As String, ByVal user As String, ByVal userRights As Integer) As Enums.ConfigDevicePostReturn Implements IPlugInAPI.ConfigDevicePost

    End Function

    Function ConfigDevice(ByVal ref As Integer, ByVal user As String, ByVal userRights As Integer, newDevice As Boolean) As String Implements IPlugInAPI.ConfigDevice
        'TODO: Coder ici la config d'un module
    End Function

    Public Function Search(SearchString As String, RegEx As Boolean) As HomeSeerAPI.SearchReturn() Implements HomeSeerAPI.IPlugInAPI.Search

    End Function


    Public Sub SpeakIn(device As Integer, txt As String, w As Boolean, host As String) Implements HomeSeerAPI.IPlugInAPI.SpeakIn

    End Sub

#If PlugDLL Then
    ' These 2 functions for internal use only
    Public Property HSObj As HomeSeerAPI.IHSApplication Implements HomeSeerAPI.IPlugInAPI.HSObj
        Get
            Return hs
        End Get
        Set(value As HomeSeerAPI.IHSApplication)
            hs = value
        End Set
    End Property

    Public Property CallBackObj As HomeSeerAPI.IAppCallbackAPI Implements HomeSeerAPI.IPlugInAPI.CallBackObj
        Get
            Return callback
        End Get
        Set(value As HomeSeerAPI.IAppCallbackAPI)
            callback = value
        End Set
    End Property
#End If

    Sub SetActions()
        Dim o As Object = Nothing
        If actions.Count = 0 Then
            actions.Add(o, "Envoi SMS")
        End If
    End Sub


    Function replacesmileys(s As String)
        Dim smils As New Smileys()
        '  smils = New Smileys()
        For Each smil As Smiley In smils
            s = s.Replace(smil.codeASCII, smil.CodeSMS)
        Next
        Return s

    End Function

    Function isUTF8(ByVal ptstr As String)
        Dim tUTFencoded As String
        Dim tUTFencodedaux
        Dim tUTFencodedASCII As String
        Dim ptstrASCII As String
        Dim iaux, iaux2 As Integer
        Dim ffound As Boolean

        ffound = False
        ptstrASCII = ""

        For iaux = 1 To Len(ptstr)
            ptstrASCII = ptstrASCII & Asc(Mid(ptstr, iaux, 1)) & "|"
        Next

        tUTFencoded = "Ã„|Ã…|Ã‡|Ã‰|Ã'|Ã–|ÃŒ|Ã¡|Ã|Ã¢|Ã¤|Ã£|Ã¥|Ã§|Ã©|Ã¨|Ãª|Ã«|Ã­|Ã¬|Ã®|Ã¯|Ã±|Ã³|Ã²|Ã´|Ã¶|Ãµ|Ãº|Ã¹|Ã»|Ã¼|â€|Â°|Â¢|Â£|Â§|â€¢|Â¶|ÃŸ|Â®|Â©|â„¢|Â´|Â¨|â‰|Ã†|Ã˜|âˆž|Â±|â‰¤|â‰¥|Â¥|Âµ|âˆ‚|âˆ‘|âˆ|Ï€|âˆ«|Âª|Âº|Î©|Ã¦|Ã¸|Â¿|Â¡|Â¬|âˆš|Æ’|â‰ˆ|âˆ†|Â«|Â»|â€¦|Â|Ã€|Ãƒ|Ã•|Å’|â€œ|â€|â€˜|â€™|Ã·|â—Š|Ã¿|Å¸|â„|â‚¬|â€¹|â€º|ï¬|ï¬‚|â€¡|Â·|â€š|â€ž|â€°|Ã‚|Ãš|Ã|Ã‹|Ãˆ|Ã|ÃŽ|Ã|ÃŒ|ï£¿|Ã'|Ãš|Ã›|Ã™|Ä±|Ë†|Ëœ|Â¯|Ë˜|Ë™|Ëš|Â¸|Ë|Ë›|Ë‡" &
                "Å|Å¡|Â¦|Â²|Â³|Â¹|Â¼|Â½|Â¾|Ã|Ã—|Ã|Ãž|Ã°|Ã½|Ã¾" &
                "â‰|âˆž|â‰¤|â‰¥|âˆ‚|âˆ‘|âˆ|Ï€|âˆ«|Î©|âˆš|â‰ˆ|âˆ†|â—Š|â„|ï¬|ï¬‚|ï£¿|Ä±|Ë˜|Ë™|Ëš|Ë|Ë›|Ë‡"

        tUTFencodedaux = Split(tUTFencoded, "|")
        If UBound(tUTFencodedaux) > 0 Then
            iaux = 0
            Do While Not ffound And Not iaux > UBound(tUTFencodedaux)
                If InStr(1, ptstr, tUTFencodedaux(iaux), vbTextCompare) > 0 Then
                    ffound = True
                End If

                If Not ffound Then
                    'ASCII numeric search
                    tUTFencodedASCII = ""
                    For iaux2 = 1 To Len(tUTFencodedaux(iaux))
                        'gets ASCII numeric sequence
                        tUTFencodedASCII = tUTFencodedASCII & Asc(Mid(tUTFencodedaux(iaux), iaux2, 1)) & "|"
                    Next
                    'tUTFencodedASCII = Left(tUTFencodedASCII, Len(tUTFencodedASCII) - 1)

                    'compares numeric sequences
                    If InStr(1, ptstrASCII, tUTFencodedASCII) > 0 Then
                        ffound = True
                    End If
                End If

                iaux = iaux + 1
            Loop
        End If

        isUTF8 = ffound
    End Function

    Function DecodeUTF8(s)
        Dim i
        Dim c
        Dim n

        s = s & " "

        i = 1
        Do While i <= Len(s)
            c = Asc(Mid(s, i, 1))
            If c And &H80 Then
                n = 1
                Do While i + n < Len(s)
                    If (Asc(Mid(s, i + n, 1)) And &HC0) <> &H80 Then
                        Exit Do
                    End If
                    n = n + 1
                Loop
                If n = 2 And ((c And &HE0) = &HC0) Then
                    c = Asc(Mid(s, i + 1, 1)) + &H40 * (c And &H1)
                Else
                    c = 191
                End If
                s = Left(s, i - 1) + Chr(c) + Mid(s, i + n)
            End If
            i = i + 1
        Loop
        DecodeUTF8 = s
    End Function


End Class

