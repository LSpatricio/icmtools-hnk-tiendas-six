Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Threading.Tasks
Imports Serilog

Public Class ArqueosService
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _sftpClient As SftpClient

    Public Sub New()
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _sftpClient = New SftpClient()
    End Sub

    Public Async Function ProcesarArqueos(
        request As ValidateFileRequest,
        idCarga As Guid,
        logger As ILogger) As Task(Of CargaResponse)

        Dim errores = Await ValidacionesArqueos(request)

        If errores.Any() Then
            Return New CargaResponse With {
                .Exitoso = False,
                .IdCarga = idCarga,
                .Errores = errores
            }
        End If

        logger.Information("No se encontraron errores de validacion en el archivo de Arqueos")
        Await _repository.EjecutarSPAsync("dbo.SP_VALIDATE_ARQUEOS", idCarga)
        logger.Information("Procedimiento almacenado SP_VALIDATE_ARQUEOS ejecutado correctamente")

        Return New CargaResponse With {
            .Exitoso = True,
            .IdCarga = idCarga,
            .Errores = New List(Of ExcelValidationError)()
        }
    End Function

    Public Async Function ValidacionesArqueos(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))
        Const tablaStaging As String = "STG_ARQUEOS"

        Dim valoresErrores As New List(Of ExcelValidationError)()
        Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)

        ' Se usa el mismo mecanismo compartido que el resto de las pantallas.
        Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) =
            _excelService.CrearMepeoAtributos(GetType(ArqueosExcelDto))

        ' Codigo de Listado se valida en la etapa /api/files/validate mediante
        ' ArqueosExcelDto, pero no forma parte de STG_ARQUEOS. Por eso no se
        ' incluye en el mapeo utilizado para la carga a staging.
        Dim mapeoCarga As Dictionary(Of PropertyInfo, ExcelColumnAttribute) =
            mapeoColumnas _
                .Where(Function(x) x.Key.Name <> NameOf(ArqueosExcelDto.CodigoListado)) _
                .ToDictionary(Function(x) x.Key, Function(x) x.Value)

        Await _repository.LimpiarStaging(tablaStaging)

        For i As Integer = 0 To cantidadHojas - 1
            valoresErrores.AddRange(
                Await _excelReader.CargaAsync(
                    request.Path,
                    request.HeaderRow,
                    i.ToString(),
                    mapeoCarga,
                    tablaStaging,
                    Nothing,
                    Nothing,
                    AddressOf ValidarFilaArqueos))
        Next

        If Not valoresErrores.Any() Then
            valoresErrores.AddRange(Await ValidarDuplicadosStagingAsync(tablaStaging))
        End If

        Return valoresErrores
    End Function

    Private Function ValidarFilaArqueos(
        fila As DataRow,
        Optional regionSelector As String = Nothing,
        Optional catalogos As CatalogosDto = Nothing) As String

        Dim errores As New List(Of String)()

        ' ExcelReader asigna DBNull.Value a los campos opcionales vacios.
        ' STG_ARQUEOS no admite NULL en estas columnas de texto, por lo que
        ' se normalizan a cadena vacia antes de que SqlBulkCopy inserte la fila.
        NormalizarTextoOpcionalParaStaging(fila, NameOf(ArqueosExcelDto.UsuarioAutorizador))
        NormalizarTextoOpcionalParaStaging(fila, NameOf(ArqueosExcelDto.UsuarioAutorizadorPerfil))
        NormalizarTextoOpcionalParaStaging(fila, NameOf(ArqueosExcelDto.TipoCierre))
        NormalizarTextoOpcionalParaStaging(fila, NameOf(ArqueosExcelDto.Comentario))

        Dim numeroSAP = fila.Field(Of Decimal?)(NameOf(ArqueosExcelDto.NumeroSAP))
        Dim codigoProducto = fila.Field(Of Decimal?)(NameOf(ArqueosExcelDto.CodigoProducto))
        Dim fechaCreacion As DateTime
        Dim fechaCreacionValida As Boolean = TryParseFechaArqueos(
            fila(NameOf(ArqueosExcelDto.FechaCreacion)),
            fechaCreacion)

        If Not fechaCreacionValida Then
            errores.Add("Fecha de Creacion debe contener una fecha valida.")
        Else
            ' Se normaliza antes del SqlBulkCopy. La columna del DataTable es String,
            ' pero el valor queda en formato ISO compatible con la columna DateTime de staging.
            fila(NameOf(ArqueosExcelDto.FechaCreacion)) =
                fechaCreacion.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        End If

        Dim fechaInicio = fila.Field(Of DateTime?)(NameOf(ArqueosExcelDto.FechaInicioConteo))
        Dim fechaFin = fila.Field(Of DateTime?)(NameOf(ArqueosExcelDto.FechaFinConteo))
        Dim fechaCierre = fila.Field(Of DateTime?)(NameOf(ArqueosExcelDto.FechaCierreConteo))
        Dim fechaTermino = fila.Field(Of DateTime?)(NameOf(ArqueosExcelDto.FechaTerminoConteo))
        Dim diferencia = fila.Field(Of Decimal?)(NameOf(ArqueosExcelDto.Diferencia))
        Dim faltante = fila.Field(Of Decimal?)(NameOf(ArqueosExcelDto.Faltante))
        Dim sobrante = fila.Field(Of Decimal?)(NameOf(ArqueosExcelDto.Sobrante))
        Dim comentario = fila.Field(Of String)(NameOf(ArqueosExcelDto.Comentario))

        If numeroSAP.HasValue AndAlso Decimal.Truncate(numeroSAP.Value) <> numeroSAP.Value Then
            errores.Add("No. SAP debe contener un numero entero.")
        End If

        If codigoProducto.HasValue AndAlso Decimal.Truncate(codigoProducto.Value) <> codigoProducto.Value Then
            errores.Add("Codigo de Producto debe contener un numero entero.")
        End If

        If fechaCreacionValida AndAlso fechaCreacion > DateTime.Now Then
            errores.Add("La fecha de creacion no puede ser posterior a la fecha de carga.")
        End If

        If fechaInicio.HasValue AndAlso fechaFin.HasValue AndAlso fechaInicio.Value > fechaFin.Value Then
            errores.Add("Inicio de Conteo debe ser menor o igual a Fin de Conteo.")
        End If

        If fechaFin.HasValue AndAlso fechaCierre.HasValue AndAlso fechaFin.Value > fechaCierre.Value Then
            errores.Add("Fin de Conteo debe ser menor o igual a Cierre de Conteo.")
        End If

        If fechaCierre.HasValue AndAlso fechaTermino.HasValue AndAlso fechaCierre.Value > fechaTermino.Value Then
            errores.Add("Cierre de Conteo debe ser menor o igual a Termino de Conteo.")
        End If

        If faltante.GetValueOrDefault() > 0D AndAlso sobrante.GetValueOrDefault() > 0D Then
            errores.Add("Faltante y Sobrante no pueden ser positivos al mismo tiempo.")
        End If

        If diferencia.HasValue Then
            If diferencia.Value > 0D AndAlso sobrante.GetValueOrDefault() <= 0D Then
                errores.Add("Si la diferencia es positiva, Sobrante debe ser mayor que cero.")
            End If

            If diferencia.Value < 0D AndAlso faltante.GetValueOrDefault() <= 0D Then
                errores.Add("Si la diferencia es negativa, Faltante debe ser mayor que cero.")
            End If

            If diferencia.Value = 0D AndAlso
                (faltante.GetValueOrDefault() > 0D OrElse sobrante.GetValueOrDefault() > 0D) Then
                errores.Add("Si la diferencia es cero, Faltante y Sobrante deben ser cero.")
            End If
        End If

        If comentario IsNot Nothing AndAlso comentario.Length > 500 Then
            errores.Add("Comentarios no puede exceder 500 caracteres.")
        End If

        If errores.Count = 0 Then Return Nothing
        Return String.Join("<br/>", errores)
    End Function

    Private Sub NormalizarTextoOpcionalParaStaging(fila As DataRow, columna As String)
        If fila.IsNull(columna) Then
            fila(columna) = String.Empty
        End If
    End Sub

    Private Function TryParseFechaArqueos(valor As Object, ByRef fecha As DateTime) As Boolean
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return False

        If TypeOf valor Is DateTime Then
            fecha = DirectCast(valor, DateTime)
            Return True
        End If

        Dim texto As String = Convert.ToString(valor, CultureInfo.InvariantCulture).Trim()
        If String.IsNullOrWhiteSpace(texto) Then Return False

        texto = System.Text.RegularExpressions.Regex.Replace(
            texto,
            "\ba\.?\s*m\.?",
            "AM",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        texto = System.Text.RegularExpressions.Regex.Replace(
            texto,
            "\bp\.?\s*m\.?",
            "PM",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        Dim formatos As String() = {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "yyyy-MM-dd hh:mm:ss tt",
            "yyyy-MM-dd hh:mm tt",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy",
            "d/MM/yyyy HH:mm:ss",
            "d/MM/yyyy HH:mm",
            "d/MM/yyyy",
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "MM/dd/yyyy",
            "M/d/yyyy HH:mm:ss",
            "M/d/yyyy HH:mm",
            "M/d/yyyy"
        }

        Return DateTime.TryParseExact(
            texto,
            formatos,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            fecha) OrElse
            DateTime.TryParse(
                texto,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                fecha) OrElse
            DateTime.TryParse(
                texto,
                CultureInfo.CurrentCulture,
                DateTimeStyles.AllowWhiteSpaces,
                fecha)
    End Function

    Private Async Function ValidarDuplicadosStagingAsync(tablaStaging As String) As Task(Of List(Of ExcelValidationError))
        Dim errores As New List(Of ExcelValidationError)()

        Dim sql As String = $"
            SELECT
                NumeroSAP,
                Almacen,
                TipoListado,
                FechaCreacion,
                UsuarioCreador,
                UsuarioAutorizador,
                UsuarioAutorizadorPerfil,
                FechaInicioConteo,
                CodigoProducto,
                COUNT(*) AS Cantidad
            FROM {tablaStaging}
            GROUP BY
                NumeroSAP,
                Almacen,
                TipoListado,
                FechaCreacion,
                UsuarioCreador,
                UsuarioAutorizador,
                UsuarioAutorizadorPerfil,
                FechaInicioConteo,
                CodigoProducto
            HAVING COUNT(*) > 1"

        Using connection As New SqlConnection(_configuration.ConnectionString)
            Await connection.OpenAsync()

            Using command As New SqlCommand(sql, connection)
                command.CommandTimeout = 600

                Using reader = Await command.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        errores.Add(New ExcelValidationError With {
                            .Problema = "El archivo contiene registros duplicados segun las llaves de Arqueos.",
                            .Detalle = $"Llave repetida ({reader("Cantidad")} veces): " &
                                $"{reader("NumeroSAP")} | {reader("Almacen")} | {reader("TipoListado")} | " &
                                $"{reader("FechaCreacion")} | {reader("UsuarioCreador")} | " &
                                $"{reader("UsuarioAutorizador")} | {reader("UsuarioAutorizadorPerfil")} | " &
                                $"{reader("FechaInicioConteo")} | {reader("CodigoProducto")}"
                        })
                    End While
                End Using
            End Using
        End Using

        Return errores
    End Function

    Public Async Function EnvioArqueos(request As SendInfoRequest, logger As ILogger) As Task
        If Not Directory.Exists(request.PathSalida) Then Directory.CreateDirectory(request.PathSalida)

        Dim rutaArchivo = Path.Combine(request.PathSalida, "BDIARQUEOS.csv")
        Dim consulta = "SELECT * FROM dbo.BDIARQUEOS ORDER BY NumeroSAP, Almacen, TipoListado, FechaCreacion, CodigoProducto"

        Await GenerarCsvArqueosAsync(consulta, rutaArchivo)
        logger.Information("Archivo CSV de Arqueos generado: {RutaArchivo}", rutaArchivo)

        Await _sftpClient.SubirArchivoAsync(rutaArchivo)
        logger.Information("Archivo CSV de Arqueos enviado al SFTP")
    End Function

    Private Async Function GenerarCsvArqueosAsync(consulta As String, rutaArchivo As String) As Task
        Using connection As New SqlConnection(_configuration.ConnectionString)
            Await connection.OpenAsync()

            Using command As New SqlCommand(consulta, connection)
                command.CommandTimeout = 600

                Using reader = Await command.ExecuteReaderAsync()
                    Using writer As New StreamWriter(rutaArchivo, False, New UTF8Encoding(True))
                        Dim encabezados = Enumerable.Range(0, reader.FieldCount) _
                            .Select(Function(indice) EscaparCsv(reader.GetName(indice)))
                        Await writer.WriteLineAsync(String.Join(";", encabezados))

                        While Await reader.ReadAsync()
                            Dim valores = Enumerable.Range(0, reader.FieldCount) _
                                .Select(Function(indice) EscaparCsv(ObtenerValorCsv(reader, indice)))
                            Await writer.WriteLineAsync(String.Join(";", valores))
                        End While
                    End Using
                End Using
            End Using
        End Using
    End Function

    Private Function ObtenerValorCsv(reader As SqlDataReader, indice As Integer) As String
        If reader.IsDBNull(indice) Then Return ""

        Dim valor = reader.GetValue(indice)
        If TypeOf valor Is DateTime Then
            Return DirectCast(valor, DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        End If

        Return Convert.ToString(valor, CultureInfo.InvariantCulture)
    End Function

    Private Function EscaparCsv(valor As String) As String
        If valor Is Nothing Then Return ""

        Dim requiereComillas = valor.Contains(";") OrElse valor.Contains("""") OrElse valor.Contains(vbCr) OrElse valor.Contains(vbLf)
        Dim resultado = valor.Replace("""", """""")
        Return If(requiereComillas, $"""{resultado}""", resultado)
    End Function
End Class
