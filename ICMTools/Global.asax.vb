Imports System.Web.Http
Imports System.Web.SessionState
Imports System.Timers
Imports System.Linq

Public Class Global_asax
    Inherits HttpApplication

    Public Sub New()

    End Sub

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
    End Sub

    Sub Application_Error(sender As Object, e As EventArgs)
        Dim ex As Exception = Server.GetLastError()
        'Dim oLog As New Log()
        'oLog.NotificacionError(ex)
    End Sub

    Sub Application_PostAuthorizeRequest()
        If HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api/") Then
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required)
        End If
    End Sub
End Class