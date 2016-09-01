Imports java.util
Imports System.IO
Imports System.Runtime.InteropServices
Imports Scheduler.Classes

Module Read1Wire


    Dim debug As Boolean = False

    Dim G_SEP = ";"

    ' Creation des variables correspondant à chaque type de container
    Dim owd_enum As Enumeration
    Dim owd As com.dalsemi.onewire.container.OneWireContainer
    Dim adc As com.dalsemi.onewire.container.ADContainer
    Dim tc As com.dalsemi.onewire.container.TemperatureContainer
    Dim sw As com.dalsemi.onewire.container.SwitchContainer
    Dim cnt As com.dalsemi.onewire.container.OneWireContainer1D

    ' Creation de la variable associé à l'adaptateur 1Wire.
    ' Pour l'instant, c'est l'adaptateur par défaut.


    Dim adapter As com.dalsemi.onewire.adapter.DSPortAdapter
    Dim deviceFound As Boolean
    Dim OneWireAddress As String
    Dim state As Object
    Dim numOfADChannels
    Dim channelCount As Integer


    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Sub LoadLibrary(lpFileName As String)
    End Sub

    Function getModules() As List(Of Module1Wire)


        'Try
        '    Dim folder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..\Microsoft.NET\Framework\v2.0.50727")
        '    folder = Path.GetFullPath(folder)
        '    LoadLibrary(Path.Combine(folder, "vjsnativ.dll"))
        'Catch ex As Exception
        '    If ex.InnerException Is Nothing Then
        '        Log("erreur de chargement jvsnativ" + " : " + ex.Message, MessageType.Error_)

        '    Else
        '        Log("erreur de chargement jvsnativ" + ex.Message & vbCrLf & "InnerException: " & ex.InnerException.Message, MessageType.Error_)
        '    End If

        'End Try

        Dim listeModules As List(Of Module1Wire)
        Log("Liste des modules trouvés :", MessageType.Debug)
        ' Enumération de tous les devices du bus
        owd_enum = adapter.getAllDeviceContainers()
        deviceFound = 0
        listeModules = New List(Of Module1Wire)
        While owd_enum.hasMoreElements()
            Try
                Dim strLog = ""
                Dim m As New Module1Wire()
                owd = owd_enum.nextElement
                OneWireAddress = owd.getAddressAsString
                strLog += "***** [" + OneWireAddress + "] "
                ' Gestion des composants 1Wire du type Entrée Analogique (DS2438 par exemple)
                If TypeOf owd Is com.dalsemi.onewire.container.ADContainer Then
                    deviceFound = 1
                    strLog += "(DS2438)"
                    ' cast the OneWireContainer to ADContainer
                    adc = DirectCast(owd, com.dalsemi.onewire.container.ADContainer)
                    channelCount = 0
                    ' Lecture du nombre de voies de mesure
                    numOfADChannels = adc.getNumberADChannels

                    ' On parcours chaque cannal et on lit les mesures
                    For channelCount = 0 To (numOfADChannels - 1)
                        m = New Module1Wire()
                        m.OneWireAdress = OneWireAddress
                        m.OneWireType = "Entrée Analogique (DS2438)"
                        m.Channel = channelCount.ToString()

                        listeModules.Add(m)
                    Next

                End If

                ' Gestion des composants 1Wire du type Température (DS18B20 par exemple)
                If TypeOf owd Is com.dalsemi.onewire.container.TemperatureContainer Then
                    deviceFound = 1
                    strLog += "(DS18B20)"
                    m = New Module1Wire()
                    m.OneWireAdress = OneWireAddress
                    m.OneWireType = "Température (DS18B20)"
                    m.Channel = "NA"
                    '   Log(strLog, MessageType.Debug)
                    listeModules.Add(m)
                End If

                ' Gestion des composants 1Wire du type Switch (DS2405 par exemple)
                If TypeOf owd Is com.dalsemi.onewire.container.SwitchContainer Then
                    deviceFound = 1

                    sw = DirectCast(owd, com.dalsemi.onewire.container.SwitchContainer)

                    numOfADChannels = sw.getNumberChannels(state)

                    ' On parcours les différents switch pour récupérer leur état
                    For channelCount = 0 To (numOfADChannels - 1)
                        m = New Module1Wire()
                        m.OneWireAdress = OneWireAddress
                        m.OneWireType = "Switch (DS2405)"
                        m.Channel = channelCount.ToString()

                        listeModules.Add(m)
                    Next
                End If

                ' Gestion des composants 1Wire du type Compteur (DS2423)
                If TypeOf owd Is com.dalsemi.onewire.container.OneWireContainer1D Then
                    deviceFound = 1
                    strLog += "(DS2423)"

                    m = New Module1Wire()
                    m.OneWireAdress = OneWireAddress
                    m.OneWireType = "Compteur (DS2423)"
                    m.Channel = "NA"

                    listeModules.Add(m)
                    ' Log(strLog, MessageType.Debug)
                End If
                Log(strLog, MessageType.Debug)
            Catch ex As Exception
                Log("******************************", MessageType.Error_)
                Log("est en anomalie", MessageType.Error_)
                Log(ex.Message, MessageType.Error_)
                Log("******************************", MessageType.Error_)
            End Try
        End While
        '   DisposeAdapter()

        Log("Fin de la recherce des modules 1-Wire", MessageType.Debug)


        If (deviceFound = 0) Then
            Log("No 1-Wire devices found!", MessageType.Debug)
        End If

        Return listeModules
    End Function

    Public Function getAdapter()
        Try

            Dim folder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..\Microsoft.NET\Framework\v2.0.50727")
            folder = Path.GetFullPath(folder)
            LoadLibrary(Path.Combine(folder, "vjsnativ.dll"))
        Catch ex As Exception
            If ex.InnerException Is Nothing Then
                Log("erreur de chargement jvsnativ" + " : " + ex.Message, MessageType.Error_)

            Else
                Log("erreur de chargement jvsnativ" + ex.Message & vbCrLf & "InnerException: " & ex.InnerException.Message, MessageType.Error_)
            End If

        End Try

        Try
            Log("Ouverture de l'adaptateur et verrouillage du bus", MessageType.Debug)
            '  Dim adapter As com.dalsemi.onewire.adapter.DSPortAdapter
            adapter = com.dalsemi.onewire.OneWireAccessProvider.getDefaultAdapter
            adapter.beginExclusive(True)
            adapter.setSearchAllDevices()
            adapter.targetAllFamilies()
            adapter.setSpeed(com.dalsemi.onewire.adapter.DSPortAdapter.SPEED_REGULAR)
            Log("--> adatper ouvert", MessageType.Debug)

        Catch ex As Exception
            Log(ex.InnerException.Message, MessageType.Error_)
            DisposeAdapter()
        Finally

        End Try

    End Function

    Public Sub DisposeAdapter()
        Log("Fermerture du bus", MessageType.Debug)
        If adapter IsNot Nothing Then
            adapter.endExclusive()
            adapter.freePort()
        End If
    End Sub



    Sub getDatas(Optional ByVal pParms As String = "")








        ' Creation des variables correspondant à chaque type de container
        Dim owd_enum As Enumeration
        Dim owd As com.dalsemi.onewire.container.OneWireContainer
        Dim adc As com.dalsemi.onewire.container.ADContainer
        Dim tc As com.dalsemi.onewire.container.TemperatureContainer
        Dim sw As com.dalsemi.onewire.container.SwitchContainer
        Dim cnt As com.dalsemi.onewire.container.OneWireContainer1D

        ' Creation de la variable associé à l'adaptateur 1Wire.
        ' Pour l'instant, c'est l'adaptateur par défaut.



        Dim deviceFound As Boolean
        Dim OneWireAddress As String
        Dim state As Object
        Dim numOfADChannels
        Dim channelCount As Integer
        Log("Lecture des données : ", MessageType.Debug)
        ' Enumération de tous les devices du bus
        owd_enum = adapter.getAllDeviceContainers()
        deviceFound = 0

        While owd_enum.hasMoreElements()
            Try
                Dim strLog = ""
                owd = owd_enum.nextElement
                OneWireAddress = owd.getAddressAsString

                strLog = "***** [" + OneWireAddress + "] "
                ' Gestion des composants 1Wire du type Entrée Analogique (DS2438 par exemple)
                If TypeOf owd Is com.dalsemi.onewire.container.ADContainer Then
                    deviceFound = 1
                    strLog += "(DS2438) : "
                    ' cast the OneWireContainer to ADContainer
                    adc = DirectCast(owd, com.dalsemi.onewire.container.ADContainer)
                    channelCount = 0
                    ' read the device
                    state = adc.readDevice

                    ' Lecture du nombre de voies de mesure
                    numOfADChannels = adc.getNumberADChannels

                    ' On parcours chaque cannal et on lit les mesures
                    For channelCount = 0 To (numOfADChannels - 1)

                        adc.doADConvert(channelCount, state)
                        strLog += "Mesure" + " : " + adc.getADVoltage(channelCount - 1, state).ToString + " "
                        ' On affecte la valeur lue au device HomeSeer
                        strLog += SetDeviceValue(OneWireAddress, channelCount, adc.getADVoltage(channelCount, state)) + " ** "

                    Next
                    '   Log(strLog, MessageType.Debug)
                End If

                ' Gestion des composants 1Wire du type Température (DS18B20 par exemple)
                If TypeOf owd Is com.dalsemi.onewire.container.TemperatureContainer Then
                    deviceFound = 1
                    strLog += "(DS18B20) : "
                    ' cast the OneWireContainer to TemperatureContainer
                    tc = DirectCast(owd, com.dalsemi.onewire.container.TemperatureContainer)

                    ' read the device
                    state = tc.readDevice
                    tc.doTemperatureConvert(state)

                    'state = tc.readDevice
                    'tc.doTemperatureConvert(state)

                    ' On affecte la valeur lue au device HomeSeer
                    strLog += "Mesure" + " : " + tc.getTemperature(state).ToString + " "
                    strLog += SetDeviceValue(OneWireAddress, -1, tc.getTemperature(state)) + " "
                    '    Log(strLog, MessageType.Debug)
                End If

                ' Gestion des composants 1Wire du type Switch (DS2405 par exemple)
                If TypeOf owd Is com.dalsemi.onewire.container.SwitchContainer Then
                    deviceFound = 1

                    sw = DirectCast(owd, com.dalsemi.onewire.container.SwitchContainer)

                    ' Lecture des informations du composant
                    state = sw.readDevice

                    numOfADChannels = sw.getNumberChannels(state)

                    ' On parcours les différents switch pour récupérer leur état
                    For channelCount = 0 To (numOfADChannels - 1)
                        ' On affecte la valeur lue au device HomeSeer
                        SetDeviceValue(OneWireAddress, channelCount, sw.getLatchState(channelCount, state))
                    Next
                End If

                ' Gestion des composants 1Wire du type Compteur (DS2423)
                If TypeOf owd Is com.dalsemi.onewire.container.OneWireContainer1D Then
                    deviceFound = 1
                    strLog += "(DS2423) : "
                    cnt = DirectCast(owd, com.dalsemi.onewire.container.OneWireContainer1D)

                    ' On affecte la valeur lue au device HomeSeer
                    strLog += SetDeviceValue(OneWireAddress, -1, cnt.readCounter(14)) + " "
                    '     Log(strLog, MessageType.Debug)
                End If
                Log(strLog, MessageType.Debug)
            Catch ex As Exception
                Log("****************************** un module 1WIRE est en  anomalie ******************************", MessageType.Error_)

                Console.WriteLine("******************************")
                Console.WriteLine("est en anomalie")
                Console.WriteLine(ex.Message)
                Console.WriteLine("******************************")
            End Try

        End While

        Log("Fin Lecture des valeur 1-Wire", MessageType.Debug)


        If (deviceFound = 0) Then
            Log("--> No 1-Wire devices found!", MessageType.Error_)
        End If

    End Sub

    Public Function SetDeviceValue(ByVal Address As String, ByVal Channel As Integer, ByVal value As Double) As String
        Dim dblCoef As Double
        Dim dblOffset As Double
        Dim sFormat As String
        Dim sDevice As String
        Dim ValeurSeuil As Double
        Dim coeffArrondi As Double = 0

        Dim canal As String = Channel.ToString()
        If Channel = -1 Then canal = "NA"
        Address = Address & "/" & canal
        'Console.WriteLine("température" + " : " + value.ToString)

        coeffArrondi = (hs.GetINISetting(Address, "COEFFICIENT_ARRONDI", "0,01", INIFILE))

        dblCoef = CDbl(hs.GetINISetting(Address, "COEFFICIENT", "1", INIFILE))
        dblOffset = CDbl(hs.GetINISetting(Address, "OFFSET", "0", INIFILE))
        sFormat = hs.GetINISetting(Address, "FORMAT", "##.##", INIFILE)
        ValeurSeuil = CDbl((hs.GetINISetting(Address, "SEUIL", "0", INIFILE)))

        Dim iDevice As Integer = hs.DeviceExistsAddress(Address, True)
        Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(iDevice)

        If (dv IsNot Nothing) Then
            Dim sOldString = dv.devString(Nothing)
            If (sOldString.Equals("")) Then sOldString = "20"
            If (ValeurSeuil > 0) Then
                If (value > ValeurSeuil) Then
                    value = 0
                End If
            End If

            If (sFormat = "ONOFF") Then
                If (value = 0) Then
                    hs.SetDeviceValue(Address, 0)
                Else
                    hs.SetDeviceValue(Address, 100)
                End If
            Else

                Dim sNewString As String = Format(value * dblCoef + dblOffset, sFormat) + ""
                'hs.SetDeviceString(iDevice, Math.Round((value * dblCoef + dblOffset), 2), True)

                If (coeffArrondi <> 0) Then
                    coeffArrondi = 1 / coeffArrondi
                    sNewString = CStr(CDbl(Math.Round(sNewString * coeffArrondi)) / coeffArrondi)
                End If

                'hs.SetDeviceValueByRef(ref, temp, False)

                ' Dim OldVal As Double = CDbl(sOldString)



                '       Dim code As String = dv.Code(Nothing).Substring(1, dv.Code(Nothing).Length() - 1)
                '      Dim refDelta As Integer = hs.DeviceExistsCode("Y" & code)

                '     If (refDelta > -1) Then
                'Dim dvDelta As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(refDelta)

                ' Dim timeDelta As Double = Now.Subtract(dvDelta.Last_Change(Nothing)).TotalMinutes
                '  Dim delta As Double = (CDbl(Math.Round(sNewString * 2)) / 2) - CDbl(dvDelta.devString(Nothing))
                ' hs.WriteLog("Read1Wire", dvDelta.devString(Nothing) & " - " & CStr(CDbl(Math.Round(sNewString * 2)) / 2))

                '   hs.SetDeviceValueByRef(dvDelta.Ref(Nothing), Math.Round((delta * 60 / timeDelta), 2), False)
                ' End If

                '      If (sNewString <> sOldString) Then
                hs.SetDeviceString(iDevice, sNewString, False)
                hs.SetDeviceValueByRef(iDevice, CDbl(sNewString), True)
                ' If (debug) Then

                ' End If
                hs.SetDeviceLastChange(iDevice, Date.Now)
                hs.SaveEventsDevices()
                ' End If
                If dv.Code(Nothing) = "" Then
                    Return ""
                Else
                    Return " || " & dv.Code(Nothing) & " <-- " + hs.DeviceString(dv.Ref(Nothing))
                End If
            End If
        Else
            Return "" & Address & " n'est pas lié à un module HS."
        End If
    End Function


    ' Permet de modifier l'état d'un switch 1-wire (On ou Off)
    Sub OneWireSwitch(ByVal Command As String)
        Dim owd As com.dalsemi.onewire.container.OneWireContainer
        Dim sw As com.dalsemi.onewire.container.SwitchContainer

        ' Creation de la variable associé à l'adaptateur 1Wire.
        ' Pour l'instant, c'est l'adaptateur par défaut.
        Dim adapter As com.dalsemi.onewire.adapter.DSPortAdapter
        Dim OneWireAddress As String
        Dim state As Object
        Dim TabCmd() As String = Command.Split("=")
        Dim TabAddress() As String = TabCmd(0).Split("/")
        Dim Address As String = TabAddress(0)
        Dim Channel As Integer = TabAddress(1)
        Dim bOnOff As Boolean = TabCmd(1)

        ' Ouverture de l'adaptateur et verrouillage du bus
        adapter = com.dalsemi.onewire.OneWireAccessProvider.getDefaultAdapter
        adapter.beginExclusive(True)
        adapter.setSearchAllDevices()
        adapter.targetAllFamilies()
        adapter.setSpeed(adapter.SPEED_REGULAR)

        owd = adapter.getDeviceContainer(Address)

        ' Gestion des composants 1Wire du type Switch (DS2405 par exemple)
        If TypeOf owd Is com.dalsemi.onewire.container.SwitchContainer Then
            sw = DirectCast(owd, com.dalsemi.onewire.container.SwitchContainer)

            ' Lecture des informations du composant
            state = sw.readDevice

            'sw.SetLatchState(Channel, state, False, tabState)
            sw.setLatchState(Channel, bOnOff, False, state)

            sw.writeDevice(state)
        End If

        adapter.endExclusive()
        adapter.freePort()
    End Sub

#If TARGET = "exe" Then
End Module
#Else

#End If