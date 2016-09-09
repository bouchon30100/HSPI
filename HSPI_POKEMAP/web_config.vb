Imports System.Text
Imports System.Web
Imports Scheduler
Imports HomeSeerAPI
Imports Scheduler.Classes
Imports System.Threading

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
                datas.Add(data)
                Dim t As New TaskTraitementData(data)
                Dim Thread1 As New Thread(AddressOf t.traite)
                Thread1.Start() ' Démarrer le nouveau thread.
                ' Thread1.Join() ' Attendre la fin du thread 1.

            Else
                logutils.SaveLogLevel(parts)
            End If



        End If



        Return MyBase.postBackProc(page, data, user, userRights)
    End Function

    Class TaskTraitementData
        Friend data As String

        Sub New(d As String)
            data = d
        End Sub

        Sub traite()
            HandleRequest(data)
        End Sub
    End Class


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

