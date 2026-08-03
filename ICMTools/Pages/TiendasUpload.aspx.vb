Imports AjaxControlToolkit

Public Class TiendasUpload
    Inherits Page

    Private mUser As User
    Private mLog As Log

    ''' <summary>
    ''' Evento Init del objeto Page.
    ''' </summary>
    ''' <param name="sender">sender</param>
    ''' <param name="e">e</param>
    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-upload fa-fw'></i>"
            Me.Master.PageName = "Tiendas"

            If Session.Item("User") IsNot Nothing Then
                mUser = CType(Session.Item("User"), User)
                LoadControls()
                mLog = New Log
                mLog.insertLog("TIENDAS", "ACCESO", "Acceso a carga de Tiendas")
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

            Dim NullOption As New ListItem With {
                .Text = "Seleccione",
                .Value = "-1"
            }

            Dim AllOption As New ListItem With {
                .Text = "Todos",
                .Value = "0"
            }

            SelectSociety.Items.Add(NullOption)
            SelectPersonnelDivision.Items.Add(NullOption)

            Dim AvailableSocieties As New List(Of Societies)

            AvailableSocieties = serviceData.GetSocieties(mUser.Model, mUser.Email)

            For Each Society As Societies In AvailableSocieties

                Dim NewSociety As New ListItem With {
                    .Text = Society.SocietyName,
                    .Value = Society.SocietyValue
                }

                SelectSociety.Items.Add(NewSociety)

            Next

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en CargarControles", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub


    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete
        If Session.Item("User") IsNot Nothing Then
            Dim fileClass As New FileClass
            Dim folder As String = "~\UploadedFiles\Tiendas\"
            fileClass.SaveUploadedFile(AsyncFileUpload1, folder)

            Dim fileName As String = IO.Path.GetFileName(AsyncFileUpload1.FileName)
            Dim safeFileName As String = fileClass.GetSafeFileName(fileName)

            mLog = New Log
            mLog.insertLog("TIENDAS", "ARCHIVO IMPORTADO", $"Archivo de Tiendas importado: {safeFileName}")
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub


    Sub AsyncFileUpload1_UploadedFileError(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedFileError
        ScriptManager.RegisterClientScriptBlock(Me, Me.[GetType](), "error", "top.$get(""" + AsyncFileUpload1.ClientID & """).innerHTML = 'Error: " & Convert.ToString(e.StatusMessage) & "';", True)
    End Sub

End Class