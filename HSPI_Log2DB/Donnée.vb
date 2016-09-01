Public Class Donnée
    Implements IComparable(Of Donnée)

    Public Str As String = ""
    Public Device As String = ""
    Public value As String = ""
    Public Status As String = ""
    Public Dte As Date
    Public Notes As String = ""

    Public Sub New(ligne As String)
        Dim valeur() As String = Split(ligne, ";")

        Dim s As String = ""
        Str = valeur(0)
        Device = valeur(1)
        Status = valeur(2)
        value = valeur(3)
        Dte = CDate(valeur(4))
        Notes = valeur(5)

    End Sub


    Private m_PartId As Object
    Public Function CompareTo(other As Donnée) As Integer Implements IComparable(Of Donnée).CompareTo
        Return Me.Dte.CompareTo(other.Dte)
    End Function

End Class
