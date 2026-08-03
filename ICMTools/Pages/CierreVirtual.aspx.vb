Public Class CierreVirtual
    Inherits Page

#Region " Variables Privadas "

    ''' <summary>
    ''' Usuario
    ''' </summary>
    Private mUser As User

    ''' <summary>
    ''' Log
    ''' </summary>
    Private mLog As Log

#End Region

#Region " Inicial "

    ''' <summary>
    ''' Evento Load del objeto Page
    ''' </summary>
    ''' <param name="sender">sender</param>
    ''' <param name="e">e</param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Try
            ''Establece el Icono y el Nombre en la MasterPage
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Cierre Virtual"

            ''Valida la sesion del usuario
            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log

                If (Not IsPostBack) Then
                    mLog.insertLog("Cierre Virtual", "ACCESO", "Acceso a Cierre Virtual")
                End If

                CargarPeriodos()
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If
        Catch ex As Exception
            mLog.insertLog("CierreVirtual.aspx", "Page_Load", ex.Message)
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

#End Region

#Region " Cargar Periodos "

    ''' <summary>
    ''' Método que carga los periodos
    ''' </summary>
    Private Sub CargarPeriodos()
        Try
            Dim queryICM As New QueriesICM()
            Dim modelo As String = GetModel()
            Dim periodos As DataTable = queryICM.GetQuery(1, modelo)

            PeriodoSelect.DataSource = periodos
            PeriodoSelect.DataBind()
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene el modelo
    ''' </summary>
    ''' <returns>Regresa el modelo</returns>
    Public Function GetModel() As String
        Dim Model As String = Nothing
        If mUser.Model = "DEBUG" Then
            Model = "femcoqa"
        Else
            Model = mUser.Model
        End If

        Return Model
    End Function

#End Region

End Class