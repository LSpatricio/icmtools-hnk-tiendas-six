Imports AjaxControlToolkit

Public Class Clasificaciones
    Inherits Page

    Private mUser As User
    Private mLog As Log

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-upload fa-fw'></i>"
            Me.Master.PageName = "Clasificaciones"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                LoadControls()
                mLog = New Log
                mLog.insertLog("CLASIFICACIONES", "ACCESO", "Acceso a Clasificaciones")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Public Sub LoadControls()
        Try
            Dim serviceData As New CustomServiceClass
            Dim NullOption As New ListItem
            NullOption.Text = "Seleccione"
            NullOption.Value = "-1"

            Dim AllOption As New ListItem
            AllOption.Text = "Todos"
            AllOption.Value = "0"

            SelectSociety.Items.Add(NullOption)
            SelectPersonnelDivision.Items.Add(NullOption)

            Dim AvailableSocieties As New List(Of Societies)
            AvailableSocieties = serviceData.GetSocieties(mUser.Model, mUser.Email)

            For Each Society As Societies In AvailableSocieties
                Dim NewSociety As New ListItem
                NewSociety.Text = Society.SocietyName
                NewSociety.Value = Society.SocietyValue
                SelectSociety.Items.Add(NewSociety)
            Next

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en CargarControles", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try

    End Sub


    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete
        If Not Session.Item("User") Is Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = "~\UploadedFiles\Clasificaciones\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            mLog = New Log
            mLog.insertLog("CLASIFICACIONES", "ARCHIVO IMPORTADO", "Archivo de Clasificaciones importado:" + AsyncFileUpload1.FileName)
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub


    Sub AsyncFileUpload1_UploadedFileError(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedFileError
        ScriptManager.RegisterClientScriptBlock(Me, Me.[GetType](), "error", "top.$get(""" + uploadResult.ClientID & """).innerHTML = 'Error: " & Convert.ToString(e.StatusMessage) & "';", True)
    End Sub

End Class