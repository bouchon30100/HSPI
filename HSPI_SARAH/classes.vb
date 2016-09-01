Imports System.Web.UI.WebControls

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


    <Serializable()>
    Friend Class MyAction

        Private mvarUID As Integer
        Public Property ActionUID As Integer
            Get
                Return mvarUID
            End Get
            Set(value As Integer)
                mvarUID = value
            End Set
        End Property

        Private mvarConfigured As Boolean
        Public ReadOnly Property Configured As Boolean
            Get
                Return mvarConfigured
            End Get
        End Property

        Private mvarClientName As String
        Public Property ClientName As String
            Get
                Return mvarClientName
            End Get
            Set(value As String)
                mvarClientName = value
            End Set
        End Property

        Private mvarTexte As String
        Public Property Texte As String
            Get
                Return mvarTexte
            End Get
            Set(value As String)
                mvarTexte = value
            End Set
        End Property

    End Class

    <Serializable()>
    Public Class SarahClient
        Private _name As String
        Public Property Name As String
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        Private _ip As String
        Public Property IP As String
            Get
                Return _ip
            End Get
            Set(value As String)
                _ip = value
            End Set
        End Property

        Private _port As String
        Public Property PORT As String
            Get
                Return _port
            End Get
            Set(value As String)
                _port = value
            End Set
        End Property

        Public Sub New(ByVal nameIni As String)
            Me.Name = nameIni
            Me.IP = hs.GetINISetting(Name, "IP", "127.0.0.1", INIFILE)
            Me.PORT = hs.GetINISetting(Name, PORT, "8888", INIFILE)
        End Sub

        Public Sub Save()
            hs.SaveINISetting(Me.Name, "IP", IP, INIFILE)
            hs.SaveINISetting(Me.Name, "PORT", PORT, INIFILE)
        End Sub

    End Class



End Module
