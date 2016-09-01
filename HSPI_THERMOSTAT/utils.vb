Imports System.IO
Imports System.Runtime.Serialization.Formatters
Imports HomeSeerAPI
Imports Scheduler.Classes

Module utils
    Public IFACE_NAME As String = "THERMOSTAT"
    Public callback As HomeSeerAPI.IAppCallbackAPI
    Public hs As HomeSeerAPI.IHSApplication
    Public Instance As String = ""
    Public InterfaceVersion As Integer
    Public bShutDown As Boolean = False
    Public gEXEPath As String = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)
    Public Const INIFILE As String = "HSPI_THERMOSTAT/HSPI_THERMOSTAT.ini"
    Public Const INIFILE_DEVICE As String = "HSPI_THERMOSTAT/####.ini"
    Public Const INIFILE_ETE As String = "HSPI_THERMOSTAT_ETE.ini"
    Public Const INIFILE_PRINTEMPS As String = "HSPI_THERMOSTAT_PRINTEMPS.ini"
    Public Const INIFILE_HIVERS As String = "HSPI_THERMOSTAT_HIVERS.ini"
    Public CurrentPage As Object

    Public Function StringIsNullOrEmpty(ByRef s As String) As Boolean
        If String.IsNullOrEmpty(s) Then Return True
        Return String.IsNullOrEmpty(s.Trim)
    End Function

    Public Structure pair
        Dim name As String
        Dim value As String
    End Structure

    Public Function GetListSTATUS() As String()

        Dim stat = hs.GetINISetting("PARAMS", "STATUS", "", INIFILE).Split(",")
        Return stat

    End Function

    Public Function GetListDEVICES() As List(Of String)
        Dim stat As New List(Of String)
        Dim s = hs.GetINISectionEx("ASSOCIATIONS", INIFILE)

        For Each dev In s
            stat.Add(dev.split("=")(0))
        Next
        Return stat

    End Function



    Public Function GetOrCreateChildModule(m As DeviceClass, type As String, ini As String, name As String) As DeviceClass

        Dim dv As Scheduler.Classes.DeviceClass
        Dim ref = hs.GetINISetting("LINKS", type, "0", ini)
        If (ref = "0") Then
            'Dim housecode As String = m.Code(Nothing)
            Dim adress As String = name & "_" & m.Ref(Nothing)
            'Dim name As String = "RESSENTI"
            Dim location As String = m.Location(Nothing)
            Dim location2 As String = m.Location2(Nothing)
            Dim TypeString As String = name
            Dim StatusSupport As Boolean = False
            Dim CanDim As Boolean = False
            Dim Nb_Status As Integer = 3
            Dim Nb_StatusGraph As Integer = 3

            Dim STATUS_ONLY As Boolean = True
            Dim NO_LOG As Boolean = True
            Dim HIDDEN As Boolean = False
            Dim INCLUDE_POWERFAIL As Boolean = True
            Dim SHOW_VALUES As Boolean = True
            Dim AUTO_VOICE_COMMAND As Boolean = False
            Dim NO_STATUS_TRIGGER As Boolean = False
            Dim VOICE_COMMAND_CONFIRM As Boolean = False

            dv = hs.GetDeviceByRef(hs.NewDeviceRef(name))

            hs.SaveINISetting("LINKS", type, dv.Ref(Nothing), ini)

            '  dv.Code(hs) = housecode
            dv.Address(hs) = adress
            Dim DT As New DeviceTypeInfo
            DT.Device_Type = DeviceTypeInfo.eDeviceAPI.Plug_In
            dv.DeviceType_Set(hs) = DT
            dv.Interface(hs) = IFACE_NAME
            dv.Location2(hs) = location2
            dv.Location(hs) = location
            '   dv.Image(hs) = ImageDevice
            '   dv.ImageLarge(hs) = ImageDevice
            dv.Device_Type_String(hs) = TypeString
            dv.Can_Dim(hs) = CanDim
            dv.Status_Support(hs) = StatusSupport

            If (AUTO_VOICE_COMMAND) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.AUTO_VOICE_COMMAND)
            End If
            If (STATUS_ONLY) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.STATUS_ONLY)
            End If
            If (NO_LOG) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.NO_LOG)
            End If
            If (HIDDEN) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.HIDDEN)
            End If
            If (INCLUDE_POWERFAIL) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.INCLUDE_POWERFAIL)
            End If
            If (SHOW_VALUES) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.SHOW_VALUES)
            End If
            If (NO_STATUS_TRIGGER) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.NO_STATUS_TRIGGER)
            End If
            If (VOICE_COMMAND_CONFIRM) Then
                dv.MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.VOICE_COMMAND_CONFIRM)
            End If

            dv.AssociatedDevice_Add(hs, m.Ref(Nothing))
            dv.Relationship(hs) = Enums.eRelationship.Child


            m.AssociatedDevice_Add(hs, dv.Ref(Nothing))
            m.Relationship(hs) = Enums.eRelationship.Parent_Root

        Else
            dv = hs.GetDeviceByRef(ref)
            dv.Location2(hs) = m.Location2(Nothing)
            dv.Location(hs) = m.Location(Nothing)
        End If

        Return dv
    End Function



    Public Sub UpdateModuleTHERMOSTAT(refINPUT As String, NewValue As Double, oldValue As Double)

        Dim saison = GetSaison()
        Dim ini = INIFILE_DEVICE.Replace("####", refINPUT)
        Dim dvINPUT As DeviceClass = hs.GetDeviceByRef(refINPUT)

        Dim dvRESSENTI As DeviceClass = GetOrCreateChildModule(dvINPUT, "IDTHERMOSTAT", ini, "RESSENTI")
        Dim dvDELTA As DeviceClass = GetOrCreateChildModule(dvINPUT, "IDDELTA", ini, "DELTA")
        Dim dvACTION As DeviceClass = GetOrCreateChildModule(dvINPUT, "IDACTION", ini, "ACTION")

        'TEMPORAIRE A supprimer :
        'MAJ des STATUS du module INPUT (Compatibilité avec MAISON.aspx
        Dim refDev = dvINPUT.Ref(Nothing)
        hs.DeviceVGP_ClearAll(refDev, True)
        hs.DeviceVSP_ClearAll(refDev, True)
        For Each StringStatus In GetListSTATUS() ' hs.GetINISetting("PARAMS", "STATUS", "", INIFILE)

            Dim mini As String = hs.GetINISetting(saison, "min", "18", ini)
            Dim maxi As String = hs.GetINISetting(saison, "max", "22", ini)
            Dim IMGStatus = hs.GetINISetting("IMG", StringStatus, " ", ini)
            If IMGStatus = " " Then
                IMGStatus = hs.GetINISetting(StringStatus, "IMG", " ", INIFILE)
            End If
            If StringStatus = "FRAIS" Then
                CreateMultipleStatusGraphiquePAIR(hs.GetDeviceByRef(refDev), -20, mini - 0.01, IMGStatus)
                CreateMultipleStatusPAIR(hs.GetDeviceByRef(refDev), -20, mini - 0.01, StringStatus)
            End If
            If StringStatus = "BON" Then
                CreateMultipleStatusGraphiquePAIR(hs.GetDeviceByRef(refDev), mini, maxi, IMGStatus)
                CreateMultipleStatusPAIR(hs.GetDeviceByRef(refDev), mini, maxi, StringStatus)
            End If
            If StringStatus = "CHAUD" Then
                CreateMultipleStatusGraphiquePAIR(hs.GetDeviceByRef(refDev), maxi + 0.01, 50, IMGStatus)
                CreateMultipleStatusPAIR(hs.GetDeviceByRef(refDev), maxi + 0.01, 50, StringStatus)
            End If
        Next

        'MAJ des STATUS du module ressenti
        refDev = dvRESSENTI.Ref(Nothing)
        hs.DeviceVGP_ClearAll(refDev, True)
        hs.DeviceVSP_ClearAll(refDev, True)
        For Each StringStatus In GetListSTATUS() ' hs.GetINISetting("PARAMS", "STATUS", "", INIFILE)

            Dim valueStatus = hs.GetINISetting(StringStatus, "VALUE", 99, INIFILE)
            Dim IMGStatus = hs.GetINISetting("IMG", StringStatus, " ", ini)
            If IMGStatus = " " Then
                IMGStatus = hs.GetINISetting(StringStatus, "IMG", " ", INIFILE)
            End If
            CreateSingleStatusPAIR(dvRESSENTI, valueStatus, StringStatus)
            CreateSingleStatusGraphiquePAIR(dvRESSENTI, valueStatus, IMGStatus)
        Next

        'MAJ des STATUS du module ACTION
        refDev = dvACTION.Ref(Nothing)
        hs.DeviceVGP_ClearAll(refDev, True)
        hs.DeviceVSP_ClearAll(refDev, True)
        CreateSingleStatusPAIR(dvACTION, 0, "STOP")
        CreateSingleStatusPAIR(dvACTION, 1, "COOLING")
        CreateSingleStatusPAIR(dvACTION, 2, "HEATING")

        'MAJ des STATUS du module DELTA
        refDev = dvDELTA.Ref(Nothing)
        hs.DeviceVGP_ClearAll(refDev, True)
        hs.DeviceVSP_ClearAll(refDev, True)
        CreateMultipleStatusPAIR(dvDELTA, -999, -0.001, "Négatif ")
        CreateSingleStatusPAIR(dvDELTA, 0, "Stable ")
        CreateMultipleStatusPAIR(dvDELTA, 0.001, 999, "Positif +")

        CreateMultipleStatusGraphiquePAIR(dvDELTA, -999, -0.001, "images/jaspmobile/DOWN.png")
        CreateSingleStatusGraphiquePAIR(dvDELTA, 0, "images/jaspmobile/STABLE.png")
        CreateMultipleStatusGraphiquePAIR(dvDELTA, 0.001, 999, "images/jaspmobile/UP.png")

        'MAJ des values Status
        Dim min As String = hs.GetINISetting(saison, "min", "18", ini)
        Dim max As String = hs.GetINISetting(saison, "max", "22", ini)
        Dim valStatus = "99"
        If (NewValue < min) Then
            valStatus = hs.GetINISetting("FRAIS", "Value", 99, INIFILE)
        ElseIf (NewValue > max) Then
            valStatus = hs.GetINISetting("CHAUD", "Value", 99, INIFILE)
        Else
            valStatus = hs.GetINISetting("BON", "Value", 99, INIFILE)
        End If


        hs.SetDeviceValueByRef(dvRESSENTI.Ref(Nothing), valStatus, True)
        hs.SetDeviceValueByRef(dvDELTA.Ref(Nothing), NewValue - oldValue, True)
        hs.SaveEventsDevices()

    End Sub




    Private Sub DeleteStatusPAIR(device As Integer)
        hs.DeviceVSP_ClearAll(device, True)
    End Sub

    Private Sub DeleteStatusGraphiquePAIR(device As Integer)
        hs.DeviceVGP_ClearAll(device, True)
    End Sub

    Private Function Status_control_texte_to_int(str As String) As Integer
        Select Case UCase(str)
            Case "STATUS"
                Status_control_texte_to_int = HomeSeerAPI.ePairStatusControl.Status
            Case "CONTROLE"
                Status_control_texte_to_int = HomeSeerAPI.ePairStatusControl.Control
            Case Else
                Status_control_texte_to_int = HomeSeerAPI.ePairStatusControl.Both
        End Select
    End Function

    Private Function Status_control_int_to_texte(integ As Integer) As String
        Select Case integ
            Case HomeSeerAPI.ePairStatusControl.Status
                Status_control_int_to_texte = "STATUS"
            Case HomeSeerAPI.ePairStatusControl.Control
                Status_control_int_to_texte = "CONTROLE"
            Case Else
                Status_control_int_to_texte = "BOTH"
        End Select
    End Function

    Public Sub CreateSingleStatusPAIR(dv As DeviceClass, valueStatus As Double, StringStatus As String)
        Dim Pair As HomeSeerAPI.VSPair
        Pair = New HomeSeerAPI.VSPair(ePairStatusControl.Both)
        Pair.PairType = HomeSeerAPI.VSVGPairType.SingleValue
        Pair.Value = valueStatus
        Pair.Status = StringStatus
        Pair.Render = HomeSeerAPI.Enums.CAPIControlType.Button
        hs.DeviceVSP_AddPair(dv.Ref(hs), Pair)
    End Sub

    Public Sub CreateMultipleStatusPAIR(dv As DeviceClass, valueStatusMin As Double, valueStatusMax As Double, StringStatus As String)
        Dim Pair As HomeSeerAPI.VSPair
        Pair = New HomeSeerAPI.VSPair(ePairStatusControl.Both)
        Pair.PairType = HomeSeerAPI.VSVGPairType.Range
        Pair.RangeStart = valueStatusMin
        Pair.RangeEnd = valueStatusMax
        'Pair.Status = StringStatus
        Pair.RangeStatusPrefix = StringStatus
        Pair.IncludeValues = True
        Pair.RangeStatusDecimals = 3
        Pair.Render = HomeSeerAPI.Enums.CAPIControlType.Button
        hs.DeviceVSP_AddPair(dv.Ref(hs), Pair)
    End Sub

    Public Sub CreateSingleStatusGraphiquePAIR(dv As DeviceClass, valueStatus As Double, IMGStatus As String)
        Dim VgPair As HomeSeerAPI.VGPair
        VgPair = New HomeSeerAPI.VGPair()
        VgPair.PairType = HomeSeerAPI.VSVGPairType.SingleValue
        VgPair.Set_Value = valueStatus
        VgPair.Graphic = IMGStatus
        hs.DeviceVGP_AddPair(dv.Ref(hs), VgPair)
    End Sub

    Public Sub CreateMultipleStatusGraphiquePAIR(dv As DeviceClass, valueStatusMin As Double, valueStatusMax As Double, IMGStatus As String)
        Dim VgPair As HomeSeerAPI.VGPair
        VgPair = New HomeSeerAPI.VGPair()
        VgPair.PairType = HomeSeerAPI.VSVGPairType.Range
        VgPair.RangeStart = valueStatusMin
        VgPair.RangeEnd = valueStatusMax
        VgPair.Graphic = IMGStatus
        hs.DeviceVGP_AddPair(dv.Ref(hs), VgPair)
    End Sub

    Sub PEDAdd(ByRef PED As clsPlugExtraData, ByVal PEDName As String, ByVal PEDValue As Object)
        Dim ByteObject() As Byte = Nothing
        If PED Is Nothing Then PED = New clsPlugExtraData
        SerializeObject(PEDValue, ByteObject)
        If Not PED.AddNamed(PEDName, ByteObject) Then
            PED.RemoveNamed(PEDName)
            PED.AddNamed(PEDName, ByteObject)
        End If
    End Sub

    Function PEDGet(ByRef PED As clsPlugExtraData, ByVal PEDName As String) As Object
        Dim ByteObject() As Byte
        Dim ReturnValue As New Object
        ByteObject = PED.GetNamed(PEDName)
        If ByteObject Is Nothing Then Return Nothing
        DeSerializeObject(ByteObject, ReturnValue)
        Return ReturnValue
    End Function

    Public Function SerializeObject(ByRef ObjIn As Object, ByRef bteOut() As Byte) As Boolean
        If ObjIn Is Nothing Then Return False
        Dim str As New MemoryStream
        Dim sf As New Binary.BinaryFormatter

        Try
            sf.Serialize(str, ObjIn)
            ReDim bteOut(CInt(str.Length - 1))
            bteOut = str.ToArray
            Return True
        Catch ex As Exception
            Log(LogLevel.Debug, IFACE_NAME & " Error: Serializing object " & ObjIn.ToString & " :" & ex.Message)
            Return False
        End Try

    End Function

    Public Function DeSerializeObject(ByRef bteIn() As Byte, ByRef ObjOut As Object) As Boolean
        ' Almost immediately there is a test to see if ObjOut is NOTHING.  The reason for this
        '   when the ObjOut is suppose to be where the deserialized object is stored, is that 
        '   I could find no way to test to see if the deserialized object and the variable to 
        '   hold it was of the same type.  If you try to get the type of a null object, you get
        '   only a null reference exception!  If I do not test the object type beforehand and 
        '   there is a difference, then the InvalidCastException is thrown back in the CALLING
        '   procedure, not here, because the cast is made when the ByRef object is cast when this
        '   procedure returns, not earlier.  In order to prevent a cast exception in the calling
        '   procedure that may or may not be handled, I made it so that you have to at least 
        '   provide an initialized ObjOut when you call this - ObjOut is set to nothing after it 
        '   is typed.
        If bteIn Is Nothing Then Return False
        If bteIn.Length < 1 Then Return False
        If ObjOut Is Nothing Then Return False
        Dim str As MemoryStream
        Dim sf As New Binary.BinaryFormatter
        Dim ObjTest As Object
        Dim TType As System.Type
        Dim OType As System.Type
        Try
            OType = ObjOut.GetType
            ObjOut = Nothing
            str = New MemoryStream(bteIn)
            ObjTest = sf.Deserialize(str)
            If ObjTest Is Nothing Then Return False
            TType = ObjTest.GetType
            'If Not TType.Equals(OType) Then Return False
            ObjOut = ObjTest
            If ObjOut Is Nothing Then Return False
            Return True
        Catch exIC As InvalidCastException
            Return False
        Catch ex As Exception
            Log(LogLevel.Debug, IFACE_NAME & " Error: DeSerializing object: " & ex.Message)
            Return False
        End Try

    End Function

    Public Sub DeleteDevices(dvINPUT As String)

        Try
            Dim ini = INIFILE_DEVICE.Replace("####", dvINPUT)
            Dim dv As DeviceClass = hs.GetDeviceByRef(dvINPUT)
            Dim dvRESSENTI As DeviceClass = GetOrCreateChildModule(dv, "IDTHERMOSTAT", ini, "RESSENTI")
            Dim dvDELTA As DeviceClass = GetOrCreateChildModule(dv, "IDDELTA", ini, "DELTA")
            Dim dvACTION As DeviceClass = GetOrCreateChildModule(dv, "IDACTION", ini, "ACTION")

            hs.DeleteDevice(dvRESSENTI.Ref(Nothing))
            hs.DeleteDevice(dvDELTA.Ref(Nothing))
            hs.DeleteDevice(dvACTION.Ref(Nothing))
            hs.SaveEventsDevices()
        Catch ex As Exception
        End Try
    End Sub

    Sub DeleteModule(ByVal n As Integer)
        Dim i As Integer
        Log("Module to Delete is " & n)
        For i = 1 To 16
            hs.DeleteDevice(hs.GetINISetting("Module " & n, "ref-" & i.ToString, "", INIFILE))
        Next

        hs.ClearINISection("Module " & n.ToString, INIFILE)

        Log("Finished deleting module.")
    End Sub

    Function InitDevice(ByVal PName As String, ByVal modNum As Integer, ByVal counter As Integer, Optional ByVal ref As Integer = 0) As Boolean
        Dim dv As Scheduler.Classes.DeviceClass = Nothing
        Log("Initiating Device " & PName, LogLevel.Debug)

        Try
            If Not hs.DeviceExistsRef(ref) Then
                ref = hs.NewDeviceRef(PName)

                hs.SaveINISetting("Module " & modNum, "ref-" & counter.ToString, ref, INIFILE)
                Try
                    dv = hs.GetDeviceByRef(ref)
                    InitHSDevice(dv, PName)
                    Return True
                Catch ex As Exception
                    Log("Error initializing device " & PName & ": " & ex.Message)
                    Return False
                End Try
            End If
        Catch ex As Exception
            Log("Error getting RefID from DeviceCode within InitDevice. " & ex.Message)
        End Try
        Return False
    End Function

    Sub InitHSDevice(ByRef dv As Scheduler.Classes.DeviceClass, Optional ByVal Name As String = "Sample")
        Dim test As Object = Nothing

        dv.Address(hs) = "HOME"
        Dim DT As New DeviceTypeInfo
        DT.Device_Type = DeviceTypeInfo.eDeviceAPI.Plug_In
        dv.DeviceType_Set(hs) = DT
        dv.Interface(hs) = IFACE_NAME
        dv.InterfaceInstance(hs) = Instance
        dv.Last_Change(hs) = Now
        dv.Name(hs) = Name
        dv.Location(hs) = "MyPlug"
        dv.Device_Type_String(hs) = "MyPlug"
        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
        dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
        dv.Status_Support(hs) = False
    End Sub

    Public Sub SendCommand(ByVal Housecode As String, ByVal Devicecode As String, ByVal Action As Integer)
        'Send a command somewhere
    End Sub

    Public Sub RegisterCallback(ByRef frm As Object)
        ' call back into HS and get a reference to the HomeSeer ActiveX interface
        ' this can be used make calls back into HS like hs.SetDeviceValue, etc.
        ' The callback object is a different interface reserved for plug-ins.
        callback = frm
        hs = frm.GetHSIface
        If hs Is Nothing Then
            MsgBox("Unable to access HS interface", MsgBoxStyle.Critical)
        Else
            Log("Register callback completed", LogLevel.Debug)
            InterfaceVersion = hs.InterfaceVersion
        End If
    End Sub

    Public Sub RegisterConfigWebPage(ByVal link As String, Optional linktext As String = "", Optional page_title As String = "")
        Try
            hs.RegisterPage(link, IFACE_NAME, Instance)
            If linktext = "" Then linktext = link
            linktext = linktext.Replace("_", " ")
            If page_title = "" Then page_title = linktext
            Dim wpd As New HomeSeerAPI.WebPageDesc
            wpd.plugInName = IFACE_NAME
            wpd.link = link
            wpd.linktext = linktext & Instance
            wpd.page_title = page_title & Instance
            callback.RegisterConfigLink(wpd)
        Catch ex As Exception
            Log(LogLevel.Debug, "Error - Registering Web Links: " & ex.Message)
        End Try
    End Sub

    Public Sub RegisterWebPage(ByVal link As String, Optional linktext As String = "", Optional page_title As String = "")
        Try
            hs.RegisterPage(link, IFACE_NAME, Instance)
            If linktext = "" Then linktext = link
            linktext = linktext.Replace("_", " ")
            If page_title = "" Then page_title = linktext
            Dim wpd As New HomeSeerAPI.WebPageDesc
            wpd.plugInName = IFACE_NAME
            wpd.link = link
            wpd.linktext = linktext
            wpd.plugInInstance = Instance
            wpd.page_title = page_title
            callback.RegisterLink(wpd)
        Catch ex As Exception
            Log(LogLevel.Debug, "Error - Registering Web Links: " & ex.Message)
        End Try
    End Sub

End Module
