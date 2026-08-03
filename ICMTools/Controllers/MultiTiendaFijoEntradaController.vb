Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class MultiTiendaFijoEntradaController
    Inherits ApiController

#Region " Propiedades Privadas "

    Private mUser As User
    Private mLog As Log
    Private ReadOnly _Pantalla As String = "MultiTienda Fijo Entrada"
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

#End Region

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    <HttpPost>
    <Route("api/multitiendafijoentrada/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim filePath = Nothing
            Dim tBody As String = Nothing
            Dim partialC As Boolean = False

            For row As Integer = 2 To usedRows
                Dim CASOTABULADORID As String = ExcelArray(row, 1).ToString()
                Dim PLAZA_AID As String = ExcelArray(row, 2).ToString()
                Dim CRTIENDA_AID As String = ExcelArray(row, 3).ToString()
                Dim PLAZA_BID As String = ExcelArray(row, 4).ToString()
                Dim CRTIENDA_BID As String = ExcelArray(row, 5).ToString()
                Dim BEGDAID As String = ExcelArray(row, 6).ToString()
                Dim ENDDAID As String = ExcelArray(row, 7).ToString()
                Dim LGARTID As String = ExcelArray(row, 8).ToString()

                If String.IsNullOrWhiteSpace(CASOTABULADORID) AndAlso String.IsNullOrWhiteSpace(PLAZA_AID) Then Continue For

                jTable.Add(New With {
                        .casotabulador = CASOTABULADORID,
                        .plaza_a = PLAZA_AID,
                        .crtienda_a = CRTIENDA_AID,
                        .plaza_b = PLAZA_BID,
                        .crtienda_b = CRTIENDA_BID,
                        .begda = BEGDAID,
                        .endda = ENDDAID,
                        .lgart = LGARTID
                    })
            Next

            If jTable.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

            Dim ws As New WebServiceICMGeneral()
            Dim success As Boolean = False
            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcoepdev"
            End If

            Dim epsapCatPlazaCols As New List(Of String) From {"IDPlaza", "Description", "EffStart_", "EffEnd_", "IDStatus"}
            Dim epsapCatPlaza As DataTable = ws.ConsultaICMAPIQuery(epsapCatPlazaCols, "CatPlaza", Model)
            Dim epsapCatPlazaTbl As String = JsonConvert.SerializeObject(epsapCatPlaza)

            Dim epsapCatStoreCols As New List(Of String) From {"IDStore", "Description", "ID5", "EffStart_", "EffEnd_"}
            Dim epsapCatStore As DataTable = ws.ConsultaICMAPIQuery(epsapCatStoreCols, "CatStore", Model)
            Dim epsapCatStoreTbl As String = JsonConvert.SerializeObject(epsapCatStore)

            Dim epsapCatWageTypeCols As New List(Of String) From {"IDWageType", "Description", "EffStart_", "EffEnd_"}
            Dim epsapCatWageType As DataTable = ws.ConsultaICMAPIQuery(epsapCatWageTypeCols, "CatWageType", Model)
            Dim epsapCatWageTypeTbl As String = JsonConvert.SerializeObject(epsapCatWageType)

            Dim epsapCfgStoreHierarchyCols As New List(Of String) From {"IDStore", "IDZone", "IDSociety", "IDPersonalDivision", "EffStart_", "EffEnd_"}
            Dim epsapCfgStoreHierarchy As DataTable = ws.ConsultaICMAPIQuery(epsapCfgStoreHierarchyCols, "CfgStoreHierarchy", Model)
            Dim epsapCfgStoreHierarchyTbl As String = JsonConvert.SerializeObject(epsapCfgStoreHierarchy)

            Dim epsapTdasComplejidadCols As New List(Of String) From {"IDSTORE"}
            Dim epsapTdasComplejidad As DataTable = ws.ConsultaICMAPIQuery(epsapTdasComplejidadCols, "TDASCOMPLEJIDAD", Model)
            Dim epsapTdascomplejidadTbl As String = JsonConvert.SerializeObject(epsapTdasComplejidad)

            Dim xlsx As New DataTable()
            Dim rTable As String = Nothing

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmdClear As New NpgsqlCommand("TRUNCATE TABLE fijoentrada_precarga;", conn)
                    cmdClear.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand("SELECT public.z_mt_inc_completo_entrada(@dataExcel, @epsapCatPlaza, @epsapCatStore, @epsapCatWageType, @epsapCfgStoreHierarchy, @epsapTdasComplejidad)", conn)
                    cmd.Parameters.AddWithValue("dataExcel", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("epsapCatPlaza", NpgsqlDbType.Json, epsapCatPlazaTbl)
                    cmd.Parameters.AddWithValue("epsapCatStore", NpgsqlDbType.Json, epsapCatStoreTbl)
                    cmd.Parameters.AddWithValue("epsapCatWageType", NpgsqlDbType.Json, epsapCatWageTypeTbl)
                    cmd.Parameters.AddWithValue("epsapCfgStoreHierarchy", NpgsqlDbType.Json, epsapCfgStoreHierarchyTbl)
                    cmd.Parameters.AddWithValue("epsapTdasComplejidad", NpgsqlDbType.Json, epsapTdascomplejidadTbl)
                    success = cmd.ExecuteScalar()
                End Using

                Dim query As String = $"SELECT * FROM fijoentrada_precarga WHERE IDS = 0"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        adapter.Fill(xlsx)
                    End Using
                End Using
            End Using

            Dim conError As Integer = xlsx.AsEnumerable().Count(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("DesError")))
            Dim sinError As Integer = xlsx.AsEnumerable().Count(Function(r) String.IsNullOrEmpty(r.Field(Of String)("DesError")))

            If conError > 0 Then
                filePath = fc.BuildXlsx(xlsx, "MultiTiendaFijoEntrada")
                partialC = True
            End If

            If usedRows > 1 AndAlso (usedRows - 1) = xlsx.Rows.Count Then
                tBody = $"
                <tr>
                    <td>Error al ejecutar el proceso de importación del archivo de MultiTienda Fijo Entrada</td>
                     <td>No se encontró información válida para importar<br>Por favor verifique la información del archivo</td>
                </tr>
"

                Return Ok(New With {
                          .d = 3,
                          .r = sc.TableBuilder(tBody, 1),
                          .f = filePath})
            End If

            If usedRows > 0 And filePath Is Nothing Then
                CargarInformacion()
                SendSFTP()
                tBody = $"
                <tr>
                    <td>Ejecución Completada Exitosamente</td>
                    <td>Se ejecutó correctamente el proceso externo
                        <br><strong>Carga de MultiTienda Fijo Entrada</strong>
                    </td>
                </tr>
                "
                Return Ok(New With {.d = 1, .r = sc.TableBuilder(tBody, 1)})
            Else
                tBody = sc.GetMessage(_Pantalla, "ProcesoIncompleto")
                Return Ok(New With {.d = 5, .r = sc.TableBuilder(tBody, 1), .f = filePath})
            End If

            rTable = sc.GetMessage(_Pantalla, "error",
                New List(Of String) From {"CASOTABULADOR", "CRPLAZA_A", "CRTIENDA_A", "CRPLAZA_B", "CRTIENDA_B", "BEGDA", "ENDDA", "LGART"},
                New List(Of String) From {"2", "10VHT COSTA ISTMO", "50CEF", "10VHT COSTA ISTMO", "50N2A", "01/08/2024", "31/08/2024", "110F"},
                sinError, conError)
            Return Ok(New With {.d = False, .r = rTable, .f = filePath})

        Catch ex As Exception
            mLog.insertLog("MultiTiendaFijoEntradaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/multitiendafijoentrada/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("MultiTiendaFijoEntradaController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL fijoentrada_cargar();"
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
            envio.Pantalla = EnvioPGPClass.enuPantalla.MultiTiendaFijoEntrada
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class
