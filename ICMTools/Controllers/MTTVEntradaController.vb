Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Imports System.Web.Http
Imports System.Threading
Imports System.Security
Imports ClassLibrary_PGP_TO_SFTP

Public Class MTTVEntradaController
    Inherits ApiController

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
    End Sub

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    Private scenario As Integer = Nothing
    Dim tBody As String = Nothing
    Private success As New DataTable()

    <HttpPost>
    <Route("api/mttventrada/insert")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Dim filePath As String = Nothing
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As List(Of Object) = ObtenerJsonExcel(ExcelArray)

            If jTable.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})

            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)
            Dim xlsx As New DataTable()
            Dim jCatalogos As List(Of String) = ObtenerCatalogos()

            success = EjecutarProceso(jsonTable, jCatalogos)

            If success.Rows.Count > 0 Then
                filePath = GetParcials(success)
            End If

            If jTable.Count <> success.Rows.Count Then
                SendSFTP()
            End If

            Dim rTable As String = MostrarMensaje(success, filePath, jTable.Count)

            Return Ok(New With {
                .d = scenario,
                .r = rTable,
                .f = filePath
            })

        Catch ex As Exception
            mLog.insertLog("MTTVEntradaController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    Function ObtenerJsonExcel(ExcelArray As Object) As List(Of Object)
        Try
            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

            For row As Integer = 2 To usedRows
                Dim CasoTabulador As String = ExcelArray(row, 1).ToString()
                Dim CRPlaza_A As String = ExcelArray(row, 2).ToString()
                Dim CRTienda_A As String = ExcelArray(row, 3).ToString()
                Dim CRPlaza_B As String = ExcelArray(row, 4).ToString()
                Dim CRTienda_B As String = ExcelArray(row, 5).ToString()

                Dim BEGDA As Date
                If Not (Date.TryParseExact(ExcelArray(row, 6).ToString(), "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, BEGDA)) Then
                    BEGDA = Date.ParseExact(ExcelArray(row, 6).ToString(), "yyyy/MM/dd", Globalization.CultureInfo.InvariantCulture)
                End If

                Dim ENDDA As Date
                If Not (Date.TryParseExact(ExcelArray(row, 7).ToString(), "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, ENDDA)) Then
                    ENDDA = Date.ParseExact(ExcelArray(row, 7).ToString(), "yyyy/MM/dd", Globalization.CultureInfo.InvariantCulture)
                End If

                Dim LGART As String = ExcelArray(row, 8).ToString()

                jTable.Add(New With {
                        .CASOTABULADOR = CasoTabulador,
                        .CRPLAZA_A = CRPlaza_A,
                        .CRTIENDA_A = CRTienda_A,
                        .CRPLAZA_B = CRPlaza_B,
                        .CRTIENDA_B = CRTienda_B,
                        .BEGDA = BEGDA,
                        .ENDDA = ENDDA,
                        .LGART = LGART
                    })
            Next

            Return jTable
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function ObtenerCatalogos() As List(Of String)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Try
            Dim modelo As String = ConfigurationManager.AppSettings("ModelFemcoEPDev")
            Using ws As New WebServiceICMGeneral()
                Dim Model As String = mUser.Model
                If Model = "DEBUG" Then
                    Model = "femcoepdev"
                End If

                Dim epsapCatPlazaCols As New List(Of String) From {"IDPlaza", "Description", "IDStatus"}
                Using epsapCatPlaza = ws.ConsultaICMAPIQuery(epsapCatPlazaCols, "CatPlaza", Model)
                    Dim epsapCatPlazaTbl As String = JsonConvert.SerializeObject(epsapCatPlaza)
                    jCatalogos.Add(epsapCatPlazaTbl)
                End Using

                Dim epsapCatStoreCols As New List(Of String) From {"IDStore", "Description", "ID5"}
                Using dataTable = ws.ConsultaICMAPIQuery(epsapCatStoreCols, "CatStore", Model)
                    Dim epsapCatStoreTbl As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(epsapCatStoreTbl)
                End Using

                Dim epsapCatWageTypeCols As New List(Of String) From {"IDWageType", "Description"}
                Using epsapCatWageType = ws.ConsultaICMAPIQuery(epsapCatWageTypeCols, "CatWageType", Model)
                    Dim epsapCatWageTypeTbl As String = JsonConvert.SerializeObject(epsapCatWageType)
                    jCatalogos.Add(epsapCatWageTypeTbl)
                End Using

                Dim epsapCfgStoreHierarchyCols As New List(Of String) From {"IDStore", "IDZone", "IDSociety", "IDPersonalDivision"}
                Using epsapCfgStoreHierarchy = ws.ConsultaICMAPIQuery(epsapCfgStoreHierarchyCols, "CfgStoreHierarchy", Model)
                    Dim epsapCfgStoreHierarchyTbl As String = JsonConvert.SerializeObject(epsapCfgStoreHierarchy)
                    jCatalogos.Add(epsapCfgStoreHierarchyTbl)
                End Using

            End Using
            Return jCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function EjecutarProceso(jtable As String, jCatalogos As List(Of String)) As DataTable
        Dim xlsx As New DataTable
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.z_mt_inc_variable_entrada(@_data, @catplazatable, @catstoretable, @catwagetypetable, @cfgstorehierarchytable)", conn)
                    cmd.Parameters.AddWithValue("_data", NpgsqlDbType.Json, jtable)
                    cmd.Parameters.AddWithValue("catplazatable", NpgsqlDbType.Json, jCatalogos(0))
                    cmd.Parameters.AddWithValue("catstoretable", NpgsqlDbType.Json, jCatalogos(1))
                    cmd.Parameters.AddWithValue("catwagetypetable", NpgsqlDbType.Json, jCatalogos(2))
                    cmd.Parameters.AddWithValue("cfgstorehierarchytable", NpgsqlDbType.Json, jCatalogos(3))
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(xlsx)
                    End Using
                End Using
            End Using
            Return xlsx
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function GetParcials(success As DataTable) As String
        Try
            Dim filePath As String = Nothing

            If success.Rows.Count > 0 Then
                filePath = fc.BuildXlsx(success, "MultiTiendaVariable_Entrada")
            End If

            Return filePath
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function MostrarMensaje(success As DataTable, filePath As String, rowCount As Integer) As String
        If rowCount > 0 AndAlso rowCount = success.Rows.Count Then
            tBody = $"
                <tr>
                    <td>Error al ejecutar el proceso de importación del archivo de MultiTienda Variable Entrada</td>
                     <td>No se encontró información válida para importar<br>Por favor verifique la información del archivo</td>
                </tr>"
            scenario = 3
            Return sc.TableBuilder(tBody, 1)
        End If

        If rowCount > 0 And filePath Is Nothing Then
            tBody = $"
                <tr>
                    <td>Ejecución Completada Exitosamente</td>
                    <td>Se ejecutó correctamente el proceso externo
                        <br><strong>Carga de MultiTienda Variable Entrada</strong>
                    </td>
                </tr>"
            scenario = 1
        Else
            tBody = $"
                <tr>
                    <td>Ejecución Completada Parcialmente</td>
                    <td>Se ejecutó parcialmente el proceso externo
                        <br><strong>Carga de MultiTienda Variable Entrada, por favor revise el archivo descargado para validar errores</strong>
                    </td>
                </tr>"
            scenario = 2
        End If

        Return sc.TableBuilder(tBody, 3)

    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.Entrada
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class
