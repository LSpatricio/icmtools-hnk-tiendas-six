Public Class ClasificacionesDocumentacion
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            'Icono de la página
            Me.Master.PageIcon = "<i class='fas fa-book fa-fw'></i>"
            'Nombre de la página
            Me.Master.PageName = ""

            'Valida la sesion del usuario, en caso de que ya haya caducado segun el web.config, 
            'redirecciona a la pagina de auttenticación.

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

End Class