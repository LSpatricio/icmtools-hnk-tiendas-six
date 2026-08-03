Public Class Home
    Inherits System.Web.UI.Page

    Private mUser As User

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
            'Me.Master.PageIcon = "<i class='fas fa-home'></i> "
            'Me.Master.PageName = "Inicio"

            'If Not Session.Item("User") Is Nothing Then
            '    mUser = CType(Session.Item("User"), User)
            'Else
            '    Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            'End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub
End Class