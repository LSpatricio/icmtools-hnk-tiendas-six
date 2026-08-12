Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Imports System.Web.Http
Imports System.Threading

Public Class VentaUnidadesCategoriaController
    Inherits ApiController

#Region "[ Propiedades Privadas ]"

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    Private ReadOnly _Pantalla As String = "Venta Unidades"

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

#End Region

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    <HttpPost>
    <Route("api/ventaunidadescategorias/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Dim filePath = Nothing
            Dim success As Boolean = False
            Dim xlsx As New DataTable()
            Dim rTable As String = Nothing
            Dim partialC As Boolean = False
            Dim respuesta As Integer

            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
            Dim file_ccn As String = request.LogBody
            Dim conError As Integer = 0
            Dim sinError As Integer = 0

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.femcovs_validacion_archivo_ventaunidadescategoria(@file_ccn)", conn)
                    cmd.Parameters.AddWithValue("@file_ccn", NpgsqlDbType.Varchar, file_ccn)
                    success = cmd.ExecuteScalar()
                End Using

                Dim query As String = $"SELECT tipo_dato AS ""Tipo de Dato"", valor AS ""Valor"", detalle AS ""Detalle"" FROM femcovs_unitscategorysales_detailstatus;"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        adapter.Fill(xlsx)
                    End Using
                End Using

                query = $"SELECT SUM(CASE WHEN idstatus = 1 THEN 1 ELSE 0 END) AS correctos, SUM(CASE WHEN idstatus = 0 THEN 1 ELSE 0 END) AS incorrectos FROM ventaunidades_precarga;"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        Dim dataTable As New DataTable()
                        adapter.Fill(dataTable)
                        sinError = dataTable.Rows(0)("correctos")
                        conError = dataTable.Rows(0)("incorrectos")
                    End Using
                End Using
            End Using

            If xlsx.Rows.Count > 0 Then
                filePath = fc.BuildXlsx(xlsx, "VentaUnidadesCategoria")
                partialC = sinError > 0
            End If

            If success = True And sinError = 0 And conError = 0 Then
                respuesta = 0
                rTable = sc.GetMessage(_Pantalla, "sinimportacion", sinError, conError)
            ElseIf success = True And partialC = False Then
                respuesta = 1
                CargarInformacion()
                SendSFTP()
                rTable = sc.GetMessage(_Pantalla, "CargaCompleta", sinError)
            ElseIf success = True And partialC = True Then
                respuesta = 5
                rTable = sc.GetMessage(_Pantalla, "ProcesoIncompleto")
            Else
                respuesta = 0
                rTable = sc.GetMessage(_Pantalla, "Error",
                       New List(Of String) From {"IDStore", "CATEGORIA_DES", "CR_PLAZ", "PLAZA", "CR_TIENDA", "TIENDA", "ID_USUARIO", "ID_EMPLEADO", "udsnetas", "creationDate"},
                       New List(Of String) From {"900FTE8282Z", "Prueba ICMTools", "40ABC", "40ABC  Prueba", "80ABC", "Prueba", "SIDEA7987234", "1398127", "8", "30/09/2025"},
                       sinError, conError)
            End If

            Return Ok(New With {.d = respuesta, .r = rTable, .f = filePath})
        Catch ex As Exception
            mLog.insertLog("VentaUnidadesCategoriaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/ventaunidadescategorias/loadcatalogs")>
    Public Function LoadCatalogs(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension, request.AllowDuplicateEntries)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

            For row As Integer = 2 To usedRows
                Dim IDStoreID As String = ExcelArray(row, 1).ToString()
                Dim CategoryID As String = ExcelArray(row, 2).ToString()
                Dim PlazaCRID As String = ExcelArray(row, 3).ToString()
                Dim PlazaID As String = ExcelArray(row, 4).ToString()
                Dim StoreCRID As String = ExcelArray(row, 5).ToString()
                Dim StoreID As String = ExcelArray(row, 6).ToString()
                Dim IDUserID As String = ExcelArray(row, 7).ToString()
                Dim IDEmployeeID As String = ExcelArray(row, 8).ToString()
                Dim UnitsTotalID As String = ExcelArray(row, 9).ToString()
                Dim CreationDateID As String = ExcelArray(row, 10).ToString()

                jTable.Add(New With {
                    .IDStore = IDStoreID,
                    .Category = CategoryID,
                    .PlazaCR = PlazaCRID,
                    .Plaza = PlazaID,
                    .StoreCR = StoreCRID,
                    .Store = StoreID,
                    .IDUser = IDUserID,
                    .IDEmployee = IDEmployeeID,
                    .UnitsTotal = UnitsTotalID,
                    .CreationDate = CreationDateID
                })
            Next

            Using ws As New WebServiceICMGeneral()
                Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
                Dim file_ccn As String = request.LogBody
                Dim Model As String = mUser.Model
                If Model = "DEBUG" Then
                    Model = "femcovsdev"
                End If

                Dim columnascatCCNominaFEMCOVSDEV As New List(Of String) From {"CCNomina"}
                Dim parametros As String = "WHERE \""CCNomina\"" IN ( '" & file_ccn & "') "
                Dim catCCNominaFEMCOVSDEV As DataTable = ws.ConsultaICMAPIQuery(columnascatCCNominaFEMCOVSDEV, "catCCNomina", Model, parametros)
                If catCCNominaFEMCOVSDEV.Rows.Count = 0 Then
                    Dim rTable As String = sc.GetMessage(_Pantalla, "nominainvalida")
                    Return Ok(New With {.d = False, .r = rTable})
                End If

                If jTable.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})
                Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

                Dim vscatPlazasCols As New List(Of String) From {"ID", "Description"}
                Dim vscatPlazas As DataTable = ws.ConsultaICMAPIQuery(vscatPlazasCols, "catPlazas", Model)
                Dim vscatPlazasTbl As String = JsonConvert.SerializeObject(vscatPlazas)

                Dim vscatDistritosCols As New List(Of String) From {"ID", "Description"}
                Dim vscatDistritos As DataTable = ws.ConsultaICMAPIQuery(vscatDistritosCols, "catDistritos", Model)
                Dim vscatDistritosTbl As String = JsonConvert.SerializeObject(vscatDistritos)

                Dim vscatCCNominaCols As New List(Of String) From {"CCNomina"}
                Dim vscatCCNomina As DataTable = ws.ConsultaICMAPIQuery(vscatCCNominaCols, "catCCNomina", Model)
                Dim vscatCCNominaTbl As String = JsonConvert.SerializeObject(vscatCCNomina)

                Dim vscatTiendasCols As New List(Of String) From {"tiendaId", "Description", "plazaId"}
                Dim vscatTiendas As DataTable = ws.ConsultaICMAPIQuery(vscatTiendasCols, "catTiendas", Model)
                Dim vscatTiendasTbl As String = JsonConvert.SerializeObject(vscatTiendas)

                Dim vsPayee_Cols As New List(Of String) From {"PayeeID_"}
                Dim vsPayee_ As DataTable = ws.ConsultaICMAPIQueryLotes(vsPayee_Cols, "Payee_", Model)
                Dim vsPayee_Tbl As String = JsonConvert.SerializeObject(vsPayee_)

                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Using cmd As New NpgsqlCommand("CALL public.ventaunidades_cargarcatalogos(@filedata_json, @catplazas_json, @catdistritos_json, @catccnomina_json, @cattiendas_json, @payee_json)", conn)
                        cmd.Parameters.AddWithValue("@filedata_json", NpgsqlDbType.Json, jTable)
                        cmd.Parameters.AddWithValue("@catplazas_json", NpgsqlDbType.Json, vscatPlazasTbl)
                        cmd.Parameters.AddWithValue("@catdistritos_json", NpgsqlDbType.Json, vscatDistritosTbl)
                        cmd.Parameters.AddWithValue("@catccnomina_json", NpgsqlDbType.Json, vscatCCNominaTbl)
                        cmd.Parameters.AddWithValue("@cattiendas_json", NpgsqlDbType.Json, vscatTiendasTbl)
                        cmd.Parameters.AddWithValue("@payee_json", NpgsqlDbType.Json, vsPayee_Tbl)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            End Using

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog.insertLog("VentaUnidadesCategoriaController", "LoadCatalogs", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/ventaunidadescategorias/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("VentaUnidadesCategoriaController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL ventaunidades_cargar();"
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
            envio.Pantalla = EnvioPGPClass.enuPantalla.VentaUnidades
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class
