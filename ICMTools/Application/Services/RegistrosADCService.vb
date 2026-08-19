Imports System.Reflection
Imports System.Threading.Tasks

Public Class RegistrosADCService

    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Public Sub New()
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)

    End Sub

    Public Async Function ProcesarRegistrosADCService(request As ValidateFileRequestt) As Task(Of List(Of ExcelValidationError))

        Dim errores = Await ValidacionesRegistrosADCService(request)

        If errores.Any() Then
            Return errores
        End If

        Await _repository.EjecutarSPAsync(
            "dbo.SP_VALIDATE_REGISTROSADC"
        )

        Return errores

    End Function



    Public Async Function ValidacionesRegistrosADCService(request As ValidateFileRequestt) As Task(Of List(Of ExcelValidationError))
        Dim errorsList As String = Nothing
        Dim tableName As String = "STG_REGISTROSADC"

        Dim tipo As Type = Type.GetType(request.FileClass)

        Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

        Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)

        Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(tipo)


        For i As Integer = 0 To cantidadHojas - 1

            Await _repository.LimpiarStaging(tableName)

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
