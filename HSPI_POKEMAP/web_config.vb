Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes

Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean


    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)
    End Sub

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String

        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)

        If (data <> "") Then

            If (parts("id") Is Nothing) Then
                HandleRequest(data)
            Else
                logutils.SaveLogLevel(parts)
            End If



        End If



        Return MyBase.postBackProc(page, data, user, userRights)
    End Function

    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        Dim stb As New StringBuilder
        Dim instancetext As String = ""
        Try

            Me.reset()

            utils.CurrentPage = Me

            stb.Append("<br>" & logutils.getLogHTMLConfig(Me.PageName))
            Me.AddBody(stb.ToString)
            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function





End Class

