Imports System.Net.NetworkInformation
Imports System.Web.ApplicationServices

Public Class login
    Inherits System.Web.UI.Page


    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _authenticationService As AuthenticationService
    Private ReadOnly _payeeService As PayeeService


    Private mUser As User
    'Private mLog As Log

    Public Sub New()
        _configuration = New AppConfiguration()
        _authenticationService = New AuthenticationService()
        _payeeService = New PayeeService()
    End Sub
    Private Sub login_Init(sender As Object, e As EventArgs) Handles Me.Init
        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Dim EncodeModel As String = "WlpaWlpa" 'Request.QueryString("m")
            Dim EncodeICMUser As String = "WlpaWlpa" ' Request.QueryString("u")
            Dim EncodeKey As String = "WlpaWlpa" 'Request.QueryString("k")

            'If (Not EncodeModel Is Nothing AndAlso Not EncodeICMUser Is Nothing AndAlso Not EncodeKey Is Nothing) Then
            'SessionStart(EncodeModel, EncodeICMUser, EncodeKey)
            RegisterAsyncTask(
            New PageAsyncTask(
                Function()
                    Return SessionStart(EncodeModel, EncodeICMUser, EncodeKey)
                End Function))
            'Else
            'MessageBoxShow("Aviso: Acceso Denegado", "El portal de ICMTools solo es accesible desde ICM Web.", "Acceso bloqueado por seguridad.", htmlMessageIcon.IconError)
            'End If
        Catch ex As Exception
            MessageBoxShow("Error en autenticación", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Private Async Function SessionStart(ByVal Model As String, ByVal User As String, ByVal Key As String) As Threading.Tasks.Task
        Try
            Dim Maintenance As Boolean
            Maintenance = _configuration.Maintenance

            If Maintenance = True Then
                MessageBoxShow("Aviso: Mantenimiento", "El sistema no se encuentra disponible debido a un mantenimiento, para cualquier duda favor de contactar al área de soporte ICM.", "Temporalmente fuera de servicio.", htmlMessageIcon.IconInfo1)
            Else

                Dim resultToken As AuthenticationResult = _authenticationService.ValidateToken(Model, User, Key)
                resultToken.Status = AuthenticationService.AuthenticationStatus.InvalidToken
                resultToken.Model = "ICMMNFHeinekenQA"
                resultToken.User = "00000301@heineken.com"

                Select Case resultToken.Status

                    Case AuthenticationService.AuthenticationStatus.InvalidToken
                        MessageBoxShow("Error: Acceso Denegado", "El portal de ICMTools solo es accesible desde ICM Web.", "Token de acceso inválido.", htmlMessageIcon.IconError)

                    Case AuthenticationService.AuthenticationStatus.ExpiredToken
                        MessageBoxShow("Error: Acceso Denegado", "El portal de ICMTools solo es accesible desde ICM Web.", "Token de acceso caducado.", htmlMessageIcon.IconError)

                    Case AuthenticationService.AuthenticationStatus.Valid


                        Dim isAuth As Boolean = Await _payeeService.ValidarPayeePorCorreoModelo(resultToken.User, resultToken.Model)

                        Dim safeUser As String = Server.HtmlEncode(resultToken.User)
                        Dim safeModel As String = Server.HtmlEncode(resultToken.Model)

                        If isAuth Then
                            mUser = New User()
                            mUser.Model = resultToken.Model
                            mUser.Email = resultToken.User.ToLower
                            Session.Add("User", mUser)

                            ''mLog = New Log
                            ''mLog.insertLog("ICMTools", "LOGIN", "Login via portal ICM Web FEMCO_EP")

                            Response.Redirect(_configuration.HomePage, False)

                        Else
                            MessageBoxShow("Aviso: Acceso Denegado", "Las credenciales de acceso no son correctas, el usuario " + safeUser + " no tiene acceso a ICMTool del Modelo " + safeModel + ".", "Acceso bloqueado por seguridad.", htmlMessageIcon.IconWarning)
                        End If
                End Select

            End If
        Catch ex As Exception
            MessageBoxShow("Error en autenticación", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Function

    Public Sub MessageBoxShow(Title As String, ByVal PrimryMessage As String, SecondaryMessage As String, type As htmlMessageIcon)

        Dim msg As New htmlMessage(Me.Page, "Message", "MessageTitle", "MessagePrimary", "MessageSecondary", "MessageIcon")

        msg.Title = Title
        msg.MessagePrimary = PrimryMessage
        msg.MessagensSecondary = SecondaryMessage
        msg.MessageType = type

        msg.Show()
    End Sub
End Class