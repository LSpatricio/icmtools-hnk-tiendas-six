Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports Serilog

Public Class EstructuraJOSServices

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

    Public Async Function ProcesarEstructuraJOS(request As ValidateFileRequest, idCarga As Guid, logger As ILogger) As Task(Of CargaResponse)

        Dim tablaStaging As String = "STG_ESTRUCTURAJOS"
        Dim tablaDestino As String = "BDIESTRUCTURAJOS"
        Dim sp As String = "SP_VALIDATE_ESTRUCTURAJOS"

        Dim errores = Await ValidacionesEstructuraJOS(request)

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



        logger.Information("No se encontraron errores de validación en el archivo de Estructura JOS. Procediendo a ejecutar el procedimiento almacenado para validar la información.")

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

        '    Dim idCarga As Guid = Guid.NewGuid()

        '    Dim errores = Await ValidacionesEstructuraJOS(request)

        '    If errores.Any() Then
        '        Return New CargaResponse With {
        '        .Exitoso = False,
        '        .IdCarga = idCarga,
        '        .Errores = errores
        '    }
        '    End If

        '    Await _repository.EjecutarSPAsync(
        '        "dbo.SP_VALIDATE_ESTRUCTURAJOS",
        '        idCarga
        '    )


        '    Return New CargaResponse With {
        '    .Exitoso = True,
        '    .IdCarga = idCarga,
        '    .Errores = New List(Of ExcelValidationError)()
        '}

    End Function



    Public Async Function ValidacionesEstructuraJOS(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))

        Dim errorsList As String = Nothing
        Dim tableName As String = "STG_ESTRUCTURAJOS"

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

    Public Async Function EnvioEstructuraJOS(request As SendInfoRequest, logger As ILogger) As Task

        If Not Directory.Exists(request.PathSalida) Then
            Directory.CreateDirectory(request.PathSalida)
        End If

        Dim nombreArchivo As String = "BDIESTRUCTURAJOS.csv"

        Dim rutaArchivo As String = Path.Combine(request.PathSalida, nombreArchivo)


        Dim sql As String = "
                SELECT
                     Region
		            ,CveAreaSix
		            ,AreaSix
		            ,GZ
		            ,Ceco
		            ,CeBe
		            ,NumeroResponsable
		            ,Responsable
		            ,IdCarga
		            ,FechaInsercion
                FROM BDIESTRUCTURAJOS
                WHERE IdCarga = @IdCarga
                "

        Await _repository.GenerarCsvAsync(
                                sql,
                                rutaArchivo,
                                request.IdGui
                            )

        Await _sftpClient.SubirArchivoAsync(rutaArchivo)

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
