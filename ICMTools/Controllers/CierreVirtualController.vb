Imports System.Web.Http
Imports AjaxControlToolkit
Imports ClosedXML.Excel
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class CierreVirtualController
    Inherits ApiController

#Region "Variables Locales"
    Private ReadOnly mUser As User
    Private ReadOnly mLog As Log
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Public Class CierreVirtualRequest
        Public Property Periodo As String
    End Class

    Public Class DescargarCierreVirtualRequest
        Public Property jsonCierreVirtual As String
        Public Property jsonCierreAsignaciones As String
        Public Property jsonCierreDistritos As String
        Public Property jsonCierrePorcentajes As String
        Public Property jsonDocsGenerados As String
        Public Property Avance As String
        Public Property HorarioConsulta As String
        Public Property EstatusImportacion As String
        Public Property Periodo As String
    End Class
#End Region
    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
    End Sub

    <HttpPost>
    <Route("api/cierrevirtual/insertdata")>
    Public Function InsertData(<FromBody> request As CierreVirtualRequest) As IHttpActionResult
        Try
            Dim lMesCierre As String = ""
            Dim currentDate As String = ""
            Dim previousDate As String = ""

            Dim sanitizar As New Sanitizacion
            Dim periodoRequest As String = sanitizar.Texto(request.Periodo)

            If (String.IsNullOrEmpty(periodoRequest)) Then
                Throw New Exception("Periodo invalido")
            End If

            Dim queryICM As New QueriesICM()
            Dim parametros As New Dictionary(Of String, String)
            parametros.Add("@periodo", periodoRequest)
            Dim periodoDataTable As DataTable = queryICM.GetQuery(2, GetModel(), parametros)

            If (periodoDataTable Is Nothing Or periodoDataTable.Rows.Count.Equals(0)) Then
                Throw New Exception("Ocurrió un problema al obtener el periodo seleccionado")
            End If

            Dim periodoDataRow As DataRow = periodoDataTable.Rows(0)
            Dim StarDate As String = periodoDataRow.Field(Of Date)("Starting_").ToString("yyyyMMdd")
            Dim endDate As String = periodoDataRow.Field(Of Date)("Ending_").ToString("yyyyMMdd")
            lMesCierre = periodoDataRow.Field(Of String)("MesCierre")
            Dim Periodo As String = periodoDataRow.Field(Of String)("Periodo")

            Dim date_lStarting As Date = DateTime.ParseExact(StarDate, "yyyyMMdd",
                                        System.Globalization.CultureInfo.InvariantCulture)
            Dim date_lEnding As Date = DateTime.ParseExact(endDate, "yyyyMMdd",
                                        System.Globalization.CultureInfo.InvariantCulture)

            Dim ws As New WebServiceICMGeneral()
            Dim dtAsignacionCentroTrabajo As DataTable = ConsultasICM_sptAsignacionCentroTrabajo(StarDate, endDate)
            If (dtAsignacionCentroTrabajo Is Nothing) Then
                Throw New Exception("No hay asignaciones de centro de trabajo en este periodo")
            End If

            Dim asignaciones = dtAsignacionCentroTrabajo.AsEnumerable() _
                    .Select(Function(a) a.Field(Of String)("CentroTrabajoID")) _
                    .Distinct() _
                    .ToList()
            Dim asignaciones_json As String = JsonConvert.SerializeObject(asignaciones)

            Dim dtsptOxxoTdaJerarquia As DataTable = ConsultasICM_dtsptOxxoTdaJerarquia()
            Dim sptoxxotdajerarquia_json As String = JsonConvert.SerializeObject(dtsptOxxoTdaJerarquia)

            Dim dtCierreVirtualPrevio As DataTable = ConsultasICM_dtCierreVirtualPrevio(lMesCierre)
            Dim dtcierrevirtualprevio_json As String = JsonConvert.SerializeObject(dtCierreVirtualPrevio)

            Dim dtDistrito As DataTable = ConsultasICM_plCatalogos("DISTRITOS")
            Dim distrito_json As String = JsonConvert.SerializeObject(dtDistrito)

            Dim dt_cierre_virtual As New DataTable()
            Dim dt_cierre_porcentajes As New DataTable()
            Dim dt_cierre_asignaciones As New DataTable()
            Dim dt_docs_generados As New DataTable()
            Dim dt_cierre_distritos As New DataTable()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.fn_cierre_virtual(@mescierre ,@asignaciones_json, @dtcierrevirtualprevio_json)", conn)
                    cmd.Parameters.AddWithValue("mescierre", NpgsqlDbType.Varchar, lMesCierre)
                    cmd.Parameters.AddWithValue("asignaciones_json", NpgsqlDbType.Json, asignaciones_json)
                    cmd.Parameters.AddWithValue("dtcierrevirtualprevio_json", NpgsqlDbType.Json, dtcierrevirtualprevio_json)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt_cierre_virtual)
                    End Using
                End Using
            End Using

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.fn_cierre_virtual_porcentaje(@mescierre ,@asignaciones_json, @dtcierrevirtualprevio_json)", conn)
                    cmd.Parameters.AddWithValue("mescierre", NpgsqlDbType.Varchar, lMesCierre)
                    cmd.Parameters.AddWithValue("asignaciones_json", NpgsqlDbType.Json, asignaciones_json)
                    cmd.Parameters.AddWithValue("dtcierrevirtualprevio_json", NpgsqlDbType.Json, dtcierrevirtualprevio_json)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt_cierre_porcentajes)
                    End Using
                End Using
            End Using

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.fn_cierre_asignaciones(@fechaini, @fechafin, @mescierre ,@asignaciones_json, @dtcierrevirtualprevio_json, @sptoxxotdajerarquia_json)", conn)
                    cmd.Parameters.AddWithValue("fechaini", NpgsqlDbType.Date, date_lStarting)
                    cmd.Parameters.AddWithValue("fechafin", NpgsqlDbType.Date, date_lEnding)
                    cmd.Parameters.AddWithValue("mescierre", NpgsqlDbType.Varchar, lMesCierre)
                    cmd.Parameters.AddWithValue("asignaciones_json", NpgsqlDbType.Json, asignaciones_json)
                    cmd.Parameters.AddWithValue("dtcierrevirtualprevio_json", NpgsqlDbType.Json, dtcierrevirtualprevio_json)
                    cmd.Parameters.AddWithValue("sptoxxotdajerarquia_json", NpgsqlDbType.Json, sptoxxotdajerarquia_json)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt_cierre_asignaciones)
                    End Using
                End Using
            End Using

            Dim XXICMGenDocumentos As DataTable = ConsultasICM_XXICMGENDOCUMENTOS(Periodo)
            Dim documentosgenerados_json As String = JsonConvert.SerializeObject(XXICMGenDocumentos)

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.fn_reporte_documentos_generados(@fechaini, @fechafin, @mescierre ,@asignaciones_json, @documentosgenerados_json, @sptoxxotdajerarquia_json, @dtcierrevirtualprevio_json)", conn)
                    cmd.Parameters.AddWithValue("fechaini", NpgsqlDbType.Date, date_lStarting)
                    cmd.Parameters.AddWithValue("fechafin", NpgsqlDbType.Date, date_lEnding)
                    cmd.Parameters.AddWithValue("mescierre", NpgsqlDbType.Varchar, lMesCierre)
                    cmd.Parameters.AddWithValue("asignaciones_json", NpgsqlDbType.Json, asignaciones_json)
                    cmd.Parameters.AddWithValue("documentosgenerados_json", NpgsqlDbType.Json, documentosgenerados_json)
                    cmd.Parameters.AddWithValue("sptoxxotdajerarquia_json", NpgsqlDbType.Json, sptoxxotdajerarquia_json)
                    cmd.Parameters.AddWithValue("dtcierrevirtualprevio_json", NpgsqlDbType.Json, dtcierrevirtualprevio_json)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt_docs_generados)
                    End Using
                End Using
            End Using

            Dim doc_gen_percentage As Decimal = 0
            If (dt_docs_generados.Rows.Count > 0) Then
                dt_docs_generados.AsEnumerable().Average(Function(r) Convert.ToDecimal(r("avance")))
            End If
            doc_gen_percentage = Math.Round(doc_gen_percentage, 2)

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.fn_cierre_distritos(@fechaini, @fechafin, @mescierre ,@asignaciones_json, @distrito_json, @dtcierrevirtualprevio_json, @sptoxxotdajerarquia_json)", conn)
                    cmd.Parameters.AddWithValue("fechaini", NpgsqlDbType.Date, date_lStarting)
                    cmd.Parameters.AddWithValue("fechafin", NpgsqlDbType.Date, date_lEnding)
                    cmd.Parameters.AddWithValue("mescierre", NpgsqlDbType.Varchar, lMesCierre)
                    cmd.Parameters.AddWithValue("asignaciones_json", NpgsqlDbType.Json, asignaciones_json)
                    cmd.Parameters.AddWithValue("distrito_json", NpgsqlDbType.Json, distrito_json)
                    cmd.Parameters.AddWithValue("dtcierrevirtualprevio_json", NpgsqlDbType.Json, dtcierrevirtualprevio_json)
                    cmd.Parameters.AddWithValue("sptoxxotdajerarquia_json", NpgsqlDbType.Json, sptoxxotdajerarquia_json)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt_cierre_distritos)
                    End Using
                End Using
            End Using

            Dim string_cierre_virtual As String = DataTableRowsToHtml(dt_cierre_virtual, "")
            Dim string_cierre_porcentajes As String = DataTableRowsToHtml(dt_cierre_porcentajes, "cierre_porcentajes")
            Dim string_cierre_asignaciones As String = DataTableRowsToHtml(dt_cierre_asignaciones, "")
            Dim string_docs_generados As String = DataTableRowsToHtml(dt_docs_generados, "")
            Dim string_cierre_distritos As String = DataTableRowsToHtml(dt_cierre_distritos, "")

            Dim jsonCierreVirtual As String = JsonConvert.SerializeObject(dt_cierre_virtual)
            Dim jsonCierrePorcentajes As String = JsonConvert.SerializeObject(dt_cierre_porcentajes)
            Dim jsonCierreAsignaciones As String = JsonConvert.SerializeObject(dt_cierre_asignaciones)
            Dim jsonCierreDistritos As String = JsonConvert.SerializeObject(dt_cierre_distritos)
            Dim jsonDocsGenerados As String = JsonConvert.SerializeObject(dt_docs_generados)

            Return Ok(New With {
                          .doc_gen_percentage = doc_gen_percentage,
                          .cierre_virtual = string_cierre_virtual,
                          .cierre_porcentajes = string_cierre_porcentajes,
                          .cierre_asignaciones = string_cierre_asignaciones,
                          .docs_generados = string_docs_generados,
                          .cierre_distritos = string_cierre_distritos,
                          .json_cierre_virtual = jsonCierreVirtual,
                          .json_cierre_porcentajes = jsonCierrePorcentajes,
                          .json_cierre_asignaciones = jsonCierreAsignaciones,
                          .json_cierre_distritos = jsonCierreDistritos,
                          .json_docs_generados = jsonDocsGenerados
                          })
        Catch ex As Npgsql.PostgresException
            mLog.insertLog("CierreVirtualController", "InsertData", ex.Where)
            mLog.insertLog("CierreVirtualController", "InsertData", ex.MessageText)
            mLog.insertLog("CierreVirtualController", "InsertData", ex.Message)
            mLog.NotificacionError(ex, "Cierre Virtual")
            Return InternalServerError(ex)
        Catch ex As Exception
            mLog.insertLog("CierreVirtualController", "InsertData", ex.Message)
            mLog.NotificacionError(ex, "Cierre Virtual")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/cierrevirtual/ImportacionICM")>
    Public Function ImportacionICM() As IHttpActionResult
        Try
            Dim model = GetModel()
            Dim import_id = GetImportId()

            Dim ws As New WebServiceICMGeneral()
            Dim runId = ws.ImportacionICM(model, import_id)
            Dim status = ws.StatusImportacionICM(model, runId)

            Return Ok(New With {.status_importacion = status})
        Catch ex As Exception
            If (Not ex.Message.Contains("XXICMGENDOCUMENTOS")) Then
                mLog.NotificacionError(ex, "Cierre Virtual")
            End If
            mLog.insertLog("CierreVirtualController", "ImportacionICM", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/cierrevirtual/descargar")>
    Public Function Descargar(<FromBody> request As DescargarCierreVirtualRequest) As IHttpActionResult
        Try
            Dim dtCierreVirtual As DataTable = JsonConvert.DeserializeObject(Of DataTable)(request.jsonCierreVirtual)
            Dim dtPlazas As DataTable = JsonConvert.DeserializeObject(Of DataTable)(request.jsonCierreAsignaciones)
            Dim dtDistritos As DataTable = JsonConvert.DeserializeObject(Of DataTable)(request.jsonCierreDistritos)
            Dim dtCierrePorcentajes As DataTable = JsonConvert.DeserializeObject(Of DataTable)(request.jsonCierrePorcentajes)
            Dim dtOracle As DataTable = JsonConvert.DeserializeObject(Of DataTable)(request.jsonDocsGenerados)

            Dim sanitizar As New Sanitizacion
            Dim avance As Double = sanitizar.TextoADouble(request.Avance)
            Dim estatusImportacion As String = sanitizar.Texto(request.EstatusImportacion)
            Dim horarioConsulta As DateTime = sanitizar.TextoADateTime(request.HorarioConsulta, "dd/MM/yyyy hh:mm tt")
            Dim periodo As String = sanitizar.Texto(request.Periodo)

            Dim timestamp As String = horarioConsulta.ToString("hh.mm tt")
            Dim fileName As String = $"C2 {timestamp} CierreVirtual_DocGene_SinGraficas_Avante.xlsx"
            Dim filePath As String = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + fileName)

            Using wb As New XLWorkbook()
                Dim ws As IXLWorksheet = wb.Worksheets.Add("CierreVirtual")
                ws.ShowGridLines = False

                ' Periodo ( fila 1)
                Dim safePeriodo = sanitizar.ExcelTexto(periodo)
                Dim trustPeriodo = HttpUtility.HtmlEncode(safePeriodo)
                ws.Cell("B1").Value = "Periodo:"
                ws.Cell("B1").Style.Font.FontSize = 10
                ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left
                ws.Cell("C1").Value = trustPeriodo
                ws.Cell("C1").Style.Font.FontSize = 10
                ws.Cell("C1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left

                ' Timestamp arriba a la derecha (columna Z, fila 1)
                Dim safeHorarioConsulta = sanitizar.ExcelTexto(horarioConsulta.ToString("dd/MM/yyyy hh:mm tt"))
                ws.Cell("J1").Value = String.Concat("Horario de consulta: ", safeHorarioConsulta)
                ws.Cell("J1").Style.Font.FontSize = 8
                ws.Cell("J1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right

                ' Título principal (fila 3, columnas B-J)
                EscribirTituloCentrado(ws, 3, 2, 10)

                ' Tabla resumen Nacional (fila 6, columna E)
                EscribirTablaResumen(ws, dtCierreVirtual, 6)

                ' Indicadores verdes (fila 9)
                EscribirIndicadores(ws, dtCierrePorcentajes, 9)

                ' Subtítulo Oracle (fila 11)
                ws.Cell(11, 7).Value = "Generación de Documentos Oracle EBS"
                ws.Cell(11, 7).Style.Font.Bold = True
                ws.Cell(11, 7).Style.Font.FontSize = 10

                ' Subtítulo Oracle (fila 12)
                Dim safeEstatusImportacion = sanitizar.ExcelTexto(estatusImportacion)
                Dim safeEstatusImportacionExcel As String = System.Web.HttpUtility.HtmlEncode(safeEstatusImportacion)
                ws.Cell(12, 7).Value = String.Concat("Status de importación: ", safeEstatusImportacionExcel)
                ws.Cell(12, 7).Style.Font.Bold = True
                ws.Cell(12, 7).Style.Font.FontSize = 10

                ' Tabla Plazas - lado izquierdo (fila 14, columna B)
                EscribirTablaPlazas(ws, dtPlazas, 14)

                ' Tabla Oracle EBS - lado derecho (fila 14, columna N)
                EscribirTablaOracle(ws, dtOracle, 14)

                ' Pie de Pagina Oracle (fila 34)
                ws.Cell(34, 7).Value = "% de Avance:"
                ws.Cell(34, 7).Style.Font.Bold = True
                ws.Cell(34, 7).Style.Font.FontSize = 10
                ws.Cell(34, 8).Value = avance.ToString("0.00")
                ws.Cell(34, 8).Style.Font.Bold = True
                ws.Cell(34, 8).Style.Font.FontSize = 10

                ' Tabla Distritos (fila 37, columna B)
                EscribirTablaDistritos(ws, dtDistritos, 37)

                AjustarColumnas(ws)
                wb.SaveAs(filePath)
            End Using

            Dim safeFilePath = HttpUtility.HtmlEncode(filePath)
            Return Ok(New With {.f = safeFilePath})
        Catch ex As Exception
            mLog.insertLog("CierreVirtualController", "Descargar", ex.Message)
            mLog.NotificacionError(ex, "Cierre Virtual")
            Return InternalServerError(ex)
        End Try
    End Function

    Public Function DataTableRowsToHtml(dt As DataTable, table As String) As String
        If dt Is Nothing OrElse dt.Columns.Count = 0 Then
            Return String.Empty
        End If

        Dim html As New System.Text.StringBuilder()

        For Each row As DataRow In dt.Rows
            html.Append("<tr>")
            Dim si_cell As Decimal = 0

            For Each col As DataColumn In dt.Columns
                Dim cell_value As String = row(col).ToString()
                Dim cell_style As String = String.Empty

                If col.ColumnName = "si" Then
                    si_cell = Convert.ToDecimal(cell_value)
                End If

                If col.ColumnName = "avance" Then
                    cell_value &= "%"
                End If

                If table = "cierre_porcentajes" Then
                    If si_cell >= 90 Then
                        cell_style = " style=""background-color:#006400; color:white; font-weight:bold;"""
                    Else
                        cell_style = " style=""background-color:#800000; color:white; font-weight:bold;"""
                    End If
                End If

                html.AppendFormat("<td{0}>{1}</td>", cell_style, System.Web.HttpUtility.HtmlEncode(cell_value))
            Next
            html.Append("</tr>")
        Next

        Return html.ToString()
    End Function

    Private Function ConsultasICM_DateStringPeriods() As DataTable
        Dim dtDateStringPeriods As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            dtDateStringPeriods = ws.Get_DateStringPeriods(GetModel())

        Catch ex As Exception
        End Try

        Return dtDateStringPeriods
    End Function

    ''' <summary>
    ''' Obtiene el modelo del usuario actual.
    ''' Si el modelo es "DEBUG", retorna el valor por defecto.
    ''' </summary>
    ''' <returns>Modelo del usuario o valor por defecto si está en modo DEBUG.</returns>
    Public Function GetModel() As String
        Dim Model As String = Nothing
        If mUser.Model = "DEBUG" Then
            Model = "femcoqa"
        Else
            Model = mUser.Model
        End If

        Return Model
    End Function

    Public Function GetImportId() As Integer
        Dim import_id As Integer = 0
        Dim dt_import_info As New DataTable()

        Using conn As New NpgsqlConnection(NpgSQL)
            conn.Open()
            Using cmd As New NpgsqlCommand("SELECT * FROM public.fn_importid_by_model(@p_modelo)", conn)
                cmd.Parameters.AddWithValue("p_modelo", NpgsqlDbType.Varchar, GetModel())

                Using adapter As New NpgsqlDataAdapter(cmd)
                    adapter.Fill(dt_import_info)
                End Using
            End Using
        End Using

        If dt_import_info?.Rows.Count > 0 Then
            import_id = If(IsDBNull(dt_import_info.Rows(0)("import_id")), 0, dt_import_info.Rows(0)("import_id"))
        End If

        Return import_id
    End Function

    Private Function ConsultasICM_Time_(StarDate As String, EndDate As String) As DataTable
        Dim dtTime_ As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            '   Dim StartingCurrent_ As String = "20250501"
            '  Dim StartingPrevious_ As String = "20250601"
            '          Dim StartingCurrent_ As String = DateAdd(DateInterval.Month, -1, Date.Now).ToString("yyyyMM") & "01"
            '         Dim StartingPrevious_ As String = Date.Now.ToString("yyyyMM") & "01"

            dtTime_ = ws.Get_Time_(StarDate, EndDate, GetModel())

        Catch ex As Exception
        End Try

        Return dtTime_
    End Function

    Private Function ConsultasICM_sptAsignacionCentroTrabajo(FechaInicio As String, FechaFin As String) As DataTable
        Dim dt As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            dt = ws.Get_sptAsignacionCentroTrabajo(FechaInicio, FechaFin, GetModel())

        Catch ex As Exception
        End Try

        Return dt
    End Function

    Private Function ConsultasICM_XXICMGENDOCUMENTOS(Periodo As String) As DataTable
        Dim dt As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            Dim queryICM As New QueriesICM()
            Dim parametros As New Dictionary(Of String, String)
            parametros.Add("@periodo", Periodo)
            dt = queryICM.GetQuery(3, GetModel(), parametros)

        Catch ex As Exception
        End Try

        Return dt
    End Function

    Private Function ConsultasICM_dtsptOxxoTdaJerarquia() As DataTable
        Dim dt As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            dt = ws.Get_sptOxxoTdaJerarquia(GetModel())

        Catch ex As Exception
        End Try

        Return dt
    End Function

    Private Function ConsultasICM_dtCierreVirtualPrevio(Periodo As String) As DataTable
        Dim dt As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            dt = ws.Get_dtCierreVirtualPrevio(Periodo, GetModel())

        Catch ex As Exception
        End Try

        Return dt
    End Function

    Private Function ConsultasICM_plCatalogos(CatalogosID As String) As DataTable
        Dim dt As DataTable = Nothing
        Dim ws As New WebServiceICMGeneral()

        Try

            dt = ws.Get_plCatalogos(CatalogosID, GetModel())

        Catch ex As Exception
        End Try

        Return dt
    End Function


#Region " Descargar "

    Dim RojoEncabezado As String = "#A52A2A"
    Dim VerdeEncabezado As String = "#556B2F"
    Dim VerdeBarraProgreso As String = "#006400"
    Dim VerdePorcentajes As String = "#00B050"
    Dim AmarilloPorcentajes As String = "#FFFF00"
    Dim RojoPorcentajes As String = "#C00000"
    Dim RojoProgreso As String = "#800000"
    Dim ColorBorde As String = "#D3D3D3"

    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirTituloCentrado(ws As IXLWorksheet, fila As Integer, colIni As Integer, colFin As Integer)
        Dim rng = ws.Range(ws.Cell(fila, colIni), ws.Cell(fila, colFin))
        rng.Merge()
        rng.FirstCell().Value = "Cierre Virtual"
        With rng.FirstCell().Style
            .Font.Bold = True
            .Font.FontSize = 14
            .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
        End With
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' Tabla Nacional: columnas Nacional | Cerradas | Avance
    ' dtResumenNacional debe tener esas mismas columnas
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirTablaResumen(ws As IXLWorksheet, dt As DataTable, fila As Integer)
        Dim colIni As Integer = 5  ' E
        Dim headers() As String = {"Nacional", "Cerradas", "Avance"}
        Dim columns() As String = {"total", "si", "avance"}

        For i = 0 To 2
            Dim c = ws.Cell(fila, colIni + i)
            c.Value = headers(i)
            With c.Style
                .Font.Bold = True
                .Font.FontColor = XLColor.White
                .Fill.BackgroundColor = XLColor.FromHtml(RojoEncabezado)
                .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
                .Border.OutsideBorder = XLBorderStyleValues.Thin
                .Border.OutsideBorderColor = XLColor.FromHtml(ColorBorde)
            End With
        Next

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim row = dt.Rows(0)
            For i = 0 To 2
                Dim c = ws.Cell(fila + 1, colIni + i)
                If dt.Columns.Contains(columns(i)) Then
                    If (i = 2) Then
                        Dim avance As Double = If(Double.TryParse(row(columns(i)), avance), avance, 0)
                        c.Value = String.Concat(avance.ToString("0.00"), "%")
                    Else
                        c.Value = row(columns(i)).ToString()
                    End If
                End If
                With c.Style
                    .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
                    .Border.OutsideBorder = XLBorderStyleValues.Thin
                    .Border.OutsideBorderColor = XLColor.FromHtml(ColorBorde)
                End With
            Next
        End If
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' Indicadores: celda verde con % cierre | celda roja con % restante
    ' dtResumenNacional debe tener columnas: PctCierre, PctRestante
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirIndicadores(ws As IXLWorksheet, dt As DataTable, fila As Integer)
        Dim val1 As Double = 0
        Dim val2 As Double = 0
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            If dt.Columns.Contains("si") Then val1 = If(Double.TryParse(dt.Rows(0)("si").ToString(), val1), val1, 0)
            If dt.Columns.Contains("no") Then val2 = If(Double.TryParse(dt.Rows(0)("no").ToString(), val2), val2, 0)
        End If

        ' Celda verde (E:F fusionadas)
        Dim rVerde = ws.Range(ws.Cell(fila, 5), ws.Cell(fila, 6))
        rVerde.Merge()
        rVerde.FirstCell().Value = val1.ToString("0.00")
        With rVerde.FirstCell().Style
            .Font.Bold = True : .Font.FontSize = 14 : .Font.FontColor = XLColor.White
            If (val1 >= 90) Then
                .Fill.BackgroundColor = XLColor.FromHtml(VerdeBarraProgreso)
            Else
                .Fill.BackgroundColor = XLColor.FromHtml(RojoEncabezado)
            End If
            .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
        End With

        ' Celda roja (G)
        Dim cRojo = ws.Cell(fila, 7)
        cRojo.Value = val2.ToString("0.00")
        With cRojo.Style
            .Font.Bold = True : .Font.FontSize = 14 : .Font.FontColor = XLColor.White
            If (val1 >= 90) Then
                .Fill.BackgroundColor = XLColor.FromHtml(VerdeBarraProgreso)
            Else
                .Fill.BackgroundColor = XLColor.FromHtml(RojoEncabezado)
            End If
            .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
        End With
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' Tabla Plazas: NombrePlaza | Tiendas | Cerradas | Avance
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirTablaPlazas(ws As IXLWorksheet, dt As DataTable, filaInicio As Integer)
        Dim headers() As String = {"Nombre Plaza", "Tiendas", "Cerradas", "Avance"}
        Dim campos() As String = {"NombrePlaza", "Tiendas", "si", "Avance"}
        EscribirEncabezado(ws, headers, filaInicio, 2, RojoEncabezado)
        EscribirFilasDatos(ws, dt, campos, filaInicio + 1, 2)
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' Tabla Oracle: NombrePlaza | Tiendas | DocGenerado | Avance
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirTablaOracle(ws As IXLWorksheet, dt As DataTable, filaInicio As Integer)
        Dim headers() As String = {"Nombre Plaza", "Tiendas", "Doc.Generados", "Avance"}
        Dim campos() As String = {"NombrePlaza", "Tiendas", "si", "Avance"}
        EscribirEncabezado(ws, headers, filaInicio, 7, VerdeEncabezado)
        EscribirFilasDatos(ws, dt, campos, filaInicio + 1, 7)
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' Tabla Distritos: Distrito | Nombre | Tiendas | Cerradas | Avance
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirTablaDistritos(ws As IXLWorksheet, dt As DataTable, filaInicio As Integer)
        Dim headers() As String = {"Distrito", "Nombre", "", "", "Tiendas", "Cerradas", "Avance"}
        Dim campos() As String = {"Distrito", "Nombre", "", "", "Tiendas", "si", "Avance"}
        EscribirEncabezado(ws, headers, filaInicio, 3, RojoEncabezado)

        Dim filaFinMerge As Integer = dt.Rows.Count + 2 + filaInicio
        For filaMerge = filaInicio To filaFinMerge
            Dim rng = ws.Range(ws.Cell(filaMerge, 4), ws.Cell(filaMerge, 6))
            rng.Style.Alignment.Horizontal = If(filaMerge = filaInicio, XLAlignmentHorizontalValues.Center, XLAlignmentHorizontalValues.Left)
            rng.Merge()
        Next
        EscribirFilasDatos(ws, dt, campos, filaInicio + 1, 3)
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' HELPER: fila de encabezado con color
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirEncabezado(ws As IXLWorksheet, headers() As String, fila As Integer, colIni As Integer, colorHex As String)
        For i = 0 To headers.Length - 1
            Dim c = ws.Cell(fila, colIni + i)
            c.Value = headers(i)
            With c.Style
                .Font.Bold = True
                .Font.FontColor = XLColor.White
                .Fill.BackgroundColor = XLColor.FromHtml(colorHex)
                .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
                .Border.OutsideBorder = XLBorderStyleValues.Thin
                .Border.OutsideBorderColor = XLColor.FromHtml(ColorBorde)
                .Font.FontSize = 9
            End With
        Next
    End Sub

    ' ──────────────────────────────────────────────────────────────
    ' HELPER: filas de datos con formato condicional en Avance
    ' ──────────────────────────────────────────────────────────────
    Private Sub EscribirFilasDatos(ws As IXLWorksheet, dt As DataTable, campos() As String, filaInicio As Integer, colIni As Integer)
        If dt Is Nothing Then Return

        Dim avanceIdx As Integer = Array.IndexOf(campos, "Avance")

        For r = 0 To dt.Rows.Count - 1
            Dim row = dt.Rows(r)
            For c = 0 To campos.Length - 1
                Dim cell = ws.Cell(filaInicio + r, colIni + c)
                If dt.Columns.Contains(campos(c)) Then
                    If c = avanceIdx Then
                        Dim avance As Double = If(Double.TryParse(row(campos(c)), avance), avance, 0)
                        cell.Value = String.Concat(avance.ToString("0.00"), "%")
                    Else
                        cell.Value = row(campos(c)).ToString()
                    End If
                End If
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml(ColorBorde)
                cell.Style.Font.FontSize = 9
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center

                ' Formato condicional columna Avance
                If c = avanceIdx Then
                    Dim pct As Double = 0
                    Dim strVal As String = row(campos(c)).ToString().Replace("%", "").Trim()
                    If Double.TryParse(strVal, pct) Then
                        If pct >= 100 Then
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(VerdePorcentajes)
                            cell.Style.Font.FontColor = XLColor.White
                        ElseIf pct >= 50 Then
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(AmarilloPorcentajes)
                            cell.Style.Font.FontColor = XLColor.Black
                        Else
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(RojoPorcentajes)
                            cell.Style.Font.FontColor = XLColor.White
                        End If
                    End If
                End If
            Next
        Next
    End Sub

    ' ──────────────────────────────────────────────────────────────
    Private Sub AjustarColumnas(ws As IXLWorksheet)
        ws.Column(2).Width = 18   ' B  NombrePlaza / Distrito
        ws.Column(3).Width = 14   ' C  Tiendas
        ws.Column(4).Width = 9    ' D  Cerradas
        ws.Column(5).Width = 9    ' E  Avance / Nacional
        ws.Column(6).Width = 9    ' F  Cerradas (nacional)
        ws.Column(7).Width = 11    ' G  Avance (nacional)
        ws.Column(8).Width = 11  ' H  Nombre Oracle
        ws.Column(9).Width = 11   ' I  Tiendas Oracle
        ws.Column(10).Width = 13  ' J  Doc.Generado
        ws.Column(11).Width = 9   ' K  Avance Oracle
    End Sub

#End Region
End Class
