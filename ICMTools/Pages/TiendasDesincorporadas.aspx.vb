Imports AjaxControlToolkit
Imports System.IO

Public Class TiendasDesincorporadas
    Inherits System.Web.UI.Page

    Private mUser As User
    Private Const NombrePagina = "TiendasDesincorporadas"
    'Private mLog As Log

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"

            Me.Master.PageName = NombrePagina

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                If Not IsPostBack Then
                    RegisterAsyncTask(
            New PageAsyncTask(
                Function()
                    Return CargarControlesAsync(mUser.Model)
                End Function))

                End If
                '           mLog = New Log
                '          mLog.insertLog("Monto Distribuible", "ACCESO", "Acceso a Monto Distribuible")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Sub FileUploader_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles FileUploader.UploadedComplete

        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = $"~\UploadedFiles\{NombrePagina}"

            fileClass.SaveUploadedFile(FileUploader, folder)

            '     mLog = New Log
            '    mLog.insertLog("Monto Distribuible", "ARCHIVO IMPORTADO", $"Archivo de Monto Distribuible importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub


    Private Async Function CargarControlesAsync(modelo As String) As Threading.Tasks.Task
        Try
            Dim periodService As New PeriodService

            Dim periodo = Await periodService.ObtenerPeriodoActual(modelo)

            SelectPeriod.Items.Clear()

            Dim item As New ListItem With {
    .Text = If(periodo?.IDPeriodString, "Sin periodo"),
    .Value = If(periodo?.IDPeriodString, "-1")
}

            SelectPeriod.Items.Add(item)

        Catch ex As Exception
            Me.Master.MessageBoxShow(
                "Error en CargarControles",
                ex.Message,
                "Fuente:" & ex.InnerException.Source,
                htmlMessageIcon.IconError)
        End Try
    End Function
End Class