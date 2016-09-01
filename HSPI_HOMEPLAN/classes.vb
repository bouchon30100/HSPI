Imports System.Text
Imports Scheduler
Imports Scheduler.Classes
Imports Scheduler.clsJQuery

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


    Public Class Maison

        Public Etages As New Dictionary(Of String, Etage)
        ' Public Exterieur As Etage
        Public Alltypes As New List(Of String)

        Public Sub ContruireMaison(styleFilter As List(Of String))
            Etages.Clear()
            Dim listeNiveaux = hs.GetINISetting("POSITION", "ETAGES", "", utils.INIFILE).Split(",")
            For Each nomEtage_ As String In listeNiveaux
                Dim niveau As Integer = Array.IndexOf(listeNiveaux, nomEtage_)

                Etages.Add(nomEtage_, New Etage(nomEtage_, niveau))
                Dim listePieces = hs.GetINISetting("POSITION", nomEtage_, "", utils.INIFILE).Split(",")
                For Each nomPiece_ As String In listePieces
                    Etages(nomEtage_).Pieces.Add(nomPiece_, New Piece(nomPiece_, nomEtage_))
                Next
            Next



            '  Dim _nomEtage As String = "EXTERIEUR"

            'Try
            '    If (Etages(_nomEtage) Is Nothing) Then

            '        Etages.Add(_nomEtage, New Etage(_nomEtage, niveau))
            '    End If
            'Catch ex As Exception
            '    Etages.Add(_nomEtage, New Etage(_nomEtage, niveau))
            'End Try



            Dim en As Object
            Dim dv As DeviceClass
            Try
                en = hs.GetDeviceEnumerator
                Do While Not en.Finished

                    If (en.CountChanged) Then

                    End If
                    dv = en.GetNext
                    If dv IsNot Nothing Then


                        If Not (Alltypes.Contains(dv.Device_Type_String(Nothing))) Then
                            Alltypes.Add(dv.Device_Type_String(Nothing))
                        End If


                        If Not (dv.MISC_Check(Nothing, HomeSeerAPI.Enums.dvMISC.HIDDEN)) Then
                            If (styleFilter.Contains(dv.Device_Type_String(Nothing))) Then
                                Dim _nomEtage = dv.Location2(Nothing)
                                If (Etages.ContainsKey(_nomEtage)) Then
                                        If Not (_nomEtage = "") Then

                                            Dim _nomPiece As String = dv.Location(Nothing)
                                            If (Etages(_nomEtage).Pieces.ContainsKey(_nomPiece)) Then
                                                If Not (_nomPiece = "") Then
                                                    Etages(_nomEtage).Pieces(_nomPiece).AddModule(dv)

                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                                End If
                    End If
                Loop
                Alltypes.Sort()

                '   Etages = Etages.OrderBy(Of Integer)(Function(Etage) Etage.Value.Niveau)

            Catch ex As Exception
                hs.WriteLog("maison", "erreur : " + ex.Message)
                hs.WriteLog("maison", "erreur : " + ex.StackTrace)
            End Try

        End Sub



        Public Function getHtml(pageName As String) As String

            Dim pieceExterieur As Piece = Etages("EXTERIEUR").Pieces("EXTERIEUR")
            Dim tabExt As New HTMLTable("tableExterieur", 0, False, -1, 100, "", "", 0, -1, -1, pieceExterieur.getFondPiece())
            tabExt.addRow()
            Dim tempé As String = ""

            If (pieceExterieur.Température IsNot Nothing) Then
                tempé = pieceExterieur.Température.getHtml(pageName)

            End If
            tabExt.addCell(pieceExterieur.Nom + " :  " + tempé, "PieceTitre", 1, "")

            tabExt.addRow("ExterieurContainer", "", HTML_Align.CENTER)
            For Each m As HsModule In pieceExterieur._hsModules.Values
                tabExt.addCell(m.getHtml(pageName), "moduleContainer", 1)
            Next
            For Each p In Etages("EXTERIEUR").Pieces
                If Not (p.Value.Nom = "EXTERIEUR") Then
                    tabExt.addCell(p.Value.getHtml(pageName), "ExterieurContainer", 1)
                End If
            Next

            tabExt.addRow("ExterieurContainer", "", HTML_Align.CENTER)
            Dim tableint As New HTMLTable(1, False)

            Dim TableIntérieur As New HTMLTable("TableIntérieur", 1, False, -1, 95, "", "interieur")

            For Each e In Etages
                If Not e.Value.Nom = "EXTERIEUR" Then
                    e.Value.getHtmlEtage(pageName, TableIntérieur)
                    '   TableIntérieur.addRow()
                    '  TableIntérieur.addCell(e.Value.getHtml(pageName), "", 1, "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

                End If
            Next
            tabExt.addCell(TableIntérieur.GetHTML(), "InterieurContainer", 1, "", "")

            Return tabExt.GetHTML()
        End Function

    End Class

End Module

Public Class Etage


    Public Pieces As New Dictionary(Of String, Piece)
    Public Niveau As Integer
    Public Nom As String
    Public _nbPiecesInterieurs As Integer


    Public Sub New(_nomEtage As String, niveau As Integer)
        Me.Nom = _nomEtage
        Me.Niveau = niveau
    End Sub
    '
    ' Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
    ' Return Niveau.CompareTo(obj)
    '  End Function


    Public Sub getHtmlEtage(pageName As String, ByRef table As HTMLTable)

        table.addRow()



        For Each p In Pieces
            table.addCell(p.Value.getHtml(pageName), "PieceContainer", 1, p.Value.getFondPiece(), "", HTML_Align.CENTER, False, 0, 0, "width:" + Math.Round(100 / Pieces.Count).ToString() + "%;", HTML_VertAlign.TOP)
        Next

    End Sub



    Public Function compterNbPieceInterieurs() As Integer

        Dim nbPiecesInterieurs As String = 0
        For Each p In Pieces
            If (p.Value.Nom <> "EXTERIEUR") Then
                nbPiecesInterieurs += 1
            End If
        Next
        _nbPiecesInterieurs = nbPiecesInterieurs
        Return nbPiecesInterieurs
    End Function


End Class

Public Class Piece

    Public _hsModules As New Dictionary(Of String, HsModule)
    Public Nom As String = ""
    Public Etage As String = ""
    Public Température As HsModule


    Public Sub New(_nomPiece As String, _nomEtage As String)
        Me.Nom = _nomPiece
        Me.Etage = _nomEtage
    End Sub

    Friend Sub AddModule(dv As DeviceClass)

        If (dv.Device_Type_String(Nothing) = "TEMPERATURE") Then
            AddModuleTempérature(dv)
        Else
            _hsModules.Add(dv.Ref(Nothing), New HsModule(dv))
        End If

    End Sub

    Friend Sub AddModuleTempérature(dv As DeviceClass)
        Température = New HsModule(dv)
    End Sub

    Public Function getFondPiece() As String
        Dim urlFondPièce As String = ""
        If (Température IsNot Nothing) Then
            urlFondPièce = "../" + Replace(hs.DeviceVGP_GetGraphic(Température.dv.Ref(Nothing), Température.dv.devValue(Nothing)), "\", "/")
        End If
        Return urlFondPièce
    End Function

    Public Function getHtml(pageName As String) As String
        Dim tempé As String = ""
        If (Température IsNot Nothing) Then
            tempé = Température.getHtml(pageName)
        End If

        Dim stb As New StringBuilder
        Dim tabPiece As New HTMLTable(Nom, 0, False, 0, 100, "", "Piece", HTML_TableAlign.INHERIT, 0, -1)
        tabPiece.addRow()
        tabPiece.addCell(Nom & " :  " & tempé, "PieceTitre", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)
        tabPiece.addRow()
        If (_hsModules.Count = 0) Then
            tabPiece.addCell(" ", "moduleContainer", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)
        End If
        For Each m In _hsModules
            tabPiece.addCell(m.Value.getHtml(pageName), "moduleContainer", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)
        Next
        stb.Append(tabPiece.GetHTML())
        Return stb.ToString
    End Function
End Class

Public Class HsModule

    Public dv As DeviceClass

    Public Sub New(_dv As DeviceClass)
        dv = _dv

    End Sub

    Public Function getHtml(pageName As String)
        Dim stb As New StringBuilder
        stb.Append(clsJQuery.DivStart(dv.Ref(Nothing), "module", False, False, "", "", pageName).ToString)
        stb.Append(construireModuleHTML())
        stb.Append(clsJQuery.DivEnd())
        Return stb.ToString
    End Function



    Public Function construireModuleHTML() As String

        Dim tab As New HTMLTable("TAB_" + dv.Ref(Nothing).ToString, 0, True, 0, -1, "", "module", HTML_TableAlign.INHERIT, 0, -1)
        tab.addRow()


        Select Case hs.GetINISetting("POSITION", dv.Device_Type_String(Nothing), "VAL", INIFILE)
            Case "TEMP"
                Return dv.devValue(Nothing).ToString + "°"

            Case "CLIMATISEUR MASTER"
                tab.addCell(getImageStatus(dv.Ref(Nothing)), "moduleImage", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

            Case "IMG"
                tab.addCell(getImageStatus(dv.Ref(Nothing)), "moduleImage", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

            Case "VAL"
                tab.addCell(getValue(dv.Ref(Nothing), ""), "moduleValue", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

            Case "STR"
                tab.addCell(getString(dv.Ref(Nothing), ""), "moduleValue", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

            Case "SEL"
                tab.addCell(getSelector(dv.Ref(Nothing)), "moduleValue", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

            Case Else
                Return ""

        End Select

        'Select Case dv.Device_Type_String(Nothing)
        '    Case "TEMPERATURE"
        '        Return dv.devValue(Nothing).ToString + "°"

        '    Case "CLIMATISEUR MASTER"
        '        tab.addCell(getImageStatus(dv.Ref(Nothing)), "moduleImage", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

        '    Case "LUMIERE", "RADIATEUR"
        '        tab.addCell(getImageStatus(dv.Ref(Nothing)), "moduleImage", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)

        '    Case "ELECTRICITE", "LUMINOSITE"
        '        tab.addCell(getValue(dv.Ref(Nothing), ""), "moduleValue", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)
        '    Case Else
        '        Return ""

        'End Select

        tab.addRow()
        tab.addCell(dv.Name(Nothing), "moduleName", 1, "", "", HTML_Align.CENTER, False, 0, 0, "", HTML_VertAlign.MIDDLE)
        Return tab.GetHTML()
    End Function








    Shared Function getString(refDevice As Integer, unité As String) As String
        Return "<div id=""DIV_STR_" & refDevice & """ style=""font-size: xx-large; font-weight: bold;"">" & hs.DeviceString(refDevice) & " " & unité & "</div>"
    End Function

    Shared Function getValue(refDevice As Integer, unité As String) As String
        Return "<div id=""DIV_VAL_" & refDevice & """ style=""font-size: xx-large; font-weight: bold;"">" & hs.DeviceValue(refDevice) & " " & unité & "</div>"
    End Function

    Shared Taille_Img As Integer = 80

    Public Shared Function getImageStatus(refDevice As Integer, Optional ByVal value As Integer = -1) As String

        Dim countStatus As Integer = hs.DeviceVSP_CountStatus(refDevice)
        Dim countStatusControl As Integer = hs.DeviceVSP_CountControl(refDevice)
        Dim dv As DeviceClass = hs.GetDeviceByRef(refDevice)
        If (dv.AssociatedDevices_Count(hs) > 0) Then
            countStatusControl = 3
        End If

        Dim CountStatusGraphic As Integer = hs.DeviceVGP_Count(refDevice)
        Dim str As String = ""
        Dim suffixAffich_Value As String = "_" & value.ToString
        If value = -1 Then
            value = hs.DeviceValue(refDevice)
            suffixAffich_Value = "_AFFICHE"
        End If

        Dim Image As String = ""
        If (CountStatusGraphic > 0) Then
            Image = "../" + hs.DeviceVGP_GetGraphic(refDevice, value)
        Else
            Image = "../" + hs.GetDeviceByRef(refDevice).Image(Nothing)
        End If
        Dim nameBTN As String = "BTN_" & refDevice & suffixAffich_Value
        If web_config.AffichageSecondaire = True Then
            If web_config.divPrincipal = "" Then
                If (web_config.divSecondaire = "") Then
                    nameBTN = "aBTN_" & refDevice & suffixAffich_Value
                Else
                    nameBTN = "cBTN_" & refDevice & suffixAffich_Value
                End If
            Else
                nameBTN = "bBTN_" & refDevice & suffixAffich_Value
            End If
        End If


        Select Case countStatusControl
            Case 0

                'pas de statusControl on affiche juste l'image
                str = "<img Class=""moduleImage"" align=""absmiddle"" id=""IMG_" & refDevice & """ name=""IMG_" & refDevice & """ src=""" & Image & """'>"
            Case 1
                str = "<img class=""moduleImage"" align=""absmiddle"" id=""IMG_" & refDevice & """ name=""IMG_" & refDevice & """ src=""" & Image & """'>"

            Case 2
                Dim button As jqButton = New jqButton("BTN_" & refDevice, "test", IFACE_NAME, False)
                button.imagePathNormal = Image
                button.className = "moduleImage"
                button.id = "BTN_" & refDevice & "_TOGGLE"
                str = button.Build
            Case Else
                Dim button As jqButton = New jqButton(nameBTN, "test", IFACE_NAME, False)
                button.imagePathNormal = Image
                button.className = "moduleImage"

                str = button.Build
        End Select


        'Dim Image As String = ""
        'If (value = -1) Then
        '    Image = "../" + hs.DeviceVGP_GetGraphic(refDevice, hs.DeviceValue(refDevice))
        'Else
        '    Image = "../" + hs.DeviceVGP_GetGraphic(refDevice, value)
        'End If
        'Dim strImg = ""

        'If hs.DeviceVSP_CountStatus(refDevice) < 2 Then
        '    If hs.DeviceVSP_CountStatus(refDevice) > 0 Then

        '        Dim button As jqButton = New jqButton("BTN_" & refDevice & "_AFFICHE", "test", IFACE_NAME, False)
        '        button.imagePathNormal = Image
        '        button.className = "moduleImage"
        '        strImg &= button.Build()
        '    Else
        '        strImg &= "<img class=""moduleImage"" align=""absmiddle"" id=""IMG_" & refDevice & """ name=""IMG_" & refDevice & """ src=""" & Image & """'>"

        '    End If
        '    'Si pas d'action possible alors je met une image simple



        'Else
        '    'sinon je mets un  bouton
        '    Dim button As jqButton = New jqButton("BTN_" & refDevice, "test", IFACE_NAME, False)
        '    button.imagePathNormal = Image
        '    button.className = "moduleImage"
        '    If hs.DeviceVSP_CountStatus(refDevice) = 2 Then
        '        'si je n'aiu que 2 action possble alors je toggle
        '        button.id = "BTN_" & refDevice & "_TOGGLE"
        '    Else
        '        If (value = -1) Then
        '            'sinon j'affiche un nouveau panel avec toutes les actions possibles car c'est le value du device
        '            If web_config.climGeneral Then
        '                button.id = "aBTN_" & refDevice & "_AFFICHE"
        '            Else
        '                button.id = "BTN_" & refDevice & "_AFFICHE"
        '            End If
        '        Else
        '            'je met un id avec la valeur a effectuer sur Homeseer
        '            button.id = "BTN_" & refDevice & "_" & value
        '        End If

        '    End If
        '    strImg &= button.Build()
        'End If
        ' 

        str &= "</div>"
        Return str
    End Function

    Shared Function getSelector(refDevice As Integer) As String

        '    If hs.DeviceVSP_GetAllStatus(refDevice)(0).PairType = HomeSeerAPI.VSVGPairs.VSVGPairType.Range Then
        '    Str = getSelector(refDevice, hs.DeviceVSP_GetAllStatus(refDevice)(0).RangeStart, hs.DeviceVSP_GetAllStatus(refDevice)(0).RangeEnd)

        '    End If
        Dim rangeStart As Integer = hs.DeviceVSP_GetAllStatus(refDevice)(0).RangeStart
        Dim rangeEnd As Integer = hs.DeviceVSP_GetAllStatus(refDevice)(0).RangeEnd
        Dim stb As New StringBuilder()
        Dim sel As New jqDropList("SEL_" & refDevice, IFACE_NAME, False)
        For i = rangeStart To rangeEnd
            sel.AddItem(i, i, i = hs.DeviceValue(refDevice))
        Next

        stb.Append(sel.Build)

        Return stb.ToString
    End Function

End Class

