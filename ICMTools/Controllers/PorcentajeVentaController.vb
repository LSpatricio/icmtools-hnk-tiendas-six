Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class PorcentajeVentaController
    Inherits ApiController

#Region "[ Variables Locales ]"

    Private ReadOnly _Pantalla As String = "Porcentaje Venta"

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
        Public categoriaId As String
        Public sociedadId As String
        Public plazaId As String
        Public PorcentajeSociedad As Decimal
    End Class

    <HttpPost>
    <Route("api/porcentajeventa/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Dim rTable As String = Nothing
            Dim filePath As String = Nothing
            Dim respuesta As Int32 = 0

            mUser = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            Dim jTable As New List(Of Registro)
            Dim resultadoExcel = ObtenerExcel(ExcelArray, jTable)
            If resultadoExcel.Equals("SR") Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})
            If resultadoExcel.Equals("RD") Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "Duplicados")})

            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)
            Dim success As Boolean = False
            Dim xlsx As New DataTable()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                success = PreparacionArchivo(jsonTable)
                If (success) Then ValidarInformacion()
                If (success) Then FinalizarProceso()
                rTable = MostrarMensaje(success, filePath, respuesta)
            End Using

            Return Ok(New With {
                    .d = respuesta,
                    .r = rTable,
                    .f = filePath
                })
        Catch ex As Exception
            mLog.insertLog("PorcentajeVentaController", "InsertData", ex.Message)
            mLog.NotificacionError(ex, "Porcentaje Venta")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/porcentajeventa/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("PorcentajeVentaController", "InsertData", ex.Message)
            mLog.NotificacionError(ex, "Porcentaje Venta")
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL porcentajeventa_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene el modelo del usuario actual.
    ''' Si el modelo es "DEBUG", retorna el valor por defecto.
    ''' </summary>
    ''' <returns>Modelo del usuario o valor por defecto si está en modo DEBUG.</returns>
    Public Function GetModel() As String
        Dim Model As String
        If mUser.Model = "DEBUG" Then
            Model = "femcovsdev"
        Else
            Model = mUser.Model
        End If
        Return Model
    End Function

    Function ObtenerExcel(ExcelArray As Object, ByRef jTable As List(Of Registro)) As String
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim categoriaId As String = ExcelArray(row, 1).ToString().Trim()
            Dim sociedadId As String = ExcelArray(row, 2).ToString().Trim()
            Dim plazaId As String = ExcelArray(row, 3).ToString().Trim()
            Dim PorcentajeSociedad As Decimal = Decimal.Parse(ExcelArray(row, 4).ToString())

            If String.IsNullOrWhiteSpace(categoriaId) AndAlso String.IsNullOrWhiteSpace(sociedadId) AndAlso String.IsNullOrWhiteSpace(plazaId) Then Continue For

            Dim registroDuplicado = jTable.Any(
            Function(r) r.categoriaId = categoriaId AndAlso
                                  r.sociedadId = sociedadId AndAlso
                                  r.plazaId = plazaId)
            If registroDuplicado Then
                Return "RD"
            End If

            jTable.Add(New Registro With {
                        .categoriaId = categoriaId,
                        .sociedadId = sociedadId,
                        .plazaId = plazaId,
                        .PorcentajeSociedad = PorcentajeSociedad
                    })
        Next

        If jTable.Count = 0 Then
            Return "SR"
        Else
            Return "OK"
        End If

    End Function

    Private Function PreparacionArchivo(jsonTable As String)
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT public.spFemcoVsImportPorcentajeVentaCategorias_Preparacion(@jTable)", conn)
                    cmd.Parameters.AddWithValue("jTable", NpgsqlDbType.Json, jsonTable)
                    Dim success As Boolean = cmd.ExecuteScalar()
                    Return success
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Private Sub ValidarInformacion()
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Dim Model As String = mUser.Model
        If Model = "DEBUG" Then
            Model = "femcovsdev"
        End If
        ValidarCampo("Categoria")
        ValidarCampo("Sociedad")
        ValidarCampo("Plaza")
        ValidarCatalogo(Model, "catTiendas", "tiendaId")
    End Sub

    Private Sub ValidarCampo(pantalla As String)
        Try
            Dim nombre As String = Nothing
            Dim catalogo As String = Nothing
            Dim campo As String = Nothing
            Dim campoSql As String = Nothing

            Dim modelo As String = mUser.Model
            modelo = If(modelo = "DEBUG", "femcovsdev", modelo)

            If (pantalla.Equals("Categoria")) Then
                nombre = "Categoria"
                catalogo = "catCategorias"
                campo = "categoriaId"
                campoSql = "categoriaid"
            ElseIf (pantalla.Equals("Sociedad")) Then
                nombre = "Sociedad"
                catalogo = "catSociedad"
                campo = "sociedadId"
                campoSql = "sociedadId"
            ElseIf (pantalla.Equals("Plaza")) Then
                nombre = "Plaza"
                catalogo = "catPlazas"
                campo = "plazaId"
                campoSql = "plazaId"
            End If

            If (Not String.IsNullOrEmpty(nombre)) Then
                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()

                    Dim sql As String = ""
                    Dim valor As String = ""
                    Dim valoresInexistentes As New List(Of String)

                    sql = $"SELECT DISTINCT ""{campo}"" FROM ""porcentajeventa_precarga"""
                    Using cmd As New NpgsqlCommand(sql, conn)
                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                valor = reader(campo)
                                Using ws As New WebServiceICMGeneral()
                                    Dim columnas As New List(Of String) From {campoSql}
                                    Dim parametros As String = "WHERE \""" + campoSql + "\"" = '" + valor + "'"
                                    Dim registro As DataTable = ws.ConsultaICMAPIQuery(columnas, catalogo, modelo, parametros)
                                    If (registro.Rows.Count.Equals(0)) Then
                                        valoresInexistentes.Add(valor)
                                    End If
                                End Using
                            End While
                        End Using
                    End Using

                    For Each valorInexistente As String In valoresInexistentes
                        sql = "UPDATE ""porcentajeventa_precarga"" 
                        SET ""IDStatus"" = false, ""StatusDetail"" = ""StatusDetail"" || '| " & nombre & " Inexistente' 
                        WHERE """ + campo + """ = @valor"
                        Using cmd As New NpgsqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("valor", valorInexistente)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next
                End Using
            End If

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Sub ValidarCatalogo(modelo As String, catalogo As String, campo As String)
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Dim Model As String = mUser.Model
                If Model = "DEBUG" Then
                    Model = "femcovsdev"
                End If
                Dim jtable As String = ""
                Using ws As New WebServiceICMGeneral()
                    Dim parametros As String = ""
                    Dim columnas As New List(Of String) From {campo}
                    Dim dtCatalogo As DataTable = ws.ConsultaICMAPIQuery(columnas, catalogo, Model, parametros)
                    jtable = JsonConvert.SerializeObject(dtCatalogo)
                End Using

                Dim sql As String = "SELECT * FROM public.spfemcovsimportporcentajeventacategorias_validarcatalogo ( @catalogo, @jtable );"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("catalogo", NpgsqlDbType.Varchar, catalogo)
                    cmd.Parameters.AddWithValue("jtable", NpgsqlDbType.Json, jtable)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub FinalizarProceso()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Dim sql As String = "SELECT public.spFemcoVsImportPorcentajeVentaCategorias_Finalizar()"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Function MostrarMensaje(ByRef success As Boolean, ByRef filePath As String, ByRef respuesta As Int32) As String
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()

                Dim tBody As String = ""

                If success = True Then

                    Dim hayRegistros As Boolean
                    Dim sql As String = "SELECT * FROM porcentajeventa_precarga WHERE ""IDStatus""=true LIMIT 1;"
                    Using cmd As New NpgsqlCommand(sql, conn)
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            Using dataTable As New DataTable()
                                adapter.Fill(dataTable)
                                hayRegistros = dataTable.Rows.Count > 0
                            End Using
                        End Using
                    End Using

                    sql = "SELECT ""tipoDato"" AS ""Tipo de Dato"", ""valor"" AS ""Valor"", ""detalle"" AS ""Detalle"" FROM ""FEMCOVS_CfgPorcentajeVentaSociedad_DetalleEstatus"";"
                    Dim xlsx As New DataTable()
                    Using cmd As New NpgsqlCommand(sql, conn)
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            adapter.Fill(xlsx)
                        End Using
                    End Using

                    If Not hayRegistros Then
                        filePath = fc.BuildXlsx(xlsx, "PorcentajeVenta")
                        tBody = sc.GetMessage(_Pantalla, "SinImportacion")
                    ElseIf (xlsx.Rows.Count > 0) Then
                        filePath = fc.BuildXlsx(xlsx, "PorcentajeVenta")
                        respuesta = 5
                        tBody = sc.GetMessage(_Pantalla, "ProcesoIncompleto")
                    Else
                        respuesta = 1
                        CargarInformacion()
                        SendSFTP()
                        tBody = sc.GetMessage(_Pantalla, "CargaCompleta")
                    End If
                Else
                    tBody = sc.GetMessage(_Pantalla, "Error",
                                  New List(Of String) From {"categoriaId", "sociedadId", "plazaId", "PorcentajeSociedad"},
                                  New List(Of String) From {"CER", "F099", "MPL-10CAN", "50"}
                )
                End If

                Return tBody
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.PorcentajeVenta
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class