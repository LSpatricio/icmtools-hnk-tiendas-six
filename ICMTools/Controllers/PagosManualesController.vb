Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Namespace Controllers
    Public Class PagosManualesController
        Inherits ApiController

#Region " Propiedades Privadas "

        Private ReadOnly _Pantalla As String = "Pagos Manuales"
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

        <HttpPost>
        <Route("api/pagosmanuales/insertdata")>
        Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
            Try
                Thread.Sleep(1000)
                Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

                Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
                If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

                Dim jTable As New List(Of Object)

                Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
                Dim EmpleadoID As String = Nothing
                Dim CentroTrabajoID As String = Nothing
                Dim Fecha As String = Nothing

                Dim Empleados As String = ""
                Dim CentrosTrabajo As String = ""
                Dim Componente As String = ""

                For row As Integer = 2 To usedRows

                    EmpleadoID = ExcelArray(row, 1).ToString()
                    CentroTrabajoID = ExcelArray(row, 2).ToString()
                    Dim ComponenteID As String = ExcelArray(row, 3).ToString()
                    Fecha = ExcelArray(row, 4).ToString()
                    Dim Monto As String = ExcelArray(row, 5).ToString()
                    Dim Comentarios As String = ExcelArray(row, 6).ToString()

                    Dim Aprobado As String = ExcelArray(row, 7).ToString()
                    Dim FechaAprobacion As String = ExcelArray(row, 8).ToString()
                    Dim Aprobador As String = ExcelArray(row, 9).ToString()
                    Dim Insercion As String = ExcelArray(row, 10).ToString()

                    If String.IsNullOrWhiteSpace(EmpleadoID) AndAlso String.IsNullOrWhiteSpace(CentroTrabajoID) AndAlso String.IsNullOrWhiteSpace(ComponenteID) Then Continue For

                    jTable.Add(New With {
                        .EmpleadoID = EmpleadoID,
                        .CentroTrabajoID = CentroTrabajoID,
                        .ComponenteID = ComponenteID,
                        .Fecha = Fecha,
                        .Monto = Monto,
                        .Comentarios = Comentarios,
                        .Aprobado = Aprobado,
                        .FechaAprobacion = FechaAprobacion,
                        .Aprobador = Aprobador,
                        .Insercion = Insercion
                    })

                    If (Empleados.Trim.Length = 0) Then
                        Empleados = "'" + EmpleadoID + "'"
                    Else
                        Empleados = Empleados + ", '" + EmpleadoID + "' "
                    End If

                    If (CentrosTrabajo.Trim.Length = 0) Then
                        CentrosTrabajo = "'" + CentroTrabajoID + "'"
                    Else
                        CentrosTrabajo = CentrosTrabajo + ",'" + CentroTrabajoID + "' "
                    End If

                    If (Componente.Trim.Length = 0) Then
                        Componente = "'" + ComponenteID + "'"
                    Else
                        Componente = Componente + ",'" + ComponenteID + "' "
                    End If

                Next

                Dim rTable As String = Nothing
                Dim Model As String = mUser.Model
                If Model = "DEBUG" Then
                    Model = "femcodev"
                End If

                If jTable.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "sinimportacion")})

                Dim jsonTable As String = JsonConvert.SerializeObject(jTable)

                Dim ws As New WebServiceICMGeneral()
                Dim success As Boolean = False
                Dim partialC As Boolean = False
                Dim Parametros As String = ""

                Dim columnascatEmpleadosFEMCODEV As New List(Of String) From {"PayeeID_"}
                Dim PayeeFEMCODEV As DataTable = ws.ConsultaICMAPIQuery(columnascatEmpleadosFEMCODEV, "Payee_", Model, Parametros)
                Dim jsonTablePayee As String = JsonConvert.SerializeObject(PayeeFEMCODEV)

                Dim columnasplCatalogosFEMCODEV As New List(Of String) From {"ID", "Descripcion"}
                Dim CatalogosFEMCODEV As DataTable = ws.ConsultaICMAPIQuery(columnasplCatalogosFEMCODEV, "plCatalogos", Model, Parametros)
                Dim jsonTableCatalgos As String = JsonConvert.SerializeObject(CatalogosFEMCODEV)

                Dim columnasPeriodosFEMCODEV As New List(Of String) From {"PeriodName", "StarDate", "EndDate", "IsOutputInterface"}
                Dim PeriodosFEMCODEV As DataTable = ws.ConsultaICMAPIQuery(columnasPeriodosFEMCODEV, "DateStringPeriods", Model, Parametros)
                Dim jsonTablePeriodos As String = JsonConvert.SerializeObject(PeriodosFEMCODEV)

                Dim Errores As New DataTable()

                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Using cmd As New NpgsqlCommand("SELECT * FROM public.femcoepsap_validacion_archivo_pagosmanuales(@file_data_json, @payee_json, @plcatalogos_json, @datestringperiods_json)", conn)
                        cmd.Parameters.AddWithValue("file_data_json", NpgsqlDbType.Json, jsonTable)
                        cmd.Parameters.AddWithValue("payee_json", NpgsqlDbType.Json, jsonTablePayee)
                        cmd.Parameters.AddWithValue("plcatalogos_json", NpgsqlDbType.Json, jsonTableCatalgos)
                        cmd.Parameters.AddWithValue("datestringperiods_json", NpgsqlDbType.Json, jsonTablePeriodos)

                        Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                            Errores.Load(reader)
                        End Using

                        success = True
                    End Using
                End Using

                Dim cantidadErrores As Integer = Errores.Rows.Count
                Dim cantidadRegistros As Integer = usedRows - 1
                Dim filePath As String = Nothing

                If (cantidadErrores) > 0 Then
                    filePath = fc.BuildXlsx(Errores, "PagosManuales")
                    partialC = True
                End If


                If cantidadRegistros = cantidadErrores Then
                    rTable = sc.GetMessage(_Pantalla, "sinimportacion", cantidadRegistros, cantidadErrores)

                    Return Ok(New With {
                         .d = False,
                         .r = rTable,
                         .f = filePath
                      })

                End If

                If success = True And partialC = False Then
                    CargarInformacion()
                    SendSFTP()
                    rTable = sc.GetMessage(_Pantalla, "CargaCompleta")
                    Return Ok(New With {.d = 1, .r = rTable})
                ElseIf success = True And partialC = True Then
                    rTable = sc.GetMessage(_Pantalla, "ProcesoIncompleto", cantidadRegistros, cantidadErrores)
                    Return Ok(New With {.d = 5, .r = rTable, .f = filePath})
                Else
                    rTable = sc.GetMessage(_Pantalla, "Error",
               New List(Of String) From {"EmpleadoID", "CentroTrabajoID", "ComponenteID", "Fecha", "Monto", "Comentarios", "Aprobado", "FechaAprobacion", "Aprobador", "Insercion"},
               New List(Of String) From {"001139", "001524", "00011258", "12-05-2025", "1500.00", "Comentarios adicionales", "OK", "12-09-2025", "Juan Perez", "12-09-2025"})

                    Return Ok(New With {
                  .d = False,
                  .r = rTable
                  })

                End If



            Catch ex As Exception
                mLog.insertLog("PagosManualesController", "InsertData", ex.Message)
                Return InternalServerError(ex)
            End Try
        End Function

        <HttpPost>
        <Route("api/pagosmanuales/uploaddata")>
        Public Function UploadData() As IHttpActionResult
            Try
                Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
                CargarInformacion()
                SendSFTP()
                Return Ok(New With {.d = 2, .r = mensaje})
            Catch ex As Exception
                mLog.insertLog("PagosManualesController", "UploadData", ex.Message)
                Return InternalServerError(ex)
            End Try
        End Function

        ''' <summary>
        ''' Método que carga la información
        ''' </summary>
        Private Sub CargarInformacion()
            Try
                Using conn As New NpgsqlConnection(NpgSQL)
                    Const sql As String = "CALL pagosmanuales_cargar();"
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
                envio.Pantalla = EnvioPGPClass.enuPantalla.PagosManuales
                envio.Enviar()
            Catch ex As Exception
                Throw
            End Try

        End Sub

    End Class
End Namespace