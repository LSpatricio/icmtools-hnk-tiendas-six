Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class VentaMontoController
    Inherits ApiController

#Region "[ Propiedades Privadas ]"

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

#End Region

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    Public Class Registro
        Public Store As String
        Public StoreCR As String
        Public Plaza As String
        Public PlazaCR As String
        Public AmountSold As Decimal
    End Class

    <HttpPost>
    <Route("api/ventamonto/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim rTable As String = Nothing
            Dim filePath As String = Nothing
            Dim respuesta As Int32 = 0
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage("Venta Monto", "SinRegistros")})

            Dim lstExcel As List(Of Registro) = ObtenerExcel(ExcelArray)
            If lstExcel.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage("Venta Monto", "SinRegistros")})

            Dim jTable As String = JsonConvert.SerializeObject(lstExcel)
            Dim xlsx As New DataTable()
            Dim CCNomina As String = request.LogBody

            If (Not ValidarCuentaNomina(CCNomina)) Then
                Return Ok(New With {.d = False, .r = sc.GetMessage("Venta Monto", "NominaInvalida")})
            End If

            Dim jCatalogos As List(Of String) = ObtenerCatalogos()

            Dim dtResultados As New DataTable()
            Dim success As Boolean
            Try
                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Dim sql As String = "SELECT tipo_dato AS ""Tipo de Dato"", valor AS ""Valor"", detalle AS ""Detalle"" FROM public.spfemcovsimportventamontocategoria(@currentcnn, @jtable, @cattiendastable, @catccnominatable, @cfgstoresocietytable, @catdistritostable, @catplazastable);"
                    Using cmd As New NpgsqlCommand(sql, conn)
                        Using adpt As New NpgsqlDataAdapter(cmd)
                            cmd.Parameters.AddWithValue("@currentcnn", NpgsqlDbType.Varchar, CCNomina)
                            cmd.Parameters.AddWithValue("@jtable", NpgsqlDbType.Json, jTable)
                            cmd.Parameters.AddWithValue("@cattiendastable", NpgsqlDbType.Json, jCatalogos(0))
                            cmd.Parameters.AddWithValue("@catccnominatable", NpgsqlDbType.Json, jCatalogos(1))
                            cmd.Parameters.AddWithValue("@cfgstoresocietytable", NpgsqlDbType.Json, jCatalogos(2))
                            cmd.Parameters.AddWithValue("@catdistritostable", NpgsqlDbType.Json, jCatalogos(3))
                            cmd.Parameters.AddWithValue("@catplazastable", NpgsqlDbType.Json, jCatalogos(4))
                            adpt.Fill(dtResultados)
                        End Using
                    End Using
                    success = True
                End Using
            Catch _ex As Exception
                success = False
            End Try

            rTable = MostrarMensaje(success, dtResultados, respuesta, filePath)

            Return Ok(New With {
                    .d = respuesta,
                    .r = rTable,
                    .f = filePath
                })
        Catch ex As Exception
            mLog.insertLog("VentaMontoController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/ventamonto/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage("Monto Distribuible", "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("VentaMontoController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL ventamonto_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Function ObtenerExcel(ExcelArray As Object) As List(Of Registro)
        Dim jTable As New List(Of Registro)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim Store As String = ExcelArray(row, 1).ToString().Trim()
            Dim StoreCR As String = ExcelArray(row, 2).ToString().Trim()
            Dim Plaza As String = ExcelArray(row, 3).ToString().Trim()
            Dim PlazaCR As String = ExcelArray(row, 4).ToString()
            Dim AmountSold As Decimal = Decimal.Parse(ExcelArray(row, 5).ToString())

            If (String.IsNullOrWhiteSpace(Store) AndAlso
            String.IsNullOrWhiteSpace(StoreCR) AndAlso
            String.IsNullOrWhiteSpace(Store) AndAlso
            String.IsNullOrWhiteSpace(StoreCR)) Then Continue For

            jTable.Add(New Registro With {
                        .Store = Store,
                        .StoreCR = StoreCR,
                        .Plaza = Plaza,
                        .PlazaCR = PlazaCR,
                        .AmountSold = AmountSold
                    })
        Next

        Return jTable
    End Function

    Function ObtenerCatalogos() As List(Of String)
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Try
            Dim modeloEP As String = ConfigurationManager.AppSettings("ModelFemcoEPDev")
            Dim modeloVS As String = ConfigurationManager.AppSettings("ModelFemcoVSDev")

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If

            Using ws As New WebServiceICMGeneral()

                columnas = New List(Of String) From {"tiendaId", "Description", "plazaId"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catTiendas", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"CCNomina"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catCCNomina", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDStore", "IDSociety"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CfgStoreSociety", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"ID", "plazaId", "Description"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catDistritos", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"ID", "plazaId", "Description"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catPlazas", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

            End Using
            Return jCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function ExistenRegistrosCorrectos() As Boolean
        Try
            Dim sql As String = "SELECT * FROM ""ventamonto_precarga"" WHERE ""IDStatus""=true LIMIT 1;"
            Dim dataTable As New DataTable

            Using conn As New NpgsqlConnection(NpgSQL)
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dataTable)
                    End Using
                End Using
            End Using

            Return dataTable.Rows.Count > 0
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function MostrarMensaje(success As Boolean, dtResultados As DataTable, ByRef respuesta As Int32, ByRef filePath As String) As String
        Dim tBody As String = ""
        Dim rTable As String = ""
        Dim partialC As Boolean = False
        Try
            If success = True Then

                Dim registrosCorrectos As Boolean = ExistenRegistrosCorrectos()
                Dim dtRegistrosDuplicados As DataTable = ObtenerRegistrosDuplicados()

                If dtResultados.Rows.Count > 0 Then
                    filePath = fc.BuildXlsx(dtResultados, "VentaMonto")
                    partialC = registrosCorrectos
                ElseIf dtRegistrosDuplicados.Rows.Count > 0 Then
                    filePath = fc.BuildXlsx(dtRegistrosDuplicados, "VentaMonto")
                End If

                If dtRegistrosDuplicados.Rows.Count > 0 Then
                    Return sc.GetMessage("Venta Monto", "Duplicados")
                ElseIf Not registrosCorrectos Then
                    Return sc.GetMessage("Venta Monto", "SinImportacion")
                ElseIf success = True And partialC = False Then
                    respuesta = 1
                    CargarInformacion()
                    SendSFTP()
                    Return sc.GetMessage("Venta Monto", "CargaCompleta")
                ElseIf success = True And partialC = True Then
                    respuesta = 5
                    Return sc.GetMessage("Venta Monto", "ProcesoIncompleto")
                End If
            Else
                Return sc.GetMessage("Venta Monto", "Error",
                       New List(Of String) From {"Tienda", "CR Tienda", "Plaza.", "CR Plaza", "CERVEZA"},
                       New List(Of String) From {"1 de Mayo PAC", "50YWM", "Pachuca", "PLA-10PCK", "41,548.41"})
            End If

        Catch ex As Exception
            Throw ex
        End Try
        Return rTable
    End Function

    Private Function ObtenerRegistrosDuplicados() As DataTable
        Try
            Dim sql As String = "SELECT * FROM ""Categoria_VentaMonto_Duplicados"";"
            Dim dataTable As New DataTable

            Using conn As New NpgsqlConnection(NpgSQL)
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dataTable)
                    End Using
                End Using
            End Using

            Return dataTable
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function ValidarCuentaNomina(CCNomina As String) As Boolean
        Try
            Dim modeloVS As String = ConfigurationManager.AppSettings("ModelFemcoVSDev")
            Using ws As New WebServiceICMGeneral
                Dim columnas As New List(Of String) From {"CCNomina"}
                Dim parametros As String = "WHERE \""CCNomina\"" IN ( '" & CCNomina & "') "
                Using dataTable As DataTable = ws.ConsultaICMAPIQuery(columnas, "catCCNomina", modeloVS, parametros)
                    Return dataTable.Rows.Count > 0
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.VentaMonto
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class