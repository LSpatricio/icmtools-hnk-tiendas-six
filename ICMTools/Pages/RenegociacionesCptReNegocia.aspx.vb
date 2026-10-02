Imports AjaxControlToolkit

Public Class RenegociacionesCptReNegocia
    Inherits System.Web.UI.Page
    Private mUser As User
    Private Const NombrePagina As String = "Renegociaciones"
    Private Const NombrePantalla As String = "Renegociaciones - CptRe-Negocia"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Master.PageIcon = "<i class='fas fa-file-invoice-dollar fa-fw'></i>"
        Me.Master.PageName = NombrePantalla
        If Session.Item("User") Is Nothing Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        Else
            mUser = CType(Session.Item("User"), User)
            If Not IsPostBack Then
                RegisterAsyncTask(New PageAsyncTask(Function() CargarControlesAsync(mUser.Model)))
            End If
        End If
    End Sub

    Protected Sub FileUploader_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles FileUploader.UploadedComplete
        If Session.Item("User") Is Nothing Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            Return
        End If
        Dim fileClass As New FileClass
        fileClass.SaveUploadedFile(FileUploader, $"~\UploadedFiles\{NombrePagina}")
    End Sub

    Private Async Function CargarControlesAsync(modelo As String) As Threading.Tasks.Task
        Try
            Dim periodService As New PeriodService
            Dim catalogoService As New CatalogoService
            Dim periodo = Await periodService.ObtenerPeriodoActual(modelo)
            Dim regiones = Await catalogoService.ObtenerRegiones(modelo)

            SelectPeriod.Items.Clear()
            SelectRegion.Items.Clear()

            SelectPeriod.Items.Add(New ListItem With {
                .Text = If(periodo?.IDPeriodString, "Sin periodo"),
                .Value = If(periodo?.IDPeriodString, "-1")
            })

            Dim hayRegiones = regiones?.Any()
            SelectRegion.Items.Add(New ListItem With {
                .Text = If(hayRegiones, "Todas (!)", "Sin regiones"),
                .Value = "Todas"
            })

            If hayRegiones Then
                For Each region In regiones
                    SelectRegion.Items.Add(New ListItem With {
                        .Text = region.Description,
                        .Value = region.Description
                    })
                Next
            End If
        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en CargarControles", ex.Message, "Fuente:" & If(ex.InnerException Is Nothing, "", ex.InnerException.Source), htmlMessageIcon.IconError)
        End Try
    End Function
End Class
