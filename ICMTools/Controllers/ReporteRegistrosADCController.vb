Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class ReporteRegistrosADCController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _registrosADCService As RegistrosADCService
    ' Private mLog As Log

    ' Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _registrosADCService = New RegistrosADCService()

        '     Me.mLog = New Log
    End Sub

    ' ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/registrosadc/cargarinfo")>
    Public Async Function CargarInfoAsync(<FromBody> request As ValidateFileRequestt) As Task(Of IHttpActionResult)
        Try
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing

            Dim valoresErrores = Await _registrosADCService.ProcesarRegistrosADCService(request)

            If valoresErrores.Count > 0 Then
                For Each errores In valoresErrores
                    errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
                Next

                Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

            End If



            'Ejecución de SP 

            Dim rTable As String = Nothing


            Dim respuesta As Integer

            If True = True Then
                respuesta = 1
                'CargarInformacion()
                'SendSFTP()
                rTable = sc.GetMessage("_registrosADCService", "CargaCompleta")

            End If

            Return Ok(New With {.d = True, .f = "RUTAFINAL", .r = rTable})
        Catch ex As Exception
            'mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    '<HttpPost>
    '<Route("api/registrosadc/validarinfo")>
    'Public Function ValidarInfo(<FromBody> request As ValidateFileRequestt) As IHttpActionResult
    '    Try
    '        Thread.Sleep(1000)

    '        Dim errorsList As String = Nothing

    '        Dim tipo As Type = GetType(RegistrosADCExcelDto)

    '        Dim hojasDefinidas As List(Of Type) = _excelService.ObtenerTipos(tipo)



    '        Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

    '        For Each hoja In hojasDefinidas
    '            Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(hoja)
    '            Dim atributo = tipo.GetProperties().ToList().FirstOrDefault(Function(p) p.PropertyType.GetGenericArguments()(0) = hoja).GetCustomAttributes(GetType(ExcelSheetAttribute), False).Cast(Of ExcelSheetAttribute)().First()

    '            valoresErrores.AddRange(_registrosADCService.ValidacionesRegistrosADCService(request.Path, atributo.HeaderRow, atributo.SheetName, mapeoColumnas))



    '        Next



    '        If valoresErrores.Count > 0 Then
    '            For Each errores In valoresErrores
    '                errorsList += $"<tr><td>{errores.Problema}</td><td>" & String.Join(", ", errores.Detalle) & "</td></tr>"
    '            Next

    '            Return Ok(New With {.d = sc.TableBuilder(errorsList, 1)})

    '        End If

    '        Dim rTable As String = Nothing


    '        Dim respuesta As Integer

    '        If True = True Then
    '            respuesta = 1
    '            'CargarInformacion()
    '            'SendSFTP()
    '            rTable = sc.GetMessage("Registros ADC", "CargaCompleta")

    '        End If

    '        Return Ok(New With {.d = True, .f = "RUTAFINAL", .r = rTable})
    '    Catch ex As Exception
    '        'mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
    '        Return InternalServerError(ex)
    '    End Try
    'End Function

    <HttpPost>
    <Route("api/registrosadc/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(500)

            Dim rTable As String = sc.GetMessage("Registros ADC", "CargaCompleta")

            Return Ok(New With {.d = True, .r = rTable})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    '<HttpPost>
    '<Route("api/registrosadc/uploaddata")>
    'Public Function UploadData() As IHttpActionResult
    '    Try
    '        Dim mensaje As String = sc.GetMessage("Registros ADC", "CargaParcial")
    '        CargarInformacion()
    '        'SendSFTP()
    '        Return Ok(New With {.d = 2, .r = mensaje})
    '    Catch ex As Exception
    '        'mLog.insertLog("MontoDistribuibleCategoriaController", "UploadData", ex.Message)
    '        Return InternalServerError(ex)
    '    End Try
    'End Function

    '''' <summary>
    '''' Método que carga la información
    '''' </summary>
    'Private Sub CargarInformacion()
    '    Try

    '    Catch ex As Exception
    '        Throw ex
    '    End Try
    'End Sub

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.MontoDistribuible
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class


