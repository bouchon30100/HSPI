Imports System.Data.SQLite
Imports System.IO
Imports System.Text
Imports System.Web

Imports Scheduler
Imports Scheduler.Classes


Public Class web_config
    Inherits clsPageBuilder
    Dim TimerEnabled As Boolean
    Dim listLocations(2) As List(Of String)
    Dim js As clsJQuery.jqMultiSelect

    ' Dim listeClient As List(Of SarahClient) = New List(Of SarahClient)

    Public Sub New(ByVal pagename As String)
        MyBase.New(pagename)

    End Sub

    Dim myTrans As SQLiteTransaction

    Public Overrides Function postBackProc(page As String, data As String, user As String, userRights As Integer) As String
        Log("page post: " & data, MessageType.Debug)
        Dim parts As Collections.Specialized.NameValueCollection
        parts = HttpUtility.ParseQueryString(data)


        If (parts("id") IsNot Nothing) Then

            Select Case parts("id")

                Case "LogLevel"
                    hs.SaveINISetting("param", "logLevel", parts("LogLevel"), INIFILE)

                Case "transfert"
                    Try
                        Dim fichier As String = "C:\Program Files (x86)\HomeSeer HS3\html\JaspMobile\datas.csv"

                        If My.Computer.FileSystem.FileExists(fichier) Then
                            My.Computer.FileSystem.DeleteFile(fichier)
                        End If


                        Dim di As DirectoryInfo = New System.IO.DirectoryInfo(hs.GetAppPath + "\data\Jasper") '
                        '   For Each diMois As System.IO.DirectoryInfo In di.GetDirectories()

                        'For Each diJour As System.IO.DirectoryInfo In diMois.GetDirectories

                        Dim ds As System.Collections.Generic.List(Of Donnée) = New System.Collections.Generic.List(Of Donnée)()
                        Dim dsFinale As System.Collections.Generic.List(Of Donnée) = New System.Collections.Generic.List(Of Donnée)()
                        Dim files As List(Of String) = New List(Of String)

                        ' files = Directory.GetFileSystemEntries(diJour.FullName)
                        files.AddRange(Directory.GetFileSystemEntries(di.FullName))
                        files.Sort()
                        '   Dim j As String = diJour.Name 'Environment.CurrentDirectory.Split("\")(Environment.CurrentDirectory.Split("\").Length - 1)
                        '   Dim m As String = diMois.Name 'Environment.CurrentDirectory.Split("\")(Environment.CurrentDirectory.Split("\").Length - 2)
                        '   Dim a As String = Environment.CurrentDirectory.Split("\")(Environment.CurrentDirectory.Split("\").Length - 1)
                        '     Console.Out.WriteLine(Environment.CurrentDirectory.Split("\")(Environment.CurrentDirectory.Split("\").Length - 1))
                        '   Dim dte As String = j + "/" + m + "/" + a

                        CON.ConnectionString = "Data Source=" & hs.GetAppPath() & "/data/test.db"
                        CON.Open()

                        Dim d As Donnée
                        Dim currentType As String = ""
                        Dim cmd = New SQLiteCommand()

                        For Each f As String In files
                            Log(f, MessageType.Debug)
                            If f.EndsWith(".csv") Then

                                If Not (f.Split("_")(0).Split("\")(f.Split("_")(0).Split("\").Count - 1) = "") Then
                                    If (f.Split("_")(0).Split("\")(f.Split("_")(0).Split("\").Count - 1) <> currentType) Then
                                        Dim tpe = f.Split("_")(0).Split("\")(f.Split("_")(0).Split("\").Count - 1)
                                        Dim listeTables = GetListTables(CON)
                                        If Not (listeTables.Contains(tpe)) Then
                                            CreateTable(tpe)
                                            Log("Création de la table " & tpe, MessageType.Normal)
                                        End If

                                        ds = (From m In ds Order By m.Device, m.Dte).ToList()
                                        Dim i = 0
                                        Dim oldD As Donnée
                                        myTrans = CON.BeginTransaction()
                                        For Each don In ds
                                            Dim ref = hs.DeviceExistsCode(don.Device)
                                            Dim dat As New m_Data(ref, don, oldD)
                                            oldD = don

                                            Try
                                                Dim strSQL As String = "INSERT INTO " & dat.type & " VALUES (@dt,@ref,@value,@oldValue, @delta, @String, @adress,'')"
                                                cmd.CommandText = strSQL
                                                cmd.Transaction = myTrans
                                                cmd.Parameters.AddWithValue("@dt", dat.DT)
                                                cmd.Parameters.AddWithValue("@ref", dat.ref)
                                                cmd.Parameters.AddWithValue("@value", dat.Value)
                                                cmd.Parameters.AddWithValue("@oldValue", dat.oldValue)
                                                cmd.Parameters.AddWithValue("@delta", dat.Delta)
                                                cmd.Parameters.AddWithValue("@String", dat.Str)
                                                cmd.Parameters.AddWithValue("@adress", dat.Adresse)
                                                cmd.ExecuteNonQuery()

                                            Catch ex As Exception
                                                Log(ex.Message, MessageType.Error_)
                                            End Try
                                            i = i + 1
                                            Dim rest = 0
                                            Math.DivRem(i, 100, rest)
                                            If (rest = 0) Then Console.WriteLine(i)
                                        Next
                                        myTrans.Commit()
                                        ds = New System.Collections.Generic.List(Of Donnée)()


                                    End If

                                    '   Console.Out.WriteLine(f.Split("\")(f.Split("\").Length - 1))
                                    '  File.Copy(f, "old_" + f.Split("\")(f.Split("\").Length - 1), True)
                                    Dim LigneFichier() As String = File.ReadAllLines(f, Encoding.Default)
                                    Dim first As Boolean = True
                                    For Each ligne As String In LigneFichier

                                        If first = False Then
                                            Try
                                                d = New Donnée(ligne)
                                                ds.Add(d)
                                            Catch ex As Exception
                                                Log("error", MessageType.Error_)
                                            End Try

                                            '  Dim TabDevices As System.Collections.Generic.List(Of String) = New System.Collections.Generic.List(Of String)()
                                            '  TabDevices.AddRange(Devices.Split(";"))


                                            ' If (TabDevices.Contains(d.Device)) Then
                                            'hs.writelog("test",date_début + " "+d.Dte)


                                            ' End If
                                        Else : first = False
                                        End If
                                    Next
                                    currentType = f.Split("_")(0).Split("\")(f.Split("_")(0).Split("\").Count - 1)

                                End If
                            End If

                        Next
                        cmd.Dispose()

                        Console.WriteLine("Both records are written to database.")
                    Catch e As Exception
                        myTrans.Commit()
                        '        myTrans.Rollback()
                        hs.WriteLogEx("UpdateGraph", e.Message, "#FF00FF")
                    Finally
                        CloseDatabase()
                    End Try


                    '   LogInCSVHighCharts(ds)
                    '  Next
                    '   Next



            End Select
        Else

        End If
        Return MyBase.postBackProc(page, data, user, userRights)
    End Function


    Public Function GetPagePlugin(ByVal pageName As String, ByVal user As String, ByVal userRights As Integer, ByVal queryString As String) As String
        Dim stb As New StringBuilder
        Me.AddTitleBar(pageName & " Configuration", user, False, "", False, False, False, False)

        Dim instancetext As String = ""
        Try

            Me.reset()

            CurrentPage = Me
            Dim refresh As Boolean = True

            ' handle any queries like mode=something
            Dim parts As Collections.Specialized.NameValueCollection = Nothing
            If (queryString <> "") Then
                parts = HttpUtility.ParseQueryString(queryString)

            End If


            If Instance <> "" Then instancetext = " - " & Instance


            Me.AddHeader(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", True, False, True))
            stb.Append(hs.GetPageHeader(pageName, IFACE_NAME & " Configuration", "", "", False, True, False, True))
            stb.Append(getLogHTMLConfig(pageName))
            Dim bt As New clsJQuery.jqButton("transfert", "transfert", pageName, False)
            stb.Append(bt.Build)
            Me.AddBody(stb.ToString)



            '  Me.AddFooter(hs.GetPageFooter())

            ' return the full page
            Return Me.BuildPage()
        Catch ex As Exception
            'WriteMon("Error", "Building page: " & ex.Message)
            Return "error - " & Err.Description
        End Try
    End Function


    Public Function getListModuleHS() As SortedList(Of String, DeviceClass)
        Dim list As New SortedList(Of String, DeviceClass)
        '    list(0) = New List(Of String)
        '    list(1) = New List(Of String)
        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            '    Dim i = 0
            Do While Not en.Finished
                dv = en.GetNext

                If dv IsNot Nothing Then
                    '        i += 1

                    Dim str = dv.Location2(Nothing) & " " & dv.Location(Nothing) & " " & dv.Name(Nothing) & "_" & dv.Ref(Nothing)
                    list.Add(str, dv)
                End If
            Loop
            '  Console.WriteLine(i)
        Catch ex As Exception
            Log(ex.Message, MessageType.Error_)
        End Try
        Return list
    End Function

    Public Function getListTypes() As List(Of String)
        Dim list As New List(Of String)
        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            Do While Not en.Finished
                dv = en.GetNext
                If dv IsNot Nothing Then
                    If Not list.Contains(dv.Device_Type_String(Nothing)) Then
                        list.Add(dv.Device_Type_String(Nothing))
                    End If
                End If
            Loop
            list.Sort()

        Catch ex As Exception
        End Try
        Return list
    End Function

    Public Function getListLocations() As List(Of String)()
        Dim list(1) As List(Of String)
        list(0) = New List(Of String)
        list(1) = New List(Of String)
        Dim en As Object
        Dim dv As DeviceClass

        Try
            en = hs.GetDeviceEnumerator
            Do While Not en.Finished
                dv = en.GetNext
                If dv IsNot Nothing Then
                    If Not list(0).Contains(dv.Location(hs)) Then
                        list(0).Add(dv.Location(hs))
                    End If
                    If Not list(1).Contains(dv.Location2(hs)) Then
                        list(1).Add(dv.Location2(hs))
                    End If
                End If
            Loop
            list(0).Sort()
            list(1).Sort()
        Catch ex As Exception
        End Try
        Return list
    End Function



    Public Function getModuleHSHTMLSelector(name As String, pageName As String, valueSelected As String, filtre As String()) As String
        Dim stb1 As New StringBuilder()

        Dim selectModuleHS As New clsJQuery.jqDropList(name, pageName, False)
        selectModuleHS.AddItem(" ", "0", True)

        Dim selected As Boolean = False

        For Each element In getListModuleHS()

            Dim dv As DeviceClass = element.Value

            If (filtre.Contains(dv.Device_Type_String(Nothing))) Or (filtre.Count = 0) Then
                selected = dv.Ref(Nothing) = valueSelected
                Dim str = dv.Location2(Nothing) & " " & dv.Location(Nothing) & " " & dv.Name(Nothing)
                selectModuleHS.AddItem(str, dv.Ref(Nothing), selected)
            End If

        Next

        stb1.Append(selectModuleHS.Build)
        Return stb1.ToString()
    End Function

    Dim CON As New SQLiteConnection
    '
    Public Sub OpenDataBase()
        Try
            CON.ConnectionString = "Data Source=" & hs.GetAppPath() & "/data/test.db"
            CON.Open()
            Log(Database & " ouverte", MessageType.Debug)
        Catch ex As Exception
            Log(ex.Message, MessageType.Error_)
        End Try
    End Sub
    '
    Public Sub CloseDatabase()
        CON.Close()
        Log("Database fermée", MessageType.Debug)
    End Sub

    Public Function GetListTables(con As SQLiteConnection) As List(Of String)

        Dim tables As New List(Of String)

        Dim dt As DataTable = con.GetSchema("Tables")
        For Each row As DataRow In dt.Rows
            Dim tablename As String = row(2)
            tables.Add(tablename)
        Next
        tables.Sort()
        Return tables
    End Function

    Public Sub CreateTable(ByVal TYPE As String)
        Try
            Dim strSQL As String = "CREATE TABLE " & TYPE & " (DATETIME DATETIME NOT NULL UNIQUE ON CONFLICT REPLACE, REF INTEGER NOT NULL, VALUE DOUBLE, OLDVALUE DOUBLE, DELTA DOUBLE, STRING TEXT, ADRESSE STRING, CODE STRING);"
            Dim cmd = New SQLiteCommand(strSQL, CON)
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Catch ex As Exception
            Log(ex.Message, MessageType.Error_)
        End Try
    End Sub

    Public Sub DBaddValue(ByVal data As m_Data)

    End Sub

    Public Structure m_Data
        Dim ref As Integer
        Dim Adresse As String
        Dim oldValue As Double
        Dim Value As Double
        Dim Delta As Double
        Dim DT As Date
        Dim Str As String
        Dim type As String

        Public Sub New(Reference As Integer, d As Donnée, oldD As Donnée)
            ref = Reference
            Dim dv As DeviceClass
            dv = hs.GetDeviceByRef(ref)
            If (dv IsNot Nothing) Then
                Adresse = dv.Address(Nothing)
                Str = d.Str
                type = dv.Device_Type_String(Nothing)
                If d.value = "" Then d.value = 0
                Value = d.value
                DT = d.Dte
                If (oldD IsNot Nothing) Then
                    If oldD.value = "" Then oldD.value = 0
                    oldValue = oldD.value
                    Delta = Value - oldValue
                End If

            End If




        End Sub
    End Structure
End Class

