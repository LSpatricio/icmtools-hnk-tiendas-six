Imports System.Threading
Imports System.Web.Http
Imports ICMTools.FileController
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class ValidateHistFileResquestEmpleadosActivos
    Inherits ValidateFileRequest
    Public Property FileType2 As String
End Class

Public Class HistEmpleadosActivosController
    Inherits ApiController

#Region " Propiedades Privadas "

    Private ReadOnly _Pantalla As String = "Histórico Empleados Activos"
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController
    Private mUser As User
    Private mLog As Log

#End Region

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    <HttpPost>
    <Route("api/histempleadosactivos/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileResquestEmpleadosActivos) As IHttpActionResult
        Try
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim IdStore As String = Nothing
            Dim rTable As String = Nothing

            Dim jTable = ProcesarExcel(request.FileType, request.Extension)
            Dim jTable2 = ProcesarExcel(request.FileType2, request.Extension)

            If jTable.Count = 0 OrElse jTable2.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            If jTable Is Nothing Or jTable2 Is Nothing Then
                rTable = sc.GetMessage(_Pantalla, "Duplicados")
                Return Ok(New With {.d = False, .r = rTable})
            End If

            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)
            Dim jsonTable2 As String = JsonConvert.SerializeObject(jTable2)

            Dim ws As New WebServiceICMGeneral()
            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsqa"
            End If

            Dim columnasconfigdatestringweeks As New List(Of String) From {"StartDate", "EndDate", "CalculationDate"}
            Dim configdatestringweeksFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnasconfigdatestringweeks, "ConfigDateStringWeeks", Model)

            Dim columnascfgdates As New List(Of String) From {"Date", "IDDate"}
            Dim cfgdatesFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascfgdates, "CfgDates", Model)

            Dim columnascatfunctions As New List(Of String) From {"Description"}
            Dim catfunctionsFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatfunctions, "catFunctions", Model)

            Dim columnascatPlazas As New List(Of String) From {"ID", "plazaId", "Description"}
            Dim catPlazasFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model)

            Dim columnascatDistritos As New List(Of String) From {"ID", "plazaId", "Description"}
            Dim catDistritosFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritos, "catDistritos", Model)

            Dim columnasCfgStoreSocietys As New List(Of String) From {"IDSociety", "IDStore"}
            Dim CfgStoreSocietyFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnasCfgStoreSocietys, "CfgStoreSociety", Model)

            Dim columnascatTiendas As New List(Of String) From {"tiendaId"}
            Dim catTiendasFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model)

            Dim columnasPayee_ As New List(Of String) From {"PayeeID_"}
            Dim PayeeFEMCOVS As DataTable = ws.ConsultaICMAPIQueryLotes(columnasPayee_, "Payee_", Model)

            Dim success As Boolean = False

            Dim configdatestringweeksJson As String = JsonConvert.SerializeObject(configdatestringweeksFEMCOVS)
            Dim cfgdatesJson As String = JsonConvert.SerializeObject(cfgdatesFEMCOVS)
            Dim catfunctionsJson As String = JsonConvert.SerializeObject(catfunctionsFEMCOVS)
            Dim catPlazasJson As String = JsonConvert.SerializeObject(catPlazasFEMCOVS)
            Dim catDistritosJson As String = JsonConvert.SerializeObject(catDistritosFEMCOVS)
            Dim CfgStoreSocietyJson As String = JsonConvert.SerializeObject(CfgStoreSocietyFEMCOVS)
            Dim catTiendasJson As String = JsonConvert.SerializeObject(catTiendasFEMCOVS)
            Dim PayeeJson As String = JsonConvert.SerializeObject(PayeeFEMCOVS)

            Dim xlsx As New DataTable()
            Dim respuestas As New DataTable()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                Using cmd As New NpgsqlCommand($"TRUNCATE TABLE public.historicoempleadosactivos_precarga;", conn)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand("SELECT * FROM public.femcovs_validacion_archivo_histactiveemployees(@vsinitialemployees_json, @vsfinalemployees_json, @configdatestringweeks_json ,@cfgdates_json, @catfunctions_json, @catplazas_json, @catdistritos_json, @cfgstoresociety_json, @cattiendas_json, @payee__json)", conn)
                    cmd.Parameters.AddWithValue("vsinitialemployees_json", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("vsfinalemployees_json", NpgsqlDbType.Json, jsonTable2)
                    cmd.Parameters.AddWithValue("configdatestringweeks_json", NpgsqlDbType.Json, configdatestringweeksJson)
                    cmd.Parameters.AddWithValue("cfgdates_json", NpgsqlDbType.Json, cfgdatesJson)
                    cmd.Parameters.AddWithValue("catfunctions_json", NpgsqlDbType.Json, catfunctionsJson)
                    cmd.Parameters.AddWithValue("catplazas_json", NpgsqlDbType.Json, catPlazasJson)
                    cmd.Parameters.AddWithValue("catdistritos_json", NpgsqlDbType.Json, catDistritosJson)
                    cmd.Parameters.AddWithValue("cfgstoresociety_json", NpgsqlDbType.Json, CfgStoreSocietyJson)
                    cmd.Parameters.AddWithValue("cattiendas_json", NpgsqlDbType.Json, catTiendasJson)
                    cmd.Parameters.AddWithValue("payee__json", NpgsqlDbType.Json, PayeeJson)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(xlsx)
                    End Using
                End Using

                Using cmd As New NpgsqlCommand($"
                    SELECT
                        COUNT(CASE WHEN ""id_status"" = '1'  THEN 1 END) AS TotalTrue,
                        COUNT(CASE WHEN ""id_status"" = '0' THEN 1 END) AS TotalFalse
                    FROM ""historicoempleadosactivos_precarga"";
                    ", conn)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(respuestas)
                    End Using
                End Using
            End Using

            Dim filePath As String = Nothing

            If CInt(respuestas.Rows(0)("TotalFalse")) > 0 Then
                filePath = fc.BuildXlsx(xlsx, "HistEmpleadosActivos")
            End If

            If CInt(respuestas.Rows(0)("TotalTrue")) = 0 And CInt(respuestas.Rows(0)("TotalFalse")) > 0 Then

                rTable = sc.GetMessage(_Pantalla, "sinregistros")

                Return Ok(New With {
                          .d = False,
                          .r = rTable,
                          .f = filePath})

            ElseIf CInt(respuestas.Rows(0)("TotalTrue")) > 0 And CInt(respuestas.Rows(0)("TotalFalse")) = 0 Then
                CargarInformacion()
                SendSFTP()
                rTable = sc.GetMessage(_Pantalla, "cargacompleta",
                                       CInt(respuestas.Rows(0)("TotalTrue")) + CInt(respuestas.Rows(0)("TotalFalse")),
                                       CInt(respuestas.Rows(0)("TotalFalse")))
                Return Ok(New With {.d = True, .r = rTable})
            ElseIf xlsx.Rows.Count = 0 Then
                rTable = sc.GetMessage(_Pantalla, "sinimportacion",
                                       CInt(respuestas.Rows(0)("TotalTrue")) + CInt(respuestas.Rows(0)("TotalFalse")),
                                       CInt(respuestas.Rows(0)("TotalFalse")))

                Return Ok(New With {
                          .d = False,
                          .r = rTable,
                          .f = filePath})
            Else
                rTable = sc.GetMessage(_Pantalla, "procesoincompleto",
                                       CInt(respuestas.Rows(0)("TotalTrue")) + CInt(respuestas.Rows(0)("TotalFalse")),
                                       CInt(respuestas.Rows(0)("TotalFalse")))
                Return Ok(New With {.d = 5, .r = rTable, .f = filePath})
            End If
        Catch ex As Exception
            mLog.insertLog("HistEmpleadosActivosController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try

    End Function

    <HttpPost>
    <Route("api/histempleadosactivos/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("HistEmpleadosActivosController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL historicoempleadosactivos_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public Function ProcesarExcel(fileType As String, extension As String) As List(Of Object)
        Dim ExcelArray(,) As Object = fc.GetExcelArray(fileType, extension)
        If ExcelArray Is Nothing Then Return Nothing

        Dim jTable As New List(Of Object)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim personalDivision As String = ExcelArray(row, 1).ToString()
            Dim OU As String = ExcelArray(row, 2).ToString()
            Dim society As String = ExcelArray(row, 3).ToString()
            Dim IDEmployee As String = ExcelArray(row, 4).ToString()
            Dim employee As String = ExcelArray(row, 5).ToString()
            Dim hireDate As String = ExcelArray(row, 6).ToString()
            Dim functionRow As String = ExcelArray(row, 7).ToString()
            Dim ceco As String = Convert.ToString(ExcelArray(row, 8))
            Dim auxilaryCeco As String = Convert.ToString(ExcelArray(row, 9))
            Dim personalSubdivision As String = ExcelArray(row, 10).ToString()
            Dim division As String = ExcelArray(row, 11).ToString()

            If String.IsNullOrWhiteSpace(personalDivision) Or String.IsNullOrWhiteSpace(functionRow) Then Continue For

            jTable.Add(New With {
                        .personaldivision = personalDivision,
                        .ou = OU,
                        .society = society,
                        .idemployee = IDEmployee,
                        .employee = employee,
                        .hiredate = hireDate,
                        .functionrow = functionRow,
                        .ceco = ceco,
                        .auxilaryceco = auxilaryCeco,
                        .personalsubdivision = personalSubdivision,
                        .division = division
                    })
        Next

        Return jTable

    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.HistoricoEmpleadosActivos
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class
