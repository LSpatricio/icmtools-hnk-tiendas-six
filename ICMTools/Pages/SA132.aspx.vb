Imports AjaxControlToolkit
Imports System.IO

Public Class SA132
    Inherits System.Web.UI.Page

    Private mUser As User
    Private Const NombrePagina = "SA132"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"

            Me.Master.PageName = NombrePagina

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Sub FileUploader_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles FileUploader.UploadedComplete

        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = $"~\UploadedFiles\{NombrePagina}"

            fileClass.SaveUploadedFile(FileUploader, folder)
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub
End Class
