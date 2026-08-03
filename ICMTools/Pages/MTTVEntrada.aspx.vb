Imports AjaxControlToolkit

Public Class MTTVEntrada
    Inherits System.Web.UI.Page

    Private mUser As User
    Private mLog As Log
    Public UserEmail As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Entrada"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)

                UserEmail = mUser.Email

                mLog = New Log
                mLog.insertLog("MultiTienda Variable Entrada", "ACCESO", "Acceso a MultiTienda Variable Entrada")
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
            Dim folder As String = "~\UploadedFiles\MultiTiendaVariable\Entrada\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            Dim fileName As String = IO.Path.GetFileName(AsyncFileUpload1.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("MultiTienda Variable Entrada", "ARCHIVO IMPORTADO", $"Archivo de Entrada importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub

End Class