Imports AjaxControlToolkit
Imports DocumentFormat.OpenXml.Drawing.Charts
Imports DocumentFormat.OpenXml.Spreadsheet

Public Class ExceptionsUpload
    Inherits System.Web.UI.Page

    Private mUser As User
    Private mLog As Log

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-upload fa-fw'></i>"
            Me.Master.PageName = "Carga de Excepciones"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                LoadControls()
                mLog = New Log
                mLog.insertLog("EXCEPCIONES", "ACCESO", "Acceso a carga de Excepciones")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Public Sub LoadControls()

        Try
            Dim serviceData As New CustomServiceClass
            Dim NullOption As New ListItem
            NullOption.Text = "Seleccione"
            NullOption.Value = "-1"
            SelectPeriod.Items.Add(NullOption)
            SelectSociety.Items.Add(NullOption)
            SelectPersonnelDivision.Items.Add(NullOption)

            Dim AvailableSocieties As New List(Of Societies)
            AvailableSocieties = serviceData.GetSocieties(mUser.Model, mUser.Email)

            For Each Society As Societies In AvailableSocieties

                Dim NewSociety As New ListItem

                NewSociety.Text = Society.SocietyName
                NewSociety.Value = Society.SocietyValue

                SelectSociety.Items.Add(NewSociety)
            Next

            Dim AvailablePeriodos As New List(Of Periods)
            AvailablePeriodos = serviceData.GetPeriods(mUser.Model, 2)

            For Each Periodo As Periods In AvailablePeriodos

                Dim NewPeriodo As New ListItem

                NewPeriodo.Text = Periodo.PeriodoName
                NewPeriodo.Value = Periodo.PeriodoValue
                SelectPeriod.Items.Add(NewPeriodo)

            Next
        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en CargarControles", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try

        'End Using

    End Sub


    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete
        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = "~\UploadedFiles\Excepciones\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            Dim fileName As String = IO.Path.GetFileName(AsyncFileUpload1.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("EXCEPCIONES", "ARCHIVO IMPORTADO", $"Archivo de Excepciones importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub


    Sub AsyncFileUpload1_UploadedFileError(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedFileError
        ScriptManager.RegisterClientScriptBlock(Me, Me.[GetType](), "error", "top.$get(""" + uploadResult.ClientID & """).innerHTML = 'Error: " & Convert.ToString(e.StatusMessage) & "';", True)
    End Sub


End Class