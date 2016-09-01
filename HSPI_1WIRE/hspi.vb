Imports System
Imports Scheduler
Imports HomeSeerAPI
Imports HSCF.Communication.Scs.Communication.EndPoints.Tcp
Imports HSCF.Communication.ScsServices.Client
Imports HSCF.Communication.ScsServices.Service
Imports System.Reflection
Imports System.Text
Imports Scheduler.Classes
Imports System.Web

Public Class HSPI
    Implements IPlugInAPI    ' this API is required for ALL plugins


    Dim sStatusPage As String = "Sample_Status"
    Dim ConfigPage 'As New web_config(sConfigPage)
    Dim StatusPage 'As New web_status(sStatusPage)
    Dim WebPage As Object
    Dim actions As New hsCollection
    Dim action As New action
    Dim es As New IPlugInAPI.strInterfaceStatus
    Dim liste1Wire As List(Of Module1Wire)

    Dim distant As Boolean = False
    Dim myproc As New Process

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

            es.intStatus = IPlugInAPI.enumInterfaceStatus.INFO
            es.sStatus = "En cours de Chargement..."
            distant = hs.GetINISetting("param", "COMDISTANT", False, INIFILE)
            If distant Then

                Dim start As New ProcessStartInfo
                start.FileName = "C:\Program Files (x86)\com0com\hub4com\com2tcp.bat"
                start.Arguments = "--baud 9600 \\.\CNCB0 " & hs.GetINISetting("param", "COMDISTANT_IP", "", INIFILE) & " " & hs.GetINISetting("param", "COMDISTANT_PORT", "", INIFILE)
                start.UseShellExecute = True
                start.RedirectStandardOutput = False
                start.RedirectStandardError = False
                start.WindowStyle = ProcessWindowStyle.Hidden
                myproc.StartInfo = start
                myproc.Start()
            End If

            getAdapter()
            liste1Wire = getModules()

            For Each m As Module1Wire In liste1Wire
                m.SearchModuleHomeseer()
                SaveCreateModule(m)
            Next

            '  liste1Wire = SearchModulesHomeseer()

            es.intStatus = IPlugInAPI.enumInterfaceStatus.OK
            es.sStatus = ""

            If Instance = "" Then
                ConfigPage = New web_config(IFACE_NAME, liste1Wire) ' & "_Config")
            Else
                ConfigPage = New web_config(IFACE_NAME & "_" & Instance, liste1Wire) ' & "_Config")
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
            es.intStatus = IPlugInAPI.enumInterfaceStatus.CRITICAL
            es.sStatus = "Error on InitIO: " & ex.Message
            Return "Error on InitIO: " & ex.Message
        End Try
        Return ""
    End Function

    Public Function RaisesGenericCallbacks() As Boolean Implements HomeSeerAPI.IPlugInAPI.RaisesGenericCallbacks
        Return False
    End Function

    Public Sub SetIOMulti(colSend As System.Collections.Generic.List(Of HomeSeerAPI.CAPI.CAPIControl)) Implements HomeSeerAPI.IPlugInAPI.SetIOMulti

    End Sub

    Public Sub ShutdownIO() Implements HomeSeerAPI.IPlugInAPI.ShutdownIO
        Try
            Try
                Log("Arrêt du plugin 1-Wire")
                DisposeAdapter()
                If distant Then
                    myproc.Kill()
                    myproc.WaitForExit(1)
                    myproc.Close()
                    myproc.Dispose()
                End If
            Catch ex As Exception
                Log("Erreur lors de la fermeture du bus : " + ex.Message & vbCrLf, MessageType.Error_)

            End Try
            bShutDown = True
        Catch ex As Exception
            Log("Error ending " & IFACE_NAME & " Plug-In")
        End Try
    End Sub

    Public Sub HSEvent(ByVal EventType As Enums.HSEvent, ByVal parms() As Object) Implements HomeSeerAPI.IPlugInAPI.HSEvent

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

        Return ""
    End Function

    Public Function ActionConfigured(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.ActionConfigured
        Return True
    End Function

    Public Function ActionReferencesDevice(ByVal ActInfo As IPlugInAPI.strTrigActInfo, ByVal dvRef As Integer) As Boolean Implements IPlugInAPI.ActionReferencesDevice

    End Function

    Public Function ActionFormatUI(ByVal ActInfo As IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.ActionFormatUI
        Return "Relève toutes les mesures sur le réseau 1Wire"
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
        Try
            getDatas()

        Catch ex As Exception
            Log("Error executing action: " & ex.Message, LogLevel.Debug)
        End Try
        Return True
    End Function

    Public ReadOnly Property HasConditions(ByVal TriggerNumber As Integer) As Boolean Implements HomeSeerAPI.IPlugInAPI.HasConditions
        Get
            Return False
        End Get
    End Property

    Public Function TriggerTrue(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerTrue
        Return True
    End Function

    Public ReadOnly Property HasTriggers() As Boolean Implements HomeSeerAPI.IPlugInAPI.HasTriggers
        Get

        End Get
    End Property

    Public ReadOnly Property SubTriggerCount(ByVal TriggerNumber As Integer) As Integer Implements HomeSeerAPI.IPlugInAPI.SubTriggerCount
        Get
            Return -1
        End Get
    End Property

    Public ReadOnly Property SubTriggerName(ByVal TriggerNumber As Integer, ByVal SubTriggerNumber As Integer) As String Implements HomeSeerAPI.IPlugInAPI.SubTriggerName
        Get
            Return -1
        End Get
    End Property

    Public Function TriggerBuildUI(ByVal sUnique As String, ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As String Implements HomeSeerAPI.IPlugInAPI.TriggerBuildUI
        Return ""
    End Function

    Public ReadOnly Property TriggerConfigured(ByVal TrigInfo As HomeSeerAPI.IPlugInAPI.strTrigActInfo) As Boolean Implements HomeSeerAPI.IPlugInAPI.TriggerConfigured
        Get
            Return True
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
        Return Nothing
    End Function

    Public ReadOnly Property TriggerCount As Integer Implements HomeSeerAPI.IPlugInAPI.TriggerCount
        Get

        End Get
    End Property

    Public Function SupportsConfigDevice() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsConfigDevice
        Return True
    End Function

    Public Function SupportsConfigDeviceAll() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsConfigDeviceAll
        Return False
    End Function

    Public Function SupportsAddDevice() As Boolean Implements HomeSeerAPI.IPlugInAPI.SupportsAddDevice
        Return True
    End Function

    Function ConfigDevicePost(ByVal ref As Integer, ByVal data As String, ByVal user As String, ByVal userRights As Integer) As Enums.ConfigDevicePostReturn Implements IPlugInAPI.ConfigDevicePost
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)

        '"ref=4292&edit=1&ref=4292&plugin=1WIRE&instance=&action=pluginpost&dblCoef_9D000003BA9D2028_NA=5&dblOffset_9D000003BA9D2028_NA=2&sFormat_9D000003BA9D2028_NA=%23%23.%23%23&BTNSAVE_4292="


        If (parts("id").Contains("BTNCANCEL")) Then

            Return Enums.ConfigDevicePostReturn.DoneAndCancel

        End If

        Dim m As Module1Wire = New Module1Wire()
        For Each modul In liste1Wire
            If (modul.REF = parts("ref")) Then
                m = modul
            End If
        Next

        Dim idModule As String = m.OneWireAdress
        Dim Canal As String = m.Channel


        m.dblCoef = parts("dblCoef_" & idModule & "_" & Canal)
        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "COEFFICIENT", m.dblCoef, INIFILE)


        m.dblOffset = parts("dblOffset_" & idModule & "_" & Canal)
        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "OFFSET", m.dblOffset, INIFILE)

        m.sFormat = parts("sFormat_" & idModule & "_" & Canal)
        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "FORMAT", m.sFormat, INIFILE)

        m.coeffArrondi = parts("ARRONDI_" & idModule & "_" & Canal).Replace(".", ",")
        hs.SaveINISetting(m.OneWireAdress & "/" & m.Channel, "COEFFICIENT_ARRONDI", m.coeffArrondi, INIFILE)
        Return Enums.ConfigDevicePostReturn.DoneAndSave






    End Function

    Function ConfigDevice(ByVal ref As Integer, ByVal user As String, ByVal userRights As Integer, newDevice As Boolean) As String Implements IPlugInAPI.ConfigDevice
        Dim stb As New StringBuilder

        stb.Append(HTML_StartTable(2, -1, 100))

        'row IP Client SARAH
        '  Dim IP1 = hs.GetINISetting("CONFIG" & Instance, "IP", "127.0.0.1", INIFILE)
        stb.Append(HTML_StartRow("", "", HTML_Align.LEFT, "", HTML_VertAlign.MIDDLE))
        Dim m As New Module1Wire
        m.SearchModuleHomeseer(ref)

        stb.Append(HTML_StartCell("", 1))
        Dim dblCoef As New clsJQuery.jqTextBox("dblCoef_" & m.OneWireAdress & "_" & m.Channel, "text", m.dblCoef, "deviceutiliy", 5, False)
        dblCoef.id = "dblCoef_" & m.OneWireAdress & "_" & m.Channel
        dblCoef.promptText = "Coefficient multiplicateur : "
        ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
        stb.Append("Coefficient : <br>")
        stb.Append(dblCoef.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim dblOffset As New clsJQuery.jqTextBox("dblOffset_" & m.OneWireAdress & "_" & m.Channel, "text", m.dblOffset, "deviceutiliy", 5, False)
        dblOffset.id = "dblOffset_" & m.OneWireAdress & "_" & m.Channel
        dblOffset.promptText = "Offset : "
        ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
        stb.Append("offset : <br>")
        stb.Append(dblOffset.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim sFormat As New clsJQuery.jqTextBox("sFormat_" & m.OneWireAdress & "_" & m.Channel, "text", m.sFormat, "deviceutiliy", 5, False)
        sFormat.id = "sFormat_" & m.OneWireAdress & "_" & m.Channel
        sFormat.promptText = "Format : "
        ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
        stb.Append("Format : <br>")
        stb.Append(sFormat.Build)
        stb.Append(HTML_EndCell)

        stb.Append(HTML_StartCell("", 1))
        Dim coefArrondi As New clsJQuery.jqTextBox("ARRONDI_" & m.OneWireAdress & "_" & m.Channel, "text", m.coeffArrondi, "deviceutiliy", 5, False)
        coefArrondi.id = "ARRONDI_" & m.OneWireAdress & "_" & m.Channel
        coefArrondi.promptText = "Arrondi : "
        ' tb.toolTip = "Adresse IP du poste sur lequel SARAH doit s'exprimer"
        stb.Append("Arrondi : <br>" & coefArrondi.Build)

        stb.Append(HTML_EndCell)

        stb.Append(HTML_EndRow)



        ' stb.Append(getSTModule1Wire(m))

        '  stb.Append(m & "<br><br>")

        stb.Append(HTML_EndTable)

        Dim BTNSAVE As clsJQuery.jqButton = New clsJQuery.jqButton("BTNSAVE_" & m.REF, "Enregistrer", "deviceutility", True)
        BTNSAVE.id = "BTNSAVE_" & m.REF

        stb.Append(BTNSAVE.Build)

        Dim BTNCANCEL As clsJQuery.jqButton = New clsJQuery.jqButton("BTNCANCEL_" & m.REF, "Annuler", "deviceutility", True)
        BTNCANCEL.id = "BTNCANCEL_" & m.REF

        stb.Append(BTNCANCEL.Build)

        Return stb.ToString()
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
            actions.Add(o, "relèver les mesures")
        End If
    End Sub

    Private Function SearchModulesHomeseer() As List(Of Module1Wire)

        Dim lm As New List(Of Module1Wire)


        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            Do While Not en.Finished
                dv = en.GetNext
                If dv IsNot Nothing Then
                    If (dv.Interface(Nothing) = IFACE_NAME) Then
                        Dim m As New Module1Wire()
                        m.ETAGE = dv.Location2(Nothing)
                        m.PIECE = dv.Location(Nothing)
                        m.REF = dv.Ref(Nothing)
                        m.hsType = dv.Device_Type_String(Nothing)
                        If (dv.Code(Nothing) <> "") Then
                            m.HC = dv.Code(Nothing).Substring(0, 1)
                            m.MC = dv.Code(Nothing).Remove(0, 1)
                        End If
                        m.NAME = dv.Name(Nothing)
                        m.dblCoef = CDbl(hs.GetINISetting(m.OneWireAdress, "COEFFICIENT", "1", INIFILE))
                        m.dblOffset = CDbl(hs.GetINISetting(m.OneWireAdress, "OFFSET", "0", INIFILE))
                        m.sFormat = hs.GetINISetting(m.OneWireAdress, "FORMAT", "##.##", INIFILE)
                        m.ValeurSeuil = CDbl((hs.GetINISetting(m.OneWireAdress, "SEUIL", "0", INIFILE)))
                        lm.Add(m)
                    End If
                End If
            Loop
        Catch ex As Exception
            hs.WriteLogEx("1WIRE", "HSPI --> " & ex.Message, COLOR_RED)
        End Try
        Return lm

    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

