Imports HSPIUtils


Module tools

    Public logutils As LogUtils
    Public utils As HSPIUtils.Outils
    Public hs As HomeSeerAPI.IHSApplication
    Public datas As New List(Of String)
    Public HandledEncounters As New Dictionary(Of String, Double)

    Public counter As Integer = 0

End Module
