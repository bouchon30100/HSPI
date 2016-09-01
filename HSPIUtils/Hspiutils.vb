﻿Imports System.Collections.Specialized
Imports System.IO
Imports System.Runtime.Serialization.Formatters
Imports HomeSeerAPI
Imports HSPIUtils.LogUtils

Public Class Outils

    Public IFACE_NAME As String = ""
    Public callback As HomeSeerAPI.IAppCallbackAPI
    Public hs As HomeSeerAPI.IHSApplication
    Public Instance As String = ""
    Public InterfaceVersion As Integer
    Public bShutDown As Boolean = False
    Public gEXEPath As String = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)
    Public INIFILE As String = "HSPI_%plugin%.ini"
    Public CurrentPage As Object


    Private logutil As LogUtils

    Public Sub New(namePlugin As String, Optional instanceName As String = "")
        IFACE_NAME = namePlugin
        Instance = instanceName
        INIFILE = INIFILE.Replace("%plugin%", IFACE_NAME)
        logutil = New LogUtils(hs, IFACE_NAME)
    End Sub



    Public Shared Function StringIsNullOrEmpty(ByRef s As String) As Boolean
        If String.IsNullOrEmpty(s) Then Return True
        Return String.IsNullOrEmpty(s.Trim)
    End Function

    Public Structure pair
        Dim name As String
        Dim value As String
    End Structure



    Sub PEDAdd(ByRef PED As clsPlugExtraData, ByVal PEDName As String, ByVal PEDValue As Object)
        Dim ByteObject() As Byte = Nothing
        If PED Is Nothing Then PED = New clsPlugExtraData
        SerializeObject(PEDValue, ByteObject)
        If Not PED.AddNamed(PEDName, ByteObject) Then
            PED.RemoveNamed(PEDName)
            PED.AddNamed(PEDName, ByteObject)
        End If
    End Sub

    Function PEDGet(ByRef PED As clsPlugExtraData, ByVal PEDName As String) As Object
        Dim ByteObject() As Byte
        Dim ReturnValue As New Object
        ByteObject = PED.GetNamed(PEDName)
        If ByteObject Is Nothing Then Return Nothing
        DeSerializeObject(ByteObject, ReturnValue)
        Return ReturnValue
    End Function

    Public Function SerializeObject(ByRef ObjIn As Object, ByRef bteOut() As Byte) As Boolean
        If ObjIn Is Nothing Then Return False
        Dim str As New MemoryStream
        Dim sf As New Binary.BinaryFormatter

        Try
            sf.Serialize(str, ObjIn)
            ReDim bteOut(CInt(str.Length - 1))
            bteOut = str.ToArray
            Return True
        Catch ex As Exception
            logutil.Log(LogLevel.Debug, IFACE_NAME & " Error: Serializing object " & ObjIn.ToString & " :" & ex.Message)
            Return False
        End Try

    End Function

    Public Function DeSerializeObject(ByRef bteIn() As Byte, ByRef ObjOut As Object) As Boolean
        ' Almost immediately there is a test to see if ObjOut is NOTHING.  The reason for this
        '   when the ObjOut is suppose to be where the deserialized object is stored, is that 
        '   I could find no way to test to see if the deserialized object and the variable to 
        '   hold it was of the same type.  If you try to get the type of a null object, you get
        '   only a null reference exception!  If I do not test the object type beforehand and 
        '   there is a difference, then the InvalidCastException is thrown back in the CALLING
        '   procedure, not here, because the cast is made when the ByRef object is cast when this
        '   procedure returns, not earlier.  In order to prevent a cast exception in the calling
        '   procedure that may or may not be handled, I made it so that you have to at least 
        '   provide an initialized ObjOut when you call this - ObjOut is set to nothing after it 
        '   is typed.
        If bteIn Is Nothing Then Return False
        If bteIn.Length < 1 Then Return False
        If ObjOut Is Nothing Then Return False
        Dim str As MemoryStream
        Dim sf As New Binary.BinaryFormatter
        Dim ObjTest As Object
        Dim TType As System.Type
        Dim OType As System.Type
        Try
            OType = ObjOut.GetType
            ObjOut = Nothing
            str = New MemoryStream(bteIn)
            ObjTest = sf.Deserialize(str)
            If ObjTest Is Nothing Then Return False
            TType = ObjTest.GetType
            'If Not TType.Equals(OType) Then Return False
            ObjOut = ObjTest
            If ObjOut Is Nothing Then Return False
            Return True
        Catch exIC As InvalidCastException
            Return False
        Catch ex As Exception
            logutil.Log(LogLevel.Debug, IFACE_NAME & " Error: DeSerializing object: " & ex.Message)
            Return False
        End Try

    End Function



    Public Sub RegisterCallback(ByRef frm As Object)
        ' call back into HS and get a reference to the HomeSeer ActiveX interface
        ' this can be used make calls back into HS like hs.SetDeviceValue, etc.
        ' The callback object is a different interface reserved for plug-ins.
        callback = frm
        hs = frm.GetHSIface
        If hs Is Nothing Then
            MsgBox("Unable to access HS interface", MsgBoxStyle.Critical)
        Else
            logutil.Log("Register callback completed", LogLevel.Debug)
            InterfaceVersion = hs.InterfaceVersion
        End If
    End Sub

    Public Sub RegisterConfigWebPage(ByVal link As String, Optional linktext As String = "", Optional page_title As String = "")
        Try
            hs.RegisterPage(link, IFACE_NAME, Instance)
            If linktext = "" Then linktext = link
            linktext = linktext.Replace("_", " ")
            If page_title = "" Then page_title = linktext
            Dim wpd As New HomeSeerAPI.WebPageDesc
            wpd.plugInName = IFACE_NAME
            wpd.link = link
            wpd.linktext = linktext & Instance
            wpd.page_title = page_title & Instance
            callback.RegisterConfigLink(wpd)
        Catch ex As Exception
            logutil.Log(LogLevel.Debug, "Error - Registering Web Links: " & ex.Message)
        End Try
    End Sub

    Public Sub RegisterWebPage(ByVal link As String, Optional linktext As String = "", Optional page_title As String = "")
        Try
            hs.RegisterPage(link, IFACE_NAME, Instance)
            If linktext = "" Then linktext = link
            linktext = linktext.Replace("_", " ")
            If page_title = "" Then page_title = linktext
            Dim wpd As New HomeSeerAPI.WebPageDesc
            wpd.plugInName = IFACE_NAME
            wpd.link = link
            wpd.linktext = linktext
            wpd.plugInInstance = Instance
            wpd.page_title = page_title
            callback.RegisterLink(wpd)
        Catch ex As Exception
            logutil.Log("Error - Registering Web Links: " & ex.Message, MessageType.Error_)
        End Try
    End Sub


End Class
