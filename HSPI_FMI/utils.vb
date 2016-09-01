Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization.Formatters
Imports HomeSeerAPI
Imports iCloudLib
Imports Scheduler.Classes

Module utils
    Public IFACE_NAME As String = "FindMyiPhone"

    Public callback As HomeSeerAPI.IAppCallbackAPI
    Public hs As HomeSeerAPI.IHSApplication
    Public Instance As String = ""
    Public InterfaceVersion As Integer
    Public bShutDown As Boolean = False
    Public gEXEPath As String = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)
    Public Const INIFILE As String = "HSPI_FindMyiPhone.ini"
    Public CurrentPage As Object
    Public AppleIDs As New hsCollection ' As List(Of iCloud)

    Sub FMIUpdated(responseUpdate As iCloud.iCloudFMIClientResponse)
        For Each device In responseUpdate.Devices
            Dim ref = hs.GetINISetting(device.DeviceName, "ref", 0, INIFILE)
            Dim dv As DeviceClass = hs.GetDeviceByRef(ref)
            If dv IsNot Nothing Then

                If (device.Location IsNot Nothing) Then
                    Dim lngDevice = device.Location.Longitude
                    Dim latDevice = device.Location.Latitude
                    Dim AccDevice = device.Location.HorizontalAccuracy
                    Log("Evaluation de la postion de " & device.DeviceName, MessageType.Debug)
                    Dim str = ""

                    For Each position In hs.GetINISectionEx("locations", INIFILE)
                        position = position.Split("=")(0)
                        Dim lngMAISON = hs.GetINISetting(position, "lng", 0, INIFILE)
                        Dim latMAISON = hs.GetINISetting(position, "lat", 0, INIFILE)
                        Dim perimètre = hs.GetINISetting(position, "perimetre", 10, INIFILE)
                        Dim dist = getDistanceFrom(position, latDevice, lngDevice, 0, latMAISON, lngMAISON, 0)

                        If dist <= perimètre Then


                            str = device.DeviceName & " : Cet iDevice est situé à la position """ & position & """."

                            If hs.GetINISetting(position, "value", "", INIFILE) <> "" Then
                                hs.SetDeviceValueByRef(ref, hs.GetINISetting(position, "value", 999, INIFILE), True)
                            End If
                            If hs.GetINISetting(position, "string", "", INIFILE) <> "" Then
                                hs.SetDeviceString(ref, hs.GetINISetting(position, "string", "", INIFILE), True)
                            End If
                            If hs.GetINISetting(position, "location", "", INIFILE) <> "" Then
                                dv.Location(hs) = hs.GetINISetting(position, "location2", "", INIFILE)
                            End If
                            If hs.GetINISetting(position, "location2", "", INIFILE) <> "" Then
                                dv.Location2(hs) = hs.GetINISetting(position, "location2", "", INIFILE)
                            End If
                            'Select Case hs.GetINISetting("param", "ElementToUpdate", " ", INIFILE)
                            '    Case "location"
                            '        
                            '    Case "location2"
                            '        dv.Location2(hs) = position
                            '    Case "string"
                            '        hs.SetDeviceString(ref, position, True)
                            '    Case "value"
                            '        hs.SetDeviceValueByRef(ref, hs.GetINISetting(position, "value", 0, INIFILE), True)
                            '    Case Else
                            'End Select

                        End If




                    Next
                    If str = "" Then str = device.DeviceName & " : Cet iDevice est situé en dehors de toutes zones connues."
                    Log(str)
                Else
                    Log(device.DeviceName & " : Cet iDevice est introuvable")
                End If
                hs.SaveEventsDevices()
            End If
        Next

    End Sub

    Function getDistanceFrom(position As String, fromLat As String, fromLng As String, fromAcc As String, lat As String, lng As String, acc As String)

        Dim delta_lat As Double = fromLat - lat
        Dim delta_lon = fromLng - lng
        Dim distance = Math.Sin(deg2rad(lat)) * Math.Sin(deg2rad(fromLat)) + Math.Cos(deg2rad(lat)) * Math.Cos(deg2rad(fromLat)) * Math.Cos(deg2rad(delta_lon))
        distance = Math.Acos(distance)
        distance = rad2deg(distance)
        distance = distance * 60 * 1.1515
        distance = distance * 1.609344 '//Miles In KM
        distance = Math.Round(distance, 4) * 1000 '//In meters

        Log(" --> Distance to " & position & " =" & distance & " m", MessageType.Debug)

        Return distance
    End Function

    Function deg2rad(angle)

        ' http//kevin.vanzonneveld.net
        ' +	 original by: Enrique Gonzalez
        ' +	   improved by: Thomas Grainger(http:  //graingert.co.uk)
        ' *	   example 1: deg2rad(45);
        ' *	   returns 1: 0.7853981633974483
        Return angle * 0.017453292519943295   ' // (angle / 180) * Math.PI
    End Function

    Function rad2deg(angle)

        '// http//kevin.vanzonneveld.net
        '// +	 original by: Enrique Gonzalez
        '// +		improved by: Brett Zamir(http:  //brett-zamir.me)
        '// *	   example 1: rad2deg(3.1415926535897931);
        '// *	   returns 1: 180
        Return angle * 57.295779513082323   ' // angle / Math.PI * 180
    End Function

    Public Function StringIsNullOrEmpty(ByRef s As String) As Boolean
        If String.IsNullOrEmpty(s) Then Return True
        Return String.IsNullOrEmpty(s.Trim)
    End Function

    Public Structure pair
        Dim name As String
        Dim value As String
    End Structure



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

    Public Sub DeleteDevices()
        Dim en As Object
        Dim dv As Object

        Try
            en = hs.GetDeviceEnumerator
            Do While Not en.Finished
                dv = en.GetNext
                If dv IsNot Nothing Then
                    If dv.interface = IFACE_NAME Then
                        Try
                            hs.DeleteDevice(dv.ref)
                        Catch ex As Exception
                        End Try
                    End If
                End If
            Loop
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
            Log("Error - Registering Web Links: " & ex.Message, MessageType.Error_)
        End Try
    End Sub


End Module
