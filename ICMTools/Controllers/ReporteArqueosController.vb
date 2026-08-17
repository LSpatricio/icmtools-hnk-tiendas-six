Imports System.Reflection
Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class ReporteArqueosController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _arqueosExcelReader As ArqueosExcelReader

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _arqueosExcelReader = New ArqueosExcelReader()
    End Sub

    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/reportearqueos/validarinfo")>
    Public Function ValidarInfo(<FromBody> request As ValidateFileRequestt) As IHttpActionResult
        Try
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing

            Dim tipo As Type = GetType(ArqueosExcelDto)

            Dim hojasDefinidas As List(Of Type) = _excelService.ObtenerTipos(tipo)

            Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

            For Each hoja In hojasDefinidas
                Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(hoja)
                Dim atributo = tipo.GetProperties().ToList().FirstOrDefault(Function(p) p.PropertyType.GetGenericArguments()(0) = hoja).GetCustomAttributes(GetType(ExcelSheetAttribute), False).Cast(Of ExcelSheetAttribute)().First()

                valoresErrores.AddRange(_arqueosExcelReader.ValidacionesArqueos(request.Path, atributo.HeaderRow, atributo.SheetName, mapeoColumnas))
            Next

            If valoresErrores.Count > 0 Then
                For Each errores In valoresErrores
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})
            End If

            Dim rTable As String = Nothing

            rTable = sc.GetMessage("Arqueos", "CargaCompleta")

            Return Ok(New With {.d = True, .path = request.Path, .f = request.Path, .r = rTable})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/reportearqueos/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(500)

            Dim rTable As String = sc.GetMessage("Arqueos", "CargaCompleta")

            Return Ok(New With {.d = True, .r = rTable})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function
End Class
