Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports Serilog

Public Class TiendasDesincorporadasService
    Private mUser As User
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
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)

    End Sub

    Public Async Function ProcesarTiendasDesincorporadas(request As ValidateFileRequest, idCarga As Guid, logger As ILogger) As Task(Of CargaResponse)

        Dim tablaStaging As String = "STG_TIENDASDESINCORPORADAS"
        Dim tablaDestino As String = "BDITIENDASDESINCORPORADAS"
        Dim sp As String = "SP_VALIDATE_TIENDASDESINCORPORADAS"

        Dim errores = Await ValidacionesTiendasDesincorporadas(request)

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



        logger.Information("No se encontraron errores de validación en el archivo de Tiendas Desincorporadas . Procediendo a ejecutar el procedimiento almacenado para validar la información.")

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



    Public Async Function ValidacionesTiendasDesincorporadas(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))

        Dim errorsList As String = Nothing
        Dim tableName As String = "STG_TIENDASDESINCORPORADAS"

        Dim tipo As Type = Type.GetType(request.FileClass)

        Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

        Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)

        Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(tipo)

        Await _repository.LimpiarStaging(tableName)

        For i As Integer = 0 To cantidadHojas - 1


            valoresErrores.AddRange(
                        Await _excelReader.CargaAsync(
                            request.Path,
                            request.HeaderRow,
                            i.ToString(),
                            mapeoColumnas,
                            tableName)
                    )
        Next

        Return valoresErrores



    End Function

    Public Async Function EnvioTiendasDesincorporadas(request As SendInfoRequest, logger As ILogger) As Task

        If Not Directory.Exists(request.PathSalida) Then
            Directory.CreateDirectory(request.PathSalida)
        End If

        Dim nombreArchivo As String = "BDITIENDASDESINCORPORADAS.csv"

        Dim rutaArchivo As String = Path.Combine(request.PathSalida, nombreArchivo)


        Dim sql As String = "
                  SELECT
                 Ceco,
                Descripcion,
                Region,
                GZ,
                DescJOS,
                DescACS,
                Madura,
                TipoCierre,
                Status,
                MesBaja,
                TipoBaja
            FROM BDITIENDASDESINCORPORADAS
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

    'AddressOf ValidarEficienciaEfectividad
    Private Function ValidarEficienciaEfectividad(
    fila As DataRow
) As String

        Dim ruta As String = fila("Ruta").ToString().Trim()



        If Not ruta Then
            Return $"La ruta '{ruta}' no existe."
        End If

        Return Nothing

    End Function


End Class
