'Imports System.Threading

Public Class logout
    Inherits System.Web.UI.Page

    ' Private mLog As Log

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        SessionEnd()
    End Sub

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        
    End Sub
    Private Sub SessionEnd()
        If Not Session.Item("User") Is Nothing Then
            'mLog = New Log
            'mLog.insertLog("ICMTools", "LOGOUT", "Termina sesion")

            Session.RemoveAll()
            Session.Abandon()

            Response.Redirect(ConfigurationManager.AppSettings("ICMUrl"), False)
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If


    End Sub

End Class