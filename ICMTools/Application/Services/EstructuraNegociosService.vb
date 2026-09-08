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

        If errores.Any(Function(x) Not x.Advertencia) Then

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
        .Errores = errores
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

        Dim catalogos = Await ObtenerCatalogos(mUser.Model)


        For i As Integer = 0 To cantidadHojas - 1


            valoresErrores.AddRange(
                        Await _excelReader.CargaAsync(
                            request.Path,
                            request.HeaderRow,
                            i.ToString(),
                            mapeoColumnas,
                            tableName,
                            request.Region,
                            catalogos,
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

    Public Function ValidarFiltroEstructuraNegociosAsync(fila As DataRow, Optional catalogos As CatalogosDto = Nothing) As ExcelValidationError

        Dim gzSixFila As String = fila.Field(Of String)("GZ")
        Dim estatusTiendaFila As String = fila.Field(Of String)("EstatusTienda")

        If Not catalogos.GZSix.Contains(gzSixFila) Then
            Return New ExcelValidationError With {.Problema = $"El valor {gzSixFila} para 'GZSIX' no se encuentra en el catálogo de ICM CatGZSix.", .Advertencia = True}
        End If

        If Not catalogos.EstatusTienda.Contains(estatusTiendaFila) Then
            Return New ExcelValidationError With {.Problema = $"El valor {estatusTiendaFila} para 'EstatusTienda' no se encuentra en el catálogo de ICM CatStoreStatusSix.", .Advertencia = True}
        End If

        Return Nothing

    End Function

    Private Async Function ObtenerCatalogos(modelo As String) As Task(Of CatalogosDto)

        Dim regiones = Await _catalogoService.ObtenerRegiones(modelo)
        Dim gzSix = Await _catalogoService.ObtenerGZSix(modelo)
        Dim estatusTienda = Await _catalogoService.ObtenerEstatusTienda(modelo)

        Return New CatalogosDto With {
        .Regiones = New HashSet(Of String)(
            regiones.Select(Function(r) r.Description),
            StringComparer.OrdinalIgnoreCase
        ),
        .GZSix = New HashSet(Of String)(
            gzSix.Select(Function(g) g.Description),
            StringComparer.OrdinalIgnoreCase
        ),
        .EstatusTienda = New HashSet(Of String)(
            estatusTienda.Select(Function(e) e.Description),
            StringComparer.OrdinalIgnoreCase
        )
    }

    End Function

End Class
