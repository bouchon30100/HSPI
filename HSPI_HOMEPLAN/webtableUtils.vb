Option Explicit On
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Globalization
Imports VB = Microsoft.VisualBasic
Imports System.Web
Imports HomeSeerAPI


Public Module WebTable

#Region "Tables"

    Public Class HTMLTable

        Public rows As New LinkedList(Of HTMLTableRow)
        Dim html = ""
        Dim _border As Integer
        Dim _CellSpacing As Short = -1
        Dim _TableWidthPercent As Short = -1
        Dim _Style As String = ""
        Dim _ClassName As String = ""
        Dim _Align As HTML_TableAlign = 0
        Dim _CellPadding As Short = -1
        Dim _TableWidthPixels As Integer = -1
        Dim _theme As String = ""
        Dim nbColonne As Integer = 0
        Dim maxColonne As Integer = 0
        Private _hasTitleLine As Boolean
        Public _id As String = ""
        Public _urlBackground As String = ""

        Public Sub New(id As String, ByVal border As Integer,
                    Optional ByRef hasTitleLine As Boolean = False,
                        Optional ByRef CellSpacing As Short = -1,
                       Optional ByRef TableWidthPercent As Short = -1,
                                    Optional ByVal Style As String = "",
                                    Optional ByVal ClassName As String = "",
                                    Optional ByVal Align As HTML_TableAlign = 0,
                                    Optional ByVal CellPadding As Short = -1,
                                    Optional ByVal TableWidthPixels As Integer = -1,
                                    Optional ByVal urlBackground As String = "")
            _id = id
            _hasTitleLine = hasTitleLine
            _border = border
            _CellSpacing = CellSpacing
            _TableWidthPercent = TableWidthPercent
            _Style = Style
            _ClassName = ClassName
            _Align = Align
            _CellPadding = CellPadding
            _TableWidthPixels = TableWidthPixels
            _urlBackground = urlBackground

        End Sub



        Public Sub addRow(Optional ByVal ClassName As String = "",
                                  Optional ByVal Style As String = "",
                                  Optional ByVal Align As HTML_Align = 0,
                                  Optional ByVal BackColor As String = "",
                                  Optional ByVal VertAlign As HTML_VertAlign = 0)


            nbColonne = 0
            Me.rows.AddLast(New HTMLTableRow(ClassName, Style, Align, BackColor, VertAlign))
        End Sub

        Public Sub addRow(row As HTMLTableRow)
            If nbColonne > maxColonne Then maxColonne = nbColonne
            nbColonne = 0
            rows.AddLast(row)
        End Sub

        Public Sub addEmptyRow(RowHeight As Integer)
            Me.addRow()
            Me.addCell("", "", 1, HTML_Align.CENTER, False, RowHeight)
        End Sub

        Public Sub addCell(html As String, ByRef Class_name As String,
                                   ByRef colspan As Short,
                                   Optional ByVal urlBackground As String = "",
                                   Optional ByVal ColorBackground As String = "transparent",
                                   Optional ByVal align As HTML_Align = 0,
                                   Optional ByVal nowrap As Boolean = False,
                                   Optional ByVal RowHeight As Integer = 0,
                                   Optional ByVal CellWidth As Integer = 0,
                                   Optional ByVal Style As String = "",
                                   Optional ByVal VertAlign As HTML_VertAlign = 0)

            Dim cell As New HTMLTableCell(html, Class_name, colspan, urlBackground, ColorBackground, align, nowrap, RowHeight, CellWidth, Style, VertAlign)
            nbColonne += colspan
            If nbColonne > maxColonne Then maxColonne = nbColonne
            GetLastTableRow().addCell(cell)
        End Sub

        Public Sub addCell(cell As HTMLTableCell)
            nbColonne += cell._colspan
            If nbColonne > maxColonne Then maxColonne = nbColonne
            GetLastTableRow().addCell(cell)
        End Sub

        Public Sub addEmptyCell(width As Integer)
            Me.addCell(New HTMLTableCell("", "", 1, "", HTML_Align.CENTER, False, 0, width))
        End Sub

        Public Function GetLastTableRow() As HTMLTableRow
            Return rows.Last.Value
        End Function

        Public Function GetHTML() As String

            Dim first As Boolean = True
            For Each r As HTMLTableRow In rows


                If first AndAlso _hasTitleLine Then

                    For i As Integer = 0 To r.Cells.Count - 1
                        If r.Cells.Count = 1 Then
                            r.Cells(i) = New HTMLTableCell("<b>" & r.Cells(i)._html & "</b>", r.Cells(i)._Class_name, maxColonne, r.Cells(i)._urlBackground, r.Cells(i)._ColorBackground, HTML_Align.CENTER, False, 36, 0, r.Cells(i)._Style, HTML_VertAlign.MIDDLE)
                        Else
                            r.Cells(i) = New HTMLTableCell("<b>" & r.Cells(i)._html & "</b>", r.Cells(i)._Class_name, r.Cells(i)._colspan, r.Cells(i)._urlBackground, r.Cells(i)._ColorBackground, HTML_Align.CENTER, False, 36, 0, r.Cells(i)._Style, HTML_VertAlign.MIDDLE)
                        End If
                    Next
                    first = False
                Else
                    If r.Cells.Count = 1 Then
                        r.Cells(0) = New HTMLTableCell(r.Cells(0)._html, r.Cells(0)._Class_name, maxColonne, r.Cells(0)._urlBackground, r.Cells(0)._ColorBackground, r.Cells(0)._align, False, r.Cells(0)._RowHeight, 0, r.Cells(0)._Style, HTML_VertAlign.MIDDLE)
                    End If
                End If

                For Each c As HTMLTableCell In r.Cells
                    r.html += html_TableCell(c._html, c._Class_name, c._colspan, c._urlBackground, c._ColorBackground, c._align, c._nowrap, c._RowHeight, c._CellWidth, c._Style, c._VertAlign)
                Next
                html += html_TableRow(r.html, r._ClassName, r._Style, r._Align, r._BackColor, r._VertAlign)
            Next


            Return html_Table(html, _border, _CellSpacing, _TableWidthPercent, _Style, _ClassName, _Align, _CellPadding, _TableWidthPixels, _urlBackground)
        End Function


    End Class

    Public Class HTMLTableRow
        Public html As String = ""
        Public Cells As New List(Of HTMLTableCell)
        Public _ClassName As String = ""
        Public _Style As String = ""
        Public _Align As HTML_Align = 0
        Public _BackColor As String = ""
        Public _VertAlign As HTML_VertAlign = 0
        Public _content As String

        Public Sub New(Optional ByVal ClassName As String = "",
                                  Optional ByVal Style As String = "",
                                  Optional ByVal Align As HTML_Align = 0,
                                  Optional ByVal BackColor As String = "",
                                  Optional ByVal VertAlign As HTML_VertAlign = 0)

            _ClassName = ClassName
            _Style = Style
            _Align = Align
            _BackColor = BackColor
            _VertAlign = VertAlign

        End Sub

        Public Sub addCell(cell As HTMLTableCell)

            Cells.Add(cell)

        End Sub

    End Class

    Public Class HTMLTableCell
        Public _html As String
        Public _Class_name As String
        Public _urlBackground As String = ""
        Public _ColorBackground As String = ""
        Public _colspan As Short
        Public _align As HTML_Align = 0
        Public _nowrap As Boolean = False
        Public _RowHeight As Integer = 0
        Public _CellWidth As Integer = 0
        Public _Style As String = ""
        Public _VertAlign As HTML_VertAlign = 0

        Public Sub New(html As String, ByRef Class_name As String,
                                   ByRef colspan As Short,
                                   Optional ByVal urlBackground As String = "",
                                   Optional ByVal ColorBackground As String = "transparent",
                                   Optional ByVal align As HTML_Align = 0,
                                   Optional ByVal nowrap As Boolean = False,
                                   Optional ByVal RowHeight As Integer = 0,
                                   Optional ByVal CellWidth As Integer = 0,
                                   Optional ByVal Style As String = "",
                                   Optional ByVal VertAlign As HTML_VertAlign = 0)
            _html = html
            _urlBackground = urlBackground
            _ColorBackground = ColorBackground
            _Class_name = Class_name
            _colspan = colspan
            _align = align
            _nowrap = nowrap
            _RowHeight = RowHeight
            _CellWidth = CellWidth
            _Style = Style
            _VertAlign = VertAlign
        End Sub

    End Class

#End Region

End Module
