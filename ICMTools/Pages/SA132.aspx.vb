Imports AjaxControlToolkit

Public Class SA132
    Inherits System.Web.UI.Page

    Private mUser As User
    Private Const NombrePagina As String = "SA132"

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-clipboard-check fa-fw'></i>"
            Me.Master.PageName = NombrePagina

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)

                If Not IsPostBack Then
                    RegisterAsyncTask(New PageAsyncTask(Function() CargarControlesAsync(mUser.Model)))
                End If
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If
        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, NombrePagina, htmlMessageIcon.IconError)
        End Try
    End Sub

    Private Async Function CargarControlesAsync(modelo As String) As Threading.Tasks.Task
        Try
            Dim periodService As New PeriodService()
            Dim periodo = Await periodService.ObtenerPeriodoActual(modelo)

            SelectPeriod.Items.Clear()
            SelectPeriod.Items.Add(New ListItem With {
                .Text = If(periodo?.IDPeriodString, "Sin periodo"),
                .Value = If(periodo?.IDPeriodString, "-1")
            })
        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en CargarControles", ex.Message, NombrePagina, htmlMessageIcon.IconError)
        End Try
    End Function

    Private Sub FileUploader_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles FileUploader.UploadedComplete
        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass()
            Dim folder As String = $"~\UploadedFiles\{NombrePagina}"
            fileClass.SaveUploadedFile(FileUploader, folder)
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub
End Class
