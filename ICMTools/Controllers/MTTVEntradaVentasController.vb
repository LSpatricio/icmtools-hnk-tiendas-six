Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Net.Sockets
Imports System.Threading
Imports System.Web.Http
Imports ClassLibrary_PGP_TO_SFTP
Imports DocumentFormat.OpenXml.Wordprocessing
Imports ICMTools.Controllers.ClasificacionesController
Imports ICMTools.TiendasController
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class MTTVEntradaVentasController
    Inherits ApiController

    Private ReadOnly mUser As User
    Private ReadOnly mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
    End Sub

    ReadOnly fc As New FileController()
    ReadOnly sc As New SharedController()
    Public Class Registro
        Public IDPLAZA As String
        Public CRTIENDA As String
        Public FECHA As String
        Public TRAFICO As Decimal
        Public VTACONTABLE As Decimal
    End Class

    Private scenario As Integer = Nothing
    Dim tBody As String = Nothing
    Private success As New DataTable()

    <HttpPost>
    <Route("api/mttventradaventas/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim rTable As String = Nothing
            Dim filePath As String = Nothing
            Dim respuesta As Int32 = 0
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim lstExcel As List(Of Registro) = ObtenerExcel(ExcelArray)
            If lstExcel.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})

            Dim jTable As String = JsonConvert.SerializeObject(lstExcel)
            Dim xlsx As New DataTable()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                Dim jCatalogos As List(Of String) = ObtenerCatalogos()

                success = EjecutarProceso(jTable, jCatalogos)

                If success.Rows.Count > 0 Then
                    filePath = GetParcials(success)
                End If

                If jTable.Count <> success.Rows.Count Then
                    SendSFTP()
                End If

                rTable = MostrarMensaje(success, filePath, lstExcel.Count)
            End Using

            Return Ok(New With {
                    .d = scenario,
                    .r = rTable,
                    .f = filePath
                })
        Catch ex As Exception
            mLog.insertLog("MTTVEntradaVentasController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    Function ObtenerExcel(ExcelArray As Object) As List(Of Registro)
        Dim jTable As New List(Of Registro)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim IDPLAZA As String = ExcelArray(row, 1).ToString().Trim()
            Dim CRTIENDA As String = ExcelArray(row, 2).ToString().Trim()
            Dim FECHA As Date = ParseDate(ExcelArray(row, 3))
            Dim TRAFICO As Decimal = Decimal.Parse(ExcelArray(row, 4).ToString())
            Dim VTACONTABLE As Decimal = Decimal.Parse(ExcelArray(row, 5).ToString())

            If String.IsNullOrWhiteSpace(IDPLAZA) AndAlso String.IsNullOrWhiteSpace(CRTIENDA) Then Continue For

            jTable.Add(New Registro With {
                        .IDPLAZA = IDPLAZA,
                        .CRTIENDA = CRTIENDA,
                        .FECHA = FECHA.ToString("yyyy/MM/dd HH:mm:ss"),
                        .TRAFICO = TRAFICO,
                        .VTACONTABLE = VTACONTABLE
                    })
        Next

        Return jTable
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
        Dim formatos() As String = {"dd/MM/yyyy", "dd-MM-yyyy", "yyyy/MM/dd", "yyyy-MM-dd"}

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

    Function ObtenerCatalogos() As List(Of String)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Try
            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcoepdev"
            End If
            Dim modelo As String = ConfigurationManager.AppSettings("ModelFemcoEPDev")
            Using ws As New WebServiceICMGeneral()
                columnas = New List(Of String) From {"IDStore"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatStore", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDPlaza", "IDStatus"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatPlaza", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using
            End Using
            Return jCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function EjecutarProceso(jtable As String, jCatalogos As List(Of String)) As DataTable
        Dim xlsx As New DataTable
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Dim sql As String = "SELECT * FROM public.z_mt_inc_variable_entrada_vtas(@jtable, @catstoretable, @catplazatable);"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("jtable", NpgsqlDbType.Json, jtable)
                    cmd.Parameters.AddWithValue("catstoretable", NpgsqlDbType.Json, jCatalogos(0))
                    cmd.Parameters.AddWithValue("catplazatable", NpgsqlDbType.Json, jCatalogos(1))
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(xlsx)
                    End Using
                End Using
            End Using
            Return xlsx
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function GetParcials(success As DataTable) As String
        Try
            Dim filePath As String = Nothing

            If success.Rows.Count > 0 Then
                filePath = fc.BuildXlsx(success, "MultiTiendaVariable_EntradaVentas")
            End If

            Return filePath
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function MostrarMensaje(success As DataTable, filePath As String, rowCount As Integer) As String
        If rowCount > 0 AndAlso rowCount = success.Rows.Count Then
            tBody = $"
                <tr>
                    <td>Error al ejecutar el proceso de importación del archivo de MultiTienda Variable Entrada Ventas</td>
                     <td>No se encontró información válida para importar <br>Por favor verifique la información del archivo</td>
                </tr>"
            scenario = 3
            Return sc.TableBuilder(tBody, 1)
        End If

        If rowCount > 0 And filePath Is Nothing Then
            tBody = $"
                <tr>
                    <td>Ejecución Completada Exitosamente</td>
                    <td>Se ejecutó correctamente el proceso externo
                        <br><strong>Carga de MultiTienda Variable Entrada Ventas</strong>
                    </td>
                </tr>"
            scenario = 1
        Else
            tBody = $"
                <tr>
                    <td>Ejecución Completada Parcialmente</td>
                    <td>Se ejecutó parcialmente el proceso externo
                        <br><strong>Carga de MultiTienda Variable Entrada Ventas, por favor revise el archivo descargado para validar errores</strong>
                    </td>
                </tr>"
            scenario = 2
        End If

        Return sc.TableBuilder(tBody, 3)

    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.EntradaVentas
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class