Imports System.Text
Imports Scheduler

Public Module LogUtils

    Public Log_Level As New Dictionary(Of Integer, String)
    Dim LOGFILE As String = IFACE_NAME & ".log"

    Enum LogLevel
        none = 0
        Normal = 1
        Debug = 2
    End Enum

    Enum MessageType
        Normal = 1
        Debug = 2
        Error_ = 3
    End Enum

    Sub New()
        Log_Level.Add(0, "Aucun")
        Log_Level.Add(1, "Normal")
        Log_Level.Add(2, "Debug")
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

    Public Sub Log(ByVal Message As String, Optional ByVal Log_Level As LogLevel = LogLevel.Normal)
        Select Case hs.GetINISetting("param", "logLevel", 0, INIFILE)
            Case LogLevel.Debug
                If Log_Level = LogLevel.Debug Then
                    If (hs.GetINISetting("param", "logLevel", 0, INIFILE)) Then

                    End If
                    hs.WriteLogEx(IFACE_NAME, Message, COLOR_ORANGE)
                    If IO.Directory.Exists(gEXEPath & "\Debug Logs") Then
                        IO.File.AppendAllText(gEXEPath & "\Debug Logs\" & LOGFILE, Now.ToString & " ~ " & Message & vbCrLf)
                    ElseIf IO.Directory.Exists(gEXEPath & "\Logs") Then
                        IO.File.AppendAllText(gEXEPath & "\Logs\" & LOGFILE, Now.ToString & " ~ " & Message & vbCrLf)
                    Else
                        IO.File.AppendAllText(gEXEPath & "\" & LOGFILE, Now.ToString & " ~ " & Message & vbCrLf)
                    End If
                Else
                    hs.WriteLog(IFACE_NAME, Message)
                End If
            Case LogLevel.Normal
                If Log_Level = LogLevel.Normal Then
                    hs.WriteLog(IFACE_NAME, Message)
                End If

            Case Else
                hs.WriteLogEx(IFACE_NAME, Message, COLOR_RED)
        End Select
    End Sub

End Module


