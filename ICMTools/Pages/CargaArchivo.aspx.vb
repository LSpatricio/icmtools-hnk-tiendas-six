Imports System
Imports System.IO


Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports AjaxControlToolkit


Public Class CargarArchivo
    Inherits System.Web.UI.Page

    Private mUser As User

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init

        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------

        mUser = CType(Session.Item("Usuario"), User)

        If mUser Is Nothing Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try

            Me.Master.PageIcon = "<i class='fa fa-upload'></i> "
            Me.Master.PageName = "Carga de Excepciones"

            If Not Me.Master.User Is Nothing Then
                Me.Master.MessageBoxShow("Usuario", "El usuario esta autenticado como: " & Me.Master.User.Name, "Mensaje secundario....", htmlMessageIcon.IconInfo1)
            Else
                Me.Master.MessageBoxShow("Usuario", "El usuario no está autenticado.", "Mensaje secundario....", htmlMessageIcon.IconInfo1)
            End If
        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub


    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete

        ScriptManager.RegisterClientScriptBlock(Me, Me.[GetType](), "size", "top.$get(""" + uploadResult.ClientID & """).innerHTML = 'Tamaño de ultimca carga: " & AsyncFileUpload1.FileBytes.Length.ToString() & "';", True)

        Dim User = Session.Item("User")
        Dim savePath As String = Server.MapPath("~\UploadedFiles\Excepciones\" + Me.Master.User.Email)
        Dim Extension As String = Path.GetExtension(AsyncFileUpload1.FileName)

        AsyncFileUpload1.SaveAs(savePath + Extension)

    End Sub


    Sub AsyncFileUpload1_UploadedFileError(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedFileError
        ScriptManager.RegisterClientScriptBlock(Me, Me.[GetType](), "error", "top.$get(""" + uploadResult.ClientID & """).innerHTML = 'Error: " & Convert.ToString(e.statusMessage) & "';", True)
    End Sub

End Class