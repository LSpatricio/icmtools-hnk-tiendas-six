Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports Serilog

Public Class EstructuraNegociosService

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _sftpClient As SftpClient
    Private ReadOnly _catalogoService As CatalogoService
    Public Sub New()
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _sftpClient = New SftpClient()
        _catalogoService = New CatalogoService()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)


    End Sub

    Public Async Function ProcesarEstructuraNegocios(request As ValidateFileRequest, idCarga As Guid, logger As ILogger) As Task(Of CargaResponse)

        Dim tablaStaging As String = "STG_ESTRUCTURANEGOCIOS"
        Dim tablaDestino As String = "BDIESTRUCTURANEGOCIOS"
        Dim sp As String = "SP_VALIDATE_ESTRUCTURANEGOCIOS"

        Dim errores = Await ValidacionesEstructuraNegocios(request)

        errores.AddRange(Await _repository.ValidarDuplicadosAsync(
                tablaStaging,
                tablaDestino))

        If errores.Any() Then

            Return New CargaResponse With {
            .Exitoso = False,
            .IdCarga = idCarga,
            .Errores = errores
        }
        End If



        logger.Information("No se encontraron errores de validación en el archivo de Estructura de Negocios. Procediendo a ejecutar el procedimiento almacenado para validar la información.")

        Await _repository.EjecutarSPAsync(
            $"dbo.{sp}",
            idCarga
        )

        logger.Information("Procedimiento almacenado {sp} ejecutado correctamente", sp)

        Return New CargaResponse With {
        .Exitoso = True,
        .IdCarga = idCarga,
        .Errores = New List(Of ExcelValidationError)()
    }

    End Function



    Public Async Function ValidacionesEstructuraNegocios(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))

        Dim errorsList As String = Nothing
        Dim tableName As String = "STG_ESTRUCTURANEGOCIOS"

        Dim tipo As Type = Type.GetType(request.FileClass)

        Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

        Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)

        Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(tipo)

        Await _repository.LimpiarStaging(tableName)

        Dim regionesCatalogo As List(Of RegionDto) = Await _catalogoService.ObtenerRegiones(mUser.Model)
        Dim regionesValidas As New HashSet(Of String)((Await _catalogoService.ObtenerRegiones(mUser.Model)).Select(Function(r) r.Description), StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To cantidadHojas - 1


            valoresErrores.AddRange(
                        Await _excelReader.CargaAsync(
                            request.Path,
                            request.HeaderRow,
                            i.ToString(),
                            mapeoColumnas,
                            tableName,
                            request.Region,
                            AddressOf ValidarFiltroEstructuraNegociosAsync))
        Next

        Return valoresErrores



    End Function

    Public Async Function EnvioEstructuraNegocios(request As SendInfoRequest, logger As ILogger) As Task

        If Not Directory.Exists(request.PathSalida) Then
            Directory.CreateDirectory(request.PathSalida)
        End If

        Dim nombreArchivo As String = "BDIESTRUCTURANEGOCIOS.csv"

        Dim rutaArchivo As String = Path.Combine(request.PathSalida, nombreArchivo)


        Dim sql As String = "
                  SELECT
                 Ceco
                ,Descripcion
                ,Region
                ,GZ
                ,EstatusTienda
                ,NumeroComerciante
                ,NombreComerciante
                ,FORMAT(FechaIngreso, 'dd/MM/yyyy') AS FechaIngreso
                ,EstatusSK
                ,FORMAT(FechaMovimiento, 'dd/MM/yyyy') AS FechaMovimiento
                ,TelefonoSK
                ,CorreoSK
                ,GOS
                ,CveJOS
                ,CveAcsComercial
                ,CveAcsControl
                ,EmpleadoJOS
                ,NombreJOS
                ,NumeroEmpleadoAcsCom
                ,NombreAcsComercial
                ,CelularAcsComercial
                ,CorreoACSComercial
                ,NumeroEmpleadoAcsControl
                ,NombreAcsControl
                ,CelularAcsControl
                ,CorreoAcsControl
                ,GZSIX2
                ,CveJOSVal
                ,CveAcsComercialVal
                ,CveAcsControlVal
                ,NumeroEmpleadoAtraccion
                ,NombreEmpleadoAtraccion
                ,CelularRedAtraccion
                ,NumeroEmpleadoCoordinador
                ,NombreCoordinador
                    FROM BDIESTRUCTURANEGOCIOS
                   WHERE IdCarga = @IdCarga
                "

        Await _repository.GenerarCsvAsync(
                                sql,
                                rutaArchivo,
                                request.IdGui
                            )

        logger.Information("Archivo CSV generado correctamente {rutaArchivo}", rutaArchivo)

        Await _sftpClient.SubirArchivoAsync(rutaArchivo)

        logger.Information("Archivo enviado al SFTP")

    End Function

    Public Async Function ValidarFiltroEstructuraNegociosAsync(fila As DataRow, Optional regionSelector As String = Nothing) As Task(Of String)


        If regionSelector IsNot Nothing Then

            If Not String.Equals(regionSelector, "Todas", StringComparison.OrdinalIgnoreCase) Then

                Dim regionFila As String = fila.Field(Of String)("Region")

                If Not String.Equals(regionFila, regionSelector, StringComparison.OrdinalIgnoreCase) Then
                    Return $"El registro no corresponde a la región seleccionada: {regionSelector}."
                End If

            Else
                Dim regionesCatalogo As List(Of RegionDto) = Await _catalogoService.ObtenerRegiones(mUser.Model)

                Dim regionValida As Boolean = regionesCatalogo.Any(
                                                    Function(r) String.Equals(
                                                        r.Description,
                                                        fila.Field(Of String)("Region"),
                                                        StringComparison.OrdinalIgnoreCase
                                                    )
                                                )
                If Not regionValida Then
                    Return $"La región {fila.Field(Of String)("Region")} no pertenece al catálogo de regiones válido."
                End If

            End If
        End If

        Return Nothing

    End Function



End Class
