Imports System.Net
Imports System.Text
Imports System.Threading
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

    Public Function sendSMS(dest As String, number As String, text As String)
        Dim IP = hs.GetINISetting("param", "IP", "", INIFILE)
        If IP = "" Then Throw New Exception("L'adresse IP de SMS GATEWAY n'est pas paramétrée !")
        Dim PORT = hs.GetINISetting("param", "PORT", "", INIFILE)
        If PORT = "" Then Throw New Exception("Le port de SMS GATEWAY n'est pas paramétré !")
        If dest <> "" And dest IsNot Nothing Then
            number = FindNumber(dest)
        Else
            dest = FindDestinataire(number)
        End If
        Dim result = hs.GetURL(IP, "/sendsms?phone=" + number + "&text=" + text + "&password=", True, PORT)
        Return result
    End Function

    Function FindNumber(dest As String)
        For Each telephone In hs.GetINISectionEx("TELEPHONES", INIFILE)
            If telephone.Split("=")(0) = dest Then
                Return telephone.Split("=")(1)
            End If
        Next
        Return ""
    End Function

    Function FindDestinataire(number As String)
        For Each telephone In hs.GetINISectionEx("TELEPHONES", INIFILE)
            If telephone.Split("=")(1) = number Then
                Return telephone.Split("=")(0)
            End If
        Next
        Return ""
    End Function



    <Serializable()>
    Public Class hsCollection
        Inherits Dictionary(Of String, Object)
        Dim KeyIndex As New Collection

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub

        Public Overloads Sub Add(value As Object, Key As String)
            If (Key.Contains("_")) Then
                Key = Key.Replace("_" & Key.Split("_")(3), "")
            End If

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

        Default Public Overloads Property Item(ByVal index As Integer) As Object
            Get
                Return MyBase.Item(KeyIndex(index))
            End Get
            Set(ByVal value As Object)
                MyBase.Item(KeyIndex(index)) = value
            End Set
        End Property

        Default Public Overloads Property Item(ByVal Key As String) As Object
            Get
                On Error Resume Next
                Return MyBase.Item(Key)
            End Get
            Set(ByVal value As Object)
                If Not MyBase.ContainsKey(Key) Then
                    Add(value, Key)
                Else
                    MyBase.Item(Key) = value
                End If
            End Set
        End Property
    End Class

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

    Public Class Smiley
        Public Name As String
        Public CodeSMS As String
        Public codeASCII As String
        Public img As String

        Public Sub New(name As String, code As String, codeAscii As Object, img As String)
            Me.Name = name
            Me.CodeSMS = code
            Me.codeASCII = codeAscii
            Me.img = img
        End Sub
    End Class

    Public Class Smileys
        Inherits CollectionBase

        Public Sub New()
            Me.addSmiley("Clin_d_oeil", "", "î„…", "")
            Me.addSmiley("Horloge", "", "î€¨", "")
            Me.addSmiley("Ange", "", "î" & ChrW(129) & "Ž", "")
            Me.addSmiley("Enervé", "", "î" & ChrW(144) & "–", "")
            Me.addSmiley("Coeur", "", "î€¢", "")
            Me.addSmiley("Sourire", "", "î" & ChrW(144) & ChrW(8221), "")
            Me.addSmiley("Maison", "", "î€¶", "")
            Me.addSmiley("Poule", "", "î" & ChrW(8221) & "®", "")
            Me.addSmiley("Lumière", "", "î„" & ChrW(143) & "", "")
            Me.addSmiley("Sapin", "", "î€³", "")
            Me.addSmiley("Soleil", "", "î" & ChrW(129) & "Š", "")
            Me.addSmiley("Orage", "", "î„½", "")
            Me.addSmiley("Pluie", "", "î" & ChrW(129) & "‹", "")
            Me.addSmiley("Nuage", "", "î" & ChrW(129) & "‰", "")
            '   Me.addSmiley("neige", "", "î" & ChrW(129) & "ˆ", "")
            Me.addSmiley("Neige", "❄", "â„", "")
            Me.addSmiley("Radio", "", "î„¨", "")
            Me.addSmiley("Télé", "", "î„ª", "")
            Me.addSmiley("Chaud", "", "î„£", "")
            Me.addSmiley("Oeuf", "", "î…‡", "")
        End Sub


        Public Sub addSmiley(s As Smiley)
            List.Add(s)
        End Sub

        Public Sub addSmiley(Name As String, code As String, codeAscii As String, img As String)
            Dim s As New Smiley(Name, code, codeAscii, img)
            addSmiley(s)
        End Sub

        Public Function FindByName(NameToFind As String) As Smiley
            For Each s As Smiley In List
                If (s.Name = NameToFind) Then
                    Return s
                End If

            Next
            Return Nothing
        End Function

        Public Function FindByCodeAscii(CodeAscii As String) As Smiley
            For Each s As Smiley In List
                If (s.codeASCII = CodeAscii) Then
                    Return s
                End If
            Next
            Return Nothing
        End Function

        Public Function FindByCodeSMS(CodeSMS As String) As Smiley
            For Each s As Smiley In List
                If (s.CodeSMS = CodeSMS) Then
                    Return s
                End If
            Next
            Return Nothing
        End Function

        Public Function CountSmileys() As Integer
            Return List.Count
        End Function
    End Class


End Module
