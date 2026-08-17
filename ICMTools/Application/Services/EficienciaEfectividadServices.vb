Imports System.Reflection
Imports System.Threading.Tasks

Public Class EficienciaEfectividadServices

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

    Public Async Function ValidacionesEficiencia(request As ValidateFileRequestt) As Task(Of List(Of ExcelValidationError))
        Dim errorsList As String = Nothing

        Dim tipo As Type = GetType(EficienciaEfectividadExcelDto)

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






End Class
