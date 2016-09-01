

Module ModuleSaison


    Function GetSaison() As String

        Try
            Dim saisons = GetListSAISONS()
            For Each saison In saisons

                Dim début = hs.GetINISetting("PARAMS", "debut", "", INIFILE_DEVICE.Replace("####", saison))
                Dim fin = hs.GetINISetting("PARAMS", "fin", "", INIFILE_DEVICE.Replace("####", saison))
                Dim f = Convert.ToDateTime(fin & "/" & Year(Now())).DayOfYear()
                Dim d = Convert.ToDateTime(début & "/" & Year(Now())).DayOfYear()
                Dim n = Now().DayOfYear

                If (d <= f) Then
                    If (d <= n) And (n <= f) Then
                        Return saison
                    End If
                Else
                    If (d <= n) Or (n <= f) Then
                        Return saison
                    End If
                End If


            Next


        Catch ex As Exception
            hs.WriteLogEx("*******", ex.Message, "#FF0000")
        End Try
        Return ""
    End Function
    Public Function GetListSAISONS() As List(Of String)

        Dim stat As New List(Of String)
        stat.AddRange(hs.GetINISetting("PARAMS", "SAISONS", "", INIFILE).Split(","))
        If stat.First() = "" Then
            stat.Remove("")
        End If
        Return stat

    End Function

    Public Sub AddSaison(Saison)
        Dim inifileSaison = INIFILE_DEVICE.Replace("####", Saison)
        Dim saisons = GetListSAISONS()
        saisons.Add(Saison)
        hs.SaveINISetting("PARAMS", "SAISONS", String.Join(",", saisons), INIFILE)

        hs.SaveINISetting("PARAMS", "debut", " ", inifileSaison)
        hs.SaveINISetting("PARAMS", "fin", " ", inifileSaison)

        For Each dev In hs.GetINISectionEx("ASSOCIATIONS", INIFILE)
            Dim device = dev.Split("=")(0)
            Dim inifileDevice = INIFILE_DEVICE.Replace("####", device)
            hs.SaveINISetting(Saison, "min", "18", inifileDevice)
            hs.SaveINISetting(Saison, "max", "22", inifileDevice)
        Next

    End Sub

    Public Sub deleteSaison(OldSaison)
        Dim saisons = GetListSAISONS()
        saisons.Remove(OldSaison)
        hs.SaveINISetting("PARAMS", "SAISONS", String.Join(",", saisons), INIFILE)
        For Each dev In hs.GetINISectionEx("ASSOCIATIONS", INIFILE)
            Dim device = dev.Split("=")(0)
            Dim inifileDevice = INIFILE_DEVICE.Replace("####", device)
            hs.ClearINISection(OldSaison, inifileDevice)

        Next
        Dim value = hs.GetINISetting("PARAMS", "VALUE", "99", "HSPI_" & IFACE_NAME & "/" & OldSaison & ".ini")
        Dim dvRef = hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE)
        hs.DeviceVSP_ClearStatus(dvRef, value)
        FileIO.FileSystem.DeleteFile(hs.GetAppPath & "/Config/" & "HSPI_" & IFACE_NAME & "/" & OldSaison & ".ini")

    End Sub

    Public Sub updateSaisonName(Oldsaison, NewSaison)
        Dim saisons = GetListSAISONS()
        Dim index = saisons.IndexOf(Oldsaison)
        saisons(index) = NewSaison
        hs.SaveINISetting("PARAMS", "SAISONS", String.Join(",", saisons), INIFILE)
        FileIO.FileSystem.RenameFile(hs.GetAppPath & "/Config/" & "HSPI_" & IFACE_NAME & "/" & Oldsaison & ".ini", NewSaison & ".ini")
        Dim dvRef = hs.GetINISetting("PARAMS", "MODULE_SAISON", "0", INIFILE)
        Dim value1 = hs.GetINISetting("PARAMS", "VALUE", "99", "HSPI_" & IFACE_NAME & "/" & NewSaison & ".ini")
        hs.DeviceVSP_ClearStatus(dvRef, value1)
        CreateSingleStatusPAIR(hs.GetDeviceByRef(dvRef), value1, NewSaison)

        For Each dev In hs.GetINISectionEx("ASSOCIATIONS", INIFILE)
            Dim device = dev.Split("=")(0)
            Dim inifileDevice = INIFILE_DEVICE.Replace("####", device)

            For Each element In hs.GetINISectionEx(Oldsaison, inifileDevice)
                Dim key = element.Split("=")(0)
                Dim value = element.Split("=")(1)
                hs.SaveINISetting(NewSaison, key, value, inifileDevice)

            Next
            hs.ClearINISection(Oldsaison, inifileDevice)
        Next

    End Sub


    Public Sub UpdateModule(refDev As String)



        '  Dim ImageDevice As String = hs.GetINISetting("DEVICE", "IMAGE_DEVICE", "", FileIni)

        Dim dv As Scheduler.Classes.DeviceClass

        If (refDev > 0) Then
            dv = hs.GetDeviceByRef(refDev)
            hs.DeviceVSP_ClearAll(refDev, True)
            hs.DeviceVGP_ClearAll(refDev, True)
            For Each saison In GetListSAISONS()
                Dim inifileSaison = INIFILE_DEVICE.Replace("####", saison)
                Dim valueStatus = hs.GetINISetting("PARAMS", "VALUE", 99, inifileSaison)
                Dim IMGStatus = hs.GetINISetting("PARAMS", "IMG", "", inifileSaison)

                CreateSingleStatusPAIR(dv, valueStatus, saison)
                CreateSingleStatusGraphiquePAIR(dv, valueStatus, IMGStatus)
            Next

            Dim inifileSaison1 = INIFILE_DEVICE.Replace("####", GetSaison)
            Dim valueStatus1 = hs.GetINISetting("PARAMS", "VALUE", 99, inifileSaison1)
            hs.SetDeviceValueByRef(dv.Ref(Nothing), valueStatus1, True)

            hs.SaveEventsDevices()
        Else
            Log("Le Device " & refDev & " n'a pas été trouvé dans Homeseer. Vérifier la configuration des SAISONS", MessageType.Error_)
        End If




    End Sub






End Module
