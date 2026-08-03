Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class ImportMetasController
    Inherits ApiController

#Region " Propiedades Privadas "

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    Private ReadOnly _Pantalla As String = "Importación de Metas"

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

#End Region

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    Public Class Registro
        Public IdStore As String
        Public Plaza As String
        Public CRPlaza As String
        Public StoreName As String
        Public Store As String
        Public Generica As String
        Public Tae As String
    End Class

    <HttpPost>
    <Route("api/importmetas/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Dim rTable As String = Nothing
            Dim filePath As String = Nothing
            Dim respuesta As Int32 = 0
            Dim success As Boolean = Nothing

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                Dim sql As String = "SELECT * FROM public.femcovs_validacion_archivo_metas();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    success = cmd.ExecuteScalar()
                End Using
            End Using

            rTable = MostrarMensaje(success, respuesta, filePath)

            Return Ok(New With {
                    .d = respuesta,
                    .r = rTable,
                    .f = filePath
                })
        Catch ex As Exception
            mLog.insertLog("ImportMetasController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/importmetas/loadcatalogs")>
    Public Function LoadCatalogs(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            Dim lstExcel As List(Of Registro) = ObtenerExcel(ExcelArray)
            If lstExcel.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            Dim jTable As String = JsonConvert.SerializeObject(lstExcel)

            Using ws As New WebServiceICMGeneral()
                Dim jCatalogos As List(Of String) = ObtenerCatalogos()

                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Using cmd As New NpgsqlCommand("CALL public.metas_cargarcatalogos(@file_data_json, @cfgdates_json, @time_json, @catdistritos_json, @cfgstorehierarchy_json, @catplazas_json, @cattiendas_json, @catcfgstoresociety_json);", conn)
                        cmd.Parameters.AddWithValue("@file_data_json", NpgsqlDbType.Json, jTable)
                        cmd.Parameters.AddWithValue("@cfgdates_json", NpgsqlDbType.Json, jCatalogos(0))
                        cmd.Parameters.AddWithValue("@time_json", NpgsqlDbType.Json, jCatalogos(1))
                        cmd.Parameters.AddWithValue("@catdistritos_json", NpgsqlDbType.Json, jCatalogos(2))
                        cmd.Parameters.AddWithValue("@cfgstorehierarchy_json", NpgsqlDbType.Json, jCatalogos(3))
                        cmd.Parameters.AddWithValue("@catplazas_json", NpgsqlDbType.Json, jCatalogos(4))
                        cmd.Parameters.AddWithValue("@cattiendas_json", NpgsqlDbType.Json, jCatalogos(5))
                        cmd.Parameters.AddWithValue("@catcfgstoresociety_json", NpgsqlDbType.Json, jCatalogos(6))
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            End Using

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog.insertLog("ImportMetasController", "LoadCatalogs", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/importmetas/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("ImportMetasController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL metas_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Function ObtenerCatalogos() As List(Of String)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Try
            Dim Model As String
            If mUser.Model = "DEBUG" Then Model = "femcovsdev" Else Model = mUser.Model

            Using ws As New WebServiceICMGeneral()
                columnas = New List(Of String) From {"IDDate", "Value"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CfgDates", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"TimeID_", "Name_", "Starting_", "Ending_"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "Time_", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"Description", "plazaId"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catDistritos", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDStore", "IDPlaza"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CfgStoreHierarchy", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"plazaId", "Description"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catPlazas", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"tiendaId", "plazaId"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catTiendas", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDStore", "IDSociety"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CfgStoreSociety", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using
            End Using
            Return jCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function ObtenerExcel(ExcelArray As Object) As List(Of Registro)
        Dim jTable As New List(Of Registro)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim IdStore As String = ExcelArray(row, 1).ToString().Trim()
            Dim Plaza As String = ExcelArray(row, 2).ToString().Trim()
            Dim CRPlaza As String = ExcelArray(row, 3).ToString().Trim()
            Dim StoreName As String = ExcelArray(row, 4).ToString().Trim()
            Dim Store As String = ExcelArray(row, 5).ToString().Trim()
            Dim Generica As String = ExcelArray(row, 6).ToString().Trim()
            Dim Tae As String = ExcelArray(row, 7).ToString().Trim()

            If String.IsNullOrWhiteSpace(IdStore) AndAlso String.IsNullOrWhiteSpace(Plaza) AndAlso
                String.IsNullOrWhiteSpace(CRPlaza) AndAlso String.IsNullOrWhiteSpace(StoreName) AndAlso
                String.IsNullOrWhiteSpace(Store) AndAlso String.IsNullOrWhiteSpace(Generica) AndAlso
                String.IsNullOrWhiteSpace(Tae) Then
                Continue For
            End If

            jTable.Add(New Registro With {
                        .IdStore = IdStore,
                        .Plaza = Plaza,
                        .CRPlaza = CRPlaza,
                        .StoreName = StoreName,
                        .Store = Store,
                        .Generica = Generica,
                        .Tae = Tae
                    })
        Next

        Return jTable
    End Function

    Private Function MostrarMensaje(success As Boolean, ByRef respuesta As Int32, ByRef filePath As String) As String
        Dim tBody As String = ""
        Dim partialC As Boolean = False
        Try
            If success = True Then

                Dim hayRegistrosDt As New DataTable
                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Dim sql As String = "SELECT * FROM metas_precarga WHERE idplaza <> 'Error';"
                    Using cmd As New NpgsqlCommand(sql, conn)
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            adapter.Fill(hayRegistrosDt)
                        End Using
                    End Using
                End Using
                Dim hayRegistros As Boolean = hayRegistrosDt.Rows.Count > 0

                Dim xlsx As New DataTable
                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Dim sql As String = "SELECT ""tipoDato"" AS ""Tipo de Dato"", ""valor"" AS ""Valor"", ""detalle"" AS ""Detalle"" FROM ""VentaSugerida_Metas_Invalidos"";"
                    Using cmd As New NpgsqlCommand(Sql, conn)
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            adapter.Fill(xlsx)
                        End Using
                    End Using
                End Using

                If xlsx.Rows.Count > 0 Then
                    filePath = fc.BuildXlsx(xlsx, "ImportacionMetas")
                    partialC = hayRegistros
                End If

                If Not hayRegistros Then
                    Return sc.GetMessage(_Pantalla, "SinImportacion")
                ElseIf success = True And partialC = False Then
                    respuesta = 1
                    CargarInformacion()
                    SendSFTP()
                    Return sc.GetMessage(_Pantalla, "CargaCompleta")
                ElseIf success = True And partialC = True Then
                    respuesta = 5
                    Return sc.GetMessage(_Pantalla, "ProcesoIncompleto")
                End If
            Else
                Return sc.GetMessage(_Pantalla, "Error",
                       New List(Of String) From {"IDSTORE", "PLAZA", "CR_PLAZA", "STORE_NAME", "CR_TIENDA", "GEN", "TAE"},
                       New List(Of String) From {"80GTO50023D", "80PDO Guanajuato", "10GTO", "Villas de la City ABC", "50249", "688", "88"})
            End If

            Return ""
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.Metas
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class