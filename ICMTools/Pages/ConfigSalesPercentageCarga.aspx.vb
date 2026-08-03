Imports AjaxControlToolkit

Public Class ConfigSalesPercentageCarga
    Inherits System.Web.UI.Page

    Private mUser As User
    Private mLog As Log

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Configuración porcentaje de ventas"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log
                mLog.insertLog("PagosManuales", "ACCESO", "Acceso a Configuración Porcentaje de Ventas")
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
            Dim folder As String = "~\UploadedFiles\IncentivoCerveza\ConfiguracionPorcentajeVentas\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            Dim fileName As String = IO.Path.GetFileName(AsyncFileUpload1.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("ConfiguracionPorcentajeVentas", "ARCHIVO IMPORTADO", $"Archivo de Configuración Porcentaje de Ventas, importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub

End Class