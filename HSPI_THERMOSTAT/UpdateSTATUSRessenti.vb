#If TARGET = "exe" Then
'Imports HomeSeerAPI

Module UpdateSTATUSRessenti
#Else
#End If

    Sub Main(Optional ByVal pParms As String = "")


        Dim device = hs.DeviceExistsCode("Z99")
        Dim saison As String = hs.DeviceVSP_GetStatus(device, hs.DeviceValue(device), 1)

        Dim inifileSaison As String = "Controles/" + saison + ".ini"

        Dim i
        For i = 1 To CInt(hs.GetINISetting("SETTINGS", "NBSTATUSGRAPH", "", "CONTROLES/RESSENTI.INI"))

            Dim modules() As String = hs.GetINISetting("STATUS_GRAPH" + CStr(i), "MODULES", "", "CONTROLES/RESSENTI.INI").Split("|")

            For Each hc As String In modules

                Dim refDev = hs.DeviceExistsCode(hc)
                Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(refDev)

                DeleteStatusGraphiquePAIR(refDev)
                DeleteStatusPAIR(refDev)
                Dim ValueMin As Double = -20.0

                For Each item As String In hs.GetINISectionEx("STATUS_GRAPH" + CStr(i), "CONTROLES/RESSENTI.INI")
                    If (item.Split("=")(0) <> "MODULES") Then
                        If (item.Split("=")(0) <> "SANS") Then
                            Dim valuemax As Double
                            Dim img As String = item.Split("=")(1)
                            Dim status As String = hs.GetINISetting("STATUS", item.Split("=")(0), "", "CONTROLES/RESSENTI.INI")
                            Dim Valuestr As String = hs.GetINISetting(dv.Location(Nothing), item.Split("=")(0), "", inifileSaison)
                            If Valuestr <> "" Then
                                valuemax = CDbl(Valuestr)


                            Else
                                valuemax = 50.0
                            End If
                            CreateStatusGraphiquePAIR(refDev, ValueMin, valuemax + 0.9, img)
                            CreateStatusPAIR(refDev, ValueMin, valuemax + 0.9, status)
                            ValueMin = valuemax + 1
                        End If
                    End If
                Next
                CreateStatusPAIR(refDev, 51, 100, "ERROR")
            Next
        Next
    End Sub

    Private Sub CreateStatusGraphiquePAIR(device As Integer, valueMin As Double, valueMax As Double, img As String)

        Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(device)
        Dim VgPair As HomeSeerAPI.VGPair

        VgPair = New HomeSeerAPI.VGPair()
        VgPair.PairType = HomeSeerAPI.VSVGPairType.Range
        VgPair.RangeStart = valueMin
        VgPair.RangeEnd = valueMax
        VgPair.Graphic = img

        hs.DeviceVGP_AddPair(dv.Ref(hs), VgPair)

    End Sub

    Private Sub CreateStatusPAIR(device As Integer, valueMin As Double, valueMax As Double, status As String)



        Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(device)
        Dim Pair As HomeSeerAPI.VSPair

        Pair = New HomeSeerAPI.VSPair(HomeSeerAPI.ePairStatusControl.Status)
        Pair.PairType = HomeSeerAPI.VSVGPairType.Range
        Pair.RangeStart = valueMin
        Pair.RangeEnd = valueMax
        Pair.RangeStatusPrefix = status
        Pair.RangeStatusSuffix = ""
        Pair.IncludeValues = False
        'Pair.Render = HomeSeerAPI.Enums.CAPIControlType.ValuesRangeSlider
        Pair.RangeStatusDecimals = 1
        hs.DeviceVSP_AddPair(dv.Ref(hs), Pair)



    End Sub

    Private Sub DeleteStatusGraphiquePAIR(device As Integer)
        hs.DeviceVGP_ClearAll(device, True)
    End Sub

    Private Sub DeleteStatusPAIR(device As Integer)
        hs.DeviceVSP_ClearAll(device, True)
    End Sub

#If TARGET = "exe" Then
End Module
#Else
#End If



