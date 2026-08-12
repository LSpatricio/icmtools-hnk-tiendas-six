Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class ConfiguracionDistribucionesController
    Inherits ApiController

#Region " Popiedades Privadas "

    Private mUser As User
    Private mLog As Log

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    Private ReadOnly _Pantalla As String = "Configuración de Distribuciones"

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

#End Region

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    Public Class Registro
        Public sociedadId As String
        Public funcionId As String
        Public PorcentajeSociedad As Decimal
    End Class

    <HttpPost>
    <Route("api/configuraciondistribuciones/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Dim rTable As String = Nothing
            Dim filePath As String = Nothing
            Dim respuesta As Int32 = 0
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            Dim lstExcel As List(Of Registro) = ObtenerExcel(ExcelArray)
            If lstExcel.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            Dim jTable As String = JsonConvert.SerializeObject(lstExcel)
            Dim xlsx As New DataTable()
            Dim jCatalogos As List(Of String) = ObtenerCatalogos()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                Dim success As Boolean = EjecutarProceso(jTable, jCatalogos)
                rTable = MostrarMensaje(success, respuesta, filePath)
            End Using

            Return Ok(New With {
                    .d = respuesta,
                    .r = rTable,
                    .f = filePath
                })
        Catch ex As Exception
            mLog.insertLog("ConfiguracionDistribucionesController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/configuraciondistribuciones/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("ConfiguracionDistribucionesController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL configuraciondistribuciones_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Function ObtenerExcel(ExcelArray As Object) As List(Of Registro)
        Dim jTable As New List(Of Registro)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim sociedadId As String = ExcelArray(row, 1).ToString().Trim()
            Dim funcionId As String = ExcelArray(row, 2).ToString().Trim()
            Dim PorcentajeSociedad As Decimal
            PorcentajeSociedad = If(Decimal.TryParse(ExcelArray(row, 3).ToString(), PorcentajeSociedad), PorcentajeSociedad, 0D)

            If String.IsNullOrWhiteSpace(sociedadId) AndAlso String.IsNullOrWhiteSpace(funcionId) Then Continue For

            jTable.Add(New Registro With {
                        .sociedadId = sociedadId,
                        .funcionId = funcionId,
                        .PorcentajeSociedad = PorcentajeSociedad
                    })
        Next

        Return jTable
    End Function

    Function ObtenerCatalogos() As List(Of String)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        mLog = New Log
        Try
            Dim modeloEP As String = ConfigurationManager.AppSettings("ModelFemcoEPDev")
            Dim modeloVS As String = ConfigurationManager.AppSettings("ModelFemcoVSDev")

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If

            Using ws As New WebServiceICMGeneral()
                columnas = New List(Of String) From {"puestoId", "DescripcionPuesto"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "catEstructuraTienda", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using
            End Using

            Return jCatalogos
        Catch ex As Exception
            mLog.insertLog("ConfiguracionDistribucionesController", "ObtenerCatalogos", ex.Message)
            Throw
        End Try
    End Function

    Private Function EjecutarProceso(jtable As String, jCatalogos As List(Of String)) As Boolean
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Dim sql As String = "SELECT public.femcovs_validacion_archivo_configdistribution(@file_data_json, @catestructuratienda_json);"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.Parameters.AddWithValue("@file_data_json", NpgsqlDbType.Json, jtable)
                    cmd.Parameters.AddWithValue("@catestructuratienda_json", NpgsqlDbType.Json, jCatalogos(0))
                    Return cmd.ExecuteScalar()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Private Function MostrarMensaje(success As Boolean, ByRef respuesta As Int32, ByRef filePath As String) As String
        Dim partialC As Boolean = False
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                If success = True Then
                    Dim hayRegistros As Boolean
                    Const sqlHayRegistros As String = "SELECT * FROM ""configuraciondistribuciones_precarga"" LIMIT 1;"
                    Using cmd As New NpgsqlCommand(sqlHayRegistros, conn)
                        cmd.CommandType = CommandType.Text
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            Using dataTable As New DataTable()
                                adapter.Fill(dataTable)
                                hayRegistros = dataTable.Rows.Count > 0
                            End Using
                        End Using
                    End Using

                    Const sqlXlsx = "SELECT ""tipoDato"" AS ""Tipo de Dato"", ""valor"" AS ""Valor"", ""detalle"" AS ""Detalle"" FROM ""IncentivoCerveza_ConfiguracionDistribuciones_Invalidos"";"
                    Dim xlsx As New DataTable()
                    Using cmd As New NpgsqlCommand(sqlXlsx, conn)
                        cmd.CommandType = CommandType.Text
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            adapter.Fill(xlsx)
                        End Using
                    End Using

                    If xlsx.Rows.Count > 0 Then
                        filePath = fc.BuildXlsx(xlsx, "ConfiguracionDistribuciones")
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
                           New List(Of String) From {"sociedadId", "funcionId", "PorcentajeSociedad"},
                           New List(Of String) From {"F110", "00001085", "5"})
                End If
            End Using

            Return ""
        Catch ex As Exception
            Throw
        End Try

    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.ConfiguracionDistribuciones
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class