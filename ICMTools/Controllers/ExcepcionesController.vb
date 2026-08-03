Imports System.Diagnostics.Eventing
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web.Helpers
Imports System.Web.Http
Imports ClassLibrary_PGP_TO_SFTP
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Public Class ExcepcionesController
    Inherits ApiController
#Region "Variables Locales"
    Private ReadOnly mUser As User
    Private ReadOnly mLog As Log
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    Private ReadOnly attemptLimit As Integer = If(ConfigurationManager.AppSettings("attemptLimit") IsNot Nothing, Convert.ToInt32(ConfigurationManager.AppSettings("attemptLimit")), 3)
    ReadOnly fc As New FileController
    ReadOnly ws As New WebServiceICMGeneral
    Private ReadOnly _PGService As PostgreService
    ReadOnly _Sanitize As New Sanitizacion
#End Region
    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
        Me._PGService = New PostgreService()
    End Sub

#Region "Clases"
    Private Shared ReadOnly catalogCache As New System.Collections.Concurrent.ConcurrentDictionary(Of String, CatsExceptionsIaV)
    Public Class CatsExceptionsIaV
        Public Property catPeriodJson As String ''CfgDateStringPeriod
        Public Property catHPJson As String ''HistoryPayee
        Public Property timeJson As String ''Time_
        Public Property PyeJson As String ''Payee_
        Public Property catPSJson As String ''CatPersonalSubdivision
        Public Property cfgOSJson As String ''CfgOracleSAP
        Public Property cfgWPJson As String ''CfgWebPermission
        Public Property tr13Json As String ''_Result374
        Public Property cfgSHJson As String ''CfgStoreHierarchy
        Public Property catPDJson As String ''CatPersonalDivision
        Public Property catJKJson As String ''CatJobKey

        Public Sub New()
        End Sub

        Public Sub New(ByVal catPeriodJson As String, ByVal HPJson As String, ByVal PSJson As String, ByVal OSJson As String, ByVal tr13Json As String, ByVal tJson As String, ByVal PyeeJson As String, ByVal wpJson As String, ByVal pdJson As String, ByVal cfgSHJson As String, ByVal catJKJson As String)
            Me.catPeriodJson = catPeriodJson
            Me.catHPJson = HPJson
            Me.catPSJson = PSJson
            Me.cfgOSJson = OSJson
            Me.tr13Json = tr13Json
            Me.timeJson = tJson
            Me.PyeJson = PyeeJson
            Me.cfgWPJson = wpJson
            Me.catPDJson = pdJson
            Me.cfgSHJson = cfgSHJson
            Me.catJKJson = catJKJson
        End Sub
    End Class

    Public Class ExceptionsConfigRequest
        Property Society As String
        Property PersonnelDivision As String
    End Class

    Public Class DivPerRequest
        Property PersonnelDivision As String
    End Class

    Public Class ExceptionsHistoryRequest
        Property Society As String
        Property PersonnelDivision As String
        Property Period As String
    End Class
    Public Class ExceptionsHistoryDetailRequest
        Property LotData As String
    End Class
    Public Class InsertInfoBDRequest
        Property Society As String
        Property PersonnelDivision As String
        Property Period As String
        Property FileType As String
        Property Extension As String
    End Class
#End Region
#Region "Metodos POST"
    <HttpPost>
    <Route("api/excepciones/configuracion/exceptionconfig")>
    Public Function listExceptionsConfiguration(<FromBody> request As ExceptionsConfigRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim Model As String
        Dim responseDT As New DataTable

        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model

        End If
        Try
            Dim shortPD As String = request.PersonnelDivision
            Thread.Sleep(1000)
            Dim ws As New WebServiceICMGeneral()

            Dim columnascatSociety As New List(Of String) From {"IDSociety", "Description"}
            Dim catSociety As DataTable = ws.ConsultaICMAPIQuery(columnascatSociety, "CatSociety", Model)

            Dim columnasCatPD As New List(Of String) From {"IDPersonalDivision", "Description"}
            Dim catPD As DataTable = ws.ConsultaICMAPIQuery(columnasCatPD, "CatPersonalDivision", Model)

            Dim colWageType As New List(Of String) From {"IDWageType", "Description"}
            Dim catWT As DataTable = ws.ConsultaICMAPIQuery(colWageType, "CatWageType", Model)

            Dim colJK As New List(Of String) From {"IDJobKey", "Description"}
            Dim catJK As DataTable = ws.ConsultaICMAPIQuery(colJK, "CatJobKey", Model)

            Dim colPlaza As New List(Of String) From {"IDPlaza", "Description"}
            Dim catPlaza As DataTable = ws.ConsultaICMAPIQuery(colPlaza, "CatPlaza", Model)

            Dim colDistrict As New List(Of String) From {"IDDistrict", "Description"}
            Dim catDistrict As DataTable = ws.ConsultaICMAPIQuery(colDistrict, "CatDistrict", Model)

            Dim catSocietyJson As String = JsonConvert.SerializeObject(catSociety)
            Dim catPDJson As String = JsonConvert.SerializeObject(catPD)
            Dim catWTJson As String = JsonConvert.SerializeObject(catWT)
            Dim catJKJson As String = JsonConvert.SerializeObject(catJK)
            Dim catPlazaJson As String = JsonConvert.SerializeObject(catPlaza)
            Dim catDistrictJson As String = JsonConvert.SerializeObject(catDistrict)

            Using conn As New NpgsqlConnection(NpgSQL)
                Using cmd As New NpgsqlCommand("SELECT * FROM public.spicmtoolsexceptionsconfigurationselect(@p_model, @p_user, @p_society, @p_personaldivision, @p_catsociety, @p_catpersonaldivision, @p_catwagetype, @p_catjobkey, @p_catplaza, @p_catdistrict )", conn)
                    cmd.Parameters.AddWithValue("p_model", NpgsqlDbType.Varchar, Model)
                    cmd.Parameters.AddWithValue("p_user", NpgsqlDbType.Varchar, mUser.Email.ToString)
                    cmd.Parameters.AddWithValue("p_society", NpgsqlDbType.Varchar, request.Society.ToString)
                    cmd.Parameters.AddWithValue("p_personaldivision", NpgsqlDbType.Varchar, request.PersonnelDivision.ToString)
                    cmd.Parameters.AddWithValue("p_catsociety", NpgsqlDbType.Json, catSocietyJson)
                    cmd.Parameters.AddWithValue("p_catpersonaldivision", NpgsqlDbType.Json, catPDJson)
                    cmd.Parameters.AddWithValue("p_catwagetype", NpgsqlDbType.Json, catWTJson)
                    cmd.Parameters.AddWithValue("p_catjobkey", NpgsqlDbType.Json, catJKJson)
                    cmd.Parameters.AddWithValue("p_catplaza", NpgsqlDbType.Json, catPlazaJson)
                    cmd.Parameters.AddWithValue("p_catdistrict", NpgsqlDbType.Json, catDistrictJson)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(responseDT)
                    End Using
                End Using
            End Using

            Dim rowString As String = Nothing
            Dim tableString As String = "<table id='Table' class='table table-sm table-hover' style='width:100%;'>" +
                                         "<thead>" +
                                            "<tr>" +
                                                "<th style='display: none;'>Sociedad</th>" +
                                                "<th style='display: none;'>Division</th>" +
                                                "<th>CCNom</th>" +
                                                "<th>Función</th>" +
                                                "<th>Monto máximo</th>" +
                                                "<th>Permite negativo</th>" +
                                                "<th>Activo</th>" +
                                            "</tr>" +
                                         "</thead> " +
                                         "<tbody> " +
                                            "@LISTA@" +
                                         "</tbody> " +
                                         "</table>"

            If Not responseDT Is Nothing Then
                Dim txtAllowsnegative As String = Nothing
                Dim txtIsActive As String = Nothing
                For Each row As DataRow In responseDT.Rows
                    If CType(row.Item("Allowsnegative"), BitArray)(0) = True Then txtAllowsnegative = "<i class='fas fa-flag text-success'></i>" Else txtAllowsnegative = "<i class='fas fa-flag text-danger'></i>"
                    If CType(row.Item("IsActive"), BitArray)(0) = True Then txtIsActive = "<i class='fas fa-flag text-success'></i>" Else txtIsActive = "<i class='fas fa-flag text-danger'></i>"
                    rowString += "<tr>" +
                                    "<td style='display: none;'>" + row.Item("plaza").ToString + "</td>" +
                                    "<td style='display: none;'>" + row.Item("district").ToString + "</td>" +
                                    "<td>" + row.Item("IDWageType").ToString + "</td>" +
                                    "<td>" + row.Item("IDJobKey").ToString + "</td>" +
                                    "<td>" + "$ " + row.Item("MaximumValue").ToString + "</td>" +
                                    "<td><span style='display: none;'>" + row.Item("Allowsnegative").ToString + "</span>" + txtAllowsnegative + "</td>" +
                                    "<td><span style='display: none;'>" + row.Item("IsActive").ToString + "</span>" + txtIsActive + "</td>" +
                                 "</tr>"
                Next
            End If

            If Not rowString Is Nothing Then
                Return Ok(New With {.d = tableString.Replace("@LISTA@", rowString)})
            Else
                Return Ok(New With {.d = False})
            End If
        Catch ex As Exception
            mLog.insertLog("ExcepcionesController", "listExceptionsConfiguration", ex.Message)
            mLog.NotificacionError(ex)
            Return InternalServerError(ex)
        End Try
    End Function
    <HttpPost>
    <Route("api/excepciones/SelectExceptionsHistory")>
    Public Function SelectExceptionsHistory(<FromBody> request As ExceptionsHistoryRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim Model As String
        Dim responseDT As New DataTable

        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model
        End If
        Try
            Dim shortPD As String = request.PersonnelDivision
            Thread.Sleep(1000)
            Dim ws As New WebServiceICMGeneral()

            Dim columnascatSociety As New List(Of String) From {"IDSociety", "Description"}
            Dim catSociety As DataTable = ws.ConsultaICMAPIQuery(columnascatSociety, "CatSociety", Model)

            Dim columnasCatPD As New List(Of String) From {"IDPersonalDivision", "Description"}
            Dim catPD As DataTable = ws.ConsultaICMAPIQuery(columnasCatPD, "CatPersonalDivision", Model)

            Dim colPlaza As New List(Of String) From {"IDPlaza", "Description"}
            Dim catPlaza As DataTable = ws.ConsultaICMAPIQuery(colPlaza, "CatPlaza", Model)

            Dim colDistrict As New List(Of String) From {"IDDistrict", "Description"}
            Dim catDistrict As DataTable = ws.ConsultaICMAPIQuery(colDistrict, "CatDistrict", Model)

            Dim catSocietyJson As String = JsonConvert.SerializeObject(catSociety)
            Dim catPDJson As String = JsonConvert.SerializeObject(catPD)
            Dim catPlazaJson As String = JsonConvert.SerializeObject(catPlaza)
            Dim catDistrictJson As String = JsonConvert.SerializeObject(catDistrict)

            Using conn As New NpgsqlConnection(NpgSQL)
                Using cmd As New NpgsqlCommand("SELECT * FROM public.spicmtoolsexceptionsreporthistory(@p_model, @p_user, @p_period, @p_society, @p_personaldivision, @p_catplaza, @p_catdistrict, @p_catsociety, @p_catpersonaldivision)", conn)
                    cmd.Parameters.AddWithValue("p_model", NpgsqlDbType.Varchar, Model)
                    cmd.Parameters.AddWithValue("p_user", NpgsqlDbType.Varchar, mUser.Email.ToString)
                    cmd.Parameters.AddWithValue("p_period", NpgsqlDbType.Varchar, request.Period.ToString)
                    cmd.Parameters.AddWithValue("p_society", NpgsqlDbType.Varchar, request.Society.ToString)
                    cmd.Parameters.AddWithValue("p_personaldivision", NpgsqlDbType.Varchar, request.PersonnelDivision.ToString)
                    cmd.Parameters.AddWithValue("p_catplaza", NpgsqlDbType.Json, catPlazaJson)
                    cmd.Parameters.AddWithValue("p_catdistrict", NpgsqlDbType.Json, catDistrictJson)
                    cmd.Parameters.AddWithValue("p_catsociety", NpgsqlDbType.Json, catSocietyJson)
                    cmd.Parameters.AddWithValue("p_catpersonaldivision", NpgsqlDbType.Json, catPDJson)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(responseDT)
                    End Using
                End Using
            End Using

            Dim rowString As String = Nothing
            Dim tableString As String = "<table id='Table' class='table table-sm table-hover'>" +
                                         "<thead>" +
                                            "<tr>" +
                                                "<th>Lote</th>" +
                                                "<th>Fecha</th>" +
                                                "<th>Usuario</th>" +
                                                "<th>Periodo</th>" +
                                                "<th class='text-center'>Estado</th>" +
                                                "<th class='text-center'>Opciones</th>" +
                                            "</tr>" +
                                         "</thead> " +
                                         "<tbody> " +
                                            "@LISTA@" +
                                         "</tbody> " +
                                         "</table>"
            If Not responseDT Is Nothing Then

                For Each row As DataRow In responseDT.Rows
                    Dim statusIcon As String = Nothing
                    Select Case row.Item("status").ToString
                        Case "IMPORTADO"
                            statusIcon = "<i class='fas fa-upload fa-fw'></i>"
                        Case "CARGADO"
                            statusIcon = "<i class='fas fa-share-square'></i>"
                        Case "PROCESADO"
                            statusIcon = "<i class='fas fa-lock-open fa-fw'></i>"
                        Case "ENVIANDO"
                            statusIcon = "<i class='fas fa-paper-plane fa-fw'></i>"
                        Case "ENVIADO A SAP"
                            statusIcon = "<i class='fas fa-lock fa-fw'></i>"
                        Case "REEMPLAZADO"
                            statusIcon = "<i class='fas fa-exchange-alt fa-fw'></i>"
                        Case "CANCELADO"
                            statusIcon = "<i class='fas fa-lock fa-fw'></i>"
                        Case Else
                            statusIcon = ""
                    End Select

                    Dim Tooltip As String
                    Tooltip = "<span class='text primary' data-toggle='tooltip' data-placement='left' data-html='true' title='" + row.Item("status").ToString + "<br><small>" + row.Item("fecha").ToString + "</small>'>" +
                                    statusIcon +
                              "</span>"

                    rowString += "<tr>" +
                                    "<td>" + row.Item("lot").ToString + "</td>" +
                                    "<td>" + row.Item("uploaddate").ToString + "</td>" +
                                    "<td>" + row.Item("usuario").ToString + "</td>" +
                                    "<td>" + row.Item("Period").ToString + "</td>" +
                                    "<td class='text-center'>" + Tooltip + "</td>" +
                                    "<td></td>" +
                                 "</tr>"
                Next
            End If

            If Not rowString Is Nothing Then
                Return Ok(New With {.d = tableString.Replace("@LISTA@", rowString)})
            Else
                Return Ok(New With {.d = False})
            End If

        Catch ex As Exception
            mLog.insertLog("ExcepcionesController", "SelectExceptionsHistory", ex.Message)
            mLog.NotificacionError(ex)
            Return InternalServerError(ex)
        End Try
    End Function
    <HttpPost>
    <Route("api/excepciones/SelectExceptionsHistoryDetails")>
    Public Function SelectExceptionsHistoryDetails(<FromBody> request As ExceptionsHistoryDetailRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim responseT As New DataTable()
        Dim Model As String
        Dim PLot As Integer = Convert.ToInt64(request.LotData)
        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model

        End If
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.spICMToolsExceptionsReportHistoryDetailsf(@pmodel, @usr, @plot)", conn)
                    cmd.Parameters.AddWithValue("pmodel", NpgsqlDbType.Varchar, Model)
                    cmd.Parameters.AddWithValue("usr", NpgsqlDbType.Varchar, mUser.Email)
                    cmd.Parameters.AddWithValue("plot", NpgsqlDbType.Bigint, PLot)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(responseT)
                    End Using
                End Using
            End Using
            Dim rowString As String = Nothing
            Dim tableString As String = "<table id='DetailsTable' class='table table-sm table-hover'>" +
                                         "<thead>" +
                                            "<tr>" +
                                                "<th>Empleado</th>" +
                                                "<th>Fecha</th>" +
                                                "<th>CCNom</th>" +
                                                "<th>Monto</th>" +
                                                "<th>Motivo</th>" +
                                            "</tr>" +
                                         "</thead> " +
                                         "<tbody> " +
                                            "@LISTA@" +
                                         "</tbody> " +
                                         "</table>"
            If Not responseT Is Nothing Then
                For Each row As DataRow In responseT.Rows
                    rowString += "<tr>" +
                                     "<td>" + row.Item("payee").ToString + "</td>" +
                                     "<td>" + row.Item("fecha").ToString + "</td>" +
                                     "<td>" + row.Item("idwagetype").ToString + "</td>" +
                                     "<td>" + row.Item("amount").ToString + "</td>" +
                                     "<td>" + row.Item("reason").ToString + "</td>" +
                                  "</tr>"
                Next
            End If
            If Not rowString Is Nothing Then
                Return Ok(New With {.d = tableString.Replace("@LISTA@", rowString)})
            Else
                Return Ok(New With {.d = False})
            End If
        Catch ex As Exception
            mLog.insertLog("ExcepcionesController", "SelectExceptionsHistoryDetails", ex.Message)
            mLog.NotificacionError(ex)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/excepciones/InsertInfoBD")>
    Public Function InsertInfoBDExcepciones(<FromBody> request As InsertInfoBDRequest) As IHttpActionResult
        Try
            Dim cookieToken As String = ""
            Dim formToken As String = ""
            Dim cookie = HttpContext.Current.Request.Cookies(AntiForgeryConfig.CookieName)
            If cookie IsNot Nothing Then cookieToken = cookie.Value
            formToken = HttpContext.Current.Request.Headers("X-XSRF-Token")
            System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken)
        Catch TokenEx As Exception
            mLog.InsertApplicationLog("ExcepcionesController", "Error Critico", "InsertInfoBD | TOKEN", TokenEx.Message)
            mLog.NotificacionError(TokenEx, "ExcepcionesController | X-XSRF-TOKEN | CRITICAL")
            Return StatusCode(HttpStatusCode.Forbidden)
        End Try

        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim Model As String
        Dim pgp As New DataTable()
        Dim attempts As Integer
        Dim filePath As String = ""
        Dim safeDivPer As String = ""

        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model

        End If
        Try
            Thread.Sleep(1000)

            Dim safePeriod = _Sanitize.Texto(request.Period)
            Dim safeSociety = _Sanitize.Texto(request.Society)

            If Not String.IsNullOrWhiteSpace(request.PersonnelDivision) Then
                safeDivPer = _Sanitize.Texto(request.PersonnelDivision)
            End If

            Dim sanitizedParams = _Sanitize.SanitizePathComponents(request.FileType, request.Extension)
            filePath = HttpContext.Current.Server.MapPath("~\UploadedFiles\" + sanitizedParams(0) + "\" + mUser.Email + sanitizedParams(1))
            Dim ExcelArray(,) As Object = fc.GetExcelArray(sanitizedParams(0), sanitizedParams(1))

            If ExcelArray IsNot Nothing Then
                HttpContext.Current.Session("Periodo") = safePeriod
                HttpContext.Current.Session("Sociedad") = safeSociety
                HttpContext.Current.Session("DivisionPersonal") = safeDivPer

                Dim Lot As Integer = Nothing
                Dim formats As String() = {"dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd"}

                ''Validación de Intentos
                attempts = _PGService.ActionTryCount(mUser.Email, safeDivPer, "GET")

                If attempts >= attemptLimit Then
                    If System.IO.File.Exists(filePath) Then System.IO.File.Delete(filePath)
                    Return BadRequest("LIMIT_EXCEED")
                End If

                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()
                    Using cmd As New NpgsqlCommand("SELECT COALESCE(MAX(""Lot""), 0) + 1 FROM ""ICMToolsExceptionsUploadLog"";", conn)
                        Lot = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    Using writer = conn.BeginBinaryImport("COPY ""ICMToolsExceptionsUploadLog"" (""Lot"", ""User"", ""Model"", ""IDSociety"", ""IDPersonalDivision"", ""Period"", ""Payee"", ""Date"", ""IdWageType"", ""Amount"", ""Reason"", ""Status"") FROM STDIN (FORMAT BINARY)")
                        For record As Integer = 2 To ExcelArray.GetUpperBound(0)
                            Dim ParseDate As Date = Date.ParseExact(ExcelArray(record, 2).ToString(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None)
                            Dim Money As Decimal = Convert.ToDecimal(ExcelArray(record, 4))
                            writer.StartRow()
                            writer.Write(Lot, NpgsqlTypes.NpgsqlDbType.Bigint)
                            writer.Write(mUser.Email.ToString, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(Model, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(request.Society.ToString, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(safeDivPer, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(safePeriod, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(ExcelArray(record, 1).ToString, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(ParseDate, NpgsqlTypes.NpgsqlDbType.Date)
                            writer.Write(ExcelArray(record, 3).ToString, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write(Money, NpgsqlTypes.NpgsqlDbType.Numeric)
                            writer.Write(ExcelArray(record, 5).ToString, NpgsqlTypes.NpgsqlDbType.Varchar)
                            writer.Write("IMPORTADO", NpgsqlTypes.NpgsqlDbType.Varchar)
                        Next
                        writer.Complete()
                    End Using
                End Using

                Return Ok(New With {.d = True, .f = filePath})
            End If

            Return Ok(New With {.d = False})

        Catch ex As Exception
            If Not String.IsNullOrEmpty(filePath) Then
                Try
                    If System.IO.File.Exists(filePath) Then
                        System.IO.File.Delete(filePath)
                    Else
                        mLog.InsertApplicationLog("ExcepcionesController", "Info", "DEBUG", "El archivo no se encontró para borrar: " & filePath)
                    End If
                Catch delEx As Exception
                    mLog.InsertApplicationLog("ExcepcionesController", "Error Critico", "DELETE_FAIL", delEx.Message)
                    mLog.NotificacionError(delEx, "Excepciones | DELETE_FAIL | CRITICAL")
                End Try
            End If
            _PGService.ActionTryCount(mUser.Email, safeDivPer, "INCREMENT")
            mLog.insertLog("ExcepcionesController", "InsertInfoBDExcepciones", ex.Message)
            mLog.NotificacionError(ex, "Excepciones")
            Return InternalServerError(ex)
        End Try
    End Function
    <HttpPost>
    <Route("api/excepciones/ValidateInfoICM")>
    Public Function ValidateInfoICM(<FromBody> request As DivPerRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim Model As String
        Dim responseT As New DataTable()
        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model
        End If

        Dim rawDivPer As String = request.PersonnelDivision
        Dim safeDivPer As String = If(String.IsNullOrWhiteSpace(rawDivPer), "C000", rawDivPer)
        safeDivPer = Regex.Replace(safeDivPer, "[^a-zA-Z0-9\s\-_]", "")

        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.spICMToolsExceptionsUploadLogValidate(@usr, @pmodel)", conn)
                    cmd.Parameters.AddWithValue("usr", NpgsqlDbType.Varchar, mUser.Email)
                    cmd.Parameters.AddWithValue("pmodel", NpgsqlDbType.Varchar, Model)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(responseT)
                    End Using
                End Using
            End Using
            Dim rowString As String = Nothing
            Dim tableString As String = "<table id='DetailsTable' class='table table-sm table-hover'>" +
                                         "<thead>" +
                                            "<tr>" +
                                                "<th>Empleado</th>" +
                                                "<th>Fecha</th>" +
                                                "<th>CCNom</th>" +
                                                "<th>Monto</th>" +
                                                "<th>Motivo</th>" +
                                            "</tr>" +
                                         "</thead> " +
                                         "<tbody> " +
                                            "@LISTA@" +
                                         "</tbody> " +
                                         "</table>"
            If Not responseT Is Nothing Then
                For Each row As DataRow In responseT.Rows
                    rowString += "<tr>" +
                                     "<td>" + row.Item("payee").ToString + "</td>" +
                                     "<td>" + row.Item("fecha").ToString + "</td>" +
                                     "<td>" + row.Item("idwagetype").ToString + "</td>" +
                                     "<td>" + row.Item("amount").ToString + "</td>" +
                                     "<td>" + row.Item("reason").ToString + "</td>" +
                                  "</tr>"
                Next
            End If

            Dim errorsList As String = Nothing
            Dim errorsString As String = "<table id='Table' class='table table-sm table-hover'>" +
                                             "<thead>" +
                                                "<tr>" +
                                                    "<th>Empleado</th>" +
                                                    "<th>Fecha</th>" +
                                                    "<th>CCNom</th>" +
                                                    "<th>Monto</th>" +
                                                    "<th>Motivo</th>" +
                                                    "<th>Rechazo</th>" +
                                                "</tr>" +
                                             "</thead> " +
                                             "<tbody> " +
                                                "@LISTA@" +
                                             "</tbody> " +
                                             "</table>"
            If Not responseT Is Nothing Then
                For Each row As DataRow In responseT.Rows
                    errorsList += "<tr><td>" + row.Item("Payee").ToString + "</td><td>" + Convert.ToDateTime(row.Item("Fecha")).ToString + "</td><td>" + row.Item("IdWageType").ToString + "</td><td>" + row.Item("Amount").ToString + "</td><td>" + row.Item("Reason").ToString + "</td><td>" + row.Item("RejectionReason").ToString + "</td></tr>"
                Next
            End If

            If Not errorsList Is Nothing Then
                Return Ok(New With {.d = errorsString.Replace("@LISTA@", errorsList)})
            Else
                Return Ok(New With {.d = True})
            End If
        Catch ex As Exception
            _PGService.ActionTryCount(mUser.Email, safeDivPer, "INCREMENT")
            mLog.insertLog("ExcepcionesController", "ValidateInfoICM", ex.Message)
            mLog.NotificacionError(ex, "Excepciones")
            _PGService.AbortLoad(mUser.Email, "Excepciones")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/excepciones/InsertExceptions")>
    Public Function InsertExceptions(<FromBody> request As DivPerRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim Model As String = Nothing
        Dim responseT As New DataTable()
        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model
        End If

        Dim rawDivPer As String = request.PersonnelDivision
        Dim safeDivPer As String = If(String.IsNullOrWhiteSpace(rawDivPer), "C000", rawDivPer)
        safeDivPer = Regex.Replace(safeDivPer, "[^a-zA-Z0-9\s\-_]", "")

        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("CALL public.""spICMToolsExceptionsUploadLogTransfer""(@pmodel, @usr)", conn)
                    cmd.Parameters.AddWithValue("pmodel", NpgsqlDbType.Varchar, Model)
                    cmd.Parameters.AddWithValue("usr", NpgsqlDbType.Varchar, mUser.Email)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            Return Ok(New With {.d = True})
        Catch ex As Exception
            _PGService.ActionTryCount(mUser.Email, safeDivPer, "INCREMENT")
            mLog.insertLog("ExcepcionesController", "InsertExceptions", ex.Message)
            mLog.NotificacionError(ex, "Excepciones")
            _PGService.AbortLoad(mUser.Email, "Excepciones")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/excepciones/ConfirmExceptions")>
    Public Function ConfirmExceptions(<FromBody> request As DivPerRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Dim Model As String = GetModel()
        Dim responseT As New DataTable()
        Dim Lot As Long
        Dim MailBody As String

        Dim rawDivPer As String = request.PersonnelDivision
        Dim safeDivPer As String = If(String.IsNullOrWhiteSpace(rawDivPer), "C000", rawDivPer)
        safeDivPer = Regex.Replace(safeDivPer, "[^a-zA-Z0-9\s\-_]", "")

        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT COALESCE(MAX(""Lot""), 0) As ""MaxLot"" FROM ""ICMToolsExceptionsUploadLog"";", conn)
                    Lot = Convert.ToInt64(cmd.ExecuteScalar())
                End Using
                Using cmd As New NpgsqlCommand("SELECT Body FROM ICMToolsReports WHERE ReportID = 1;", conn)
                    MailBody = Convert.ToString(cmd.ExecuteScalar())
                End Using
            End Using

            SendSFTP()
            SendMail(Lot, MailBody)

            _PGService.ActionTryCount(mUser.Email, safeDivPer, "RESET")

            Return Ok(New With {.d = True})
        Catch ex As Exception
            _PGService.ActionTryCount(mUser.Email, safeDivPer, "INCREMENT")
            mLog.insertLog("ExcepcionesController", "ConfirmExceptions", ex.Message)
            mLog.NotificacionError(ex, "Excepciones")
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/excepciones/validacatalogo")>
    Public Function ValidaCatalogos() As IHttpActionResult
        Try

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog.insertLog("ExcepcionesController", "ValidaCatalogos", ex.Message)
            mLog.NotificacionError(ex, "Excepciones")
            Return InternalServerError(ex)
        End Try
    End Function
#End Region
#Region "Metodos GET"


#End Region
#Region "Funciones"

    ''' <summary>
    ''' Método que obtiene el modelo
    ''' </summary>
    ''' <returns></returns>
    Private Function GetModel() As String
        Dim Model As String
        If mUser.Model = "DEBUG" Then
            Model = "femcoepqa"
        Else
            Model = mUser.Model
        End If
        Return Model
    End Function

    ''' <summary>
    ''' Método que envia la información insertada a SFTP.
    ''' </summary>
    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.Excepciones
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub SendMail(ByVal lot As Long, ByVal MailBody As String)
        If lot <= 0 Then
            Throw New Exception("Lote Invalido")
        End If
        Try
            Dim NowDate As String = Now.ToString("yyyy-MM-dd")
            Dim sql As String = $"SELECT ""Lot"" As ""Lote"",
									""UploadDate"" As ""FechaCarga"",
									""User"" As ""Usuario"",
									""Model"" As ""ModeloICM"",
									""IDSociety"" As ""IDSociety"",
									""IDPersonalDivision"" As ""IDPersonalDivision"",
									""Period"" As ""Periodo"",
									""Payee"" As ""Empleado"",
									""Date"" As ""Fecha"",
									""IdWageType"" As ""CCNom"",
									""Amount"" As ""Valor"",
									""Reason"" As ""Motivo"",
									""Status"" As ""Estado""
								FROM ""ICMToolsExceptionsUploadLog"" WHERE ""Lot"" = @Lot"
            Dim TableResponse As New DataTable()

            Try
                Using conn As New NpgsqlConnection(NpgSQL)
                    conn.Open()

                    ''Validación de Existencia de Lote
                    Dim ValidLot As Boolean = False
                    Using lotCheck As New NpgsqlCommand("SELECT COUNT(1) FROM ""ICMToolsExceptionsUploadLog"" WHERE ""Lot"" = @Lot;", conn)
                        lotCheck.Parameters.AddWithValue("Lot", NpgsqlDbType.Bigint, lot)
                        If Convert.ToInt32(lotCheck.ExecuteScalar()) = 0 Then
                            Throw New ArgumentException("Lote no encontrado.")
                        End If
                    End Using

                    ''Obtencion de Lote
                    Using cmd As New NpgsqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("Lot", NpgsqlDbType.Bigint, lot)
                        Using da As New NpgsqlDataAdapter(cmd)
                            da.Fill(TableResponse)
                        End Using
                    End Using
                End Using

                Dim filePath As String = fc.BuildXlsx(TableResponse, "Excepciones")
                Dim Model As String = GetModel()

                ws.WebServiceSendMail(mUser.Email, "ICMTools - (Excepciones) Confirmación de carga lote " + Convert.ToString(lot), MailBody, "femcoepdev", filePath)
            Catch ex As Exception
                Throw
            End Try
        Catch ex As Exception

        End Try
    End Sub

#End Region
End Class