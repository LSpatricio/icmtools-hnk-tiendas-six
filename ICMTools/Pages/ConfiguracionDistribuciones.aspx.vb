Imports AjaxControlToolkit
Imports System.IO

Public Class ConfiguracionDistribuciones
    Inherits System.Web.UI.Page

    Private mUser As User
    Private mLog As Log

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Configuración de Distribuciones"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log
                mLog.insertLog("Configuración de Distribuciones", "ACCESO", "Acceso a Configuración de Distribuciones")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete
        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = "~\UploadedFiles\IncentivoCerveza\ConfiguracionDistribuciones\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            Dim fileName As String = Path.GetFileName(AsyncFileUpload1.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("Configuración de Distribuciones", "ARCHIVO IMPORTADO", $"Archivo de Configuracion de Distribuciones importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub

End Class