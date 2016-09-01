Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes

Public Class SMSWebPage
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim lbList As New Collection
    Dim ddTable As DataTable = Nothing

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String

        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)



        Return MyBase.postBackProc(page, data, user, userRights)
    End Function

    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        Dim stb As New StringBuilder
        Dim instancetext As String = ""
        Try

            Me.reset()

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)
            End If
            Dim number As String = parts("phone").Replace("+33", "0")
            Dim smsCenter As String = parts("smscenter")
            Dim text As String = parts("text")

            Dim name As String = FindDestinataire(number)

            Dim cmd As String = text.Split(" ")(0)

            Log("SMS reçu de " + name + " : """ + text + """", LogLevel.Debug)
            For Each redirection As String In hs.GetINISectionEx("CODES", INIFILE)
                Dim code As String = redirection.Split("=")(0)
                Dim refDevice As String = redirection.Split("=")(1)

                If (cmd.ToUpper() = code) Then
                    PutHSDevice(refDevice, text, name)
                    Return Me.BuildPage()
                End If
            Next



            'si pas de code particulier trouvé alors envoi du sms à Pepito pour avoir une réponse
            PutHSDevice(hs.GetINISetting("param", "moduleRef", "", INIFILE), text, name)

            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function

    Public Sub PutHSDevice(ref As Integer, text As String, expéditeur As String)
        Dim dvSMS As DeviceClass = hs.GetDeviceByRef(ref)
        Dim dvSender As DeviceClass = hs.GetDeviceByRef(hs.GetINISetting("param", "moduleSender", "", INIFILE))
        hs.SetDeviceString(dvSender.Ref(Nothing), expéditeur, True)
        hs.SetDeviceString(dvSMS.Ref(Nothing), text, True)
        Dim devValue = dvSMS.devValue(Nothing)
        If devValue = 0 Then
            devValue = 1
        Else
            devValue = 0
        End If
        hs.SetDeviceValueByRef(dvSMS.Ref(Nothing), devValue, True)
    End Sub


    Function uniformise(str As String)

        Dim bytes As Byte() = System.Text.Encoding.GetEncoding(1251).GetBytes(str)
        Return System.Text.Encoding.ASCII.GetString(bytes)
    End Function

    Private Sub sendValues(dest As String, param As String, piece As String, etage As String)

        Dim devs = getListModuleHS()
        Dim result As New SortedList(Of String, DeviceClass)

        For Each keyValue In devs
            Dim dv As DeviceClass = keyValue.Value
            If etage <> "" Then
                If uniformise(dv.Location2(Nothing)).ToUpper <> uniformise(etage).ToUpper Then Continue For 'devs.Remove(keyValue.Key)

            End If
            If piece <> "" Then
                If uniformise(dv.Location(Nothing)).ToUpper <> uniformise(piece).ToUpper Then Continue For 'devs.Remove(keyValue.Key)

            End If
            If param <> "" Then
                    If uniformise(dv.Device_Type_String(Nothing)).ToUpper <> uniformise(param).ToUpper(Globalization.CultureInfo.InvariantCulture) Then Continue For 'devs.Remove(keyValue.Key)

                End If
            result.Add(dv.Ref(Nothing), dv)
        Next

        For Each keyValue In result
            Dim dv As DeviceClass = keyValue.Value
            Dim str As String = dv.Device_Type_String(Nothing) + "-" + dv.Location(Nothing) + " (" + dv.Location2(Nothing) + ") : " + CStr(dv.devValue(Nothing))
            sendSMS(dest, "", str)
        Next
        If result.Count = 0 Then sendSMS(dest, "", "Je n'ai rien trouvé !")

    End Sub

    Function BuildContent() As String

    End Function




End Class
