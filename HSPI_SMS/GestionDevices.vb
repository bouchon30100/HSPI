Imports HomeSeerAPI
Imports HomeSeerAPI.VSVGPairs
Imports Scheduler
Imports Scheduler.Classes

Module GestionDevices

    Friend Function CreateDevice(name, Optional location = "", Optional location2 = "", Optional TypeString = "", Optional housecode = "", Optional adresse = "") As DeviceClass

        Dim StatusSupport As Boolean = False
        Dim CanDim As Boolean = False
        Dim Nb_Status As Integer = 2
        Dim Nb_StatusGraph As Integer = 2

        Dim STATUS_ONLY As Boolean = False
        Dim NO_LOG As Boolean = True
        Dim HIDDEN As Boolean = False
        Dim INCLUDE_POWERFAIL As Boolean = True
        Dim SHOW_VALUES As Boolean = True
        Dim AUTO_VOICE_COMMAND As Boolean = False
        Dim NO_STATUS_TRIGGER As Boolean = False
        Dim VOICE_COMMAND_CONFIRM As Boolean = False



        Dim dv As Scheduler.Classes.DeviceClass

        Dim refDev = hs.NewDeviceRef(name)
        dv = hs.GetDeviceByRef(refDev)
        ' If (refDev > 0) Then
        '   dv = hs.GetDeviceByRef(refDev)
        ' Else
        '   dv = hs.GetDeviceByRef(hs.NewDeviceRef(name))
        ' End If


        ' dv.Interface(hs) = IFACE_NAME
        dv.Location2(hs) = location2
        dv.Location(hs) = location
        '   dv.Image(hs) = ImageDevice
        '   dv.ImageLarge(hs) = ImageDevice
        dv.Device_Type_String(hs) = TypeString
        '  dv.Can_Dim(hs) = CanDim
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
        hs.SaveEventsDevices()

        Return dv

    End Function

    Public Function getDeviceActions(ref As String, Action As String, pagename As String, Optional selectedValue As Integer = 99999)
        Dim SelectorAction As New clsJQuery.jqDropList("Actions_" & Action & "_" & ref, pagename, False)
        SelectorAction.id = "Actions_" & Action & "_" & ref



        Dim pairs As VSPair() = hs.DeviceVSP_GetAllStatus(ref)

        SelectorAction.AddItem("", "99999", True)
        For Each pair In pairs

            Dim StrStatus = hs.DeviceVSP_GetStatus(ref, pair.Value, ePairStatusControl.Status)
            ' Dim StrCurrentStatus = hs.DeviceVSP_GetStatus(ref, pair.Value, ePairStatusControl.Status)
            Dim selected As Boolean = pair.Value = selectedValue
            SelectorAction.AddItem(StrStatus, pair.Value, selected)
        Next

        Return SelectorAction.Build
    End Function

    Friend Sub CreateStatusPAIR(device As Integer, Value As String, status As String, type_status As String, Optional StatusOrControl As ePairStatusControl = ePairStatusControl.Both)

        Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(device)
        Dim Pair As HomeSeerAPI.VSPair

        If (UCase(type_status) = "SINGLE") Then
            'Création d'un PAIR SINGLE
            Pair = New VSPair(StatusOrControl)
            Pair.PairType = HomeSeerAPI.VSVGPairType.SingleValue
            Pair.Value = Convert.ToDouble(Value)
            Pair.Status = status
            Pair.Render = HomeSeerAPI.Enums.CAPIControlType.Button
            hs.DeviceVSP_AddPair(dv.Ref(hs), Pair)
        Else
            'Création d'un PAIR RANGER
            Pair = New HomeSeerAPI.VSPair(HomeSeerAPI.ePairStatusControl.Both)
            Pair.PairType = HomeSeerAPI.VSVGPairType.Range
            Pair.RangeStart = Convert.ToDouble(Value.Split("-")(0))
            Pair.RangeEnd = Convert.ToDouble(Value.Split("-")(1))
            Pair.RangeStatusPrefix = ""
            Pair.RangeStatusSuffix = ""
            ' Pair.ValueOffset = Convert.ToDouble(Offset)
            Pair.Render = HomeSeerAPI.Enums.CAPIControlType.ValuesRangeSlider
            hs.DeviceVSP_AddPair(dv.Ref(hs), Pair)
        End If

    End Sub

    Friend Sub CreateStatusGraphiquePAIR(device As Integer, Value As String, Image As String, type_status As String)

        Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(device)
        Dim VgPair As HomeSeerAPI.VGPair

        If (UCase(type_status) = "SINGLE") Then
            'Création d'un PAIR SINGLE

            VgPair = New HomeSeerAPI.VGPair()
            VgPair.PairType = HomeSeerAPI.VSVGPairType.SingleValue
            VgPair.Set_Value = Convert.ToDouble(Value)
            VgPair.Graphic = Image
            hs.DeviceVGP_AddPair(dv.Ref(hs), VgPair)
        Else
            VgPair = New HomeSeerAPI.VGPair()
            VgPair.PairType = HomeSeerAPI.VSVGPairType.Range
            VgPair.RangeStart = Convert.ToDouble(Value.Split("-")(0))
            VgPair.RangeEnd = Convert.ToDouble(Value.Split("-")(1))
            VgPair.Graphic = Image
            hs.DeviceVGP_AddPair(dv.Ref(hs), VgPair)
        End If
    End Sub


    Private Sub DeleteStatusPAIR(device As Integer)
        hs.DeviceVSP_ClearAll(device, True)
    End Sub

    Private Sub DeleteStatusGraphiquePAIR(device As Integer)
        hs.DeviceVGP_ClearAll(device, True)
    End Sub

    Public Sub DeleteDevice(device As Integer)

        Dim dv As Scheduler.Classes.DeviceClass = hs.GetDeviceByRef(device)
        dv.AssociatedDevice_ClearAll(hs)
        '  Dim intDevs = dv.AssociatedDevices_List(hs).Split(",")
        '  For Each dev As Integer In intDevs
        'hs.DeleteDevice(dev)
        '    Next
        hs.DeleteDevice(device)

    End Sub


    Private Function VSVGPairType_int_to_text(integ As Integer) As String
        Select Case integ
            Case HomeSeerAPI.VSVGPairType.SingleValue
                VSVGPairType_int_to_text = "SINGLE"
            Case Else 'HomeSeerAPI.VSVGPairType.Range
                VSVGPairType_int_to_text = "RANGE"
        End Select
    End Function

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

    Public Sub createRelationShip(Ref1 As Integer, Ref2 As Integer, relation As HomeSeerAPI.Enums.eRelationship)
        Dim dv1 As DeviceClass = hs.GetDeviceByRef(Ref1)
        Dim dv2 As DeviceClass = hs.GetDeviceByRef(Ref2)

        Select Case relation
            Case HomeSeerAPI.Enums.eRelationship.Child
                dv1.Relationship(hs) = relation
                dv1.AssociatedDevice_Add(hs, dv2.Ref(Nothing))
                dv2.Relationship(hs) = HomeSeerAPI.Enums.eRelationship.Parent_Root
                dv2.AssociatedDevice_Add(hs, dv1.Ref(Nothing))

            Case HomeSeerAPI.Enums.eRelationship.Parent_Root
                dv1.Relationship(hs) = relation
                dv1.AssociatedDevice_Add(hs, dv2.Ref(Nothing))
                ' dv2.Relationship(hs) = HomeSeerAPI.Enums.eRelationship.Child
                'dv2.AssociatedDevice_Add(hs, dv1.Ref(Nothing))
            Case HomeSeerAPI.Enums.eRelationship.Indeterminate
                dv1.Relationship(hs) = relation
                dv1.AssociatedDevice_Add(hs, dv2.Ref(Nothing))
        End Select
    End Sub

    Public Sub DetacheRelationShip(ref1 As Integer, FromRef2 As Integer)
        Dim dv1 As DeviceClass = hs.GetDeviceByRef(ref1)
        Dim FromDv2 As DeviceClass = hs.GetDeviceByRef(FromRef2)
        dv1.AssociatedDevice_Remove(hs, FromRef2)
        FromDv2.AssociatedDevice_Remove(hs, ref1)

        If (dv1.AssociatedDevices_Count(hs) = 0) Then
            dv1.Relationship(hs) = HomeSeerAPI.Enums.eRelationship.Not_Set
        End If
        If (FromDv2.AssociatedDevices_Count(hs) = 0) Then
            FromDv2.Relationship(hs) = HomeSeerAPI.Enums.eRelationship.Not_Set
        End If



    End Sub

End Module




