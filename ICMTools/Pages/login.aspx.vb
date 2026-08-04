Imports System.Net.NetworkInformation
Imports System.Web.ApplicationServices

Public Class login
    Inherits System.Web.UI.Page


    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _authenticationService As AuthenticationService

    Private mUser As User
    Private mLog As Log

    Public Sub New()
        _configuration = New AppConfiguration()
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
            Dim EncodeModel As String = Request.QueryString("m")
            Dim EncodeICMUser As String = Request.QueryString("u")
            Dim EncodeKey As String = Request.QueryString("k")

            If (Not EncodeModel Is Nothing AndAlso Not EncodeICMUser Is Nothing AndAlso Not EncodeKey Is Nothing) Then
                SessionStart(EncodeModel, EncodeICMUser, EncodeKey)
            Else
                MessageBoxShow("Aviso: Acceso Denegado", "El portal de ICMTools solo es accesible desde ICM Web.", "Acceso bloqueado por seguridad.", htmlMessageIcon.IconError)
            End If
        Catch ex As Exception
            MessageBoxShow("Error en autenticación", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Private Sub SessionStart(ByVal Model As String, ByVal User As String, ByVal Key As String)
        Try
            Dim Maintenance As Boolean
            Maintenance = _configuration.Maintenance

            If Maintenance = True Then
                MessageBoxShow("Aviso: Mantenimiento", "El sistema no se encuentra disponible debido a un mantenimiento, para cualquier duda favor de contactar al área de soporte ICM.", "Temporalmente fuera de servicio.", htmlMessageIcon.IconInfo1)
            Else

                Dim isAuth As Boolean = False
                Dim resultToken As AuthenticationResult = _authenticationService.ValidateToken(Model, User, Key)

                Select Case resultToken.Status

                    Case AuthenticationService.AuthenticationStatus.InvalidToken
                        MessageBoxShow("Error: Acceso Denegado", "El portal de ICMTools solo es accesible desde ICM Web.", "Token de acceso inválido.", htmlMessageIcon.IconError)

                    Case AuthenticationService.AuthenticationStatus.ExpiredToken
                        MessageBoxShow("Error: Acceso Denegado", "El portal de ICMTools solo es accesible desde ICM Web.", "Token de acceso caducado.", htmlMessageIcon.IconError)

                    Case AuthenticationService.AuthenticationStatus.Valid
                        Try
                            Dim ws As New WebServiceICMGeneral()
                            Dim dtPayee As DataTable

                            dtPayee = ws.Validate_Payee_byUserEmailAndModel(resultToken.User, resultToken.Model)



                            If dtPayee.Rows.Count > 0 Then isAuth = True

                        Catch ex As Exception
                            MessageBoxShow("Error en la autentificación", "No se pudo conectar con el servicio de validación", ex.Message, htmlMessageIcon.IconError)
                            Return
                        End Try

                        Dim safeUser As String = Server.HtmlEncode(userAccess)
                        Dim safeModel As String = Server.HtmlEncode(modelAccess)

                        If isAuth Then
                            If (ValidarModelo(DecodeModel, DecodeICMUser.ToLower)) Then
                                mUser = New User()
                                mUser.Model = DecodeModel
                                mUser.Email = DecodeICMUser.ToLower
                                mUser.DataBase = _configuration.ObtenerModelo(DecodeModel)
                                Session.Add("User", mUser)

                                ''mLog = New Log
                                ''mLog.insertLog("ICMTools", "LOGIN", "Login via portal ICM Web FEMCO_EP")

                                Response.Redirect(_configuration.HomePage, False)
                            Else
                                MessageBoxShow("Aviso: Acceso Denegado", "Las credenciales de acceso no son correctas, el usuario " + safeUser + " no tiene acceso a ICMTool del Modelo " + safeModel + ".", "Acceso bloqueado por seguridad.", htmlMessageIcon.IconWarning)
                            End If
                        Else
                            MessageBoxShow("Aviso: Acceso Denegado", "Las credenciales de acceso no son correctas, el usuario " + safeUser + " no tiene acceso a ICMTool del Modelo " + safeModel + ".", "Acceso bloqueado por seguridad.", htmlMessageIcon.IconWarning)
                        End If
                End Select

            End If
        Catch ex As Exception
            MessageBoxShow("Error en autenticación", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    ''' <summary>
    ''' Método que valida que el modelo seleccionado exista.
    ''' </summary>
    ''' <returns>Regresa true si todo esta correcto.</returns>
    Private Function ValidarModelo(Modelo As String, Usuario As String) As Boolean
        Dim esValido As Boolean = True
        Try
            'Dim listaPermisosModelo As List(Of ModelPermission) = ScreenPermission.ModelPermission(Usuario)
            'esValido = listaPermisosModelo.Where(Function(w) w.Model = Modelo).Any()
            'Return esValido
            Return True
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Método que de decodifica una credencial.
    ''' </summary>
    ''' <param name="Key">Llave.</param>
    ''' <returns>Regresa la llave decodificada.</returns>
    Private Function DecodificarCredencial(Key As String) As String
        Try
            If (String.IsNullOrEmpty(Key)) Then
                Throw New ArgumentNullException("Las credenciales no pueden estar vacías, por favor ingresa nuevamente.")
            End If

            Dim DecodeKey As String = Encoding.UTF8.GetString(Convert.FromBase64String(Key))
            Return DecodeKey

        Catch ex As FormatException
            Throw New FormatException("Las credenciales no están en un formato válido, por favor ingresa nuevamente.", ex)
        Catch ex As ArgumentNullException
            Throw New ArgumentNullException("Las credenciales no pueden estar vacías, por favor ingresa nuevamente.", ex)
        Catch ex As Exception
            Throw New ApplicationException("Las credenciales de acceso sin inválidas, por favor ingresa nuevamente.", ex)
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