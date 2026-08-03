Imports AjaxControlToolkit

Public Class HistEmpleadosActivos
    Inherits System.Web.UI.Page
    Private mUser As User
    Private mLog As Log
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Histórico Empleados Activos"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log
                mLog.insertLog("Histórico Empleados Activos", "ACCESO", "Acceso a Histórico Empleados Activos")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Sub AsyncFileUpload_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete, AsyncFileUpload2.UploadedComplete
        If Not Session.Item("User") Is Nothing Then
            Dim fileUpload As AsyncFileUpload = DirectCast(sender, AsyncFileUpload)
            Dim fileClass As New FileClass
            Dim folder As String = "~\UploadedFiles\IncentivoCerveza\HistEmpleadosActivos\" & If(sender.ClientID = "ContentPlaceHolder1_AsyncFileUpload1", "Inicial", "Final")
            fileClass.SaveUploadedFile(fileUpload, folder)

            Dim fileName As String = IO.Path.GetFileName(fileUpload.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("Histórico Empleados Activos", "ARCHIVO IMPORTADO", $"Archivo Histórico de Empleados Activos importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub
End Class