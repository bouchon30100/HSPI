Imports System.IO
Imports HSPIUtils.LogUtils
Imports Newtonsoft.Json


Module classes

    Dim keys As String()
    Enum PokemonMessageTypes
        None
        Pokemon
        Pokestop
        Gym
    End Enum
    Dim HandledEncounters As New Dictionary(Of String, Double)

    Public Function DeserializeFromStream(data As String) As Object '(stream As Stream) As Object

        Dim serializer = New JsonSerializer()

        Dim results = JsonConvert.DeserializeObject(Of PokemonPOST)(data)
        '  Using sr = New StreamReader(data) '(Stream)


        ' Using jsonTextReader = New JsonTextReader(sr)

        Return results 'serializer.Deserialize(Of PokemonPOST)(jsonTextReader)
        '     End Using
        ' End Using
    End Function

    Dim encounterLockObject As Object = New Object()


    Public Sub HandleRequest(data As String) '(request As HttpRequest)

        Try
            RemoveOldPokemon()
        Catch ex As Exception
            logutils.Log("RemoveOldPokemon : " & ex.Message, MessageType.Error_)
        End Try
        ' déplacement de ce bloc pour optimiser le traitement des pokestop
        Try
            Dim handlingRequest As Boolean = False
            Dim msg As PokemonPOST
            Dim foundPokemon As Pokemon
            msg = DeserializeFromStream(data) '(request.InputStream)

            Select Case msg.MessageType
                Case PokemonMessageTypes.Pokemon
                    '   fin du bloc déplacé
                    Try
            HandledEncounters.Clear()
            For Each item As String In hs.GetINISectionEx("HandledEncounters", utils.INIFILE)
                HandledEncounters.Add((item.Split("=")(0)).Replace("""", "="), item.Split("=")(1))
            Next
        Catch ex As Exception
            logutils.Log("Add_HandleEncounters : " & ex.Message, MessageType.Error_)
        End Try

                    'position du bloc déplacé

                    foundPokemon = New Pokemon(msg.Message.Pokemon_ID)

                    If Not (HandledEncounters.ContainsKey(msg.Message.Encounter_ID)) Then

                        handlingRequest = True
                        HandledEncounters.Add(msg.Message.Encounter_ID, msg.Message.Disappear_Time)
                        Try
                            If (handlingRequest = True) Then

                                Dim loc As String = ""    '"44.129787,4.095396"
                                ' Dim url As String = String.Format("http://maps.google.com/?q={0},{1}", cstr(msg.Message.Latitude).replace(",","."), cstr(msg.Message.Longitude).replace(",","."))
                                Dim url As String = String.Format("https://www.google.com/maps/dir/{2}/{0},{1}?hl=fr", CStr(msg.Message.Latitude).Replace(",", "."), CStr(msg.Message.Longitude).Replace(",", "."), loc)
                                Dim image As String = "http://assets.pokemon.com/assets/cms2/img/pokedex/detail/" & foundPokemon.ID & ".png"


                                Dim dt As DateTime = UnixTimeStampToDateTime(msg.Message.Disappear_Time) ' + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)
                                Dim diff As TimeSpan = dt - DateTime.Now

                                If (diff.Minutes < 0 Or diff.Minutes > 45) Then
                                    Return
                                End If
                                Dim html As String = "<table><td><img src='" & "\POKEMAP\icons\" & foundPokemon.ID & ".png'></td>"
                                html += String.Format("<td> Un <b> {0} </b> ({1}) est apparu", foundPokemon.Name, foundPokemon.ID) + "<br>"
                                html += String.Format("Il disparaitra a {0} (soit dans {1}m {2}s)", dt.ToLongTimeString(), diff.Minutes, diff.Seconds) + "<br>"
                                html += String.Format("<a href=" + url + ">Voir sur une carte </a> </td></tr></table>")

                                Dim sms As String = foundPokemon.Name + "(" & foundPokemon.ID & ") --> " + dt.ToLongTimeString() + vbCrLf + url
                                logutils.Log(html, MessageType.Debug)

                                Dim PokemonToNotifyFor As String()
                                For Each name As String In hs.GetINISectionEx("LISTES", utils.INIFILE)
                                    If Not (name.Split("=")(1).StartsWith("_")) Then
                                        PokemonToNotifyFor = hs.GetINISectionEx(name.Split("=")(0), utils.INIFILE)
                                        If Not (PokemonToNotifyFor.Contains(foundPokemon.ID & "= ")) Then
                                            logutils.Log("notif à " & name.Split("=")(0), MessageType.Debug)
                                            hs.SetDeviceString(name.Split("=")(1), sms, True)
                                        End If
                                    End If
                                Next

                            End If
                        Catch ex As Exception
                            logutils.Log("Traitement du HandleRequest : " & ex.Message, MessageType.Error_)
                        End Try
                    End If
                Case PokemonMessageTypes.Pokestop
                    Dim loc As String = ""
                    Dim dt As DateTime = UnixTimeStampToDateTime(msg.Message.lure_expiration) ' + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)
                    Dim diff As TimeSpan = dt - DateTime.Now
                    Dim url As String = String.Format("https://www.google.com/maps/dir/{2}/{0},{1}?hl=fr", CStr(msg.Message.Latitude).Replace(",", "."), CStr(msg.Message.Longitude).Replace(",", "."), Loc)

                    Dim html As String = "<table border='1'><td> pokestop n° " & msg.Message.pokestop_id & "</td>"
                    html += String.Format("<td>lured = a {0} (soit dans {1}m {2}s)</td>", dt.ToLongTimeString(), diff.Minutes, diff.Seconds)
                    html += String.Format("<td>enable : {0}</td>", msg.Message.enabled)
                    html += String.Format("<td><a href=" + url + ">Voir sur une carte </a> </td></tr></table>")
                    logutils.Log(html, MessageType.Debug)



                Case PokemonMessageTypes.Gym


                    logutils.Log("WebHook ARENE !!! ", MessageType.Debug)



                Case Else

            End Select

            '  Lock(encounterLockObject)


            ' Unlock(encounterLockObject)
        Catch ex As Exception
            logutils.Log("désérialisation du pokemon : " & ex.Message, MessageType.Error_)
        End Try



        Try
            For Each item In HandledEncounters
                hs.SaveINISetting("HandledEncounters", item.Key.Replace("=", """"), item.Value, utils.INIFILE)
            Next
        Catch ex As Exception
            logutils.Log("Save des HandledEncounters : " & ex.Message, MessageType.Error_)
        End Try

    End Sub


    Public Sub RemoveOldPokemon()
        Dim currTime As DateTime = DateTime.Now

        For Each item As String In hs.GetINISectionEx("HandledEncounters", utils.INIFILE)
            Dim time As DateTime = UnixTimeStampToDateTime(item.Split("=")(1)) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)
            ' If (time < currTime) Then
            ' hs.WriteLog("POKEMAP", currTime & " / " & time & " --> " & time.CompareTo(currTime))
            If (time.CompareTo(currTime) = -1) Then
                hs.SaveINISetting("HandledEncounters", item.Split("=")(0), "", utils.INIFILE)
            End If
        Next
    End Sub

    Function UnixTimeStampToDateTime(unixTimeStamp As Double) As DateTime
        Dim dtDateTime As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime()
        Return dtDateTime
    End Function

    Class PokemonPOST
        <JsonProperty("type")>
        Private typeString As String

        Public Property MessageType As PokemonMessageTypes
            Get
                If (typeString IsNot Nothing) Then

                    Select Case typeString.Trim().ToLower()
                        Case "pokemon"
                            Return PokemonMessageTypes.Pokemon
                        Case "pokestop"
                            Return PokemonMessageTypes.Pokestop
                        Case "gym"
                            Return PokemonMessageTypes.Gym
                        Case Else
                            Return PokemonMessageTypes.None
                    End Select
                Else
                    Return PokemonMessageTypes.None
                End If
            End Get
            Set(value As PokemonMessageTypes)
            End Set
        End Property

        <JsonProperty("message")>
        Public Message As POGOMessage


        Sub New()
            typeString = Nothing
        End Sub

    End Class

    Class POGOMessage
        Inherits PokemonMessage

        <JsonProperty("latitude")>
        Public Latitude As Double

        <JsonProperty("longitude")>
        Public Longitude As Double

    End Class



    Class PokemonMessage
        Inherits PokestopMessage

        <JsonProperty("encounter_id")>
        Public Encounter_ID As String

        <JsonProperty("spawnpoint_id")>
        Public Spawnpoint_ID As String

        <JsonProperty("pokemon_id")>
        Public Pokemon_ID As Integer

        <JsonProperty("disappear_time")>
        Public Disappear_Time As Double
    End Class


    Class GymMessage


        'gym_id': f['id'],
        'team_id': f.get('owned_by_team', 0),
        'guard_pokemon_id': f.get('guard_pokemon_id', 0),
        'gym_points': f.get('gym_points', 0),
        'enabled': f['enabled'],
        'latitude': f['latitude'],
        'longitude': f['longitude'],
        'last_modified': datetime.utcfromtimestamp(f['last_modified_timestamp_ms'] / 1000.0),

        'pokemon_uid': member['pokemon_data']['id'],
        'pokemon_id': member['pokemon_data']['pokemon_id'],
        'cp': member['pokemon_data']['cp'],
        'num_upgrades': member['pokemon_data'].get('num_upgrades', 0),
        'move_1': member['pokemon_data'].get('move_1'),
        'move_2': member['pokemon_data'].get('move_2'),
        'height': member['pokemon_data'].get('height_m'),
        'weight': member['pokemon_data'].get('weight_kg'),
        'stamina': member['pokemon_data'].get('stamina'),
        'stamina_max': member['pokemon_data'].get('stamina_max'),
        'cp_multiplier': member['pokemon_data'].get('cp_multiplier'),
        'additional_cp_multiplier': member['pokemon_data'].get('additional_cp_multiplier', 0),
        'iv_defense': member['pokemon_data'].get('individual_defense', 0),
        'iv_stamina': member['pokemon_data'].get('individual_stamina', 0),
        'iv_attack': member['pokemon_data'].get('individual_attack', 0),
        'trainer_name': member['trainer_public_profile']['name'],
        'trainer_level': member['trainer_public_profile']['level'],

    End Class

    Class PokestopMessage
        Inherits GymMessage

        'pokestop_id': b64encode(str(f['id'])),
        'enabled': f['enabled'],
        'latitude': f['latitude'],
        'longitude': f['longitude'],
        'last_modified': calendar.timegm(pokestops[f['id']]['last_modified'].timetuple()),
        'lure_expiration': l_e,
        'active_fort_modifier': active_fort_modifier

        <JsonProperty("pokestop_id")>
        Public pokestop_id As String

        <JsonProperty("enabled")>
        Public enabled As String

        <JsonProperty("last_modified")>
        Public last_modified As Object

        <JsonProperty("lure_expiration")>
        Public lure_expiration As Double

        <JsonProperty("active_fort_modifier")>
        Public active_fort_modifier As Object
    End Class

    Public Class Pokemon

        Private PokemonList As New List(Of PokemonListEntry)

        Private PokemonList1 As PokemonListEntry() = {New PokemonListEntry("Bulbasaur", 1),
            New PokemonListEntry("Ivysaur", 2),
            New PokemonListEntry("Venusaur", 3),
            New PokemonListEntry("Charmander", 4),
            New PokemonListEntry("Charmeleon", 5),
            New PokemonListEntry("Charizard", 6),
            New PokemonListEntry("Squirtle", 7),
            New PokemonListEntry("Wartortle", 8),
            New PokemonListEntry("Blastoise", 9),
            New PokemonListEntry("Caterpie", 10),
            New PokemonListEntry("Metapod", 11),
            New PokemonListEntry("Butterfree", 12),
            New PokemonListEntry("Weedle", 13),
            New PokemonListEntry("Kakuna", 14),
            New PokemonListEntry("Beedrill", 15),
            New PokemonListEntry("Pidgey", 16),
            New PokemonListEntry("Pidgeotto", 17),
            New PokemonListEntry("Pidgeot", 18),
            New PokemonListEntry("Rattata", 19),
            New PokemonListEntry("Raticate", 20),
            New PokemonListEntry("Spearow", 21),
            New PokemonListEntry("Fearow", 22),
            New PokemonListEntry("Ekans", 23),
            New PokemonListEntry("Arbok", 24),
            New PokemonListEntry("Pikachu", 25),
            New PokemonListEntry("Raichu", 26),
            New PokemonListEntry("Sandshrew", 27),
            New PokemonListEntry("Sandslash", 28),
            New PokemonListEntry("Nidoran♀", 29),
            New PokemonListEntry("Nidorina", 30),
            New PokemonListEntry("Nidoqueen", 31),
            New PokemonListEntry("Nidoran♂", 32),
            New PokemonListEntry("Nidorino", 33),
            New PokemonListEntry("Nidoking", 34),
            New PokemonListEntry("Clefairy", 35),
            New PokemonListEntry("Clefable", 36),
            New PokemonListEntry("Vulpix", 37),
            New PokemonListEntry("Ninetales", 38),
            New PokemonListEntry("Jigglypuff", 39),
            New PokemonListEntry("Wigglytuff", 40),
            New PokemonListEntry("Zubat", 41),
            New PokemonListEntry("Golbat", 42),
            New PokemonListEntry("Oddish", 43),
            New PokemonListEntry("Gloom", 44),
            New PokemonListEntry("Vileplume", 45),
            New PokemonListEntry("Paras", 46),
            New PokemonListEntry("Parasect", 47),
            New PokemonListEntry("Venonat", 48),
            New PokemonListEntry("Venomoth", 49),
            New PokemonListEntry("Diglett", 50),
            New PokemonListEntry("Dugtrio", 51),
            New PokemonListEntry("Meowth", 52),
            New PokemonListEntry("Persian", 53),
            New PokemonListEntry("Psyduck", 54),
            New PokemonListEntry("Golduck", 55),
            New PokemonListEntry("Mankey", 56),
            New PokemonListEntry("Primeape", 57),
            New PokemonListEntry("Growlithe", 58),
            New PokemonListEntry("Arcanine", 59),
            New PokemonListEntry("Poliwag", 60),
            New PokemonListEntry("Poliwhirl", 61),
            New PokemonListEntry("Poliwrath", 62),
            New PokemonListEntry("Abra", 63),
            New PokemonListEntry("Kadabra", 64),
            New PokemonListEntry("Alakazam", 65),
            New PokemonListEntry("Machop", 66),
            New PokemonListEntry("Machoke", 67),
            New PokemonListEntry("Machamp", 68),
            New PokemonListEntry("Bellsprout", 69),
            New PokemonListEntry("Weepinbell", 70),
            New PokemonListEntry("Victreebel", 71),
            New PokemonListEntry("Tentacool", 72),
            New PokemonListEntry("Tentacruel", 73),
            New PokemonListEntry("Geodude", 74),
            New PokemonListEntry("Graveler", 75),
            New PokemonListEntry("Golem", 76),
            New PokemonListEntry("Ponyta", 77),
            New PokemonListEntry("Rapidash", 78),
            New PokemonListEntry("Slowpoke", 79),
            New PokemonListEntry("Slowbro", 80),
            New PokemonListEntry("Magnemite", 81),
            New PokemonListEntry("Magneton", 82),
            New PokemonListEntry("Farfetch'd", 83),
            New PokemonListEntry("Doduo", 84),
            New PokemonListEntry("Dodrio", 85),
            New PokemonListEntry("Seel", 86),
            New PokemonListEntry("Dewgong", 87),
            New PokemonListEntry("Grimer", 88),
            New PokemonListEntry("Muk", 89),
            New PokemonListEntry("Shellder", 90),
            New PokemonListEntry("Cloyster", 91),
            New PokemonListEntry("Gastly", 92),
            New PokemonListEntry("Haunter", 93),
            New PokemonListEntry("Gengar", 94),
            New PokemonListEntry("Onix", 95),
            New PokemonListEntry("Drowzee", 96),
            New PokemonListEntry("Hypno", 97),
            New PokemonListEntry("Krabby", 98),
            New PokemonListEntry("Kingler", 99),
            New PokemonListEntry("Voltorb", 100),
            New PokemonListEntry("Electrode", 101),
            New PokemonListEntry("Exeggcute", 102),
            New PokemonListEntry("Exeggutor", 103),
            New PokemonListEntry("Cubone", 104),
            New PokemonListEntry("Marowak", 105),
            New PokemonListEntry("Hitmonlee", 106),
            New PokemonListEntry("Hitmonchan", 107),
            New PokemonListEntry("Lickitung", 108),
            New PokemonListEntry("Koffing", 109),
            New PokemonListEntry("Weezing", 110),
            New PokemonListEntry("Rhyhorn", 111),
            New PokemonListEntry("Rhydon", 112),
            New PokemonListEntry("Chansey", 113),
            New PokemonListEntry("Tangela", 114),
            New PokemonListEntry("Kangaskhan", 115),
            New PokemonListEntry("Horsea", 116),
            New PokemonListEntry("Seadra", 117),
            New PokemonListEntry("Goldeen", 118),
            New PokemonListEntry("Seaking", 119),
            New PokemonListEntry("Staryu", 120),
            New PokemonListEntry("Starmie", 121),
            New PokemonListEntry("Mr. Mime", 122),
            New PokemonListEntry("Scyther", 123),
            New PokemonListEntry("Jynx", 124),
            New PokemonListEntry("Electabuzz", 125),
            New PokemonListEntry("Magmar", 126),
            New PokemonListEntry("Pinsir", 127),
            New PokemonListEntry("Tauros", 128),
            New PokemonListEntry("Magikarp", 129),
            New PokemonListEntry("Gyarados", 130),
            New PokemonListEntry("Lapras", 131),
            New PokemonListEntry("Ditto", 132),
            New PokemonListEntry("Eevee", 133),
            New PokemonListEntry("Vaporeon", 134),
            New PokemonListEntry("Jolteon", 135),
            New PokemonListEntry("Flareon", 136),
            New PokemonListEntry("Porygon", 137),
            New PokemonListEntry("Omanyte", 138),
            New PokemonListEntry("Omastar", 139),
            New PokemonListEntry("Kabuto", 140),
            New PokemonListEntry("Kabutops", 141),
            New PokemonListEntry("Aerodactyl", 142),
            New PokemonListEntry("Snorlax", 143),
            New PokemonListEntry("Articuno", 144),
            New PokemonListEntry("Zapdos", 145),
            New PokemonListEntry("Moltres", 146),
            New PokemonListEntry("Dratini", 147),
            New PokemonListEntry("Dragonair", 148),
            New PokemonListEntry("Dragonite", 149),
            New PokemonListEntry("Mewtwo", 150),
            New PokemonListEntry("Mew", 151)
       }

        Public Name As String
        Public ID As Integer

        Public Sub New(Name As String)

            populatePokemonList()
            Dim entries As PokemonListEntry() = PokemonList.Where(Function(pokemo) pokemo.Name.ToLower().Equals(Name.ToLower())).ToArray()

            If (entries.Length <> 1) Then

                Throw New InvalidDataException()
            End If

            Me.Name = entries(0).Name
            Me.ID = entries(0).ID
        End Sub

        Public Sub New(_ID As Integer)
            populatePokemonList()
            Dim entries As PokemonListEntry() = PokemonList.Where(Function(pokemo) pokemo.ID = _ID).ToArray()
            If (entries.Length <> 1) Then
                Throw New InvalidDataException()
            End If

            Me.Name = entries(0).Name
            Me.ID = entries(0).ID
        End Sub

        Sub populatePokemonList()

            PokemonList = New List(Of PokemonListEntry)
            Dim f As String = "C:\Program Files (x86)\HomeSeer HS3\html\POKEMAP\pokemon.fr.json"
            Dim text As String = My.Computer.FileSystem.ReadAllText(f).Replace("{", "").Replace("}", "").Replace("""", "")
            For Each item As String In text.Split(",")
                PokemonList.Add(New PokemonListEntry(item.Split(":")(1), CInt(item.Split(":")(0))))
            Next

        End Sub

        Class PokemonListEntry

            Public Name As String
            Public ID As Integer

            Sub New(_name As String, _id As Integer)
                Name = _name
                ID = _id
            End Sub

        End Class
    End Class


#Region "GestionSMS"
    Sub gérerSMS(name As String, text As String)

        Dim cmd As String = text.Split(" ")(0)

        logutils.Log("SMS reçu de " + name + " : """ + text + """", MessageType.Debug)

        If (cmd.ToUpper() = "PKM") Then
            Dim pkmid As String = ""
            Dim réponse As String = "PokeMap : Je n'ai pas compris !"
            Try
                'arrêt notif pour un pkm
                If text.ToUpper().Contains("STOP") Then
                    pkmid = text.Split(" ")(2)
                    Dim pkm As Pokemon = New Pokemon(CInt(pkmid))
                    hs.SaveINISetting(name, pkmid, " ", utils.INIFILE)
                    réponse = "Le pokemon " & pkm.Name & " (" & pkmid & ") ne te sera plus notifé."

                    'PAUSE sur les notif
                ElseIf (text.ToUpper().Contains("PAUSE")) Then
                    Dim refDevice As String = hs.GetINISetting("LISTES", name, " ", utils.INIFILE)
                    hs.SaveINISetting("LISTES", name, "_" + refDevice, utils.INIFILE)
                    réponse = "Les notifications PokeMap sont DESACTIVEES."

                    'START sur les notif
                ElseIf (text.ToUpper().Contains("START")) Then
                    Dim refDevice As String = hs.GetINISetting("LISTES", name, " ", utils.INIFILE).Replace("_", "")
                    hs.SaveINISetting("LISTES", name, refDevice, utils.INIFILE)
                    réponse = "Les notifications PokeMap sont ACTIVEES."

                    'Donner la liste des PKM non notifiés
                ElseIf (text.ToUpper().Contains("STATUS")) Then
                    Dim refDevice As String = hs.GetINISetting("LISTES", name, " ", utils.INIFILE)
                    If (refDevice.StartsWith("_")) Then
                        réponse = "Les notifications PokeMap sont DESACTIVEES." + vbCrLf
                    Else
                        réponse = "Les notifications PokeMap sont ACTIVEES." + vbCrLf
                    End If
                    réponse += "Liste des Pokemon EXCLUS :" + vbCrLf
                    Dim liste As New List(Of Integer)
                    For Each item As String In hs.GetINISectionEx(name, utils.INIFILE)
                        liste.Add(item.Split("=")(0))
                    Next

                    liste.Sort()
                    For Each item In liste
                        Dim pkm As Pokemon = New Pokemon(item)
                        réponse += pkm.Name & " (" & pkm.ID & ")" & ", "
                    Next

                    réponse = réponse.Substring(0, réponse.Length - 2)
                    réponse += "."
                Else
                    pkmid = text.Split(" ")(1)
                    Dim pkm As Pokemon = New Pokemon(CInt(pkmid))
                    hs.SaveINISetting(name, pkmid, "", utils.INIFILE)
                    réponse = "Le pokemon " & pkm.Name & " (" & pkmid & ") te sera désormais notifié."
                End If
                Dim device As String = hs.GetINISetting("LISTES", name, 0, utils.INIFILE).Replace("_", "")
                hs.SetDeviceString(device, réponse, True)

            Catch ex As Exception
                logutils.Log("Envoi de message à " & name & " : " & ex.Message, MessageType.Error_)
            End Try
            logutils.Log("Message envoyé à " & name & " : " & réponse, MessageType.Debug)
        End If
    End Sub
#End Region


End Module
