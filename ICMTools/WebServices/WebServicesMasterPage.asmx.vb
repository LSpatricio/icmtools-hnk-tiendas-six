Imports System.ComponentModel
Imports System.Web.Script.Services
Imports System.Web.Services
Imports System.Web.Services.Protocols

' Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente.
<System.Web.Script.Services.ScriptService()>
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WebServicesMasterPage1
    Inherits System.Web.Services.WebService

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Sub ChangeUserModel(modelo As String)

        HttpContext.Current.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN")
        HttpContext.Current.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'self'")

        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
            mUser.Model = modelo

        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If

    End Sub

End Class