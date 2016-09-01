

' ---------------------------------------------------------------------------
' Filename:		GoogleCalendar.vb
' By:			Steph@ne
' Created:		29-01-2011
' Updated:		
' Version:		1
' 
' ---------------------------------------------------------------------------


'Imports HomeSeerAPI


Imports System.IO
Imports System.Threading
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Calendar.v3
Imports Google.Apis.Calendar.v3.Data
Imports Google.Apis.Services
Imports Google.Apis.Util.Store

Module GoogleCalendar


    Dim Scopes = {CalendarService.Scope.CalendarReadonly}
    Dim ApplicationName As String = "HomeseerGoogleCalendarReader"



    Sub Main(ByVal param As Object)
        Try
            Dim StrModeLog As String = hs.GetINISetting("Parametres", "ModeLog", "", INIFILE)
            Dim comptes() As String = hs.GetINISectionEx("comptes", INIFILE) ' GetINISetting("Parametres", "Comptes", "", INIFILE).Split(",")

            Dim i As Integer

            ' For i = LBound(comptes) To UBound(comptes)
            For Each compte In comptes
                compte = compte.Split("=")(0)
                Dim ref As Integer = CInt(hs.GetINISetting(compte, "ref", 0, INIFILE))


                Dim credential As UserCredential

                    '  Using Str As New FileStream("client_secret.json", FileMode.Open, FileAccess.Read)

                    Dim credPath As String = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
                    credPath = Path.Combine(credPath, ".credentials")

                    Dim cs As New ClientSecrets()
                    cs.ClientId = "a renseigner"
                    cs.ClientSecret = "a resneigner"

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                   cs,
                    Scopes,
                    Compte, CancellationToken.None,
                    New FileDataStore(credPath, True)).Result
                    'Console.WriteLine("Credential file saved to: " + credPath)
                    ' End Using

                    ' Create Google Calendar API service.
                    Dim bcs As New BaseClientService.Initializer()
                    bcs.HttpClientInitializer = credential
                    bcs.ApplicationName = ApplicationName

                    Dim Service = New CalendarService(bcs)

                    ' Define parameters of request.
                    Dim request As EventsResource.ListRequest = Service.Events.List("primary")
                    request.TimeMin = DateTime.Now.Date
                    request.TimeMax = Now.AddDays(1).Date
                    request.ShowDeleted = False
                    request.SingleEvents = True
                    'request.MaxResults = 10
                    request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime

                    ' List events.
                    Dim events As Events = request.Execute()
                ' Console.WriteLine("Upcoming events:")
                Log("recherche des événements CONGES dans le calendrier de " + compte)

                hs.SetDeviceString(ref, "", True)
                If (Not férié(compte)) Then




                    Dim StatusJour = GetDefautToday(compte)
                    ' Dim Valueclé As String = hs.GetINISetting(StatusJour, "Value", "", INIFILE)
                    ' application du paramètre JOUR sur les RefCompte
                    If (StatusJour <> " ") Then
                        hs.SetDeviceValueByRef(ref, StatusJour, True)
                    End If

                    'hs.SetDeviceValueByRef(ref, Valueclé, True)

                    If ((events.Items IsNot Nothing) And (events.Items.Count > 0)) Then


                        For Each eventItem As [Event] In events.Items
                            Dim correspondanceStatus As String = findDictionnaryStatus(eventItem.Summary)
                            If (correspondanceStatus <> "") Then
                                Log(" [" & compte & "] :  " & eventItem.Summary, MessageType.Debug)
                                hs.SetDeviceValueByRef(ref, correspondanceStatus, True)
                                '  hs.SetDeviceString(StrStatut, "EN REPOS", True)
                            End If



                            '  End If
                        Next

                        ' Else Console.WriteLine("No upcoming events found.")

                    End If
                End If
            Next

        Catch ex As Exception
            Log("erreur dans Main : " + ex.Message, MessageType.Error_)
            If (ex.InnerException IsNot Nothing) Then
                Log("détail : " + ex.InnerException.Message, MessageType.Error_)
            End If
        End Try
    End Sub

    Function findDictionnaryStatus(key As String) As String
        Dim clés() = hs.GetINISectionEx("clés", INIFILE)
        For Each clé In clés
            clé = clé.Split("=")(0)
            If (UCase(key).StartsWith(UCase(clé))) Then
                '    If (UCase(clé.Split("|")(0)) = UCase(key)) Then
                Return hs.GetINISetting(clé, "Value", "9999", INIFILE)
            End If
        Next
        Return ""
    End Function

    Dim jourdelasemaine = Weekday(Now)
    Dim jours As String() = {"", "dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi"}

    Function GetDefautToday(compte As String)
        jourdelasemaine = Weekday(Now)
        Log("Nous sommes " + jours(jourdelasemaine) + ". Le status par défaut pour aujourd'hui est " + hs.GetINISetting(compte, jours(jourdelasemaine), " ", INIFILE), MessageType.Debug)
        Return hs.GetINISetting(compte, jours(jourdelasemaine), " ", INIFILE)
    End Function


    Function férié(compte As String) As Boolean
        Try
            Dim refCompte = hs.GetINISetting(compte, "ref", 0, INIFILE)
            Dim Text = "Ce n'est pas"
            Dim thisYear = Year(Now)
            Dim thisDay = Day(Now) & "/" & Month(Now)




            'Application du paramètre FERIE sur les réfCompte
            Dim StatusFerie = hs.GetINISetting(compte, "ferie", " ", INIFILE)
            If (StatusFerie <> " ") Then

                Dim jFixes = "1/1,1/5,8/5,14/7,15/8,1/11,11/11,25/12"
                Dim G = thisYear Mod 19
                Dim c = thisYear \ 100
                Dim H = (c - (c \ 4) - ((8 * c + 13) \ 25) + (19 * G) + 15) Mod 30
                Dim I = H - (H \ 28) * ((1 - (H \ 28) * (29 \ (H + 1)) * (21 - G) \ 11))
                Dim J = (thisYear + (thisYear \ 4) + I + 2 - c + (c \ 4)) Mod 7
                Dim L = I - J
                Dim mois = 3 + ((L + 40) \ 44)
                Dim jour = L + 28 - 31 * (mois \ 4)
                Dim jPaques = jour & "/" & mois & "/" & thisYear
                Dim jLundiPaques = DateSerial(Year(jPaques), Month(jPaques), Day(jPaques) + 1)
                Dim jAscension = DateSerial(Year(jPaques), Month(jPaques), Day(jPaques) + 39)
                Dim jPentecote = DateSerial(Year(jPaques), Month(jPaques), Day(jPaques) + 50)
                Dim jFeries = jFixes & "," & Day(jPaques) & "/" & Month(jPaques) & "," & Day(jLundiPaques) & "/" & Month(jLundiPaques) & "," & Day(jAscension) & "/" & Month(jAscension) & "," & Day(jPentecote) & "/" & Month(jPentecote)
                Dim tabFeries = Split(jFeries, ",")


                'hs.writelog "f�ri�",thisDay
                For Each Jo As String In tabFeries
                    If Jo = thisDay Then
                        Log("Mais nous sommes un jour férié.", MessageType.Debug)

                        hs.SetDeviceValueByRef(refCompte, hs.GetINISetting(compte, "ferie", "", INIFILE), True)
                        Return True
                    End If
                Next
            End If
            Return False
        Catch ex As Exception
            Log("erreur dans les jours Fériés : " + ex.Message, MessageType.Error_)
            Return False
        End Try
    End Function
End Module
