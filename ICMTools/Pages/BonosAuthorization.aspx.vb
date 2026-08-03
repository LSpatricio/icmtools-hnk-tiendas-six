Imports System
Imports System.IO

Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports AjaxControlToolkit

Imports System.Data.SqlClient
Imports System.ComponentModel.DataAnnotations
Imports System.Runtime.InteropServices


Public Class BonosAuthorization
    Inherits System.Web.UI.Page
    Private mUser As User

    Private mLog As Log
    Dim ws As WebServiceICMGeneral

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init

        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------

        mUser = CType(Session.Item("User"), User)
        Dim maskModel As String

        If mUser.Model = "DEBUG" Then
            maskModel = "femcoepdev"
        Else
            maskModel = mUser.Model
        End If

        Dim PayeeID As String = ws.GetPayeeByUserEmail(mUser.Email, maskModel)
        Dim LastHistoryPayee As String = ws.GetLastHistoryPayee(PayeeID, maskModel)

        If mUser Is Nothing Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        ElseIf Not ScreenPermission.Access(LastHistoryPayee, mUser.Email, "BONOSAUT") Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Master.PageIcon = "<i class='fas fa-upload fa-fw'></i>"
        Me.Master.PageName = "Autorizar Bonos de transporte"
    End Sub
End Class