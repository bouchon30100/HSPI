﻿Imports System
Imports Scheduler
Imports HomeSeerAPI
Imports HSCF.Communication.Scs.Communication.EndPoints.Tcp
Imports HSCF.Communication.ScsServices.Client
Imports HSCF.Communication.ScsServices.Service
Imports System.Reflection
Imports System.Text

Public Class HSPI
    Implements IPlugInAPI    ' this API is required for ALL plugins

    Dim sConfigPage As String = "Config"
    Dim sStatusPage As String = "Sample_Status"
    Dim ConfigPage As New web_config(sConfigPage)
    Dim StatusPage As New web_status(sStatusPage)
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
            Return Nothing
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
        Return ""
    End Function

    Public Function InitIO(ByVal port As String) As String Implements HomeSeerAPI.IPlugInAPI.InitIO
        Try

            If Instance = "" Then
                ConfigPage = New web_config(IFACE_NAME) ' & "_Config")
            Else
                ConfigPage = New web_config(IFACE_NAME & "_" & Instance) ' & "_Config")
            End If

            RegisterWebPage(ConfigPage.PageName) ', ConfigPage.PageName, ConfigPage.PageName)
            RegisterConfigWebPage(ConfigPage.PageName) ', ConfigPage.PageName, ConfigPage.PageName)

            'ceci est pour utiliser le getPluginPage au lieu de GenPage(déprécié)
            'If Instance = "" Then
            '    hs.RegisterPage(IFACE_NAME, IFACE_NAME, Instance)
            'Else
            '    hs.RegisterPage(IFACE_NAME & Instance, IFACE_NAME, Instance)
            'End If

            '' register a configuration link that will appear on the interfaces page
            'Dim wpd As New WebPageDesc
            'wpd.link = IFACE_NAME & Instance                ' we add the instance so it goes to the proper plugin instance when selected
            'If Instance <> "" Then
            '    wpd.linktext = sConfigPage & " - " & Instance
            'Else
            '    wpd.linktext = sConfigPage
            'End If

            'wpd.page_title = IFACE_NAME & " Config"
            'wpd.plugInName = IFACE_NAME
            'wpd.plugInInstance = Instance
            'callback.RegisterConfigLink(wpd)
            'callback.RegisterLink(wpd)

        Catch ex As Exception
            Return "Error on InitIO: " & ex.Message
        End Try
        Return ""
    End Function

    Public Function RaisesGenericCallbacks() As Boolean Implements HomeSeerAPI.IPlugInAPI.RaisesGenericCallbacks

    End Function

    Public Sub SetIOMulti(colSend As System.Collections.Generic.List(Of HomeSeerAPI.CAPI.CAPIControl)) Implements HomeSeerAPI.IPlugInAPI.SetIOMulti

    End Sub

    Public Sub ShutdownIO() Implements HomeSeerAPI.IPlugInAPI.ShutdownIO
        Try
            Try
                DeleteDevices()
            Catch ex As Exception
                Log("could not delete devices")
            End Try
            bShutDown = True
        Catch ex As Exception
            Log("Error ending " & IFACE_NAME & " Plug-In")
        End Try
    End Sub

    Public Sub HSEvent(ByVal EventType As Enums.HSEvent, ByVal parms() As Object) Implements HomeSeerAPI.IPlugInAPI.HSEvent
        Console.WriteLine("HSEvent: " & EventType.ToString)
        Select Case EventType
            Case Enums.HSEvent.VALUE_CHANGE
        End Select
    End Sub

    Public Function PollDevice(ByVal dvref As Integer) As IPlugInAPI.PollResultInfo Implements HomeSeerAPI.IPlugInAPI.PollDevice

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
            Case StatusPage.PageName
                SelectPage = StatusPage
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
        Dim idClient As String = ""
        Dim text As String = ""
        Dim COMMANDE As String = ""

        If Not (ActInfo.DataIn Is Nothing) Then
            DeSerializeObject(ActInfo.DataIn, action)
        Else 'new event, so clean out the action object
            action = New action
        End If

        For Each sKey In action.Keys
            Select Case True
                Case InStr(sKey, "client") > 0
                    idClient = action(sKey)
                Case InStr(sKey, "TBtext_") > 0
                    text = action(sKey)
                Case InStr(sKey, "COMMANDE_") > 0
                    COMMANDE = action(sKey)
            End Select
        Next

        'ajout du client
        Dim stb As New StringBuilder
        Dim dd As New clsJQuery.jqDropList("client_" & UID & sUnique, "Events", True)
        dd.autoPostBack = True
        dd.AddItem("--Please Select--", "", True)
        Dim i = 0
        For Each C In hs.GetINISetting("CONFIG", "LISTE_CLIENTS", "", INIFILE).Split("|")
            If (C <> "") Then
                dd.AddItem(C, C, (C = idClient))
                i += 1
            End If
        Next
        stb.Append("Client : ")
        stb.Append(dd.Build)

        'ajout de la commande

        dd = New clsJQuery.jqDropList("COMMANDE_" & UID & sUnique, "Events", True)
        dd.autoPostBack = True
        dd.AddItem("--Please Select--", "", True)
        dd.AddItem("dit", "tts", (COMMANDE = "tts"))
        dd.AddItem("fait", "emulate", (COMMANDE = "emulate"))

        stb.Append("Commande : ")
        stb.Append(dd.Build)

        'ajout arguement (tts ou emulate?...)
        stb.Append(" Texte : ")
        Dim tb As New clsJQuery.jqTextBox("TBtext_" & UID & sUnique, "text", text, "Events", 10, True)
        tb.id = "TBtext_" & UID & sUnique
        stb.Append(tb.Build)

        Return stb.ToString()
    End Function

    Public Function ActionConfigured(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.ActionConfigured
        Dim Configured As Boolean = False

        Dim UID As String
        UID = ActInfo.UID.ToString

        Dim sKey As String
        Dim itemsConfigured As Integer = 0
        Dim itemsToConfigure As Integer = 3

        If Not (ActInfo.DataIn Is Nothing) Then
            DeSerializeObject(ActInfo.DataIn, action)
            For Each sKey In action.Keys
                Select Case True
                    Case InStr(sKey, "client_") > 0 AndAlso action(sKey) <> ""
                        itemsConfigured += 1
                    Case InStr(sKey, "TBtext_") > 0 AndAlso action(sKey) <> ""
                        itemsConfigured += 1
                    Case InStr(sKey, "COMMANDE_") > 0 AndAlso action(sKey) <> ""
                        itemsConfigured += 1
                End Select
            Next
            If itemsConfigured = itemsToConfigure Then Configured = True
        End If
        Return Configured
    End Function

    Public Function ActionReferencesDevice(ByVal ActInfo As IPlugInAPI.strTrigActInfo, ByVal dvRef As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.ActionReferencesDevice

    End Function

    Public Function ActionFormatUI(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.ActionFormatUI
        Dim stb As New StringBuilder
       Dim idClient As String = ""
        Dim text As String = ""
        Dim COMMANDE As String = ""
        If Not (ActInfo.DataIn Is Nothing) Then
            DeSerializeObject(ActInfo.DataIn, action)
        End If

          For Each sKey In action.Keys
            Select Case True
                Case InStr(sKey, "client") > 0
                    idClient = action(sKey)
                Case InStr(sKey, "TBtext_") > 0
                    text = action(sKey)
                Case InStr(sKey, "COMMANDE_") > 0
                    COMMANDE = action(sKey)
            End Select
        Next


        stb.Append(" SARAH " & COMMANDE & " """ & text & """ via le client : " & idClient & ".")
        Return stb.ToString
    End Function

    Friend Function ValidAct(ByVal ActIn As Integer) As Boolean
        If ActIn > 0 AndAlso ActIn < 3 Then Return True
        Return False
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
                    Case InStr(sKey, "client_" & UID) > 0
                        action.Add(CObj(parts(sKey)), sKey)
                    Case InStr(sKey, "TBtext_" & UID) > 0
                        action.Add(CObj(parts(sKey)), sKey)
                    Case InStr(sKey, "COMMANDE_" & UID) > 0
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

        End Get
    End Property

    Public Function HandleAction(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.HandleAction
        Dim client As String = ""
        Dim text As String = ""
        Dim commande As String = ""

        Try
            If Not (ActInfo.DataIn Is Nothing) Then
                DeSerializeObject(ActInfo.DataIn, action)
            Else
                Return False
            End If

            For Each sKey In action.Keys
                Select Case True
                    Case InStr(sKey, "client_") > 0
                        client = action(sKey)
                    Case InStr(sKey, "TBtext_") > 0
                        text = action(sKey)
                        text = hs.ReplaceVariables(text)

                    Case InStr(sKey, "COMMANDE_") > 0
                        commande = action(sKey)
                End Select
            Next
            Dim toto = hs.ReplaceVariables("$$DTR:4640:")
            '******************************************************
            '   send URL http://IP:PORT/?tts=text
            '******************************************************
            Dim cl As SarahClient = New SarahClient(client)
            Dim arguments As String = ""
            Select Case commande
                Case "tts"
                    arguments = "/?" & commande & "=" & text & "."
                Case "emulate"
                    Dim sender As String = hs.GetINISetting("param", "moduleSender", "unknown", INIFILE)
                    arguments = "/?" & commande & "=" & cl.Name & ", " & text & ".& profile=" & sender & "& idClient=JASPER"
            End Select


            Log(hs.GetURL(cl.IP, arguments, False, cl.PORT), LogLevel.Normal)

        Catch ex As Exception
            hs.WriteLog(IFACE_NAME, "Error executing action: " & ex.Message)
        End Try
        Return True
    End Function

    Public ReadOnly Property HasConditions(ByVal TriggerNumber As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.HasConditions
        Get

        End Get
    End Property

    Public Function TriggerTrue(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerTrue

    End Function

    Public ReadOnly Property HasTriggers() As Boolean Implements HomeSeerAPI.IPlugInAPI.HasTriggers
        Get

        End Get
    End Property

    Public ReadOnly Property SubTriggerCount(ByVal TriggerNumber As Integer) As Integer Implements HomeSeerAPI.IPlugInAPI.SubTriggerCount
        Get

        End Get
    End Property

    Public ReadOnly Property SubTriggerName(ByVal TriggerNumber As Integer, ByVal SubTriggerNumber As Integer) As String Implements HomeSeerAPI.IPlugInAPI.SubTriggerName
        Get

        End Get
    End Property

    Public Function TriggerBuildUI(ByVal sUnique As String, ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.TriggerBuildUI

    End Function

    Public ReadOnly Property TriggerConfigured(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerConfigured
        Get

        End Get
    End Property

    Public Function TriggerReferencesDevice(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo, ByVal dvRef As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerReferencesDevice

    End Function

    Public Function TriggerFormatUI(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.TriggerFormatUI

    End Function

    Public ReadOnly Property TriggerName(ByVal TriggerNumber As Integer) As String Implements HomeSeerAPI.IPlugInAPI.TriggerName
        Get

        End Get
    End Property

    Public Function TriggerProcessPostUI(ByVal PostData As System.Collections.Specialized.NameValueCollection, _
                                         ByVal TrigInfoIn As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As HomeSeerAPI.IPlugInAPI.strMultiReturn Implements HomeSeerAPI.IPlugInAPI.TriggerProcessPostUI

    End Function

    Public ReadOnly Property TriggerCount As Integer Implements HomeSeerAPI.IPlugInAPI.TriggerCount
        Get

        End Get
    End Property

    Public Function SupportsConfigDevice() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsConfigDevice

    End Function

    Public Function SupportsConfigDeviceAll() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsConfigDeviceAll

    End Function

    Public Function SupportsAddDevice() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsAddDevice

    End Function

    Function ConfigDevicePost(ByVal ref As Integer, ByVal data As String, ByVal user As String, ByVal userRights As Integer) As Enums.ConfigDevicePostReturn Implements IPlugInAPI.ConfigDevicePost

    End Function

    Function ConfigDevice(ByVal ref As Integer, ByVal user As String, ByVal userRights As Integer, newDevice As Boolean) As String Implements IPlugInAPI.ConfigDevice

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
            actions.Add(o, "dit/fait...")
        End If
    End Sub

    Function AskSARAH(texte As String, sender As String, Optional ClientName As String = "Pepito") As String

        Dim cl As SarahClient = New SarahClient(ClientName)
        Dim arguments = "/?emulate=" & cl.Name & " " & texte & ".& profile=" & sender & "&notts=true"
        Dim result As String = hs.GetURL(cl.IP, arguments, False, cl.PORT)
        result = result.Replace(vbCrLf, "").Replace("  ", " ").TrimEnd(" ")


        If result <> "" Then


            '   Do While Char.IsLetter(result(0)) = False
            '  result = result.TrimStart(" ")
            ' result = result.TrimStart(",")
            ' Loop
            result = result.Replace("  ", " ")
            ' result = UCase(Left(result, 1)) & Mid(result, 2)
        Else
            result = ("Je n'ai pas compris la question ")
        End If

        Return result
    End Function


End Class

