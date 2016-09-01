Imports HomeSeerAPI
Imports Scheduler
Imports System.Reflection
Imports System.Text


Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters


Module Util
   
    ' interface status
    ' for InterfaceStatus function call
    Public Const ERR_NONE = 0
    Public Const ERR_SEND = 1
    Public Const ERR_INIT = 2

     Public mediacallback As HomeSeerAPI.IMediaAPI_HS

    Public gEXEPath As String = ""
    Public gGlobalTempScaleF As Boolean = True

    Public colTrigs_Sync As System.Collections.SortedList
    Public colTrigs As System.Collections.SortedList
    Public colActs_Sync As System.Collections.SortedList
    Public colActs As System.Collections.SortedList

    Private Demo_ARE As Threading.AutoResetEvent
    Private Demo_Thread As Threading.Thread


    Friend Function GetDecimals(ByVal D As Double) As Integer
        Dim s As String = ""
        Dim c(0) As Char
        c(0) = "0"  ' Trailing zeros to be removed.
        D = Math.Abs(D) - Math.Abs(Int(D))  ' Remove the whole number so the result always starts with "0." which is a known quantity.
        s = D.ToString("F30")
        s = s.TrimEnd(c)
        Return s.Length - 2     ' Minus 2 because that is the length of "0."
    End Function

    Friend RNum As New Random(2000)
    Friend Function Demo_Generate_Weight() As Double
        Dim Mult As Integer
        Dim W As Double
        ' The sole purpose of this procedure is to generate random weights
        '   for the purpose of testing the triggers and actions in this plug-in.

        Try
            Do
                Mult = RNum.Next(3)
            Loop Until Mult > 0
            W = (RNum.NextDouble * 2001) * Mult ' Generates a random weight between 0 and 6003 lbs.
        Catch ex As Exception
            Log(IFACE_NAME & " Error: Exception in demo number generation for Trigger 1: " & ex.Message, MessageType.Warning)
        End Try

        Return W

    End Function

 
    

    Friend Enum eTriggerType
        OneTon = 1
        TwoVolts = 2
        Unknown = 0
    End Enum
    Friend Enum eActionType
        Unknown = 0
        Weight = 1
        Voltage = 2
    End Enum

    Friend Structure strTrigger
        Public WhichTrigger As eTriggerType
        Public TrigObj As Object
        Public Result As Boolean
    End Structure
    Friend Structure strAction
        Public WhichAction As eActionType
        Public ActObj As Object
        Public Result As Boolean
    End Structure

 
    Public MyDevice As Integer = -1
    Public MyTempDevice As Integer = -1

    Friend Sub Find_Create_Devices()
        Dim col As New Collections.Generic.List(Of Scheduler.Classes.DeviceClass)
        Dim dv As Scheduler.Classes.DeviceClass
        Dim Found As Boolean = False

        Try
            Dim EN As Scheduler.Classes.clsDeviceEnumeration
            EN = hs.GetDeviceEnumerator
            If EN Is Nothing Then Throw New Exception(IFACE_NAME & " failed to get a device enumerator from HomeSeer.")
            Do
                dv = EN.GetNext
                If dv Is Nothing Then Continue Do

                

                If dv.Interface(Nothing) IsNot Nothing Then
                    If dv.Interface(Nothing).Trim = IFACE_NAME Then
                        col.Add(dv)
                    End If
                End If
            Loop Until EN.Finished
        Catch ex As Exception
            hs.WriteLog(IFACE_NAME & " Error", "Exception in Find_Create_Devices/Enumerator: " & ex.Message)
        End Try

        Try
            Dim DT As DeviceTypeInfo = Nothing
            If col IsNot Nothing AndAlso col.Count > 0 Then
                For Each dv In col
                    If dv Is Nothing Then Continue For
                    If dv.Interface(hs) <> IFACE_NAME Then Continue For
                    DT = dv.DeviceType_Get(hs)
                    If DT IsNot Nothing Then
                        If DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat AndAlso DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Temperature Then
                            ' this is our temp device
                            Found = True
                            MyTempDevice = dv.Ref(Nothing)
                            hs.SetDeviceValueByRef(dv.Ref(Nothing), 72, False)
                        End If

                        If DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat AndAlso DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Setpoint Then
                            Found = True
                            If DT.Device_SubType = DeviceTypeInfo.eDeviceSubType_Setpoint.Heating_1 Then
                                hs.SetDeviceValueByRef(dv.Ref(Nothing), 68, False)
                            End If
                        End If

                        If DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat AndAlso DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Setpoint Then
                            Found = True
                            If DT.Device_SubType = DeviceTypeInfo.eDeviceSubType_Setpoint.Cooling_1 Then
                                hs.SetDeviceValueByRef(dv.Ref(Nothing), 75, False)
                            End If
                        End If

                        If DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In AndAlso DT.Device_Type = 69 Then
                            Found = True
                            MyDevice = dv.Ref(Nothing)

                            ' Now (mostly for demonstration purposes) - work with the PlugExtraData object.
                            Dim EDO As HomeSeerAPI.clsPlugExtraData = Nothing
                            EDO = dv.PlugExtraData_Get(Nothing)
                            If EDO IsNot Nothing Then
                                Dim obj As Object = Nothing
                                obj = EDO.GetNamed("My Special Object")
                                If obj IsNot Nothing Then
                                    Log("Plug-In Extra Data Object Retrieved = " & obj.ToString, MessageType.Normal)
                                End If
                                obj = EDO.GetNamed("My Count")
                                Dim MC As Integer = 1
                                If obj Is Nothing Then
                                    If Not EDO.AddNamed("My Count", MC) Then
                                        Log("Error adding named data object to plug-in sample device!", MessageType.Error_)
                                        Exit For
                                    End If
                                    dv.PlugExtraData_Set(hs) = EDO
                                    hs.SaveEventsDevices()
                                Else
                                    Try
                                        MC = Convert.ToInt32(obj)
                                    Catch ex As Exception
                                        MC = -1
                                    End Try
                                    If MC < 0 Then Exit For
                                    Log("Retrieved count from plug-in sample device is: " & MC.ToString, MessageType.Normal)
                                    MC += 1
                                    ' Now put it back - need to remove the old one first.
                                    EDO.RemoveNamed("My Count")
                                    EDO.AddNamed("My Count", MC)
                                    dv.PlugExtraData_Set(hs) = EDO
                                    hs.SaveEventsDevices()
                                End If
                            End If


                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            hs.WriteLog(IFACE_NAME & " Error", "Exception in Find_Create_Devices/Find: " & ex.Message)
        End Try

        Try
            If Not Found Then
                Dim ref As Integer
                Dim GPair As VGPair = Nothing
                ref = hs.NewDeviceRef("Sample Plugin Device with buttons and slider")
                If ref > 0 Then
                    MyDevice = ref
                    dv = hs.GetDeviceByRef(ref)
                    Dim disp(1) As String
                    disp(0) = "This is a test of the"
                    disp(1) = "Emergency Broadcast Display Data system"
                    dv.AdditionalDisplayData(hs) = disp
                    dv.Address(hs) = "HOME"
                    dv.Code(hs) = "A1"              ' set a code if needed, but not required
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 69                                 ' our own device type
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim EDO As New HomeSeerAPI.clsPlugExtraData
                    dv.PlugExtraData_Set(hs) = EDO
                    ' Now just for grins, let's modify it.
                    Dim HW As String = "Hello World"
                    If EDO.GetNamed("My Special Object") IsNot Nothing Then
                        EDO.RemoveNamed("My Special Object")
                    End If
                    EDO.AddNamed("My Special Object", HW)
                    ' Need to re-save it.
                    dv.PlugExtraData_Set(hs) = EDO

                    ' add an ON button and value
                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 0
                    Pair.Status = "Off"
                    Pair.Render = Enums.CAPIControlType.Button
                    Pair.Render_Location.Row = 1
                    Pair.Render_Location.Column = 1
                    Pair.ControlUse = ePairControlUse._Off            ' set this for UI apps like HSTouch so they know this is for OFF
                    hs.DeviceVSP_AddPair(ref, Pair)
                    GPair = New VGPair
                    GPair.PairType = VSVGPairType.SingleValue
                    GPair.Set_Value = 0
                    GPair.Graphic = "/images/HomeSeer/status/off.gif"
                    dv.ImageLarge(hs) = "/images/browser.png"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    ' add DIM values
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.Range
                    Pair.ControlUse = ePairControlUse._Dim            ' set this for UI apps like HSTouch so they know this is for lighting control dimming
                    Pair.RangeStart = 1
                    Pair.RangeEnd = 99
                    Pair.RangeStatusPrefix = "Dim "
                    Pair.RangeStatusSuffix = "%"
                    Pair.Render = Enums.CAPIControlType.ValuesRangeSlider
                    Pair.Render_Location.Row = 2
                    Pair.Render_Location.Column = 1
                    Pair.Render_Location.ColumnSpan = 3
                    hs.DeviceVSP_AddPair(ref, Pair)
                    ' add graphic pairs for the dim levels
                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 1
                    GPair.RangeEnd = 5.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-00.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 6
                    GPair.RangeEnd = 15.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-10.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 16
                    GPair.RangeEnd = 25.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-20.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 26
                    GPair.RangeEnd = 35.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-30.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 36
                    GPair.RangeEnd = 45.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-40.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 46
                    GPair.RangeEnd = 55.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-50.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 56
                    GPair.RangeEnd = 65.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-60.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 66
                    GPair.RangeEnd = 75.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-70.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 76
                    GPair.RangeEnd = 85.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-80.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 86
                    GPair.RangeEnd = 95.99999999
                    GPair.Graphic = "/images/HomeSeer/status/dim-90.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.Range
                    GPair.RangeStart = 96
                    GPair.RangeEnd = 98.99999999
                    GPair.Graphic = "/images/HomeSeer/status/on.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    ' add an OFF button and value
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 100
                    Pair.Status = "On"
                    Pair.ControlUse = ePairControlUse._On            ' set this for UI apps like HSTouch so they know this is for lighting control ON
                    Pair.Render = Enums.CAPIControlType.Button
                    Pair.Render_Location.Row = 1
                    Pair.Render_Location.Column = 2
                    hs.DeviceVSP_AddPair(ref, Pair)
                    GPair = New VGPair
                    GPair.PairType = VSVGPairType.SingleValue
                    GPair.Set_Value = 100
                    GPair.Graphic = "/images/HomeSeer/status/on.gif"
                    hs.DeviceVGP_AddPair(ref, GPair)

                    ' add an last level button and value
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 255
                    Pair.Status = "On Last Level"
                    Pair.Render = Enums.CAPIControlType.Button
                    Pair.Render_Location.Row = 1
                    Pair.Render_Location.Column = 3
                    hs.DeviceVSP_AddPair(ref, Pair)

                    ' add a button that executes a special command but does not actually set any device value, here we will speak something
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Control)   ' set the type to CONTROL only so that this value will never be displayed as a status
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 1000                           ' we use a value that is not used as a status, this value will be handled in SetIOMult, see that function for the handling
                    Pair.Status = "Speak Hello"
                    Pair.Render = Enums.CAPIControlType.Button
                    Pair.Render_Location.Row = 1
                    Pair.Render_Location.Column = 3
                    hs.DeviceVSP_AddPair(ref, Pair)




                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    Dim PED As clsPlugExtraData = dv.PlugExtraData_Get(hs)
                    If PED Is Nothing Then PED = New clsPlugExtraData
                    PED.AddNamed("Test", New Boolean)
                    PED.AddNamed("Device", dv)
                    dv.PlugExtraData_Set(hs) = PED
                    dv.Status_Support(hs) = True
                    dv.UserNote(hs) = "This is my user note - how do you like it? This device is version " & dv.Version.ToString
                    'hs.SetDeviceString(ref, "Not Set", False)  ' this will override the name/value pairs
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with list values")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 70
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"
                    Dim Pair As VSPair
                    ' add list values, will appear as drop list control
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.Values
                    Pair.Value = 1
                    Pair.Status = "1"
                    hs.DeviceVSP_AddPair(ref, Pair)

                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.Values
                    Pair.Value = 2
                    Pair.Status = "2"
                    hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    dv.Status_Support(hs) = True
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with radio type control")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 71
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"
                    Dim Pair As VSPair
                    ' add values, will appear as a radio control and only allow one option to be selected at a time
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.Radio_Option
                    Pair.Value = 1
                    Pair.Status = "Value 1"
                    hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 2
                    Pair.Status = "Value 2"
                    hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 3
                    Pair.Status = "Value 3"
                    hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    dv.Status_Support(hs) = True
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with list text single selection")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 72
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"
                    Dim Pair As VSPair
                    ' add list values, will appear as drop list control
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.Single_Text_from_List
                    Pair.Value = 1
                    Pair.Status = "String 1"
                    hs.DeviceVSP_AddPair(ref, Pair)

                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.Single_Text_from_List
                    Pair.Value = 2
                    Pair.Status = "String 2"
                    hs.DeviceVSP_AddPair(ref, Pair)



                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    dv.Status_Support(hs) = True
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with list text multiple selection")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 73
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"
                    Dim Pair As VSPair
                    ' add list values, will appear as drop list control
                    Pair = New VSPair(ePairStatusControl.Control)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.List_Text_from_List
                    Pair.StringListAdd = "String 1"
                    Pair.StringListAdd = "String 2"
                    hs.DeviceVSP_AddPair(ref, Pair)





                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    dv.Status_Support(hs) = True
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with text box text")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "Sample Device with textbox input"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 74
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"
                    Dim Pair As VSPair
                    ' add text value it will appear in an editable text box
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.TextBox_String
                    Pair.Value = 0
                    Pair.Status = "Default Text"
                    hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    dv.Status_Support(hs) = True
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with text box number")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "Sample Device with textbox input"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 75
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"
                    Dim Pair As VSPair
                    ' add text value it will appear in an editable text box
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Render = Enums.CAPIControlType.TextBox_Number
                    Pair.Value = 0
                    Pair.Status = "Default Number"
                    Pair.Value = 0
                    hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)
                    'dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)      ' set this for a status only device, no controls, and do not include the DeviceVSP calls above
                    dv.Status_Support(hs) = True
                End If

                ' this demonstrates some controls that are displayed in a pop-up dialog on the device utility page
                ' this device is also set so the values/graphics pairs cannot be edited and no graphics displays for the status
                ref = hs.NewDeviceRef("Sample Plugin Device with pop-up control")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 76
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"



                    Dim Pair As VSPair
                    ' add an OFF button and value
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 0
                    Pair.Status = "Off"
                    Pair.Render = Enums.CAPIControlType.Button
                    hs.DeviceVSP_AddPair(ref, Pair)

                    ' add an ON button and value

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 100
                    Pair.Status = "On"
                    Pair.Render = Enums.CAPIControlType.Button
                    hs.DeviceVSP_AddPair(ref, Pair)

                    ' add DIM values
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.Range
                    Pair.RangeStart = 1
                    Pair.RangeEnd = 99
                    Pair.RangeStatusPrefix = "Dim "
                    Pair.RangeStatusSuffix = "%"
                    Pair.Render = Enums.CAPIControlType.ValuesRangeSlider

                    hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP)     ' cause control to be displayed in a pop-up dialog
                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)

                    dv.Status_Support(hs) = True
                End If

                ' this is a device that pop-ups and uses row/column attributes to position the controls on the form
                ref = hs.NewDeviceRef("Sample Plugin Device with pop-up control row/column")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 77
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"


                    ' add an array of buttons formatted like a number pad
                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 1
                    Pair.Status = "1"
                    Pair.Render = Enums.CAPIControlType.Button
                    Pair.Render_Location.Column = 1
                    Pair.Render_Location.Row = 1
                    hs.DeviceVSP_AddPair(ref, Pair)

                    Pair.Value = 2 : Pair.Status = "2" : Pair.Render_Location.Column = 2 : Pair.Render_Location.Row = 1 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 3 : Pair.Status = "3" : Pair.Render_Location.Column = 3 : Pair.Render_Location.Row = 1 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 4 : Pair.Status = "4" : Pair.Render_Location.Column = 1 : Pair.Render_Location.Row = 2 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 5 : Pair.Status = "5" : Pair.Render_Location.Column = 2 : Pair.Render_Location.Row = 2 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 6 : Pair.Status = "6" : Pair.Render_Location.Column = 3 : Pair.Render_Location.Row = 2 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 7 : Pair.Status = "7" : Pair.Render_Location.Column = 1 : Pair.Render_Location.Row = 3 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 8 : Pair.Status = "8" : Pair.Render_Location.Column = 2 : Pair.Render_Location.Row = 3 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 9 : Pair.Status = "9" : Pair.Render_Location.Column = 3 : Pair.Render_Location.Row = 3 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 10 : Pair.Status = "*" : Pair.Render_Location.Column = 1 : Pair.Render_Location.Row = 4 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 0 : Pair.Status = "0" : Pair.Render_Location.Column = 2 : Pair.Render_Location.Row = 4 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 11 : Pair.Status = "#" : Pair.Render_Location.Column = 3 : Pair.Render_Location.Row = 4 : hs.DeviceVSP_AddPair(ref, Pair)
                    Pair.Value = 12 : Pair.Status = "Clear" : Pair.Render_Location.Column = 1 : Pair.Render_Location.Row = 5 : Pair.Render_Location.ColumnSpan = 3 : hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP)     ' cause control to be displayed in a pop-up dialog
                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)

                    dv.Status_Support(hs) = True
                End If

                ' this device is created so that no graphics are displayed and the value/graphics pairs cannot be edited
                ref = hs.NewDeviceRef("Sample Plugin Device no graphics")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 76
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    dv.MISC_Set(hs, Enums.dvMISC.NO_GRAPHICS_DISPLAY)    ' causes no graphics to display and value/graphics pairs cannot be edited
                    dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP)     ' cause control to be displayed in a pop-up dialog
                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)

                    dv.Status_Support(hs) = True
                End If

                ref = hs.NewDeviceRef("Sample Plugin Device with color control")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    'dv.Can_Dim(hs) = True
                    dv.Device_Type_String(hs) = "My Sample Device"
                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
                    DT.Device_Type = 76
                    dv.DeviceType_Set(hs) = DT
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim Pair As VSPair
                    Pair = New VSPair(ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.Range
                    Pair.RangeStart = 1
                    Pair.RangeEnd = 99
                    Pair.RangeStatusPrefix = ""
                    Pair.RangeStatusSuffix = ""
                    Pair.Render = Enums.CAPIControlType.Color_Picker

                    hs.DeviceVSP_AddPair(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)


                    dv.Status_Support(hs) = True
                End If

                ' build a thermostat device group,all of the following thermostat devices are grouped under this root device
                gGlobalTempScaleF = Convert.ToBoolean(hs.GetINISetting("Settings", "gGlobalTempScaleF", "True").Trim)   ' get the F or C setting from HS setup
                Dim therm_root_dv As Scheduler.Classes.DeviceClass = Nothing
                ref = hs.NewDeviceRef("Sample Plugin Thermostat Root Device")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    therm_root_dv = dv
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Z-Wave Thermostat"     ' this device type is set up in the default HSTouch projects so we set it here so the default project displays
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Root
                    DT.Device_SubType = 0
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.MISC_Set(hs, Enums.dvMISC.STATUS_ONLY)
                    dv.Relationship(hs) = Enums.eRelationship.Parent_Root

                    hs.SaveEventsDevices()
                End If

                ref = hs.NewDeviceRef("Sample Plugin Thermostat Fan Device")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Fan"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Fan_Mode_Set
                    DT.Device_SubType = 0
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 0
                    Pair.Status = "Auto"
                    Pair.Render = Enums.CAPIControlType.Button
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 1
                    Pair.Status = "On"
                    Pair.Render = Enums.CAPIControlType.Button
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.Status_Support(hs) = True
                    hs.SaveEventsDevices()
                End If

                ref = hs.NewDeviceRef("Sample Plugin Thermostat Mode Device")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Mode"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Operating_Mode
                    DT.Device_SubType = 0
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 0
                    Pair.Status = "Off"
                    Pair.Render = Enums.CAPIControlType.Button
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 1
                    Pair.Status = "Heat"
                    Pair.Render = Enums.CAPIControlType.Button
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)
                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.SingleValue
                    GPair.Set_Value = 1
                    GPair.Graphic = "/images/HomeSeer/status/Heat.png"
                    Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 2
                    Pair.Status = "Cool"
                    Pair.Render = Enums.CAPIControlType.Button
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)
                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.SingleValue
                    GPair.Set_Value = 2
                    GPair.Graphic = "/images/HomeSeer/status/Cool.png"
                    Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 3
                    Pair.Status = "Auto"
                    Pair.Render = Enums.CAPIControlType.Button
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)
                    GPair = New VGPair()
                    GPair.PairType = VSVGPairType.SingleValue
                    GPair.Set_Value = 3
                    GPair.Graphic = "/images/HomeSeer/status/Auto.png"
                    Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.Status_Support(hs) = True
                    hs.SaveEventsDevices()
                End If

                ref = hs.NewDeviceRef("Sample Plugin Thermostat Heat Setpoint")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Heat Setpoint"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Setpoint
                    DT.Device_SubType = DeviceTypeInfo.eDeviceSubType_Setpoint.Heating_1
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.Range
                    Pair.RangeStart = -2147483648
                    Pair.RangeEnd = 2147483647
                    Pair.RangeStatusPrefix = ""
                    Pair.RangeStatusSuffix = " " & VSPair.ScaleReplace
                    Pair.IncludeValues = True
                    Pair.ValueOffset = 0
                    Pair.RangeStatusDecimals = 0
                    Pair.HasScale = True
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Control)
                    Pair.PairType = VSVGPairType.Range
                    ' 39F = 4C
                    ' 50F = 10C
                    ' 90F = 32C
                    If gGlobalTempScaleF Then
                        Pair.RangeStart = 50
                        Pair.RangeEnd = 90
                    Else
                        Pair.RangeStart = 10
                        Pair.RangeEnd = 32
                    End If
                    Pair.RangeStatusPrefix = ""
                    Pair.RangeStatusSuffix = " " & VSPair.ScaleReplace
                    Pair.IncludeValues = True
                    Pair.ValueOffset = 0
                    Pair.RangeStatusDecimals = 0
                    Pair.HasScale = True
                    Pair.Render = Enums.CAPIControlType.TextBox_Number
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    ' The scale does not matter because the global temperature scale setting
                    '   will override and cause the temperature to always display in the user's
                    '   selected scale, so use that in setting up the ranges.
                    'If dv.ZWData.Sensor_Scale = 1 Then  ' Fahrenheit
                    If gGlobalTempScaleF Then
                        ' Set up the ranges for Fahrenheit
                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -50
                        GPair.RangeEnd = 5
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 5.00000001
                        GPair.RangeEnd = 15.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 16
                        GPair.RangeEnd = 25.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 26
                        GPair.RangeEnd = 35.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 36
                        GPair.RangeEnd = 45.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 46
                        GPair.RangeEnd = 55.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 56
                        GPair.RangeEnd = 65.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 66
                        GPair.RangeEnd = 75.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 76
                        GPair.RangeEnd = 85.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 86
                        GPair.RangeEnd = 95.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 96
                        GPair.RangeEnd = 104.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 105
                        GPair.RangeEnd = 150.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    Else
                        ' Celsius
                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -45
                        GPair.RangeEnd = -15
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -14.999999
                        GPair.RangeEnd = -9.44
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -9.43999999
                        GPair.RangeEnd = -3.88
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -3.8799999
                        GPair.RangeEnd = 1.66
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 1.67
                        GPair.RangeEnd = 7.22
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 7.23
                        GPair.RangeEnd = 12.77
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 12.78
                        GPair.RangeEnd = 18.33
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 18.34
                        GPair.RangeEnd = 23.88
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 23.89
                        GPair.RangeEnd = 29.44
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 29.45
                        GPair.RangeEnd = 35
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 35.0000001
                        GPair.RangeEnd = 40
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 40.0000001
                        GPair.RangeEnd = 75
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    End If

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.Status_Support(hs) = True
                    hs.SaveEventsDevices()
                End If

                ref = hs.NewDeviceRef("Sample Plugin Thermostat Cool Setpoint")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Cool Setpoint"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Setpoint
                    DT.Device_SubType = DeviceTypeInfo.eDeviceSubType_Setpoint.Cooling_1
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.Range
                    Pair.RangeStart = -2147483648
                    Pair.RangeEnd = 2147483647
                    Pair.RangeStatusPrefix = ""
                    Pair.RangeStatusSuffix = " " & VSPair.ScaleReplace
                    Pair.IncludeValues = True
                    Pair.ValueOffset = 0
                    Pair.RangeStatusDecimals = 0
                    Pair.HasScale = True
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Control)
                    Pair.PairType = VSVGPairType.Range
                    ' 39F = 4C
                    ' 50F = 10C
                    ' 90F = 32C
                    If gGlobalTempScaleF Then
                        Pair.RangeStart = 50
                        Pair.RangeEnd = 90
                    Else
                        Pair.RangeStart = 10
                        Pair.RangeEnd = 32
                    End If
                    Pair.RangeStatusPrefix = ""
                    Pair.RangeStatusSuffix = " " & VSPair.ScaleReplace
                    Pair.IncludeValues = True
                    Pair.ValueOffset = 0
                    Pair.RangeStatusDecimals = 0
                    Pair.HasScale = True
                    Pair.Render = Enums.CAPIControlType.TextBox_Number
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    ' The scale does not matter because the global temperature scale setting
                    '   will override and cause the temperature to always display in the user's
                    '   selected scale, so use that in setting up the ranges.
                    'If dv.ZWData.Sensor_Scale = 1 Then  ' Fahrenheit
                    If gGlobalTempScaleF Then
                        ' Set up the ranges for Fahrenheit
                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -50
                        GPair.RangeEnd = 5
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 5.00000001
                        GPair.RangeEnd = 15.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 16
                        GPair.RangeEnd = 25.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 26
                        GPair.RangeEnd = 35.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 36
                        GPair.RangeEnd = 45.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 46
                        GPair.RangeEnd = 55.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 56
                        GPair.RangeEnd = 65.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 66
                        GPair.RangeEnd = 75.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 76
                        GPair.RangeEnd = 85.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 86
                        GPair.RangeEnd = 95.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 96
                        GPair.RangeEnd = 104.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 105
                        GPair.RangeEnd = 150.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    Else
                        ' Celsius
                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -45
                        GPair.RangeEnd = -15
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -14.999999
                        GPair.RangeEnd = -9.44
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -9.43999999
                        GPair.RangeEnd = -3.88
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -3.8799999
                        GPair.RangeEnd = 1.66
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 1.67
                        GPair.RangeEnd = 7.22
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 7.23
                        GPair.RangeEnd = 12.77
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 12.78
                        GPair.RangeEnd = 18.33
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 18.34
                        GPair.RangeEnd = 23.88
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 23.89
                        GPair.RangeEnd = 29.44
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 29.45
                        GPair.RangeEnd = 35
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 35.0000001
                        GPair.RangeEnd = 40
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 40.0000001
                        GPair.RangeEnd = 75
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    End If

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.Status_Support(hs) = True
                    hs.SaveEventsDevices()
                End If
                ref = hs.NewDeviceRef("Sample Plugin Thermostat Temp")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Temp"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Temperature
                    DT.Device_SubType = 1   ' temp
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.Range
                    Pair.RangeStart = -2147483648
                    Pair.RangeEnd = 2147483647
                    Pair.RangeStatusPrefix = ""
                    Pair.RangeStatusSuffix = " " & VSPair.ScaleReplace
                    Pair.IncludeValues = True
                    Pair.ValueOffset = 0
                    Pair.HasScale = True
                    Pair.RangeStatusDecimals = 0
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    If gGlobalTempScaleF Then
                        ' Set up the ranges for Fahrenheit
                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = -50
                        GPair.RangeEnd = 5
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 5.00000001
                        GPair.RangeEnd = 15.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 16
                        GPair.RangeEnd = 25.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 26
                        GPair.RangeEnd = 35.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range
                        GPair.RangeStart = 36
                        GPair.RangeEnd = 45.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 46
                        GPair.RangeEnd = 55.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 56
                        GPair.RangeEnd = 65.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 66
                        GPair.RangeEnd = 75.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 76
                        GPair.RangeEnd = 85.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 86
                        GPair.RangeEnd = 95.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 96
                        GPair.RangeEnd = 104.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 105
                        GPair.RangeEnd = 150.99999999
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                    Else
                        ' Celsius
                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = -45
                        GPair.RangeEnd = -15
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-00.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = -14.999999
                        GPair.RangeEnd = -9.44
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-10.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = -9.43999999
                        GPair.RangeEnd = -3.88
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-20.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = -3.8799999
                        GPair.RangeEnd = 1.66
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-30.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 1.67
                        GPair.RangeEnd = 7.22
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-40.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 7.23
                        GPair.RangeEnd = 12.77
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-50.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 12.78
                        GPair.RangeEnd = 18.33
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-60.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 18.34
                        GPair.RangeEnd = 23.88
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-70.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 23.89
                        GPair.RangeEnd = 29.44
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-80.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 29.45
                        GPair.RangeEnd = 35
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-90.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 35.0000001
                        GPair.RangeEnd = 40
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-100.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        GPair = New VGPair()
                        GPair.PairType = VSVGPairType.Range

                        GPair.RangeStart = 40.0000001
                        GPair.RangeEnd = 75
                        GPair.Graphic = "/images/HomeSeer/status/Thermometer-110.png"
                        Default_VG_Pairs_AddUpdateUtil(ref, GPair)

                        dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                        dv.Status_Support(hs) = True
                        hs.SaveEventsDevices()
                    End If
                End If


                ref = hs.NewDeviceRef("Sample Plugin Thermostat Fan State")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Fan State"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Fan_Status
                    DT.Device_SubType = 0
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 0
                    Pair.Status = "Off"
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 1
                    Pair.Status = "On"
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.Status_Support(hs) = True
                    hs.SaveEventsDevices()
                End If

                ref = hs.NewDeviceRef("Sample Plugin Thermostat Mode Status")
                If ref > 0 Then
                    dv = hs.GetDeviceByRef(ref)
                    dv.Address(hs) = "HOME"
                    dv.Device_Type_String(hs) = "Thermostat Mode Status"
                    dv.Interface(hs) = IFACE_NAME
                    dv.InterfaceInstance(hs) = ""
                    dv.Location(hs) = IFACE_NAME
                    dv.Location2(hs) = "Sample Devices"

                    Dim DT As New DeviceTypeInfo
                    DT.Device_API = DeviceTypeInfo.eDeviceAPI.Thermostat
                    DT.Device_Type = DeviceTypeInfo.eDeviceType_Thermostat.Operating_State
                    DT.Device_SubType = 0
                    DT.Device_SubType_Description = ""
                    dv.DeviceType_Set(hs) = DT
                    dv.Relationship(hs) = Enums.eRelationship.Child
                    If therm_root_dv IsNot Nothing Then
                        therm_root_dv.AssociatedDevice_Add(hs, ref)
                    End If
                    dv.AssociatedDevice_Add(hs, therm_root_dv.Ref(hs))

                    Dim Pair As VSPair
                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 0
                    Pair.Status = "Idle"
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 1
                    Pair.Status = "Heating"
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Status)
                    Pair.PairType = VSVGPairType.SingleValue
                    Pair.Value = 2
                    Pair.Status = "Cooling"
                    Default_VS_Pairs_AddUpdateUtil(ref, Pair)

                    dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
                    dv.Status_Support(hs) = True
                    hs.SaveEventsDevices()
                End If
            End If

        Catch ex As Exception
            hs.WriteLog(IFACE_NAME & " Error", "Exception in Find_Create_Devices/Create: " & ex.Message)
        End Try

    End Sub

    Private Sub Default_VG_Pairs_AddUpdateUtil(ByVal dvRef As Integer, ByRef Pair As VGPair)
        If Pair Is Nothing Then Exit Sub
        If dvRef < 1 Then Exit Sub
        If Not hs.DeviceExistsRef(dvRef) Then Exit Sub

        Dim Existing As VGPair = Nothing

        ' The purpose of this procedure is to add the protected, default VS/VG pairs WITHOUT overwriting any user added
        '   pairs unless absolutely necessary (because they conflict).

        Try
            Existing = hs.DeviceVGP_Get(dvRef, Pair.Value) 'VGPairs.GetPairByValue(Pair.Value)

            If Existing IsNot Nothing Then
                hs.DeviceVGP_Clear(dvRef, Pair.Value)
                hs.DeviceVGP_AddPair(dvRef, Pair)
            Else
                ' There is not a pair existing, so just add it.
                hs.DeviceVGP_AddPair(dvRef, Pair)
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Sub Default_VS_Pairs_AddUpdateUtil(ByVal dvRef As Integer, ByRef Pair As VSPair)
        If Pair Is Nothing Then Exit Sub
        If dvRef < 1 Then Exit Sub
        If Not hs.DeviceExistsRef(dvRef) Then Exit Sub

        Dim Existing As VSPair = Nothing

        ' The purpose of this procedure is to add the protected, default VS/VG pairs WITHOUT overwriting any user added
        '   pairs unless absolutely necessary (because they conflict).

        Try
            Existing = hs.DeviceVSP_Get(dvRef, Pair.Value, Pair.ControlStatus) 'VSPairs.GetPairByValue(Pair.Value, Pair.ControlStatus)

            If Existing IsNot Nothing Then

                ' This is unprotected, so it is a user's value/status pair.
                If Existing.ControlStatus = HomeSeerAPI.ePairStatusControl.Both And Pair.ControlStatus <> HomeSeerAPI.ePairStatusControl.Both Then
                    ' The existing one is for BOTH, so try changing it to the opposite of what we are adding and then add it.
                    If Pair.ControlStatus = HomeSeerAPI.ePairStatusControl.Status Then
                        If Not hs.DeviceVSP_ChangePair(dvRef, Existing, HomeSeerAPI.ePairStatusControl.Control) Then
                            hs.DeviceVSP_ClearBoth(dvRef, Pair.Value)
                            hs.DeviceVSP_AddPair(dvRef, Pair)
                        Else
                            hs.DeviceVSP_AddPair(dvRef, Pair)
                        End If
                    Else
                        If Not hs.DeviceVSP_ChangePair(dvRef, Existing, HomeSeerAPI.ePairStatusControl.Status) Then
                            hs.DeviceVSP_ClearBoth(dvRef, Pair.Value)
                            hs.DeviceVSP_AddPair(dvRef, Pair)
                        Else
                            hs.DeviceVSP_AddPair(dvRef, Pair)
                        End If
                    End If
                ElseIf Existing.ControlStatus = HomeSeerAPI.ePairStatusControl.Control Then
                    ' There is an existing one that is STATUS or CONTROL - remove it if ours is protected.
                    hs.DeviceVSP_ClearControl(dvRef, Pair.Value)
                    hs.DeviceVSP_AddPair(dvRef, Pair)

                ElseIf Existing.ControlStatus = HomeSeerAPI.ePairStatusControl.Status Then
                    ' There is an existing one that is STATUS or CONTROL - remove it if ours is protected.
                    hs.DeviceVSP_ClearStatus(dvRef, Pair.Value)
                    hs.DeviceVSP_AddPair(dvRef, Pair)

                End If

            Else
                ' There is not a pair existing, so just add it.
                hs.DeviceVSP_AddPair(dvRef, Pair)

            End If

        Catch ex As Exception

        End Try
    End Sub
    Private Sub CreateOneDevice(dev_name As String)
        Dim ref As Integer
        Dim dv As Scheduler.Classes.DeviceClass

        ref = hs.NewDeviceRef(dev_name)
        Console.WriteLine("Creating device named: " & dev_name)
        If ref > 0 Then
            dv = hs.GetDeviceByRef(ref)
            dv.Address(hs) = "HOME"
            'dv.Can_Dim(hs) = True
            dv.Device_Type_String(hs) = "My Sample Device"
            Dim DT As New DeviceTypeInfo
            DT.Device_API = DeviceTypeInfo.eDeviceAPI.Plug_In
            DT.Device_Type = 69
            dv.DeviceType_Set(hs) = DT
            dv.Interface(hs) = IFACE_NAME
            dv.InterfaceInstance(hs) = ""
            dv.Last_Change(hs) = #5/21/1929 11:00:00 AM#
            dv.Location(hs) = IFACE_NAME
            dv.Location2(hs) = "Sample Devices"


            ' add an ON button and value
            Dim Pair As VSPair
            Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
            Pair.PairType = VSVGPairType.SingleValue
            Pair.Value = 100
            Pair.Status = "On"
            Pair.Render = Enums.CAPIControlType.Button
            hs.DeviceVSP_AddPair(ref, Pair)

            ' add an OFF button and value
            Pair = New VSPair(HomeSeerAPI.ePairStatusControl.Both)
            Pair.PairType = VSVGPairType.SingleValue
            Pair.Value = 0
            Pair.Status = "Off"
            Pair.Render = Enums.CAPIControlType.Button
            hs.DeviceVSP_AddPair(ref, Pair)

            ' add DIM values
            Pair = New VSPair(ePairStatusControl.Both)
            Pair.PairType = VSVGPairType.Range
            Pair.RangeStart = 1
            Pair.RangeEnd = 99
            Pair.RangeStatusPrefix = "Dim "
            Pair.RangeStatusSuffix = "%"
            Pair.Render = Enums.CAPIControlType.ValuesRangeSlider

            hs.DeviceVSP_AddPair(ref, Pair)

            'dv.MISC_Set(hs, Enums.dvMISC.CONTROL_POPUP)     ' cause control to be displayed in a pop-up dialog
            dv.MISC_Set(hs, Enums.dvMISC.SHOW_VALUES)
            dv.MISC_Set(hs, Enums.dvMISC.NO_LOG)

            dv.Status_Support(hs) = True
        End If
    End Sub


   
End Module
