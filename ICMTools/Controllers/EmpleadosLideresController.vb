Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class EmpleadosLideresController
    Inherits ApiController

    Private mUser As User
    Private mLog As Log
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    Public Class Registro
        Public LLAVE As Integer
        Public ZONA As String
        Public PLAZA As String
        Public CR_PALZA As String
        Public CR_TIENDA As String
        Public DIMENSION As String
        Public TIENDA As String
        Public DISTRITO As String
        Public ASESOR As String
        Public MESOPS As String
        Public LIDER As String
        Public RFC_NOEMP As String
        Public CLAS_FINAL As String
    End Class

    <HttpPost>
    <Route("api/empleadoslideres/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try

            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage("Empleados Líderes", "SinRegistros")})

            Dim jTable As New List(Of Registro)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim filePath = Nothing
            Dim valor As String

            Dim Subject As String = Nothing
            Dim tBody As String = Nothing
            Dim rTable As String = Nothing

            For row As Integer = 2 To usedRows
                Dim llave As Integer = ExcelArray(row, 1).ToString().Trim()
                Dim zona As String = ExcelArray(row, 2).ToString().Trim()
                Dim plaza As String = ExcelArray(row, 3).ToString().Trim()
                Dim crPlaza As String = ExcelArray(row, 4).ToString().Trim()
                Dim crTienda As String = ExcelArray(row, 5).ToString().Trim()
                Dim dimension As String = ExcelArray(row, 6).ToString().Trim()
                Dim tienda As String = ExcelArray(row, 7).ToString().Trim()
                Dim distrito As String = Convert.ToString(ExcelArray(row, 8))
                Dim asesor As String = ExcelArray(row, 9).ToString().Trim()
                Dim mesOps As String = ExcelArray(row, 10).ToString().Trim()
                Dim lider As String = Convert.ToString(ExcelArray(row, 11))
                Dim rfcNoemp As String = Convert.ToString(ExcelArray(row, 12))
                Dim clasFinal As String = ExcelArray(row, 13).ToString().Trim()

                If String.IsNullOrWhiteSpace(plaza) AndAlso String.IsNullOrWhiteSpace(tienda) Then Continue For

                Dim registroDuplicado As Boolean = jTable.Any(Function(r) _
                r.LLAVE = llave AndAlso
                r.ZONA = zona AndAlso
                r.PLAZA = plaza AndAlso
                r.CR_PALZA = crPlaza AndAlso
                r.CR_TIENDA = crTienda AndAlso
                r.DIMENSION = dimension AndAlso
                r.TIENDA = tienda AndAlso
                r.DISTRITO = distrito AndAlso
                r.ASESOR = asesor AndAlso
                r.MESOPS = mesOps AndAlso
                r.LIDER = lider AndAlso
                r.RFC_NOEMP = rfcNoemp AndAlso
                r.CLAS_FINAL = clasFinal)
                If registroDuplicado Then
                    rTable = sc.GetMessage("Empleados Líderes", "Duplicados")
                    Return Ok(New With {.d = False, .r = rTable})
                End If

                jTable.Add(New Registro With {
                        .LLAVE = llave,
                        .ZONA = zona,
                        .PLAZA = plaza,
                        .CR_PALZA = crPlaza,
                        .CR_TIENDA = crTienda,
                        .DIMENSION = dimension,
                        .TIENDA = tienda,
                        .DISTRITO = distrito,
                        .ASESOR = asesor,
                        .MESOPS = mesOps,
                        .LIDER = lider,
                        .RFC_NOEMP = rfcNoemp,
                        .CLAS_FINAL = clasFinal
                    })

            Next

            Dim success As Boolean = False
            Dim partialC As Boolean = False
            Dim hayRegistros As Boolean
            Dim xlsx As New DataTable()

            If jTable.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage("Empleados Líderes", "SinRegistros")})
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

            valor = "FECHA PARA PROCESAR SALDOS"

            Dim ws As New WebServiceICMGeneral()
            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcodev"
            End If

            Dim FechaMesProcesando As DataTable = ws.Get_Inicio_Mes_Procesando(valor, Model)
            Dim jsonTableFechaMesProcesando As String = JsonConvert.SerializeObject(FechaMesProcesando)

            Dim StarDate As String = String.Empty

            If FechaMesProcesando IsNot Nothing AndAlso FechaMesProcesando.Rows.Count > 0 Then
                Dim valorRaw As Object = FechaMesProcesando.Rows(0)("StarDate")

                If valorRaw IsNot Nothing AndAlso Not IsDBNull(valorRaw) Then
                    Dim fechaConvertida As DateTime

                    If DateTime.TryParse(valorRaw.ToString(), fechaConvertida) Then
                        StarDate = fechaConvertida.ToString("yyyy-MM-dd")
                    Else
                        Throw New Exception("El valor de StarDate no tiene un formato de fecha válido.")
                    End If
                Else
                    Throw New Exception("El valor de StarDate es nulo o vacío.")
                End If
            Else
                Throw New Exception("No se encontró ningún registro en FechaMesProcesando.")
            End If

            Dim Payee_ As DataTable = ws.Get_Payee_(Model)
            Dim jsonPayee_ As String = JsonConvert.SerializeObject(Payee_)

            Dim AsignacionCentroTrabajoCompleto As DataTable = ws.Get_sptAsignacionCentroTrabajoCompleto(Model)
            Dim jsonAsignacionCentroTrabajoCompleto As String = JsonConvert.SerializeObject(AsignacionCentroTrabajoCompleto)


            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("CALL public.importarcatalogo(@p_tabladestino, @p_tablajson)", conn)
                    cmd.Parameters.AddWithValue("p_tabladestino", NpgsqlDbType.Text, "payee_")
                    cmd.Parameters.AddWithValue("p_tablajson", NpgsqlDbType.Json, jsonPayee_)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand("CALL public.importarcatalogo(@p_tabladestino, @p_tablajson)", conn)
                    cmd.Parameters.AddWithValue("p_tabladestino", NpgsqlDbType.Text, "sptAsignacionCentroTrabajo")
                    cmd.Parameters.AddWithValue("p_tablajson", NpgsqlDbType.Json, jsonAsignacionCentroTrabajoCompleto)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand("SELECT public.spfemcoimportclasificaciones_empleadoslideres(@jtable , @jsonpayee_, @jsonasignacioncentrotrabajocompleto, @datetoprocess)", conn)
                    cmd.Parameters.AddWithValue("jtable", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("jsonpayee_", NpgsqlDbType.Json, jsonPayee_)
                    cmd.Parameters.AddWithValue("jsonasignacioncentrotrabajocompleto", NpgsqlDbType.Json, jsonAsignacionCentroTrabajoCompleto)
                    cmd.Parameters.AddWithValue("datetoprocess", NpgsqlDbType.Text, StarDate)
                    success = cmd.ExecuteScalar()
                End Using

                Dim query As String = $"SELECT ""TiendaID"", ""EmpleadoID"", ""ConceptoEvaluadoID"", ""Fecha"", ""Calificacion"", ""CalificacionTexto"", ""Insercion"", ""CalificacionTextoID"", ""Usuario"" FROM ""FEMCO_dtOxxoTdaEvaluaciones"" LIMIT 1"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        Using dataTable As New DataTable()
                            adapter.Fill(dataTable)
                            hayRegistros = dataTable.Rows.Count > 0
                        End Using
                    End Using
                End Using
            End Using

            If xlsx.Rows.Count > 0 Then
                filePath = fc.BuildXlsx(xlsx, "EmpleadosLideres")
                partialC = hayRegistros
            End If

            SendSFTP()

            If Not hayRegistros Then
                rTable = sc.GetMessage("Empleados Líderes", "SinImportacion")
                Return Ok(New With {.d = False, .r = rTable, .f = filePath})
            ElseIf success = True And partialC = False Then
                rTable = sc.GetMessage("Empleados Líderes", "CargaCompleta")
                Return Ok(New With {.d = 1, .r = rTable})
            ElseIf success = True And partialC = True Then
                rTable = sc.GetMessage("Empleados Líderes", "CargaParcial")
                Return Ok(New With {.d = 2, .r = rTable, .f = filePath})
            Else
                rTable = sc.GetMessage("Empleados Líderes", "Error",
                        New List(Of String) From {"TiendaID", "EmpleadoID", "ConceptoEvaluadoID", "Fecha", "Calificacion", "CalificacionTexto", "Insercion", "CalificacionTextoID", "Usuario", "Plaza", "Tienda", "IDConcurso"},
                        New List(Of String) From {"Error en Tienda", "Error en Empleado", "Error en Concepto", Now.ToString("yyyy-MM-dd"), "0", "Error", Now.ToString("yyyy-MM-dd"), "Error", "Error", "10CAN DESCRIPCION", "50GLC", "IDConcurso"})
                Return Ok(New With {.d = False, .r = rTable})

            End If
        Catch ex As Exception
            mLog.insertLog("EmpleadosLideresController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.EmpleadosLideres
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Sub SendSuccessResponse()
        Dim ws As New WebServiceICMGeneral()
        Dim sql As String = $"SELECT ""TiendaID"", ""EmpleadoID"", ""ConceptoEvaluadoID"", ""Fecha"", ""Calificacion"", ""CalificacionTexto"", ""Insercion"", ""CalificacionTextoID"", ""Usuario"" FROM ""FEMCO_dtOxxoTdaEvaluaciones"";;"
        Dim TableResponse As New DataTable()
        Dim mailBody As String = "Se Ejecuto el proceso de Validacion <strong>Favor de revisar el archivo anexo al correo</strong>"
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        da.Fill(TableResponse)
                    End Using
                End Using
            End Using

            Dim filePath As String = fc.BuildXlsx(TableResponse, "Empleados_Lideres")

            ws.WebServiceSendMail(mUser.Email, "ICMTools | Empleados Lideres - STATUS VALIDACION", mailBody, "femcoepdev", filePath)
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class