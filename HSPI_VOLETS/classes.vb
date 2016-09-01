Imports HomeSeerAPI
Imports HomeSeerAPI.VSVGPairs
Imports Scheduler.Classes
Module classes

    Public Function CreateVoletParent(dvCOMMANDE As DeviceClass) As Integer

        Dim dv As DeviceClass = CreateDevice("Volet", dvCOMMANDE.Location(Nothing), dvCOMMANDE.Location2(Nothing), "VOLET")
        CreateStatusPAIR(dv.Ref(Nothing), "900", "FERME", "SINGLE", ePairStatusControl.Status)
        CreateStatusGraphiquePAIR(dv.Ref(Nothing), "900", "/HSPI_VOLETS/Volet_0.png", "SINGLE")
        CreateStatusPAIR(dv.Ref(Nothing), "950", "PARTIEL", "SINGLE", ePairStatusControl.Status)
        CreateStatusGraphiquePAIR(dv.Ref(Nothing), "950", "/HSPI_VOLETS/Volet_50.png", "SINGLE")
        CreateStatusPAIR(dv.Ref(Nothing), "1000", "OUVERT", "SINGLE", ePairStatusControl.Status)
        CreateStatusGraphiquePAIR(dv.Ref(Nothing), "1000", "/HSPI_VOLETS/Volet_100.png", "SINGLE")

        Dim pairs As VSPair() = hs.DeviceVSP_GetAllStatus(dvCOMMANDE.Ref(Nothing))
        For Each pair In pairs
            Dim StrStatus = hs.DeviceVSP_GetStatus(dvCOMMANDE.Ref(Nothing), pair.Value, ePairStatusControl.Status)
            ' Dim StrCurrentStatus = hs.DeviceVSP_GetStatus(ref, pair.Value, ePairStatusControl.Status)
            CreateStatusPAIR(dv.Ref(Nothing), pair.Value, StrStatus, "SINGLE", ePairStatusControl.Both)
            CreateStatusGraphiquePAIR(dv.Ref(Nothing), pair.Value, hs.DeviceVGP_GetGraphic(dvCOMMANDE.Ref(Nothing), pair.Value), "SINGLE")
        Next

        createRelationShip(dv.Ref(Nothing), dvCOMMANDE.Ref(Nothing), Enums.eRelationship.Indeterminate)

        Return dv.Ref(Nothing)
    End Function

    Public Sub deleteVOLET(refCOMMANDE)
        Dim dv As DeviceClass = hs.GetDeviceByRef(refCOMMANDE)
        Dim refVOLET As Integer = findParent(hs.GetINISetting("GROUPS", "GENERAL", "", INIFILE), refCOMMANDE)
        Dim dvModule As DeviceClass = hs.GetDeviceByRef(refVOLET)
        DetacheRelationShip(refVOLET, hs.GetINISetting("GROUPS", "GENERAL", "", INIFILE))
        DetacheRelationShip(refVOLET, refCOMMANDE)
        DeleteDevice(refVOLET)


    End Sub
    Friend Function findParent(refRoot As Integer, refToFind As Integer) As Integer

        Dim dvRoot As DeviceClass = hs.GetDeviceByRef(refRoot)
        For Each ref In dvRoot.AssociatedDevices(hs)
            If ref = refToFind Then
                Return refRoot
            Else
                Dim refFound = findParent(ref, refToFind)
                If refFound > 0 Then
                    Return refFound
                End If
            End If

        Next
        Return 0

    End Function

End Module
