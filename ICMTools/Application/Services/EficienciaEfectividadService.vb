Imports System.Reflection
Imports System.Threading.Tasks

Public Class EficienciaEfectividadService

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

    Public Async Function ProcesarEficienciaEfectividad(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))

        Dim errores = Await ValidacionesEficienciaEfectividad(request)

        If errores.Any() Then
            Return errores
        End If

        '    Await _repository.EjecutarSPAsync(
        '    "dbo.SP_VALIDATE_EFECTIVIDAD"
        ')

        '    Await _repository.EjecutarSPAsync(
        '    "dbo.SP_VALIDATE_EFICIENCIA"
        ')

        Return errores

    End Function



    Public Async Function ValidacionesEficienciaEfectividad(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))
        Dim errorsList As String = Nothing

        Dim tipo As Type = Type.GetType(request.FileClass)

        Dim hojasDefinidas As List(Of Type) = _excelService.ObtenerTipos(tipo)

        Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

        For Each hoja In hojasDefinidas


            Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(hoja)
            Dim atributo = tipo.GetProperties().ToList().FirstOrDefault(Function(p) p.PropertyType.GetGenericArguments()(0) = hoja).GetCustomAttributes(GetType(ExcelSheetAttribute), False).Cast(Of ExcelSheetAttribute)().First()

            Await _repository.LimpiarStaging(atributo.TableName)

            valoresErrores.AddRange(
                        Await _excelReader.CargaAsync(
                            request.Path,
                            atributo.HeaderRow,
                            atributo.SheetName,
                            mapeoColumnas,
                            atributo.TableName
                        )
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
