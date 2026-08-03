Public Class ExceptionsDocumentation
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
            'Icono de la página
            Me.Master.PageIcon = "<i class='fas fa-book fa-fw'></i>"
            'Nombre de la página
            Me.Master.PageName = "Documentación de Excepciones"

            'Valida la sesion del usuario, en caso de que ya haya caducado segun el web.config, 
            'redirecciona a la pagina de auttenticación.
            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log
                mLog.insertLog("EXCEPCIONES", "ACCESO", "Acceso a documentación de Excepciones")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

End Class