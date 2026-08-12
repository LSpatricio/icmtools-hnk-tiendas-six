Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Namespace Controllers
    Public Class ConfigSalesPercentageController
        Inherits ApiController

#Region " Propiedades Privadas "

        Private mUser As User
        Private mLog As Log

        Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
        Private ReadOnly _Pantalla As String = "Configuración Porcentaje de Ventas"

        ReadOnly fc As New FileController
        ReadOnly sc As New SharedController

#End Region

        Public Sub New()
            Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
            Me.mLog = New Log
        End Sub

        <HttpPost>
        <Route("api/configsalespercentage/insertdata")>
        Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
            Try
                Thread.Sleep(1000)
                Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
                Dim registrosTotal As Integer = 0
                Dim registrosConError As Integer = 0
                Dim registrosSinError As Integer = 0

                Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
                If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

                Dim jTable As New List(Of Object)
                Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
                Dim filePath = Nothing

                For row As Integer = 2 To usedRows
                    Dim SociedadID As String = ExcelArray(row, 1).ToString()
                    Dim PlazaID As String = ExcelArray(row, 2).ToString()
                    Dim PorcentajeSociedad As Decimal = Convert.ToDecimal(ExcelArray(row, 3).ToString())

                    If String.IsNullOrWhiteSpace(SociedadID) AndAlso String.IsNullOrWhiteSpace(PlazaID) Then Continue For

                    jTable.Add(New With {
                        .SociedadId = SociedadID,
                        .PlazaID = PlazaID,
                        .PorcentajeSociedad = PorcentajeSociedad
                    })
                Next

                Dim Model As String = mUser.Model
                If Model = "DEBUG" Then
                    Model = "femcovsdev"
                End If

                registrosTotal = jTable.Count
                If jTable.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinImportacion")})
                Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

                Dim ws As New WebServiceICMGeneral()
                Dim success As Boolean = False
                Dim ParcialC As Boolean = False

                Dim columnascatDistritos As New List(Of String) From {"distritoId", "Description", "plazaId"}
                Dim catDistritos As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritos, "catDistritos", Model)
                Dim jsonTableCatDistritos As String = JsonConvert.SerializeObject(catDistritos)

                Dim columnascatPlazas As New List(Of String) From {"plazaId", "Description"}
                Dim catPlazas As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model)
                Dim jsonTableCatPlazas As String = JsonConvert.SerializeObject(catPlazas)

                Dim columnasCfgStoreHierarchy As New List(Of String) From {"IDPlaza", "IDDistrict", "IDStore"}
                Dim catCfgStoreHierarchy As DataTable = ws.ConsultaICMAPIQuery(columnasCfgStoreHierarchy, "CfgStoreHierarchy", Model)
                Dim jsonTableCatCfgStoreHierarchy As String = JsonConvert.SerializeObject(catCfgStoreHierarchy)

                Dim columnasCatSociedad As New List(Of String) From {"sociedadId"}
                Dim catSociedad As DataTable = ws.ConsultaICMAPIQuery(columnasCatSociedad, "catSociedad", Model)
                Dim jsonTableCatSociedad As String = JsonConvert.SerializeObject(catSociedad)

                Dim columnascatTiendas As New List(Of String) From {"tiendaId"}
                Dim catTiendas As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model)
                Dim jsonTableCatTiendas As String = JsonConvert.SerializeObject(catTiendas)

                Dim xlsx As New DataTable()
                Dim rTable As String = Nothing
                Dim current_ccn As String = request.LogBody

                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Using cmd As New NpgsqlCommand("SELECT public.femcovs_validacion_archivo_configsalespercentage(@file_data_json, 
                                                        @catdistritos_json, @catplazas_json, @cfgstorehierarchy_json, @catsociedad_json,
                                                        @cattiendas_json)", conn)
                        cmd.Parameters.AddWithValue("file_data_json", NpgsqlDbType.Json, jsonTable)
                        cmd.Parameters.AddWithValue("catdistritos_json", NpgsqlDbType.Json, jsonTableCatDistritos)
                        cmd.Parameters.AddWithValue("catplazas_json", NpgsqlDbType.Json, jsonTableCatPlazas)
                        cmd.Parameters.AddWithValue("cfgstorehierarchy_json", NpgsqlDbType.Json, jsonTableCatCfgStoreHierarchy)
                        cmd.Parameters.AddWithValue("catsociedad_json", NpgsqlDbType.Json, jsonTableCatSociedad)
                        cmd.Parameters.AddWithValue("cattiendas_json", NpgsqlDbType.Json, jsonTableCatTiendas)

                        success = cmd.ExecuteScalar()
                    End Using

                    Dim query As String = $"SELECT DISTINCT idsociety, idplaza, value AS porcentajeSociedad, statusdetail AS detalle FROM public.configuracionporcentajeventas_precarga WHERE idstatus = '0';"
                    Using cmdQ As New NpgsqlCommand(query, conn)
                        Using adapter As New NpgsqlDataAdapter(cmdQ)
                            adapter.Fill(xlsx)
                            registrosConError = xlsx.Rows.Count
                        End Using
                    End Using

                    query = $"SELECT COUNT(*) FROM public.configuracionporcentajeventas_precarga WHERE idstatus = '1';"
                    Using cmdQ As New NpgsqlCommand(query, conn)
                        registrosSinError = cmdQ.ExecuteScalar()
                    End Using
                End Using

                If xlsx.Rows.Count > 0 Then
                    filePath = fc.BuildXlsx(xlsx, "ConfigPorcentajeVentas")
                    ParcialC = registrosSinError > 0
                End If

                If success = True And ParcialC = False And registrosConError.Equals(0) Then
                    CargarInformacion()
                    SendSFTP()
                    rTable = sc.GetMessage(_Pantalla, "CargaCompleta")
                    Return Ok(New With {.d = 1, .r = rTable})
                ElseIf success = True And ParcialC = True Then
                    rTable = sc.GetMessage(_Pantalla, "ProcesoIncompleto")
                    Return Ok(New With {.d = 5, .r = rTable, .f = filePath})
                ElseIf success = True And ParcialC = False Then
                    rTable = sc.GetMessage(_Pantalla, "sinimportacion")
                    Return Ok(New With {.d = False, .r = rTable, .f = filePath})
                Else
                    rTable = sc.GetMessage(_Pantalla, "Error",
                           New List(Of String) From {"SociedadID", "PlazaID", "PorcentajeSociedad"},
                           New List(Of String) From {"F099", "MPL-10SGD", "1.32"})
                    Return Ok(New With {.d = False, .r = rTable, .f = filePath})
                End If
            Catch ex As Exception
                mLog.insertLog("ConfigSalesPercentageController", "InsertData", ex.Message)
                Return InternalServerError(ex)
            End Try
        End Function

        <HttpPost>
        <Route("api/configsalespercentage/uploaddata")>
        Public Function UploadData() As IHttpActionResult
            Try
                Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
                CargarInformacion()
                SendSFTP()
                Return Ok(New With {.d = 2, .r = mensaje})
            Catch ex As Exception
                mLog.insertLog("ConfigSalesPercentageController", "UploadData", ex.Message)
                Return InternalServerError(ex)
            End Try
        End Function

        ''' <summary>
        ''' Método que carga la información
        ''' </summary>
        Private Sub CargarInformacion()
            Try
                Using conn As New NpgsqlConnection(NpgSQL)
                    Const sql As String = "CALL configuracionporcentajeventas_cargar();"
                    Using cmd As New NpgsqlCommand(sql, conn)
                        conn.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
                Throw
            End Try
        End Sub

        Private Sub SendSFTP()
            Try
                Dim envio As New EnvioPGPClass
                envio.Pantalla = EnvioPGPClass.enuPantalla.ConfiguracionPorcentajeVenta
                envio.Enviar()
            Catch ex As Exception
                Throw
            End Try
        End Sub
    End Class
End Namespace