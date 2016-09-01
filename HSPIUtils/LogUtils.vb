
Imports System.Collections.Specialized
Imports System.Text
Imports HomeSeerAPI
Imports Scheduler

Public Class LogUtils

    Public hs As IHSApplication
    Public Log_Level As New Dictionary(Of Integer, String)
    Dim LOGFILE As String
    Private INIFILE As String = "HSPI_%plugin%.ini"
    Private IFACE_NAME As String = ""
    Private gEXEPath As String = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)

    Public Sub New(hsInstance As IHSApplication, namePlugin As String)
        hs = hsInstance
        IFACE_NAME = namePlugin
        LOGFILE = IFACE_NAME & ".log"
        INIFILE = INIFILE.Replace("%plugin%", IFACE_NAME)
        Log_Level.Add(0, "Aucun")
        Log_Level.Add(1, "Normal")
        Log_Level.Add(2, "Debug")
    End Sub



    Public Enum LogLevel
        none = 0
        Normal = 1
        Debug = 2
    End Enum

    Public Enum MessageType
        Normal = 1
        Debug = 2
        Error_ = 0
    End Enum

    Sub New()
    End Sub


    Public Function getLogHTMLConfig(pageName As String) As String
        Dim stb1 As New StringBuilder()

        stb1.Append(" LogLevel : ")
        Dim selectLogLevel As New clsJQuery.jqDropList("LogLevel", pageName, False)
        Dim selectedLogLevel As Boolean = False

        '  Dim Level = {"location", "location2", "value", "string"}

        For Each Str As String In Log_Level.Keys
            selectedLogLevel = hs.GetINISetting("param", "LogLevel", 0, INIFILE) = Str
            selectLogLevel.AddItem(Log_Level(Str), Str, selectedLogLevel)
        Next

        stb1.Append(selectLogLevel.Build)
        Return stb1.ToString()
    End Function

    Public Sub SaveLogLevel(parts As NameValueCollection)
        If (parts("id") = "LogLevel") Then
            hs.SaveINISetting("param", "logLevel", parts("LogLevel"), INIFILE)
        End If
    End Sub

    Public Sub Log(ByVal Message As String, Optional ByVal messType As MessageType = MessageType.Normal)

        Dim debugLevel As Integer = hs.GetINISetting("param", "logLevel", 0, INIFILE)

        If (messType <= debugLevel) Then
            If IO.Directory.Exists(gEXEPath & "\Logs") Then
                IO.File.AppendAllText(gEXEPath & "\Logs\" & LOGFILE, Now.ToString & " ~ " & Message & vbCrLf)
            End If

            Select Case messType
                Case MessageType.Normal
                    hs.WriteLog(IFACE_NAME, Message)

                Case MessageType.Debug
                    hs.WriteLogEx(IFACE_NAME, Message, COLOR_ORANGE)

                Case MessageType.Error_
                    hs.WriteLogEx(IFACE_NAME, Message, COLOR_RED)
            End Select
        End If
    End Sub




End Class
