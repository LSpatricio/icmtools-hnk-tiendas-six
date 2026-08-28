Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks

Public Class RegistrosADCService

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

    'Public Async Function ProcesarRegistrosADCService(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))

    '    Dim errores = Await ValidacionesRegistrosADCService(request)

    '    If errores.Any() Then
    '        Return errores
    '    End If

    '    Await _repository.EjecutarSPAsync(
    '        "dbo.SP_VALIDATE_REGISTROSADC"
    '    )

    '    Return errores

    'End Function

    Public Async Function ProcesarRegistrosADC(request As ValidateFileRequest) As Task(Of CargaResponse)

        Dim idCarga As Guid = Guid.NewGuid()

        Dim errores = Await ValidacionesRegistrosADCService(request)

        If errores.Any() Then
            Return New CargaResponse With {
            .Exitoso = False,
            .IdCarga = idCarga,
            .Errores = errores
        }
        End If

        Await _repository.EjecutarSPAsync(
            "dbo.SP_VALIDATE_REGISTROSADC",
            idCarga
        )


        Return New CargaResponse With {
        .Exitoso = True,
        .IdCarga = idCarga,
        .Errores = New List(Of ExcelValidationError)()
    }

    End Function



    Public Async Function ValidacionesRegistrosADCService(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))

        Dim errorsList As String = Nothing
        Dim tableName As String = "STG_REGISTROSADC"

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

    Public Async Function EnvioRegistrosADC(request As SendInfoRequest) As Task

        If Not Directory.Exists(request.PathSalida) Then
            Directory.CreateDirectory(request.PathSalida)
        End If

        Dim nombreArchivo As String = "BDIREGISTROSADC.csv"

        Dim rutaArchivo As String = Path.Combine(request.PathSalida, nombreArchivo)


        Dim sql As String = "
                  SELECT
                 ID
				,ConteoArchivos
				,Ceco
				,FechaAprobacion
				,Accion
				,Ruta
				,Region
				,ComentarioAnalista
				,NombreTienda
				,CargadoPor
				,RevisadoExpins
				,Estatus
                    FROM BDIREGISTROSADC
                   WHERE IdCarga = @IdCarga
                "

        Await _repository.GenerarCsvAsync(
                                sql,
                                rutaArchivo,
                                request.IdGui
                            )

        Await _sftpClient.SubirArchivoAsync(rutaArchivo)


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
