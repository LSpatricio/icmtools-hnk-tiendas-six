Imports System.IO
Imports ClassLibrary_PGP_TO_SFTP

Public Class MasterPage
    Inherits System.Web.UI.MasterPage
#Region "Variables"
    Private mUser As User
    Private mUsuario As String
    Private mModelo As String
    Private mPageName As String
    Private mPageIcon As String
    Public bShowMessageBox As Boolean
#End Region

#Region "Propiedades"
    Public Property Model As String
        Get
            Return mModelo
        End Get
        Set(value As String)
            mModelo = value
        End Set
    End Property
    Public ReadOnly Property User() As User
        Get
            Return mUser
        End Get
    End Property

    Public Property PageName As String
        Get
            Return mPageName
        End Get
        Set(value As String)
            mPageName = value
            moduleName.InnerText = value
        End Set
    End Property

    Public Property PageIcon As String
        Get
            Return mPageIcon
        End Get
        Set(value As String)
            mPageIcon = value
        End Set
    End Property

#End Region

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Valida la sesion del usuario, en caso de que ya haya caducado segun el web.config, 
        'redirecciona a la pagina de auttenticación.
        'If Not Session.Item("User") Is Nothing Then
        '    mUser = CType(Session.Item("User"), User)
        'Else
        '    Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        'End If

        'MessageBoxRefresh()

        'If Not Me.IsPostBack Then
        '    'Establece los valores iniciales.

        '    'Dim idModel As String = CargarModelos()
        '    'CargarPantallas(idModel)
        '    'SetInitialValues()
        'End If

    End Sub

    Private Sub SetInitialValues()
        If Not mUser Is Nothing Then
            lblModel.Text = mUser.Model
            lblUserEmail.Text = mUser.Email
            lblUserName.Text = mUser.Name
            lblTopUserEmail.Text = mUser.Email
        End If

    End Sub

    ''' <summary>
    ''' Método que carga los Modelos a los que el usuario tiene permisos
    ''' </summary>
    ''' <returns></returns>
    Private Function CargarModelos() As String
        mUser = CType(Session.Item("User"), User)
        Dim modeloActualID As String = ""

        If Not mUser Is Nothing Then
            Dim modeloActual = mUser.Model
            Dim listaPermisosModelo As List(Of ModelPermission) = ScreenPermission.ModelPermission(mUser.Email)

            Dim listaModelos As List(Of ModelPermission) = listaPermisosModelo.Where(Function(w) w.Model <> modeloActual).Distinct().ToList()

            If listaPermisosModelo.Any() Then
                modeloActualID = listaPermisosModelo.Where(Function(w) w.Model = modeloActual).FirstOrDefault().IDModel

                RepeaterModelo.DataSource = listaModelos
                RepeaterModelo.DataBind()
                litModeloInicial.Text = modeloActual
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If

        Return modeloActualID
    End Function

    ''' <summary>
    ''' Método que carga las pantallas habilitadas para el modelo seleccionado.
    ''' </summary>
    ''' <param name="idModelo"></param>
    Private Sub CargarPantallas(idModelo As String)
        mUser = CType(Session.Item("User"), User)

        If Not mUser Is Nothing Then
            ' La función ahora devuelve un solo objeto MenuData, no una lista.
            Dim menu As MenuData = ScreenPermission.ScreenPermission(CInt(idModelo))

            If menu IsNot Nothing Then
                ' Asigna directamente las listas del objeto MenuData a los repeaters.
                RepeaterModuloAgrupado.DataSource = menu.AggregatedItems
                RepeaterModuloAgrupado.DataBind()
                RepeaterModuloSinAgrupas.DataSource = menu.SimpleItems
                RepeaterModuloSinAgrupas.DataBind()
            End If
        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub
    Protected Sub RepeaterModulo_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)

        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim repeaterInterno As Repeater = e.Item.FindControl("RepeaterPantallas")
            ' La propiedad ahora se llama "Screens" en la clase ScreenAggregator
            Dim aggregatorItem As ScreenAggregator = CType(e.Item.DataItem, ScreenAggregator)

            repeaterInterno.DataSource = aggregatorItem.Screens
            repeaterInterno.DataBind()
        End If

    End Sub

    Public Sub MessageBoxRefresh()
        Dim myGenericControl As HtmlGenericControl
        myGenericControl = Me.FindControl("Message")

        myGenericControl.Visible = bShowMessageBox
        bShowMessageBox = False

        ' For Each p As PerfilObjeto In User.Perfil.Objetos

        ' Next

    End Sub

    Public Sub MessageBoxShow(Title As String, ByVal PrimryMessage As String, SecondaryMessage As String, type As htmlMessageIcon)

        Dim msg As New htmlMessage(Me.Page.Master, "Message", "MessageTitle", "MessagePrimary", "MessageSecondary", "MessageIcon")

        msg.Title = Title
        msg.MessagePrimary = PrimryMessage
        msg.MessagensSecondary = SecondaryMessage
        msg.MessageType = type

        msg.Show()

    End Sub

    Public Sub MessageBoxShow(ex As Exception)

        Dim msg As New htmlMessage(Me, "Message", "MessageTitle", "MessagePrimary", "MessageSecondary", "MessageIcon")

        msg.Title = ex.Source
        msg.MessagePrimary = ex.Message
        msg.MessagensSecondary = ex.StackTrace
        msg.MessageType = htmlMessageIcon.IconError

        msg.Show()

    End Sub


End Class