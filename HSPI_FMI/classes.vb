Imports System.Net
Imports System.Web.Script.Serialization
Imports Scheduler.Classes
Imports Newtonsoft.Json.Linq
Imports iCloudLib

Public Module classes

    ' ==========================================================================
    ' ==========================================================================
    ' ==========================================================================
    '       These class objects are used to hold plug-in specific information 
    '   about its various triggers and actions.  If there is no information 
    '   needed other than the Trigger/Action number and/or the SubTrigger
    '   /SubAction number, then these are not needed as they are intended to 
    '   store additional information beyond those selection values.  The UID
    '   (Unique Trigger ID or Unique Action ID) can be used as the key to the
    '   storage of these class objects when the plug-in is running.  When the 
    '   plug-in is not running, the serialized copy of these classes is stored
    '   and restored by HomeSeer.
    ' ==========================================================================
    ' ==========================================================================
    ' ==========================================================================

    <Serializable()>
    Public Class action
        Inherits hsCollection
        Public Sub New()
            MyBase.New()
        End Sub
        Protected Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub
    End Class

    <Serializable()>
    Public Class hsCollection
        Inherits Dictionary(Of String, iCloud)
        Dim KeyIndex As New Collection

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub

        Public Overloads Sub Add(value As iCloud, Key As String)
            '  If (Key.Contains("_")) Then
            '  Key = Key.Replace("_" & Key.Split("_")(3), "")
            ' End If

            If Not MyBase.ContainsKey(Key) Then
                MyBase.Add(Key, value)
                KeyIndex.Add(Key, Key)
            Else
                MyBase.Item(Key) = value
            End If
        End Sub

        Public Overloads Sub Remove(Key As String)
            On Error Resume Next
            MyBase.Remove(Key)
            KeyIndex.Remove(Key)
        End Sub

        Public Overloads Sub Remove(Index As Integer)
            MyBase.Remove(KeyIndex(Index))
            KeyIndex.Remove(Index)
        End Sub

        Public Overloads ReadOnly Property Keys(ByVal index As Integer) As Object
            Get
                Dim i As Integer
                Dim key As String = Nothing
                For Each key In MyBase.Keys
                    If i = index Then
                        Exit For
                    Else
                        i += 1
                    End If
                Next
                Return key
            End Get
        End Property

        Default Public Overloads Property Item(ByVal index As Integer) As iCloud
            Get
                Return MyBase.Item(KeyIndex(index))
            End Get
            Set(ByVal value As iCloud)
                MyBase.Item(KeyIndex(index)) = value
            End Set
        End Property

        Default Public Overloads Property Item(ByVal Key As String) As iCloud
            Get
                On Error Resume Next
                Return MyBase.Item(Key)
            End Get
            Set(ByVal value As iCloud)
                If Not MyBase.ContainsKey(Key) Then
                    Add(value, Key)
                Else
                    MyBase.Item(Key) = value
                End If
            End Set
        End Property
    End Class


    <Serializable()>
    Friend Class MyAction

        Private mvarUID As Integer
        Public Property ActionUID As Integer
            Get
                Return mvarUID
            End Get
            Set(value As Integer)
                mvarUID = value
            End Set
        End Property

        Private mvarConfigured As Boolean
        Public ReadOnly Property Configured As Boolean
            Get
                Return mvarConfigured
            End Get
        End Property

        Private mvarClientName As String
        Public Property ClientName As String
            Get
                Return mvarClientName
            End Get
            Set(value As String)
                mvarClientName = value
            End Set
        End Property

        Private mvarTexte As String
        Public Property Texte As String
            Get
                Return mvarTexte
            End Get
            Set(value As String)
                mvarTexte = value
            End Set
        End Property

    End Class


    Class Extensions

        Public Shared Function PostDataToWebsite(ByRef wc As WebClient, url As String, postData As String) As String

            Dim result = String.Empty

            wc.Encoding = System.Text.Encoding.UTF8
            wc.Headers(HttpRequestHeader.ContentType) = "application/x-www-form-urlencoded"

            result = wc.UploadString(url, "POST", postData)

            Return result
        End Function
    End Class


    'Public Class iCloud1

    '    Const iCloudUrl As String = "https://www.icloud.com"
    '    Const iCloudLoginUrl As String = "https://setup.icloud.com/setup/ws/1/login"
    '    ' Const iCloudPlaySoundUrl As String = "https://p03-fmipweb.icloud.com/fmipservice/client/web/playSound"
    '    ' Const iCloudInitClientUrl As String = "https://p19-fmipweb.icloud.com/fmipservice/client/web/initClient"

    '    Const iCloudPlaySoundUrl As String = "/fmipservice/client/web/playSound?dsid="
    '    Const iCloudInitClientUrl As String = "/fmipservice/client/web/initClient?dsid="
    '    Const iCloudSendMessage As String = "/fmipservice/client/web/sendMessage?dsid="

    '    Const iCloudPush As String = "/registerTopics?attempt=1&clientBuildNumber=14H 40&dsid="

    '    Public Sub New()

    '    End Sub

    '    Public Function getsDevicesFromAppleID(appleId As String, password As String) As String()
    '        Dim jsonString As String = ContactAppleID(appleId, password)
    '        Dim devices As String = ""
    '        If (jsonString.StartsWith("{""statusCode"":""200""")) Then

    '            Dim js = New JavaScriptSerializer()
    '            Dim response = js.Deserialize(Of Object)(jsonString)
    '            Dim content = response("content")

    '            For Each o As Object In content
    '                If devices = "" Then
    '                    devices = o("name")
    '                Else
    '                    devices = devices & "|" & o("name")
    '                End If
    '                '     If (o("name") = deviceName) Then
    '                '  Dim psResult = Extensions.PostDataToWebsite(wc, iCloudDeviceUrlFMI + iCloudSendMessage + iCloudDsId, String.Format("{{""device"":""{0}"",""subject"":""Find My iPhone Alert"",""sound"": ""False"",""userText"": ""True"",""text"": ""petit message à faire passer à mon chéridou !""}}", o("id")))
    '                ' Dim psResult = Extensions.PostDataToWebsite(wc, iCloudDeviceUrlFMI + iCloudPlaySoundUrl + iCloudDsId, String.Format("{{""device"":""{0}"",""subject"":""Find My iPhone Alert""}}", o("id")))
    '                '    Dim lat = o("location")("latitude")
    '                '     Dim lng = o("location")("longitude")
    '                '   Dim adresse As String = Extensions.PostDataToWebsite(wc, "http://maps.googleapis.com/maps/api/geocode/json?latlng=44.129830,4.095377", "")
    '                '  Console.WriteLine(adresse)
    '                '  End If
    '            Next
    '        End If
    '        Return devices.Split("|")
    '    End Function

    '    Public Sub LocaliseDevices(compte As String)
    '        Dim jsonString As String = ContactAppleID(compte.Split("=")(0), compte.Split("=")(1))
    '        FileIO.FileSystem.WriteAllText("jsonString.txt", jsonString, False)
    '        Dim devices As String = ""
    '        If (jsonString.StartsWith("{""statusCode"":""200""")) Then

    '            Dim js = New JavaScriptSerializer()
    '            Dim response = js.Deserialize(Of Object)(jsonString)
    '            Dim content = response("content")

    '            For Each o As Object In content
    '                If (hs.GetINISetting(o("name"), "ref", 0, INIFILE) > 0) Then

    '                    UpdatePosition(o)


    '                End If
    '            Next
    '        End If
    '    End Sub

    '    Public Sub Ping(appleId As String, password As String, deviceName As String)
    '        Dim jsonString As String = ContactAppleID(appleId, password)

    '        If (jsonString.StartsWith("{""statusCode"":""200""")) Then

    '            Dim js = New JavaScriptSerializer()
    '            Dim response = js.Deserialize(Of Object)(jsonString)
    '            Dim content = response("content")

    '            For Each o As Object In content

    '                If (o("name") = deviceName) Then
    '                    '  Dim psResult = Extensions.PostDataToWebsite(wc, iCloudDeviceUrlFMI + iCloudSendMessage + iCloudDsId, String.Format("{{""device"":""{0}"",""subject"":""Find My iPhone Alert"",""sound"": ""False"",""userText"": ""True"",""text"": ""petit message à faire passer à mon chéridou !""}}", o("id")))
    '                    ' Dim psResult = Extensions.PostDataToWebsite(wc, iCloudDeviceUrlFMI + iCloudPlaySoundUrl + iCloudDsId, String.Format("{{""device"":""{0}"",""subject"":""Find My iPhone Alert""}}", o("id")))
    '                    Dim lat = o("location")("latitude")
    '                    Dim lng = o("location")("longitude")
    '                    '   Dim adresse As String = Extensions.PostDataToWebsite(wc, "http://maps.googleapis.com/maps/api/geocode/json?latlng=44.129830,4.095377", "")
    '                    '  Console.WriteLine(adresse)
    '                End If
    '            Next
    '        End If
    '    End Sub

    '    Shared Function ContactAppleID(appleId As String, password As String) As String

    '        '  Dim wc As WebClient = New WebClient()

    '        Dim authCookies As String = String.Empty
    '        If (wc.Headers.AllKeys.Any(Function(k) k = "Origin")) Then
    '            wc.Headers.Add("Origin", iCloudUrl)
    '        End If
    '        If (wc.Headers.AllKeys.Any(Function(k) k = "Content-Type")) Then
    '            wc.Headers.Add("Content-Type", "text/plain")
    '        End If

    '        ' Extensions.PostDataToWebsite(wc, iCloudLoginUrl, String.Format("{{""apple_id"":""{0}"",""password"":""{1}"",""extended_login"":false}}", appleId, password))
    '        Dim Data As String = Extensions.PostDataToWebsite(wc, iCloudLoginUrl, String.Format("{{""apple_id"":""{0}"",""password"":""{1}"",""extended_login"":false}}", appleId, password))


    '        Dim obj As JObject = JObject.Parse(Data)
    '        Dim iCloudDeviceUrlFMI As String = CStr(obj("webservices")("findme")("url"))
    '        Dim iCloudDsId As String = CStr(obj("dsInfo")("dsid"))

    '        Dim iCloudDeviceUrlPush As String = CStr(obj("webservices")("push")("url"))

    '        ' If (wc.ResponseHeaders.AllKeys.Any(k =& gt; k == "Set-Cookie")) Then
    '        If (wc.ResponseHeaders.AllKeys.Any(Function(k) k = "Set-Cookie")) Then
    '            wc.Headers.Add("Cookie", wc.ResponseHeaders("Set-Cookie"))

    '        Else

    '            Throw New System.Security.SecurityException("Invalid username / password")
    '        End If

    '        ' Dim jsonString = Extensions.PostDataToWebsite(wc, iCloudInitClientUrl, "{""clientContext"":{""appName"":""iCloud Find (Web) "", ""appVersion"":  ""2.0""," + """timezone"":""Europe/Paris"",""inactiveTime"":2255,""apiVersion"":""3.0"",""webStats"":""0:15""}}")
    '        Return Extensions.PostDataToWebsite(wc, iCloudDeviceUrlFMI + iCloudInitClientUrl + iCloudDsId, "{""clientContext"": {""appName"":""iCloud Find (Web) "", ""appVersion"":  ""2.0"",""timezone"":""Europe/Paris"",""inactiveTime"":1,""apiVersion"":""3.0""}}")
    '    End Function





    '    Sub UpdatePosition(o)

    '        Dim device = o("name")
    '        If (o("location") IsNot Nothing) Then
    '            Dim lngDevice = o("location")("longitude")
    '            Dim latDevice = o("location")("latitude")
    '            Dim AccDevice = o("location")("horizontalAccuracy")
    '            Console.WriteLine("Evaluation de la postion de " & o("name"))
    '            Dim str = ""
    '            For Each position In hs.GetINISectionEx("locations", INIFILE)
    '                position = position.Split("=")(0)
    '                Dim lngMAISON = hs.GetINISetting(position, "lng", 0, INIFILE)
    '                Dim latMAISON = hs.GetINISetting(position, "lat", 0, INIFILE)
    '                Dim perimètre = hs.GetINISetting(position, "perimetre", 10, INIFILE)
    '                Dim dist = getDistanceFrom(position, latDevice, lngDevice, 0, latMAISON, lngMAISON, 0)

    '                If dist <= perimètre Then
    '                    str = device & " : Cet iDevice est situé à la position """ & position & """."
    '                    Dim ref = hs.GetINISetting(device, "ref", 0, INIFILE)
    '                    Dim dv As DeviceClass = hs.GetDeviceByRef(ref)
    '                    If dv IsNot Nothing Then
    '                        Select Case hs.GetINISetting(device, "ElementToUpdate", " ", INIFILE)
    '                            Case "location"
    '                                dv.Location(hs) = position
    '                            Case "location2"
    '                                dv.Location2(hs) = position
    '                            Case "string"
    '                                hs.SetDeviceString(ref, position, True)
    '                            Case "value"
    '                                hs.SetDeviceValueByRef(ref, hs.GetINISetting(position, "value", 0, INIFILE), True)
    '                            Case Else
    '                        End Select

    '                    End If

    '                End If


    '            Next
    '            If str = "" Then str = device & " : Cet iDevice est situé en dehors de toutes zones connues."
    '            hs.WriteLog(IFACE_NAME, str)
    '        Else
    '            hs.WriteLog(IFACE_NAME, device & " : Cet iDevice est introuvable")
    '        End If
    '        hs.SaveEventsDevices()
    '    End Sub

    '    Shared Function getDistanceFrom(position As String, fromLat As String, fromLng As String, fromAcc As String, lat As String, lng As String, acc As String)

    '        Dim delta_lat As Double = fromLat - lat
    '        Dim delta_lon = fromLng - lng
    '        Dim distance = Math.Sin(deg2rad(lat)) * Math.Sin(deg2rad(fromLat)) + Math.Cos(deg2rad(lat)) * Math.Cos(deg2rad(fromLat)) * Math.Cos(deg2rad(delta_lon))
    '        distance = Math.Acos(distance)
    '        distance = rad2deg(distance)
    '        distance = distance * 60 * 1.1515
    '        distance = distance * 1.609344 '//Miles In KM
    '        distance = Math.Round(distance, 4) * 1000 '//In meters

    '        Console.WriteLine("Distance to " & position & " =" & distance & " m")

    '        Return distance
    '    End Function

    '    Shared Function deg2rad(angle)

    '        ' http//kevin.vanzonneveld.net
    '        ' +	 original by: Enrique Gonzalez
    '        ' +	   improved by: Thomas Grainger(http:  //graingert.co.uk)
    '        ' *	   example 1: deg2rad(45);
    '        ' *	   returns 1: 0.7853981633974483
    '        Return angle * 0.017453292519943295   ' // (angle / 180) * Math.PI
    '    End Function

    '    Shared Function rad2deg(angle)

    '        '// http//kevin.vanzonneveld.net
    '        '// +	 original by: Enrique Gonzalez
    '        '// +		improved by: Brett Zamir(http:  //brett-zamir.me)
    '        '// *	   example 1: rad2deg(3.1415926535897931);
    '        '// *	   returns 1: 180
    '        Return angle * 57.295779513082323   ' // angle / Math.PI * 180
    '    End Function

    'End Class

End Module
