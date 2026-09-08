Imports System.IO
Imports System.Globalization
Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Text.RegularExpressions
Imports ExcelDataReader
Imports Serilog

Public Class RenegociacionesService
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _sftpClient As SftpClient
    Private ReadOnly _catalogoService As CatalogoService
    Private ReadOnly mUser As User

    Public Sub New()
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _sftpClient = New SftpClient()
        _catalogoService = New CatalogoService()
        mUser = CType(HttpContext.Current.Session.Item("User"), User)
    End Sub

    Public Async Function ProcesarRenegociaciones(request As ValidateFileRequest, idCarga As Guid, logger As ILogger) As Task(Of CargaResponse)
        Dim errores = Await ValidacionesRenegociaciones(request)
        errores.AddRange(Await _repository.ValidarDuplicadosAsync("STG_RENEGOCIACIONES", "BDIRENEGOCIACIONES"))

        If errores.Any(Function(x) Not x.Advertencia) Then
            Return New CargaResponse With {.Exitoso = False, .IdCarga = idCarga, .Errores = errores}
        End If

        Await _repository.EjecutarSPAsync("dbo.SP_VALIDATE_RENEGOCIACIONES", idCarga)
        logger.Information("Procedimiento almacenado de renegociaciones ejecutado correctamente")

        Return New CargaResponse With {.Exitoso = True, .IdCarga = idCarga, .Errores = errores}
    End Function

    Public Async Function ValidacionesRenegociaciones(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))
        Dim tipo As Type = Type.GetType(request.FileClass)
        Dim hoja As Type = _excelService.ObtenerTipos(tipo).FirstOrDefault()
        Dim propiedadHoja = tipo.GetProperties().First(Function(p) p.PropertyType.IsGenericType AndAlso p.PropertyType.GetGenericArguments()(0) = hoja)
        Dim atributo = propiedadHoja.GetCustomAttributes(GetType(ExcelSheetAttribute), False).Cast(Of ExcelSheetAttribute)().First()
        Dim mapeoColumnas = _excelService.CrearMepeoAtributos(hoja)

        Await _repository.LimpiarStaging(atributo.TableName)

        Dim regiones = Await _catalogoService.ObtenerRegiones(mUser.Model)
        Dim catalogos = New CatalogosDto With {
            .Regiones = New HashSet(Of String)(regiones.Select(Function(r) r.Description), StringComparer.OrdinalIgnoreCase)
        }

        Return Await CargarRenegociacionesDesdeHojaAsync(
            request,
            atributo,
            mapeoColumnas,
            request.Region,
            catalogos)
    End Function

    Private Async Function CargarRenegociacionesDesdeHojaAsync(
        request As ValidateFileRequest,
        hoja As ExcelSheetAttribute,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute),
        regionSelector As String,
        catalogos As CatalogosDto) As Task(Of List(Of ExcelValidationError))

        Dim errores As New List(Of ExcelValidationError)()
        Dim erroresEncabezados = _excelReader.ValidarEncabezadosExcel(
            GetType(RenegociacionesCptReNegociacionesExcelDto),
            request.Path,
            hoja.HeaderRow,
            hoja.SheetName,
            mapeoColumnas)

        If erroresEncabezados.Any() Then Return erroresEncabezados

        Dim mesPeriodo = ObtenerMesPeriodo(request.Period)
        If Not mesPeriodo.HasValue Then
            Return New List(Of ExcelValidationError) From {
                New ExcelValidationError With {
                    .Problema = "El periodo seleccionado no tiene un valor numérico válido.",
                    .Detalle = $"Valor recibido: '{request.Period}'."
                }
            }
        End If

        Dim indices = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
            {"Rev", 2}, {"Cliente", 3}, {"Negocio", 4}, {"Region", 5}, {"GZ", 6},
            {"Mes", 7}, {"Semana", 8}, {"Motivo", 9}, {"TipoRenta", 10}, {"Renegociador", 11},
            {"NumeroEmpleado", 12}, {"NumeroContrato", 13}, {"FechaInicio", 14}, {"FechaTermino", 15},
            {"RentaAnterior", 16}, {"MismoContrato", 17}, {"NumeroContratoNuevo", 18}, {"Estatus", 19},
            {"FechaInicioNuevo", 20}, {"FechaTerminoNuevo", 21}, {"NuevaRenta", 22}, {"MesesAdelantados", 23},
            {"TotalAdto", 24}, {"RentaAntesConInflacion", 26}, {"PorcentajeIncremento", 27}, {"ImpactoAnual", 28},
            {"FlujoAnual", 29}, {"Inflacion", 31}, {"MesInicioContrato", 32}, {"AnioInicioContrato", 33},
            {"MesesAPagarAnuales", 34}, {"AnualSinInflacion", 35}, {"TipoRentaFiscal", 36}, {"Renegociadas", 38},
            {"AportacionComerciante", 39}, {"NuevoIncremento", 40}, {"ContratoConAgua", 41}, {"Subsidio", 42},
            {"Rev1", 43}, {"Rev2", 44}, {"RevContrato", 45}, {"FormulaSub", 46}, {"Rev3", 47},
            {"RevCeco", 48}, {"Rev4", 49}, {"Excepciones", 50}, {"JOS", 51}, {"RenegociacionVSPlan", 52},
            {"IncrementoDecremento", 53}, {"Aportacion", 54}, {"Resultado", 55}, {"ComentariosRegion", 56},
            {"RentaConAgua", 57}, {"MontoAgua", 58}
        }

        Dim dataTable = _excelService.CrearDataTable(mapeoColumnas)
        dataTable.Columns.Add("MesPeriodo", GetType(Long))
        dataTable.Columns("MesPeriodo").SetOrdinal(0)

        Using stream = File.Open(request.Path, FileMode.Open, FileAccess.Read)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                If Not MoverAHoja(reader, hoja.SheetName) Then
                    Return New List(Of ExcelValidationError) From {
                        New ExcelValidationError With {.Problema = $"La hoja '{hoja.SheetName}' no existe en el archivo Excel."}
                    }
                End If

                For i As Integer = 1 To hoja.HeaderRow
                    reader.Read()
                Next
                reader.Read()

                Dim filaExcel As Integer = hoja.HeaderRow + 1
                Do
                    Dim tieneInformacion = False
                    For Each indice In indices.Values
                        Dim valor = reader.GetValue(indice)
                        If valor IsNot Nothing AndAlso valor IsNot DBNull.Value AndAlso Not String.IsNullOrWhiteSpace(valor.ToString()) Then
                            tieneInformacion = True
                            Exit For
                        End If
                    Next
                    If Not tieneInformacion Then Exit Do

                    Dim fila = dataTable.NewRow()
                    fila("MesPeriodo") = mesPeriodo.Value
                    Dim filaValida = True

                    For Each mapeo In mapeoColumnas
                        Dim indice = indices(mapeo.Key.Name)
                        Dim valor = reader.GetValue(indice)
                        If valor Is Nothing OrElse valor Is DBNull.Value OrElse String.IsNullOrWhiteSpace(valor.ToString()) Then
                            If mapeo.Value.Requerido Then
                                filaValida = False
                                errores.Add(New ExcelValidationError With {
                                    .Problema = $"La columna '{mapeo.Value.ColumnName}' no admite valores vacíos.",
                                    .Detalle = $"Columna sin información en la fila {filaExcel}. Hoja <strong>{hoja.SheetName}</strong>."
                                })
                            ElseIf mapeo.Key.PropertyType = GetType(Decimal) Then
                                fila(mapeo.Key.Name) = 0D
                            Else
                                fila(mapeo.Key.Name) = DBNull.Value
                            End If
                        ElseIf Not EsTipoValidoRenegociacion(valor, mapeo.Key.PropertyType) Then
                            filaValida = False
                            errores.Add(New ExcelValidationError With {
                                .Problema = $"La columna '{mapeo.Value.ColumnName}' requiere {_excelService.ObtenerDescripcionTipo(mapeo.Key.PropertyType)}.",
                                .Detalle = $"Valor:'{valor}'. Fila {filaExcel}. Hoja <strong>{hoja.SheetName}</strong>."
                            })
                        Else
                            If String.Equals(mapeo.Key.Name, "Mes", StringComparison.OrdinalIgnoreCase) Then
                                fila(mapeo.Key.Name) = NormalizarMesRenegociacion(valor)
                            Else
                                fila(mapeo.Key.Name) = NormalizarValorRenegociacion(valor, mapeo.Key.PropertyType)
                            End If
                        End If
                    Next

                    If filaValida Then
                        Dim errorFiltro = ValidarFiltroRenegociaciones(fila, regionSelector, catalogos)
                        If Not String.IsNullOrWhiteSpace(errorFiltro) Then
                            filaValida = False
                            errores.Add(New ExcelValidationError With {.Problema = errorFiltro, .Detalle = $"Fila {filaExcel}. Hoja <strong>{hoja.SheetName}</strong>."})
                        End If
                    End If

                    If filaValida Then dataTable.Rows.Add(fila)
                    If dataTable.Rows.Count >= 50000 Then
                        Await _repository.InsertarBatch(hoja.TableName, dataTable)
                        dataTable.Clear()
                    End If

                    filaExcel += 1
                Loop While reader.Read()
            End Using
        End Using

        If dataTable.Rows.Count > 0 Then Await _repository.InsertarBatch(hoja.TableName, dataTable)
        Return errores
    End Function

    Private Function ObtenerMesPeriodo(periodo As String) As Long?
        If String.IsNullOrWhiteSpace(periodo) Then Return Nothing

        Dim valor As Long
        If Long.TryParse(periodo.Trim(), valor) Then Return valor

        Dim coincidencia = Regex.Match(periodo, "MONTH\s+(\d+)", RegexOptions.IgnoreCase)
        If coincidencia.Success AndAlso Long.TryParse(coincidencia.Groups(1).Value, valor) Then Return valor

        Return Nothing
    End Function

    Private Function EsTipoValidoRenegociacion(valor As Object, tipo As Type) As Boolean
        If tipo = GetType(Decimal) Then
            Dim resultado As Decimal
            Return Decimal.TryParse(
                valor.ToString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                resultado) OrElse
                Decimal.TryParse(
                    valor.ToString(),
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    resultado)
        End If

        Return _excelService.EsTipoValido(valor, tipo)
    End Function

    Private Function NormalizarValorRenegociacion(valor As Object, tipo As Type) As Object
        If tipo = GetType(Decimal) Then
            Dim resultado As Decimal
            If Decimal.TryParse(valor.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, resultado) Then
                Return Decimal.Round(resultado, 14, MidpointRounding.AwayFromZero)
            End If
            Return Decimal.Round(
                Decimal.Parse(valor.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture),
                14,
                MidpointRounding.AwayFromZero)
        End If

        Return valor
    End Function

    Private Function NormalizarMesRenegociacion(valor As Object) As String
        Dim fecha As DateTime

        If TypeOf valor Is DateTime Then
            fecha = DirectCast(valor, DateTime)
            Return fecha.ToString("MMMM-yyyy", CultureInfo.GetCultureInfo("es-MX")).ToLowerInvariant()
        End If

        Dim numero As Double
        If Double.TryParse(valor.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, numero) Then
            fecha = DateTime.FromOADate(numero)
            Return fecha.ToString("MMMM-yyyy", CultureInfo.GetCultureInfo("es-MX")).ToLowerInvariant()
        End If

        Return valor.ToString()
    End Function

    Private Function MoverAHoja(reader As IExcelDataReader, nombreHoja As String) As Boolean
        Do
            If String.Equals(reader.Name, nombreHoja, StringComparison.OrdinalIgnoreCase) Then Return True
        Loop While reader.NextResult()
        Return False
    End Function

    Public Function ValidarFiltroRenegociaciones(fila As DataRow, regionSelector As String, catalogos As CatalogosDto) As String
        If String.IsNullOrWhiteSpace(regionSelector) Then Return Nothing

        Dim regionFila = fila.Field(Of String)("Region")
        If String.Equals(regionSelector, "Todas", StringComparison.OrdinalIgnoreCase) Then
            If catalogos Is Nothing OrElse catalogos.Regiones Is Nothing OrElse Not catalogos.Regiones.Contains(regionFila) Then
                Return $"La región {regionFila} no pertenece al catálogo de regiones válido."
            End If
        ElseIf Not String.Equals(regionFila, regionSelector, StringComparison.OrdinalIgnoreCase) Then
            Return $"El registro no corresponde a la región seleccionada: {regionSelector}."
        End If

        Return Nothing
    End Function

    Public Async Function EnvioRenegociaciones(request As SendInfoRequest, logger As ILogger) As Task
        If Not Directory.Exists(request.PathSalida) Then Directory.CreateDirectory(request.PathSalida)

        Dim rutaArchivo = Path.Combine(request.PathSalida, "BDIRENEGOCIACIONES.csv")
        Dim sql As String = "SELECT MesPeriodo, Rev, Cliente, Negocio, Region, GZ, Mes, Semana, Motivo, TipoRenta, Renegociador, NumeroEmpleado, NumeroContrato, FORMAT(FechaInicio, 'dd/MM/yyyy') AS FechaInicio, FORMAT(FechaTermino, 'dd/MM/yyyy') AS FechaTermino, RentaAnterior, MismoContrato, NumeroContratoNuevo, Estatus, FORMAT(FechaInicioNuevo, 'dd/MM/yyyy') AS FechaInicioNuevo, FORMAT(FechaTerminoNuevo, 'dd/MM/yyyy') AS FechaTerminoNuevo, NuevaRenta, MesesAdelantados, TotalAdto, RentaAntesConInflacion, PorcentajeIncremento, ImpactoAnual, FlujoAnual, Inflacion, MesInicioContrato, AnioInicioContrato, MesesAPagarAnuales, AnualSinInflacion, TipoRentaFiscal, Renegociadas, AportacionComerciante, NuevoIncremento, ContratoConAgua, Subsidio, Rev1, Rev2, RevContrato, FormulaSub, Rev3, RevCeco, Rev4, Excepciones, JOS, RenegociacionVSPlan, IncrementoDecremento, Aportacion, Resultado, ComentariosRegion, RentaConAgua, MontoAgua FROM BDIRENEGOCIACIONES WHERE IdCarga = @IdCarga"

        Await _repository.GenerarCsvAsync(sql, rutaArchivo, request.IdGui)
        Await _sftpClient.SubirArchivoAsync(rutaArchivo)
        logger.Information("Archivo de renegociaciones enviado al SFTP")
    End Function
End Class
