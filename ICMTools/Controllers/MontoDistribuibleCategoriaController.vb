Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class MontoDistribuibleCategoriaController
    Inherits ApiController

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/montodistribuiblecategoria/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage("Monto Distribuible", "SinRegistros")})

            Dim jTable As New List(Of Object)

            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim RegistrosEnviados As Integer = usedRows - 1
            Dim filePath = Nothing

            Dim Plazas As String = ""
            Dim Tiendas As String = ""
            Dim Distritos As String = ""
            Dim CCNominas As String = ""
            Dim CfgStoreSociedades As String = ""

            For row As Integer = 2 To usedRows

                Dim plaza As String = ExcelArray(row, 1).ToString()
                Dim storecr As String = ExcelArray(row, 2).ToString()
                Dim store As String = ExcelArray(row, 3).ToString()
                Dim amount As String = ExcelArray(row, 4).ToString()
                Dim taxamount As String = ExcelArray(row, 5).ToString()

                If String.IsNullOrWhiteSpace(plaza) AndAlso String.IsNullOrWhiteSpace(storecr) AndAlso String.IsNullOrWhiteSpace(store) AndAlso String.IsNullOrWhiteSpace(amount) Then Continue For

                jTable.Add(New With {
                        .plaza = plaza,
                        .storecr = storecr,
                        .store = store,
                        .amount = amount,
                        .taxamount = taxamount
                    })

            Next

            If jTable.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage("Monto Distribuible", "SinImportacion")})
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

            Dim ws As New WebServiceICMGeneral()
            Dim success As Boolean = False
            Dim partialC As Boolean = False
            Dim Parametros As String = ""
            Dim current_ccn As String = request.LogBody
            Dim rTable As String = Nothing

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If

            Dim columnas As New List(Of String) From {"CCNomina"}
            Dim catCCNominaFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnas, "catCCNomina", Model, Parametros)

            Dim filas As DataRow() = catCCNominaFEMCOVSDEV.Select("CCNomina = '" + current_ccn + "'")
            If filas.Count = 0 Then
                Return Ok(New With {.d = False, .r = sc.GetMessage("Monto Distribuible", "nominainvalida")})
            End If

            Dim jsonTableCatCCNomina As String = JsonConvert.SerializeObject(catCCNominaFEMCOVSDEV)

            Dim columnascatPlazas As New List(Of String) From {"ID", "plazaId", "Description"}
            Dim catPlazasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model, Parametros)
            Dim jsonTableCatPlazas As String = JsonConvert.SerializeObject(catPlazasFEMCOVSDEV)

            Dim columnascatTiendas As New List(Of String) From {"tiendaId", "plazaId", "Description"}
            Dim catTiendasFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model, Parametros)
            Dim jsonTableCatTiendas As String = JsonConvert.SerializeObject(catTiendasFEMCOVSDEV)

            Dim columnascatDistritos As New List(Of String) From {"ID", "plazaId", "Description"} ' distritoId, Description,	plazaId, Inicio_efectivo, Finalización_efectiva, ID
            Dim catDistritosFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritos, "catDistritos", Model, Parametros)
            Dim jsonTableCatDistritos As String = JsonConvert.SerializeObject(catDistritosFEMCOVSDEV)

            Dim columnascfgstoresociety As New List(Of String) From {"IDStore", "IDSociety"} ' IDStore	IDSociety	Inicio_efectivo	Finalización_efectiva
            Dim columnascfgstoresocietyFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascfgstoresociety, "CfgStoreSociety", Model, Parametros)
            Dim jsonTableCfgstoreSociety As String = JsonConvert.SerializeObject(columnascfgstoresocietyFEMCOVSDEV)

            Dim xlsx As New DataTable()
            Dim RegistrosErrores As Integer = 0

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT public.femcovs_validacion_archivo_montodistribuiblecategoria(@file_ccnomina, @file_data_json, @catccnomina_json, @catplazas_json, @cattiendas_json, @catdistritos_json, @cfgstoresociety_json)", conn)
                    cmd.Parameters.AddWithValue("file_ccnomina", NpgsqlDbType.Varchar, current_ccn)
                    cmd.Parameters.AddWithValue("file_data_json", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("catccnomina_json", NpgsqlDbType.Json, jsonTableCatCCNomina)
                    cmd.Parameters.AddWithValue("catplazas_json", NpgsqlDbType.Json, jsonTableCatPlazas)
                    cmd.Parameters.AddWithValue("cattiendas_json", NpgsqlDbType.Json, jsonTableCatTiendas)
                    cmd.Parameters.AddWithValue("catdistritos_json", NpgsqlDbType.Json, jsonTableCatDistritos)
                    cmd.Parameters.AddWithValue("cfgstoresociety_json", NpgsqlDbType.Json, jsonTableCfgstoreSociety)

                    success = cmd.ExecuteScalar()
                End Using


                Dim queryInsertados As String = $"SELECT COUNT(*) FROM public.montodistribuible_precarga WHERE idstatus = 0;"
                Using cmdQ As New NpgsqlCommand(queryInsertados, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        RegistrosErrores = cmdQ.ExecuteScalar()
                    End Using
                End Using


                Dim query As String = $"SELECT tipo_dato AS ""Tipo de Dato"", valor AS ""Valor"", detalle AS ""Detalle"" FROM public.montodistribuibledetalles;"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        adapter.Fill(xlsx)
                    End Using
                End Using

            End Using

            If (RegistrosErrores = jTable.Count) Then
                filePath = fc.BuildXlsx(xlsx, "MontoDistribuible")
                Return Ok(New With {.d = 3, .r = sc.GetMessage("Monto Distribuible", "sinimportacion"), .f = filePath})
            End If

            If (xlsx.Rows.Count) > 0 Then
                filePath = fc.BuildXlsx(xlsx, "MontoDistribuible")
                partialC = True
            End If

            Dim respuesta As Integer

            If success = True And partialC = False Then
                respuesta = 1
                CargarInformacion()
                SendSFTP()
                rTable = sc.GetMessage("Monto Distribuible", "CargaCompleta")
            ElseIf success = True And partialC = True Then
                respuesta = 5
                rTable = sc.GetMessage("Monto Distribuible", "ProcesoIncompleto")
            Else
                respuesta = 0
                rTable = sc.GetMessage("Monto Distribuible", "Error",
                             New List(Of String) From {"PLAZA", "CR TIENDA", "DESC_TIENDA", "MONTO SIN IMPUESTOS", "MONTO CON IMPUESTOS"},
                             New List(Of String) From {"F012", "MZL-88MEN", "TIE-88SERB500KL", "1761", "2"})
            End If

            Return Ok(New With {.d = respuesta, .f = filePath, .r = rTable})
        Catch ex As Exception
            mLog.insertLog("MontoDistribuibleCategoriaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/montodistribuiblecategoria/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage("Monto Distribuible", "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("MontoDistribuibleCategoriaController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL montodistribuible_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

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


