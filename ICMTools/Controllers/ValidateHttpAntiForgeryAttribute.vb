Imports System.Net
Imports System.Net.Http
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters
Imports System.Web.Helpers

Public Class ValidateHttpAntiForgeryAttribute
    Inherits ActionFilterAttribute

    Public Overrides Sub OnActionExecuting(actionContext As HttpActionContext)
        Dim headers = actionContext.Request.Headers

        If Not headers.Contains("X-XSRF-Token") Then
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Falta Token de Seguridad")
            Return
        End If

        Dim formToken As String = headers.GetValues("X-XSRF-Token").FirstOrDefault()
        Dim cookie = HttpContext.Current.Request.Cookies("__RequestVerificationToken")
        Dim cookieToken As String = If(cookie IsNot Nothing, cookie.Value, Nothing)

        AntiForgery.Validate(cookieToken, formToken)
    End Sub
End Class