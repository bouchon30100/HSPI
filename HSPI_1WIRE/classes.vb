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

    <Serializable()> _
    Public Class action
        Inherits hsCollection
        Public Sub New()
            MyBase.New()
        End Sub
        Protected Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub
    End Class

    <Serializable()> _
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


    <Serializable()> _
    Public Class Module1Wire
        Private _OneWireAdress As String = ""



        Public Property OneWireAdress As String
            Get
                Return _OneWireAdress
            End Get
            Set(value As String)
                _OneWireAdress = value
            End Set
        End Property

        Private _OneWireType As String = ""
        Public Property OneWireType As String
            Get
                Return _OneWireType
            End Get
            Set(value As String)
                _OneWireType = value
            End Set
        End Property

        Private _Channel As String = ""
        Public Property Channel As String
            Get
                Return _Channel
            End Get
            Set(value As String)
                _Channel = value
            End Set
        End Property

        Private _hsType As String = ""
        Public Property hsType As String
            Get
                Return _hsType
            End Get
            Set(value As String)
                _hsType = value
            End Set
        End Property

        Private _PIECE As String = ""
        Public Property PIECE As String
            Get
                Return _PIECE
            End Get
            Set(value As String)
                _PIECE = value
            End Set
        End Property

        Private _ETAGE As String = ""
        Public Property ETAGE As String
            Get
                Return _ETAGE
            End Get
            Set(value As String)
                _ETAGE = value
            End Set
        End Property

        Private _REF As String = ""
        Public Property REF As String
            Get
                Return _REF
            End Get
            Set(value As String)
                _REF = value
            End Set
        End Property

        Private _HC As String = ""
        Public Property HC As String
            Get
                Return _HC
            End Get
            Set(value As String)
                _HC = value
            End Set
        End Property

        Private _MC As String = ""
        Public Property MC As String
            Get
                Return _MC
            End Get
            Set(value As String)
                _MC = value
            End Set
        End Property

        Private _NAME As String = ""
        Public Property NAME As String
            Get
                Return _NAME
            End Get
            Set(value As String)
                _NAME = value
            End Set
        End Property

        Property coeffArrondi As Double
        Property dblCoef As String
        Property sFormat As String
        Friend dblOffset As String
        Property ValeurSeuil As String


        'Public Sub New(ByVal _OneWireAdresse As String, ByVal _OneWireType As String)
        '    Me.OneWireAdress = _OneWireAdresse
        '    Me.OneWireType = _OneWireType
        'End Sub

        Public Sub Save()
            '   hs.SaveINISetting(Me.Name, "IP", IP, INIFILE)
            '   hs.SaveINISetting(Me.Name, "PORT", PORT, INIFILE)
        End Sub

        Sub SearchModuleHomeseer()
            Dim dv As Scheduler.Classes.DeviceClass

            Dim refDev = hs.DeviceExistsAddress(OneWireAdress & "/" & Channel, True)

            If (refDev > 0) Then

                dv = hs.GetDeviceByRef(refDev)
            
            Else : Return
            End If
            Me.ETAGE = dv.Location2(hs)
            Me.PIECE = dv.Location(hs)
            Me.REF = refDev
            Me.hsType = dv.Device_Type_String(hs)
            If (dv.Code(hs) <> "") Then
                Me.HC = dv.Code(hs).Substring(0, 1)
                Me.MC = dv.Code(hs).Remove(0, 1)
            End If
            Me.NAME = dv.Name(hs)
            Me.coeffArrondi = CDbl(hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "COEFFICIENT_ARRONDI", "0,01", INIFILE))
            Me.dblCoef = CDbl(hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "COEFFICIENT", "1", INIFILE))
            Me.dblOffset = CDbl(hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "OFFSET", "0", INIFILE))
            Me.sFormat = hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "FORMAT", "##.##", INIFILE)
            ' Me.ValeurSeuil = CDbl((hs.GetINISetting(Me.OneWireAdress & "/" & m.Channel, "SEUIL", "0", INIFILE)))

        End Sub

        Sub SearchModuleHomeseer(ref As Integer)
            Dim dv As Scheduler.Classes.DeviceClass

            ' Dim refDev = hs.DeviceExistsAddress(OneWireAdress & "/" & Channel, True)

            If (ref > 0) Then

                dv = hs.GetDeviceByRef(ref)

            Else : Return
            End If

            Me.OneWireAdress = dv.Address(Nothing).Split("/")(0)
            Me.Channel = dv.Address(Nothing).Split("/")(1).Split("-")(0)
            Me.ETAGE = dv.Location2(Nothing)
            Me.PIECE = dv.Location(Nothing)
            Me.REF = ref
            Me.hsType = dv.Device_Type_String(Nothing)
            If (dv.Code(Nothing) <> "") Then
                Me.HC = dv.Code(Nothing).Substring(0, 1)
                Me.MC = dv.Code(Nothing).Remove(0, 1)
            End If
            Me.NAME = dv.Name(Nothing)
            Me.dblCoef = CDbl(hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "COEFFICIENT", "1", INIFILE))
            Me.coeffArrondi = CDbl(hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "COEFFICIENT_ARRONDI", "0,01", INIFILE))
            Me.dblOffset = CDbl(hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "OFFSET", "0", INIFILE))
            Me.sFormat = hs.GetINISetting(Me.OneWireAdress & "/" & Me.Channel, "FORMAT", "##.##", INIFILE)
            ' Me.ValeurSeuil = CDbl((hs.GetINISetting(Me.OneWireAdress & "/" & m.Channel, "SEUIL", "0", INIFILE)))

        End Sub

        Sub clear()

            Me.HC = ""
            Me.hsType = ""
            Me.NAME = ""
            Me.MC = ""
            Me.REF = ""
            Me.ETAGE = ""
            Me.PIECE = ""

        End Sub

    End Class





End Module
