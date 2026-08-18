Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class EficienciaEfectividadController
    Inherits ApiController

    Private mUser As User
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _eficienciaEfectividadServices As EficienciaEfectividadServices


    ' Private mLog As Log

    ' Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _eficienciaEfectividadServices = New EficienciaEfectividadServices()

        '     Me.mLog = New Log
    End Sub

    ' ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/eficienciaefectividad/cargarinfo")>
    Public Async Function CargarInfoAsync(<FromBody> request As ValidateFileRequestt) As Task(Of IHttpActionResult)
        Try
            Thread.Sleep(1000)

            Dim errorsList As String = Nothing

            Dim valoresErrores = Await _eficienciaEfectividadServices.ProcesarEficienciaEfectividad(request)


            ' valoresErrores = Await _eficienciaEfectividadServices.ValidacionesEficiencia(request)

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
                rTable = sc.GetMessage("Eficiencia Efectividad", "CargaCompleta")

            End If

            Return Ok(New With {.d = True, .f = "RUTAFINAL", .r = rTable})
        Catch ex As Exception
            'mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function


    '<HttpPost>
    '<Route("api/eficienciaefectividad/validarinfo")>
    'Public Function ValidarInfo(<FromBody> request As ValidateFileRequestt) As IHttpActionResult
    '    Try
    '        Thread.Sleep(1000)

    '        Dim errorsList As String = Nothing

    '        Dim tipo As Type = GetType(EficienciaExcelDto)

    '        Dim valoresErrores As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()

    '        Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)

    '        For i As Integer = 0 To cantidadHojas - 1

    '            Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(tipo)


    '            valoresErrores.AddRange(_eficienciaEfectividadExcelReader.ValidacionesEficienciaEfectividad(request.Path, request.HeaderRow, i.ToString(), mapeoColumnas))

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
    '            rTable = sc.GetMessage("Eficiencia Efectividad", "CargaCompleta")

    '        End If

    '        Return Ok(New With {.d = True, .f = "RUTAFINAL", .r = rTable})
    '    Catch ex As Exception
    '        'mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
    '        Return InternalServerError(ex)
    '    End Try
    'End Function

    '<HttpPost>
    '<Route("api/EficienciaEfectividad/insertdata")>
    'Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
    '    Try
    '        Thread.Sleep(1000)
    '        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

    '        Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
    '        If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage("Monto Distribuible", "SinRegistros")})

    '        Dim jTable As New List(Of Object)

    '        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
    '        Dim RegistrosEnviados As Integer = usedRows - 1
    '        Dim filePath = Nothing

    '        Dim Plazas As String = ""
    '        Dim Tiendas As String = ""
    '        Dim Distritos As String = ""
    '        Dim CCNominas As String = ""
    '        Dim CfgStoreSociedades As String = ""

    '        For row As Integer = 2 To usedRows

    '            Dim plaza As String = ExcelArray(row, 1).ToString()
    '            Dim storecr As String = ExcelArray(row, 2).ToString()
    '            Dim store As String = ExcelArray(row, 3).ToString()
    '            Dim amount As String = ExcelArray(row, 4).ToString()
    '            Dim taxamount As String = ExcelArray(row, 5).ToString()

    '            If String.IsNullOrWhiteSpace(plaza) AndAlso String.IsNullOrWhiteSpace(storecr) AndAlso String.IsNullOrWhiteSpace(store) AndAlso String.IsNullOrWhiteSpace(amount) Then Continue For

    '            jTable.Add(New With {
    '                    .plaza = plaza,
    '                    .storecr = storecr,
    '                    .store = store,
    '                    .amount = amount,
    '                    .taxamount = taxamount
    '                })

    '        Next

    '        If jTable.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage("Monto Distribuible", "SinImportacion")})
    '        Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

    '        Dim ws As New WebServiceICMGeneral()
    '        Dim success As Boolean = False
    '        Dim partialC As Boolean = False
    '        Dim Parametros As String = ""
    '        Dim current_ccn As String = request.LogBody
    '        Dim rTable As String = Nothing

    '        Dim Model As String = mUser.Model
    '        If Model = "DEBUG" Then
    '            Model = "femcovsdev"
    '        End If

    '        Dim columnas As New List(Of String) From {"CCNomina"}
    '        Dim catCCNominaFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnas, "catCCNomina", Model, Parametros)

    '        Dim filas As DataRow() = catCCNominaFEMCOVSDEV.Select("CCNomina = '" + current_ccn + "'")
    '        If filas.Count = 0 Then
    '            Return Ok(New With {.d = False, .r = sc.GetMessage("Monto Distribuible", "nominainvalida")})
    '        End If

    '        Dim jsonTableCatCCNomina As String = JsonConvert.SerializeObject(catCCNominaFEMCOVSDEV)

    '        Dim columnascatPlazas As New List(Of String) From {"ID", "plazaId", "Description"}
    '        Dim catPlazasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model, Parametros)
    '        Dim jsonTableCatPlazas As String = JsonConvert.SerializeObject(catPlazasFEMCOVSDEV)

    '        Dim columnascatTiendas As New List(Of String) From {"tiendaId", "plazaId", "Description"}
    '        Dim catTiendasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model, Parametros)
    '        Dim jsonTableCatTiendas As String = JsonConvert.SerializeObject(catTiendasFEMCOVSDEV)

    '        Dim columnascatDistritos As New List(Of String) From {"ID", "plazaId", "Description"} ' distritoId, Description,	plazaId, Inicio_efectivo, Finalización_efectiva, ID
    '        Dim catDistritosFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritos, "catDistritos", Model, Parametros)
    '        Dim jsonTableCatDistritos As String = JsonConvert.SerializeObject(catDistritosFEMCOVSDEV)

    '        Dim columnascfgstoresociety As New List(Of String) From {"IDStore", "IDSociety"} ' IDStore	IDSociety	Inicio_efectivo	Finalización_efectiva
    '        Dim columnascfgstoresocietyFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascfgstoresociety, "CfgStoreSociety", Model, Parametros)
    '        Dim jsonTableCfgstoreSociety As String = JsonConvert.SerializeObject(columnascfgstoresocietyFEMCOVSDEV)

    '        Dim xlsx As New DataTable()
    '        Dim RegistrosErrores As Integer = 0



    '        If (RegistrosErrores = jTable.Count) Then
    '            filePath = fc.BuildXlsx(xlsx, "MontoDistribuible")
    '            Return Ok(New With {.d = 3, .r = sc.GetMessage("Monto Distribuible", "sinimportacion"), .f = filePath})
    '        End If

    '        If (xlsx.Rows.Count) > 0 Then
    '            filePath = fc.BuildXlsx(xlsx, "MontoDistribuible")
    '            partialC = True
    '        End If

    '        Dim respuesta As Integer

    '        If success = True And partialC = False Then
    '            respuesta = 1
    '            CargarInformacion()
    '            SendSFTP()
    '            rTable = sc.GetMessage("Monto Distribuible", "CargaCompleta")
    '        ElseIf success = True And partialC = True Then
    '            respuesta = 5
    '            rTable = sc.GetMessage("Monto Distribuible", "ProcesoIncompleto")
    '        Else
    '            respuesta = 0
    '            rTable = sc.GetMessage("Monto Distribuible", "Error",
    '                         New List(Of String) From {"PLAZA", "CR TIENDA", "DESC_TIENDA", "MONTO SIN IMPUESTOS", "MONTO CON IMPUESTOS"},
    '                         New List(Of String) From {"F012", "MZL-88MEN", "TIE-88SERB500KL", "1761", "2"})
    '        End If

    '        Return Ok(New With {.d = respuesta, .f = filePath, .r = rTable})
    '    Catch ex As Exception
    '        ' mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
    '        Return InternalServerError(ex)
    '    End Try
    'End Function

    '<HttpPost>
    '<Route("api/montodistribuiblecategoria/uploaddata")>
    'Public Function UploadData() As IHttpActionResult
    '    Try
    '        Dim mensaje As String = sc.GetMessage("Monto Distribuible", "CargaParcial")
    '        CargarInformacion()
    '        SendSFTP()
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


