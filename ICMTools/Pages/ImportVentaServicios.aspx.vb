Imports AjaxControlToolkit

Public Class ImportVentaServicios
    Inherits System.Web.UI.Page
    Private mUser As User
    Private mLog As Log

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Venta Servicios"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log
                mLog.insertLog("Empleados Activos", "ACCESO", "Acceso a Empleados Activos")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Sub AsyncFileUpload_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete
        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = "~\UploadedFiles\Servicios\VentaServicios\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            Dim fileName As String = IO.Path.GetFileName(AsyncFileUpload1.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("Venta Servicios", "ARCHIVO IMPORTADO", $"Archivo de Venta Servicios importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If


    End Sub

End Class