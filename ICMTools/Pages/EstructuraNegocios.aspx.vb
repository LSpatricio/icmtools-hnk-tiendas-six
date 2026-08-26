Imports AjaxControlToolkit
Imports System.IO

Public Class EstructuraNegocios
    Inherits System.Web.UI.Page

    Private mUser As User
    Private Const NombrePagina = "EstructuraNegocios"

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
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub


    Private Async Function CargarControlesAsync(modelo As String) As Threading.Tasks.Task
        Try
            Dim periodService As New PeriodService
            Dim catalogoService As New CatalogoService

            Dim periodo = Await periodService.ObtenerPeriodoActual(modelo)
            Dim regiones = Await catalogoService.ObtenerRegiones(modelo)


            SelectPeriod.Items.Clear()
            SelectRegion.Items.Clear()

            Dim item As New ListItem With {
    .Text = If(periodo?.IDPeriodString, "Sin periodo"),
    .Value = If(periodo?.IDPeriodString, "-1")
}

            Dim hayRegiones = regiones?.Any()

            SelectRegion.Items.Add(New ListItem With {
    .Text = If(hayRegiones, "Todas (!)", "Sin regiones"),
    .Value = "Todas"})

            If hayRegiones Then
                For Each region In regiones
                    SelectRegion.Items.Add(New ListItem With {
            .Text = region.Description,
            .Value = region.Description
        })
                Next
            End If

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