Imports System.Diagnostics.CodeAnalysis
Imports System.Globalization
Imports System.Net
Imports System.Threading
Imports System.Web.Helpers
Imports System.Web.Http
Imports System.Web.Http.Controllers
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Namespace Controllers
    Public Class ClasificacionesController
        Inherits ApiController

#Region " Variables Privadas "

        ''' <summary>
        ''' Usuario
        ''' </summary>
        Private ReadOnly mUser As User

        ''' <summary>
        ''' Log
        ''' </summary>
        Private ReadOnly mLog As Log

        ''' <summary>
        ''' Cadena de conexión a Postgress
        ''' </summary>
        Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

        ''' <summary>
        ''' FileController
        ''' </summary>
        ReadOnly fc As New FileController

        ''' <summary>
        ''' SharedController
        ''' </summary>
        ReadOnly sc As New SharedController

#End Region

#Region " Constructor "

        ''' <summary>
        ''' Constructor
        ''' </summary>
        Public Sub New()
            Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
            Me.mLog = New Log()
        End Sub

#End Region

#Region " Clases "

        ''' <summary>
        ''' Registro de Clasificaciones
        ''' </summary>
        Private Class RegistroClasificaciones
            Public IDStore As String
            Public [Date] As Date
            Public [Value] As String
        End Class

        ''' <summary>
        ''' CustonValidateFileRequest
        ''' </summary>
        Public Class CustomValidateFileRequest
            Property FileType As String
            Property Extension As String
            Property columns As String()
            Property types As String()
            Property Society As String
            Property PersonnelDivision As String
        End Class

        ''' <summary>
        ''' ConfirmExceptionsRequest
        ''' </summary>
        Public Class ConfirmExceptionsRequest
            Property FileType As String
            Property Extension As String
            Property Society As String
            Property PersonnelDivision As String
        End Class

        Class Tienda
            Public IDStore As String
            Public Society As String
            Public PersonnelDivision As String
        End Class
#End Region

#Region " Métodos POST "

        <HttpPost>
        <Route("api/clasificaciones/InsertInfoBD")>
        Public Function InsertInfoBD(_request As ConfirmExceptionsRequest) As IHttpActionResult
            If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
            Dim ws As New WebServiceICMGeneral()
            Try
                Dim headers = Request.Headers
                If headers.Contains("X-XSRF-Token") Then
                    Dim formToken As String = headers.GetValues("X-XSRF-Token").FirstOrDefault()
                    Dim cookie = HttpContext.Current.Request.Cookies("__RequestVerificationToken")
                    Dim cookieToken As String = If(cookie IsNot Nothing, cookie.Value, Nothing)
                    System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken)
                Else
                    Return BadRequest("Token de Seguridad Inválido")
                End If

                Dim sanitize As New Sanitizacion
                Dim safeFileType As String = sanitize.Texto(_request.FileType)
                Dim safeExtension As String = sanitize.Texto(_request.Extension)
                Dim safePersonnelDivision As String = sanitize.Texto(_request.PersonnelDivision)
                Dim safeSociety As String = sanitize.Texto(_request.Society)

                Dim ExcelArray(,) As Object = fc.GetExcelArray(safeFileType, safeExtension)
                Dim countCorrectos As Integer = 0
                Dim countIncorrectos As Integer = 0
                Dim PersonnelDivision As String = safePersonnelDivision
                Dim Society As String = safeSociety

                If PersonnelDivision Is Nothing Then
                    PersonnelDivision = -1
                End If

                If Society Is Nothing Then
                    Society = -1
                End If

                Dim xlsx As New DataTable()

                Dim epsapCfgStoreHierarchyCols As New List(Of String) From {"IDStore", "IDSociety", "IDPersonalDivision"}
                Dim epsapCfgStoreHierarchy As DataTable = ws.ConsultaICMAPIQueryLotes(epsapCfgStoreHierarchyCols, "CfgStoreHierarchy", GetModel())
                Dim epsapCfgStoreHierarchyTbl As String = JsonConvert.SerializeObject(epsapCfgStoreHierarchy)

                If ExcelArray IsNot Nothing Then
                    Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
                    Dim usedColumns As Integer = ExcelArray.GetUpperBound(1)
                    Dim fechaActual As Date = Date.Today
                    Dim FECHAHORA As DateTime = DateTime.Now

                    Using conn As New NpgsqlConnection(NpgSQL)
                        conn.Open()
                        Dim i As Integer = 0

                        ''Ejecución de la función
                        Using cmd As New NpgsqlCommand("CALL public.spicmtoolsclasificacionesinsert(@p_usuario, @p_modelo, @p_fechaHora, @p_idsociety, @p_idpersonaldivision, @p_filedata, @p_cfgstorehierarchy)", conn)
                            cmd.Parameters.AddWithValue("p_usuario", NpgsqlDbType.Varchar, mUser.Email)
                            cmd.Parameters.AddWithValue("p_modelo", NpgsqlDbType.Varchar, GetModel())
                            cmd.Parameters.AddWithValue("p_fechaHora", NpgsqlDbType.Date, FECHAHORA)
                            cmd.Parameters.AddWithValue("p_idsociety", NpgsqlDbType.Varchar, Society)
                            cmd.Parameters.AddWithValue("p_idpersonaldivision", NpgsqlDbType.Varchar, PersonnelDivision)
                            cmd.Parameters.AddWithValue("p_filedata", NpgsqlDbType.Json, ProcesarExcelClasificaciones(safeFileType, safeExtension))
                            cmd.Parameters.AddWithValue("p_cfgstorehierarchy", NpgsqlDbType.Json, epsapCfgStoreHierarchyTbl)
                            cmd.ExecuteNonQuery()
                        End Using

                        Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM clasificaciones_registros WHERE status = 'true'", conn)
                            countCorrectos = cmd.ExecuteScalar()
                        End Using

                        Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM clasificaciones_registros WHERE status = 'false'", conn)
                            countIncorrectos = cmd.ExecuteScalar()
                        End Using

                        Dim query As String = $"SELECT * FROM clasificaciones_registros WHERE status = 'false'"
                        Using cmdQ As New NpgsqlCommand(query, conn)
                            Using adapter As New NpgsqlDataAdapter(cmdQ)
                                adapter.Fill(xlsx)
                            End Using
                        End Using
                    End Using
                End If

                ' 2) Construir archivo si hay errores
                Dim filePath As String = If(countIncorrectos > 0, fc.BuildXlsx(xlsx, "Clasificaciones"), String.Empty)

                ' 3) Determinar código y mensaje
                Dim codigoRespuesta As Integer
                Dim mensaje As String
                Dim cantidadTotal As Integer = countCorrectos + countIncorrectos

                If cantidadTotal = 0 Then
                    codigoRespuesta = 0
                    mensaje = sc.GetMessage("Clasificaciones", "SinImportacion")
                ElseIf countIncorrectos = cantidadTotal Then
                    codigoRespuesta = 0
                    mensaje = sc.GetMessage("Clasificaciones", "SinImportacion", cantidadTotal, countIncorrectos)
                ElseIf countIncorrectos > 0 Then
                    codigoRespuesta = 2
                    mensaje = sc.GetMessage("Clasificaciones", "CargaParcial", cantidadTotal, countIncorrectos)
                    SendSuccessResponse()
                    SendSFTP()
                Else
                    codigoRespuesta = 1
                    mensaje = sc.GetMessage("Clasificaciones", "CargaCompleta", cantidadTotal)
                    SendSuccessResponse()
                    SendSFTP()
                End If

                ' 4) Respuesta
                Return Ok(New With {.d = codigoRespuesta, .f = filePath, .r = mensaje})

            Catch ex As Exception
                mLog.insertLog("ClasificacionesController", "InsertInfoBD", ex.Message)
                Return InternalServerError(ex)
            End Try
        End Function

#End Region

#Region "Funciones"

        ''' <summary>
        ''' Obtiene las tiendas.
        ''' </summary>
        ''' <param name="dr">DataTable</param>
        ''' <returns>Regresa las tiendas</returns>
        Public Function GetListaTiendas(dr As DataTable) As List(Of Tienda)
            Dim Clasificaciones = New List(Of Tienda)
            For Each row As DataRow In dr.Rows
                Dim tienda As New Tienda With {
                    .IDStore = row("IDStore").ToString(),
                    .Society = row("IDSociety").ToString(),
                    .PersonnelDivision = row("IDPersonalDivision").ToString()
                }
                Clasificaciones.Add(tienda)
            Next
            Return Clasificaciones
        End Function

        ''' <summary>
        ''' Obtiene el modelo del usuario actual.
        ''' Si el modelo es "DEBUG", retorna el valor por defecto.
        ''' </summary>
        ''' <returns>Modelo del usuario o valor por defecto si está en modo DEBUG.</returns>
        Public Function GetModel() As String
            Dim Model As String = Nothing
            If mUser.Model = "DEBUG" Then
                Model = "femcoepdev"
            Else
                Model = mUser.Model
            End If
            Return Model
        End Function

        ''' <summary>
        ''' Convierte una cadena en una fecha utilizando formatos específicos.
        ''' </summary>
        ''' <param name="fechaTexto">Cadena que contiene la fecha a convertir.</param>
        ''' <returns>Un valor <see cref="DateTime"/> convertido desde la cadena.</returns>
        ''' <exception cref="ArgumentNullException">Se lanza si <paramref name="fechaTexto"/> es nulo o vacío.</exception>
        ''' <exception cref="FormatException">Se lanza si la cadena no coincide con ninguno de los formatos esperados.</exception>
        Private Function ParseDate(fechaTexto As String) As DateTime
            If String.IsNullOrWhiteSpace(fechaTexto) Then
                Throw New ArgumentNullException(NameOf(fechaTexto), "El texto de la fecha no puede ser nulo o vacío.")
            End If

            Dim fechaResultado As DateTime
            Dim formatos() As String = {"dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd"}

            Dim exito As Boolean = DateTime.TryParseExact(
                fechaTexto,
                formatos,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                fechaResultado
            )

            If Not exito Then
                Throw New FormatException($"No se pudo convertir el texto '{fechaTexto}' a un valor de fecha válido.")
            End If

            Return fechaResultado
        End Function

        ''' <summary>
        ''' Método que procesa el excel de clasificaciones.
        ''' </summary>
        ''' <param name="fileType">Tipo de archivo.</param>
        ''' <param name="extension">Extension.</param>
        ''' <returns>Regresa el json del excel.</returns>
        Private Function ProcesarExcelClasificaciones(fileType As String, extension As String) As String
            Dim ExcelArray(,) As Object = fc.GetExcelArray(fileType, extension)
            Dim jTable As New List(Of RegistroClasificaciones)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

            For row As Integer = 2 To usedRows
                Dim IDStore As String = ExcelArray(row, 1)
                Dim [Date] As Date = ParseDate(ExcelArray(row, 2))
                Dim Grade As String = ExcelArray(row, 3)
                jTable.Add(New RegistroClasificaciones With {
                            .IDStore = IDStore,
                            .Date = [Date],
                            .Value = Grade})
            Next

            Dim jsonFileData As String = JsonConvert.SerializeObject(jTable)
            Return jsonFileData
        End Function

        ''' <summary>
        ''' Método que envia a PGP.
        ''' </summary>
        Private Sub SendSFTP()
            Try
                Dim envio As New EnvioPGPClass
                envio.Pantalla = EnvioPGPClass.enuPantalla.Clasificaciones
                envio.Enviar()
            Catch ex As Exception
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Método que envía la respuesta de éxito.
        ''' </summary>
        Sub SendSuccessResponse()
            Dim ws As New WebServiceICMGeneral()
            Dim NowDate As String = Now.ToString("yyyy-MM-dd")
            Dim sql As String = $"SELECT idstore, fecha, grade, dateinsertion AS ""FechaInsercion"" FROM DataObjetivesClasificaciones WHERE usuario = '{mUser.Email}' AND dateinsertion::DATE = '{NowDate}';"
            Dim TableResponse As New DataTable()
            Dim mailBody As String = "Se Ejecuto el proceso de Validacion <strong>Favor de revisar el archivo anexo al correo</strong>"
            Try
                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Using cmd As New NpgsqlCommand(sql, conn)
                        Using da As New NpgsqlDataAdapter(cmd)
                            da.Fill(TableResponse)
                        End Using
                    End Using
                End Using

                Dim filePath As String = fc.BuildXlsx(TableResponse, "Clasificaciones")
                Dim Model As String = GetModel()

                ws.WebServiceSendMail(mUser.Email, "ICMTools | Clasificaciones  - STATUS VALIDACION", mailBody, "femcoepdev", filePath)
            Catch ex As Exception
                Throw
            End Try
        End Sub

#End Region

    End Class
End Namespace