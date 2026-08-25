Imports System.Configuration
Imports System.Globalization
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Imports System.Text
Imports Serilog

Public Class ReporteIngresosSIXController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _arqueosExcelReader As ArqueosExcelReader
    Private ReadOnly _sftpClient As SftpClient

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _arqueosExcelReader = New ArqueosExcelReader()
        _sftpClient = New SftpClient()
    End Sub

    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/reporteingresossix/validarinfo")>
    Public Function ValidarInfo(<FromBody> request As ValidateFileRequestt) As IHttpActionResult
        Try
            CrearLogger("ValidarArchivo").Information("Inicio de validación del archivo de SA132. Ruta: {Ruta}", If(request Is Nothing, Nothing, request.Path))
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing
            Dim tipoHoja As Type = GetType(ArqueosDetalleExcelDto)
            Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(tipoHoja)
            Dim valoresErrores As List(Of ExcelValidationError) =
                _arqueosExcelReader.ValidacionesArqueosTodasLasHojas(request.Path, 1, mapeoColumnas)

            If valoresErrores.Count > 0 Then
                CrearLogger("ValidarArchivo").Warning("El archivo de SA132 contiene {CantidadErrores} errores de validación", valoresErrores.Count)
                For Each errores In valoresErrores
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
            End If

            Dim rTable As String = sc.GetMessage("SA132", "CargaCompleta")
            CrearLogger("ValidarArchivo").Information("Validación del archivo de SA132 completada correctamente")
            Return Ok(New With {.d = True, .path = request.Path, .f = request.Path, .r = rTable})
        Catch ex As Exception
            CrearLogger("ValidarArchivo").Error(ex, "Error durante la validación del archivo de SA132")
            Return Ok(New With {
                .d = False,
                .r = ex.Message
            })
        End Try
    End Function

    <HttpGet>
    <Route("api/reporteingresossix/periodos")>
    Public Async Function ObtenerPeriodos() As Task(Of IHttpActionResult)
        Try
            Dim modelo As String = If(mUser Is Nothing, Nothing, mUser.Model)

            If String.IsNullOrWhiteSpace(modelo) Then
                Return Ok(New With {.d = False, .r = "No se encontró el modelo de ICM Cloud para el usuario."})
            End If

            Dim consulta As String = ConfigurationManager.AppSettings("ICM_PERIOD_QUERY")
            If String.IsNullOrWhiteSpace(consulta) Then
                consulta = "SELECT DISTINCT ""IDPeriodString"" FROM ""CfgDateStringPeriods"" ORDER BY ""IDPeriodString"" DESC"
            End If

            Dim respuesta As IcmQueryResponseDto = Await New IcmApiClient().Query(
                New IcmQueryRequestDto With {
                    .QueryString = consulta,
                    .Offset = 0,
                    .Limit = 1000
                }, modelo)

            Dim periodos As New List(Of Object)

            If respuesta IsNot Nothing AndAlso respuesta.Data IsNot Nothing Then
                For Each fila In respuesta.Data
                    If fila Is Nothing OrElse fila.Count < 1 Then Continue For

                    Dim periodo As String = Convert.ToString(fila(0), CultureInfo.InvariantCulture).Trim()
                    If String.IsNullOrWhiteSpace(periodo) Then Continue For

                    periodos.Add(New With {
                        .Value = periodo,
                        .Text = periodo
                    })
                Next
            End If

            Return Ok(New With {.d = True, .periodos = periodos})
        Catch ex As Exception
            Return Ok(New With {.d = False, .r = ex.Message})
        End Try
    End Function

    <HttpPost>
    <Route("api/reporteingresossix/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Dim idCarga As Guid = Guid.NewGuid()
        Try
            CrearLogger("CargarInformacion", idCarga).Information("Inicio de carga de información de SA132")
            Thread.Sleep(500)

            Dim sqlConnSetting = ConfigurationManager.ConnectionStrings("SQLSERVER_CONNECTION")
            Dim connStr As String = If(sqlConnSetting IsNot Nothing, sqlConnSetting.ConnectionString, Nothing)

            If String.IsNullOrWhiteSpace(connStr) Then
                Return InternalServerError(New InvalidOperationException("No se encontró la cadena de conexión SQLSERVER_CONNECTION en Web.config."))
            End If

            Dim rutaArchivo As String = ObtenerRutaArchivoCarga(request)
            If String.IsNullOrWhiteSpace(rutaArchivo) OrElse Not File.Exists(rutaArchivo) Then
                Return Ok(New With {.d = False, .r = "No se encontró el archivo cargado en el servidor."})
            End If

            Dim tablaStg As DataTable = _arqueosExcelReader.ObtenerDataTableStgArqueos(rutaArchivo)
            CrearLogger("CargarInformacion", idCarga).Information("Archivo leído correctamente. Registros preparados: {CantidadRegistros}", tablaStg.Rows.Count)

            Using conn As New SqlConnection(connStr)
                conn.Open()

                Using tran As SqlTransaction = conn.BeginTransaction()
                    Try
                        Using deleteCmd As New SqlCommand("DELETE FROM dbo.STG_ARQUEOS;", conn, tran)
                            deleteCmd.ExecuteNonQuery()
                        End Using

                        Using bulkCopy As New SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran)
                            bulkCopy.DestinationTableName = "dbo.STG_ARQUEOS"
                            bulkCopy.BatchSize = 1000
                            bulkCopy.BulkCopyTimeout = 0

                            For Each col As DataColumn In tablaStg.Columns
                                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName)
                            Next

                            bulkCopy.WriteToServer(tablaStg)
                        End Using

                        tran.Commit()
                    Catch
                        tran.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            CrearLogger("CargarInformacion", idCarga).Information("Registros insertados correctamente en STG_ARQUEOS: {CantidadRegistros}", tablaStg.Rows.Count)

            Using db As New DataBase(connStr)
                db.ExecuteStoredProcedure(
                    "dbo.SP_VALIDATE_ARQUEOS",
                    DataBase.EnumExecutionType.NonQuery,
                    New SqlParameter("@IdCarga", SqlDbType.UniqueIdentifier) With {.Value = idCarga}
                )
            End Using

            CrearLogger("EnviarInformacion", idCarga).Information("SP_VALIDATE_ARQUEOS ejecutado correctamente")

            Dim csvPath As String = ExportarBdiArqueosCsv(connStr)
            CrearLogger("EnviarInformacion", idCarga).Information("CSV BDIARQUEOS generado correctamente. Ruta: {RutaCsv}", csvPath)
            EnviarCsvSftpEnSegundoPlano(csvPath)

            Dim rTable As String = sc.GetMessage("SA132", "CargaCompleta")
            Return Ok(New With {.d = True, .r = rTable, .rows = tablaStg.Rows.Count, .csv = csvPath, .idCarga = idCarga})
        Catch ex As Exception
            CrearLogger("CargarInformacion", idCarga).Error(ex, "Error durante la carga de SA132")
            Return Ok(New With {
                .d = False,
                .r = ex.Message
            })
        End Try
    End Function

    Private Sub EnviarCsvSftpEnSegundoPlano(rutaCsv As String)
        Task.Run(
            Async Function()
                Try
                    Await _sftpClient.SubirArchivoAsync(rutaCsv)
                    CrearLogger("EnviarInformacion").Information("CSV BDIARQUEOS enviado al SFTP correctamente. Ruta: {RutaCsv}", rutaCsv)
                Catch ex As Exception
                    CrearLogger("EnviarInformacion").Error(ex, "Error al enviar el CSV BDIARQUEOS al SFTP. Ruta: {RutaCsv}", rutaCsv)
                    ' La generación del CSV no debe quedar bloqueada por una demora del SFTP.
                End Try
            End Function)
    End Sub

    Private Function ExportarBdiArqueosCsv(connStr As String) As String
        Dim carpetaSalida As String = "C:\Users\dsuazo\OneDrive - Excelencia en Soluciones Informaticas SA\Escritorio\csv prueba"
        Directory.CreateDirectory(carpetaSalida)

            Dim nombreArchivo As String = "BDIARQUEOS.csv"
        Dim rutaSalida As String = Path.Combine(carpetaSalida, nombreArchivo)

        Using conn As New SqlConnection(connStr)
            conn.Open()

            Using cmd As New SqlCommand("SELECT * FROM dbo.BDIARQUEOS ORDER BY NumeroSAP, Almacen, TipoListado, FechaCreacion, CodigoProducto", conn)
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    Using writer As New StreamWriter(rutaSalida, False, New UTF8Encoding(True))
                        Dim headers(reader.FieldCount - 1) As String
                        For i As Integer = 0 To reader.FieldCount - 1
                            headers(i) = EscapeCsvValue(reader.GetName(i))
                        Next
                        writer.WriteLine(String.Join(";", headers))

                        While reader.Read()
                            Dim values(reader.FieldCount - 1) As String
                            For i As Integer = 0 To reader.FieldCount - 1
                                values(i) = EscapeCsvValue(GetCsvValue(reader, i))
                            Next
                            writer.WriteLine(String.Join(";", values))
                        End While
                    End Using
                End Using
            End Using
        End Using

        Return rutaSalida
    End Function

    Private Function GetCsvValue(reader As SqlDataReader, index As Integer) As String
        If reader.IsDBNull(index) Then
            Return ""
        End If

        Dim value As Object = reader.GetValue(index)

        If TypeOf value Is DateTime Then
            Return DirectCast(value, DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        End If

        If TypeOf value Is Decimal OrElse TypeOf value Is Double OrElse TypeOf value Is Single Then
            Return Convert.ToString(value, CultureInfo.InvariantCulture)
        End If

        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Function EscapeCsvValue(value As String) As String
        If value Is Nothing Then
            Return ""
        End If

        Dim needsQuotes As Boolean = value.Contains(";") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf)
        Dim escaped As String = value.Replace("""", """""")

        If needsQuotes Then
            Return $"""{escaped}"""
        End If

        Return escaped
    End Function

    Private Function ObtenerRutaArchivoCarga(request As ValidateFileRequest) As String
        Dim rawFileType As String = "SA132"
        Dim rawExtension As String = ".xlsx"

        If request IsNot Nothing Then
            If Not String.IsNullOrWhiteSpace(request.FileType) Then
                rawFileType = request.FileType
            End If

            If Not String.IsNullOrWhiteSpace(request.Extension) Then
                rawExtension = request.Extension
            End If
        End If

        Dim userEmail As String = CType(HttpContext.Current.Session.Item("User"), User).Email

        Dim baseDir As String = Path.GetFullPath(HttpContext.Current.Server.MapPath("~/UploadedFiles/"))
        If Not baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()) Then
            baseDir &= Path.DirectorySeparatorChar
        End If

        Dim fileName As String = Path.GetFileName(userEmail & rawExtension)
        Dim fullPath As String = Path.GetFullPath(Path.Combine(baseDir, rawFileType, fileName))

        If Not fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase) Then
            Return Nothing
        End If

        Return fullPath
    End Function

    Private Function CrearLogger(proceso As String, Optional idCarga As Nullable(Of Guid) = Nothing) As ILogger
        Dim logger As ILogger = Log.ForContext("Pantalla", "SA132") _
            .ForContext("Proceso", proceso) _
            .ForContext("Usuario", ObtenerUsuarioEmail())

        If idCarga.HasValue Then
            logger = logger.ForContext("IdCarga", idCarga.Value)
        End If

        Return logger
    End Function

    Private Function ObtenerUsuarioEmail() As String
        If HttpContext.Current Is Nothing OrElse HttpContext.Current.Session Is Nothing Then
            Return Nothing
        End If

        Dim usuario As User = TryCast(HttpContext.Current.Session.Item("User"), User)
        Return If(usuario Is Nothing, Nothing, usuario.Email)
    End Function
End Class
