Imports System.ComponentModel
Imports System.IO
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.Services
Imports System.Web.Services.Description
Imports System.Web.Services.Protocols
Imports DocumentFormat.OpenXml.Drawing
Imports DocumentFormat.OpenXml.EMMA
Imports DocumentFormat.OpenXml.Office2016.Presentation.Command
Imports ICMTools.ICMService
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
<System.Web.Script.Services.ScriptService()>
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WebServiceICMGeneral
    Inherits System.Web.Services.WebService
#Region "Variables"
    Private Shared ReadOnly httpClient As HttpClient = New HttpClient()
    Private ReadOnly icmService As ICMService
    Private ReadOnly dao As DAO_SQL
#End Region
#Region "Sub"
    Public Sub New()
        icmService = New ICMService(httpClient)
        dao = New DAO_SQL()
    End Sub



#End Region
#Region "General"
    <WebMethod()>
    Public Function Select_AUDIT_() As String
        Try
            Dim modeloICM As String = ConfigurationManager.AppSettings("ModelFemcoEPDev")
            Dim tablaICM As String = "Audit_"
            Dim consultaICM As String =
                "SELECT AuditID_, UserType_, UserID_, Module_, Event_, Time_, Message_, _Start, _End FROM " & tablaICM

            Dim dt As New DataTable()
            dt.Columns.Add("AuditID_", GetType(Decimal))
            dt.Columns.Add("UserType_", GetType(Decimal))
            dt.Columns.Add("UserID_", GetType(String))
            dt.Columns.Add("Module_", GetType(String))
            dt.Columns.Add("Event_", GetType(String))
            dt.Columns.Add("Time_", GetType(DateTime))
            dt.Columns.Add("Message_", GetType(String))
            dt.Columns.Add("_Start", GetType(Decimal))
            dt.Columns.Add("_End", GetType(Decimal))

            Dim task As Task(Of DataTable) = icmService.ConsultarICM(tablaICM, consultaICM, modeloICM, dt)
            dt = task.GetAwaiter().GetResult()

            Dim mensaje As String = ""
            Dim result = New With {
                .message = mensaje,
                .timestamp = DateTime.UtcNow
            }
            Return JsonConvert.SerializeObject(result)

        Catch ex As Exception
            Throw New SoapException("Error en BulkCreate_AUDIT_: " & ex.Message,
                                    SoapException.ServerFaultCode,
                                    ex)
        End Try
    End Function

    <WebMethod()>
    Public Function Validate_Payee_byUserEmailAndModel(ByVal UserEmail As String, Model As String) As DataTable
        Try

            Dim tablaICM As String = "Payee_"
            Dim consultaICM As String = "SELECT 1 As \""Response\"" FROM \""" & tablaICM & "\"" "
            Dim parametros As String = $"WHERE LOWER(\""Email_\"") = '{UserEmail.ToLower()}'"
            Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
            Dim mensaje As String = ""
            Dim result = New With {
                .message = mensaje,
                .timestamp = DateTime.UtcNow
            }
            Return dt

        Catch ex As Exception
            Throw New SoapException("Error en BulkCreate_AUDIT_: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function ConsultaICMAPIQueryLotes(columnas As List(Of String), tabla As String, Model As String) As DataTable
        Try
            Dim Dtcount As DataTable = icmService.QueryICM(tabla, $"SELECT COUNT(*) FROM \""{tabla}\"" ", Model).GetAwaiter().GetResult()
            Dim total As Integer = Convert.ToInt32(Dtcount.Rows(0)("count"))

            Dim listaColumnasFormateada As String = String.Join(",", columnas.Select(Function(s) $"\""{s}\"""))
            Dim consultaICM As String = $"SELECT {listaColumnasFormateada} FROM \""{tabla}\"" "
            Dim parametros As String = ""
            Dim dt As DataTable = Nothing

            Dim loteSize As Integer = 250000
            Dim lotes As Integer = CInt(Math.Ceiling(total / loteSize))

            If total > loteSize Then
                For i As Integer = 0 To lotes - 1
                    Dim offset As Integer = i * loteSize
                    Dim dtLote As DataTable = icmService.QueryICM(
                    tabla,
                    consultaICM,
                    Model,
                    parametros,
                    loteSize,
                    offset
                ).GetAwaiter().GetResult()

                    If dt Is Nothing Then
                        dt = dtLote.Clone()
                    End If

                    For Each row As DataRow In dtLote.Rows
                        dt.ImportRow(row)
                    Next
                Next

                Return dt
            Else
                dt = icmService.QueryICM(tabla, consultaICM, Model, parametros).GetAwaiter().GetResult()
                Return dt
            End If
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function
    <WebMethod>
    Public Sub InsertaICMAPIQueryLotes(ByVal columnas As List(Of String), ByVal Vtabla As String, ByVal Model As String, ByVal Ntabla As String)
        Try
            Dim Dtcount As DataTable = icmService.QueryICM(Vtabla, $"SELECT COUNT(*) FROM \""{Vtabla}\"" ", Model).GetAwaiter().GetResult()
            Dim total As Integer = Convert.ToInt32(Dtcount.Rows(0)("count"))

            'Dim listaColumnasFormateada As String = String.Join(",", columnas.Select(Function(s) $"\""{s}\"""))
            Dim consultaICM As String = $"SELECT \""PayeeID_\"", TO_CHAR(\""Termination_Date_\"", 'DD/MM/YYYY') AS \""Termination_Date_\"" FROM \""{Vtabla}\"" "

            Dim loteSize As Integer = 250000
            Dim lotes As Integer = CInt(Math.Ceiling(total / loteSize))

            If total > loteSize Then
                For i As Integer = 0 To lotes - 1
                    Dim offset As Integer = i * loteSize
                    Me.icmService.AsyncStream(Ntabla, columnas, Model, consultaICM, loteSize, offset).Wait()
                Next
            Else
                Me.icmService.AsyncStream(Ntabla, columnas, Model, consultaICM).Wait()
            End If
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {Vtabla}: " & ex.Message, SoapException.ServerFaultCode, ex)
        End Try
    End Sub

    <WebMethod>
    Public Function ConsultaOffSet(ByVal tabla As String, ByVal offset As Integer, ByVal limit As Integer, ByVal model As String, ByVal columnas As List(Of String)) As DataTable
        Try
            Dim dt As DataTable = icmService.OffsetChunk(tabla, offset, limit, model, columnas).GetAwaiter().GetResult()
            Return dt
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function
    <WebMethod>
    Public Function CountQuery(columna As String, tabla As String, Model As String) As DataTable
        Try
            Dim consultaICM As String = $"SELECT {columna} FROM \""{tabla}\"" "
            Dim parametros As String = ""

            Dim dt As DataTable = icmService.QueryICM(tabla, consultaICM, Model, parametros).GetAwaiter().GetResult()

            Return dt
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function ConsultaICMAPIQuery(columnas As List(Of String), tabla As String, Model As String) As DataTable
        Try
            Dim listaColumnasFormateada As String = String.Join(",", columnas.Select(Function(s) $"\""{s}\"""))
            Dim consultaICM As String = $"SELECT {listaColumnasFormateada} FROM \""{tabla}\"" "
            Dim parametros As String = ""

            Dim dt As DataTable = icmService.QueryICM(tabla, consultaICM, Model, parametros).GetAwaiter().GetResult()

            Return dt
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function ImportacionICM(modelo As String, importacion As String) As String
        Try
            Dim runId As String = ""

            runId = icmService.ImportacionICM(modelo, importacion).Result

            If String.IsNullOrWhiteSpace(runId) Then
                Throw New Exception("El runId regresó vacío.")
            End If

            Return runId
        Catch ex As Exception
            Throw New SoapException($"Error al obtener runId desde ImportacionICM: {ex.Message}",
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function StatusImportacionICM(modelo As String, runId As String) As String
        Try

            Dim status As StatusImportacionEnum = StatusImportacionEnum.SinRespuesta
            Dim statusString As String = ""

            Dim relooj As Stopwatch = Stopwatch.StartNew()

            Do
                If relooj.Elapsed.TotalSeconds > 90 Then
                    Throw New Exception("La operación está tardando más de lo esperado. Por favor, inténtelo nuevamente.")
                End If

                statusString = icmService.StatusLiveActivitiesICM(modelo, runId).Result

                If Not [Enum].TryParse(statusString, True, status) Then
                    Throw New Exception($"Status inválido recibido: {statusString}")
                End If

            Loop While status = StatusImportacionEnum.Running

            ' ya no estáa en Running
            statusString = icmService.StatusCompletedActivitiesICM(modelo, runId).Result

            If Not [Enum].TryParse(statusString, True, status) Then
                Throw New Exception($"Status inválido recibido: {statusString}")
            End If

            If status = StatusImportacionEnum.SinRespuesta Then
                Throw New Exception("El status no es válido.")
            End If


            Return icmService.ObtenerTextoEstado(status)

        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla : " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Sub InsertaCatalogos(ByVal columnas As List(Of String), ByVal Vtabla As String, ByVal Model As String, ByVal Ntabla As String)
        Try
            Dim SColumnas As String = String.Join(",", columnas.Select(Function(s) $"\""{s}\"""))
            Dim BuildQuery As String = $"SELECT {SColumnas} FROM \""{Vtabla}\"" "

            Me.icmService.AsyncStream(Ntabla, columnas, Model, BuildQuery).Wait()

        Catch ex As Exception

        End Try
    End Sub

    <WebMethod>
    Public Function ConsultaICMAPIQuery(columnas As List(Of String), tabla As String, Model As String, parametros As String) As DataTable
        Try
            Dim listaColumnasFormateada As String = String.Join(",", columnas.Select(Function(s) $"\""{s}\"""))
            Dim consultaICM As String = $"SELECT {listaColumnasFormateada} FROM \""{tabla}\"" "

            Dim dt As DataTable = icmService.QueryICM(tabla, consultaICM, Model, parametros).GetAwaiter().GetResult()

            Return dt
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function ConsultaICMAPIQuery(columnas As String, tabla As String, Model As String, parametros As String) As DataTable
        Try
            Dim consultaICM As String = $"SELECT {columnas} FROM \""{tabla}\"" "

            Dim dt As DataTable = icmService.QueryICM(tabla, consultaICM, Model, parametros).GetAwaiter().GetResult()

            Return dt
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function ConsultaICMAPIQuery(tabla As String, consulta As String, Model As String) As DataTable
        Try
            Dim dt As DataTable = icmService.QueryICM(tabla, consulta, Model, String.Empty).GetAwaiter().GetResult()
            Return dt
        Catch ex As Exception
            Throw New SoapException($"Error en la consulta hacia la tabla {tabla}: " & ex.Message, SoapException.ServerFaultCode, ex)
        End Try
    End Function

    <WebMethod>
    Public Function GetFullTableByPublish(Table As String, Model As String) As String
        Dim tempFileName As String = $"Table_{DateTime.Now.Ticks}.csv"
        Dim destinationPath As String = IO.Path.Combine(IO.Path.GetTempPath(), tempFileName)
        Dim success As Boolean
        Try
            success = icmService.PublishTable(Table, destinationPath, Model).GetAwaiter().GetResult()

            If success Then Return destinationPath

            Return Nothing
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    <WebMethod>
    Public Function GetFullTableByPublishWFilter(Table As String, FilterField As String, Filter As String) As String
        Dim tempFileName As String = $"Table_{DateTime.Now.Ticks}.csv"
        Dim destinationPath As String = System.Web.Hosting.HostingEnvironment.MapPath("~/File/" + tempFileName)
        Dim success As Boolean = False
        Try
            success = icmService.PublishTable(Table, destinationPath, "femcoepprd", FilterField, Filter).GetAwaiter().GetResult()

            If success Then Return destinationPath

            Return Nothing
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    <WebMethod>
    Public Async Function ConsultaAudit(Table As String, AEvent As String, Model As String, LastDate As String) As Task(Of JArray)
        Dim ICMResponse As String = Nothing
        Try
            Dim LastAuditRow As String = Await icmService.GetAudit(Table, AEvent, Model, LastDate).ConfigureAwait(False)
            If Not String.IsNullOrWhiteSpace(LastAuditRow) Then
                Dim JsonResponse As JArray = JArray.Parse(LastAuditRow)
                Return JsonResponse
            End If

            Return Nothing
        Catch ex As Exception
            Throw
        End Try
    End Function

    <WebMethod>
    Public Function GetFullCalcByPublish(Table As String) As String
        Dim tempFileName As String = $"Table_{DateTime.Now.Ticks}.csv"
        Dim destinationPath As String = System.Web.Hosting.HostingEnvironment.MapPath("~/File/" + tempFileName)
        Dim success As Boolean
        Dim ActionStatus As Boolean
        Try

            ActionStatus = icmService.GlobalActionStatus("femcoepprd").GetAwaiter.GetResult()
            If ActionStatus = False Then
                success = icmService.PublishCalc(Table, destinationPath, "femcoepprd").GetAwaiter().GetResult()
                If success Then Return destinationPath
            End If

            Return Nothing

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    <WebMethod>
    Public Function EmpleadosActivos_catCCNomina_APIQuery(Model As String) As DataTable
        Try
            Dim tablaICM As String = "catCCNomina"
            Dim consultaICM As String = "SELECT \""CCNomina\"" FROM \""" & tablaICM & "\"" "
            Dim parametros As String = ""

            Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

            Dim mensaje As String = ""
            Return dt
        Catch ex As Exception
            Throw New SoapException("Error en EmpleadosActivos_catCCNomina_APIQuery: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function


    <WebMethod>
    Public Function TiendasGanadorasAPIQuery(ByVal IdStore As String, Model As String) As DataTable
        Try
            Dim tablaICM As String = "CfgStoreHierarchy"
            Dim consultaICM As String = "SELECT \""IDStore\"", \""IDZone\"", \""IDSociety\"", \""IDPersonalDivision\"" FROM \""" & tablaICM & "\"" "
            Dim parametros As String = "" '' = "WHERE RIGHT(\""IDStore\"", 5) = '" & IdStore & "'"

            Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

            Dim mensaje As String = ""
            Return dt
        Catch ex As Exception
            Throw New SoapException("Error en TiendasGanadorasErrorsQuery: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function PagosManualesAPIQuery(ByVal EmpleadoID As String, CentroTrabajoID As String, Fecha As String, Model As String) As DataSet
        Try
            Dim dsRespuesta As New DataSet


            Dim tablaICM As String = "Payee_"
            Dim consultaICM As String = "SELECT \""PayeeID_\"" as \""PayeeID\""  FROM \""" & tablaICM & "\"" "
            Dim parametros As String = "WHERE \""PayeeID_\"" IN ( " & EmpleadoID & ") "
            Dim dtEmpleadoID As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()


            tablaICM = "plCatalogos"
            consultaICM = "SELECT \""Descripcion\"" FROM \""" & tablaICM & "\"" "
            parametros = "WHERE \""Descripcion\"" IN ( " & CentroTrabajoID & ") "
            Dim dtTiendaRegistrada As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()


            tablaICM = "DateStringPeriods"
            consultaICM = "SELECT \""PeriodName\"",  \""StarDate\"",  \""EndDate\""  FROM \""" & tablaICM & "\"" "
            parametros = "WHERE \""IsOutputInterface\"" = 'SI' "
            Dim dtFecha As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

            dsRespuesta.Tables.Add(dtEmpleadoID)
            dsRespuesta.Tables.Add(dtTiendaRegistrada)
            dsRespuesta.Tables.Add(dtFecha)

            Return dsRespuesta
        Catch ex As Exception
            Throw New SoapException("Error en PagosManuales ErrorsQuery: " & ex.Message,
                                SoapException.ServerFaultCode,
                                ex)
        End Try
    End Function

    <WebMethod>
    Public Function PagosManualesSendMail(ByVal Mail As String, ByVal Subject As String, ByVal Body As String) As Boolean
        Try
            Dim Model As String = "femcoepdev"
            Dim response As Boolean = icmService.SendMail_ICM(Model, Mail, Subject, Body).GetAwaiter().GetResult()
            Return response
        Catch ex As Exception
            Return False
        End Try
    End Function

    <WebMethod>
    Public Function GetPayeeByUserEmail(User As String, Model As String) As String
        Dim tablaICM As String = "Payee_"
        Dim consultaICM As String = "SELECT \""PayeeID_\"" FROM \""" & tablaICM & "\"" "
        Dim parametros As String = "WHERE LOWER(\""Email_\"") = '" & User.ToLower() & "' LIMIT 1"
        Dim PayeeID As String = Nothing

        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim firstRow As DataRow = dt.Rows(0)
            If firstRow.ItemArray.Length > 0 Then
                PayeeID = firstRow(0).ToString()
            End If
        End If
        Return PayeeID

    End Function

    <WebMethod>
    Public Function GetSocietiesExternalTables(ByVal PayeeID As String, ByVal Model As String) As DataTable
        Dim tablaICM As String = "CfgWebPermission"
        Dim consultaICM As String = $"SELECT DISTINCT
                                        J.\""IDSociety\"" AS \""IDSociety\"",
                                        CONCAT('(', J.\""IDSociety\"", ') ', D.\""Description\"") AS \""Description\""
                                        FROM \""" & tablaICM & "\"" C"
        Dim parametros As String = $"INNER JOIN \""CfgStoreHierarchy\"" J ON C.\""IDPersonalDivision\"" = J.\""IDPersonalDivision\""
                                        INNER JOIN \""CatSociety\"" D ON J.\""IDSociety\"" = D.\""IDSociety\""
                                        WHERE C.\""PayeeID\"" = '" & PayeeID & "'
                                        UNION

                                        SELECT DISTINCT
                                        C.\""Level1\"" AS \""IDSociety\"", CONCAT('(', C.\""Level1\"", ') ', S.\""Description\"") AS \""Description\""
                                        FROM
                                        \""CfgWebPermissionLevel\"" C
                                        INNER JOIN \""CatSociety\"" S ON C.\""Level1\"" = S.\""IDSociety\""
                                        WHERE C.\""PayeeID\"" = '" & PayeeID & "' ORDER BY 1;"

        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function

    <WebMethod>
    Public Function GetPersonnelDExternalTables(ByVal PayeeID As String, ByVal Model As String, ByVal Society As String) As DataTable
        Dim tablaICM As String = "CfgWebPermissionLevel"
        Dim consultaICM As String = $"SELECT DISTINCT P.\""IDPersonalDivision\"" AS \""PersonalDivision\"",
                                        CONCAT('(', P.\""IDPersonalDivision\"", ') ', P.\""Description\"") AS \""Description\"" FROM \""" & tablaICM & "\"" C"
        Dim parametros As String = $"INNER JOIN \""CatPersonalDivision\"" P ON C.\""Level1\"" = P.\""IDSociety\"" AND C.\""Level2\"" = P.\""IDPersonalDivision\""
                                    WHERE C.\""Level1\"" = '" & Society & "' AND C.\""PayeeID\"" = '" & PayeeID & "'

                                    UNION

                                    Select DISTINCT D.\""IDPersonalDivision\"" AS \""PersonalDivision\"",
                                    CONCAT('(', D.\""IDPersonalDivision\"", ') ', D.\""Description\"") AS \""Description\"" FROM
                                    \""CfgWebPermission\"" W
                                    INNER JOIN
                                    \""CatSociety\"" S ON W.\""IDSociety\"" = S.\""IDSociety\""
                                    INNER JOIN
                                    \""CatPersonalDivision\"" D ON W.\""IDPersonalDivision\"" = D.\""IDPersonalDivision\""
                                    WHERE
                                    W.\""IDSociety\"" = '" & Society & "'
                                    AND W.\""PayeeID\"" = '" & PayeeID & "';"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function

    <WebMethod>
    Public Function GetPersonnelDivisions(ByVal PayeeID As String, ByVal Model As String, ByVal Society As String) As DataTable
        Dim tablaICM As String = "CfgWebPermission"
        Dim consultaICM As String = $"SELECT DISTINCT D.\""IDPersonalDivision\"" AS \""PersonalDivision\"",
                                        CONCAT('(', D.\""IDPersonalDivision\"", ') ', D.\""Description\"") AS \""Description\""
                                        FROM \""" & tablaICM & "\"" W"
        Dim parametros As String = $"INNER JOIN \""CatSociety\"" S ON W.\""IDSociety\"" = S.\""IDSociety\""
                                        INNER JOIN \""CatPersonalDivision\"" D ON W.\""IDPersonalDivision\"" = D.\""IDPersonalDivision\""
                                        WHERE W.\""IDSociety\"" = '" & Society & "' AND W.\""PayeeID\"" = '" & PayeeID & "';"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function

    <WebMethod>
    Public Function GetExternalPeriods(ByVal Model As String, ByVal Limit As String) As DataTable
        Dim tablaICM As String = "CfgDateStringPeriod"
        Dim consultaICM As String = $"SELECT DISTINCT C.\""IDPeriod\"" AS \""PeriodId\"",
                                        CONCAT('(', C.\""PeriodName\"", ') #', LEFT(C.\""PeriodNumber\""::TEXT, 1), ' de ',
                                        SUBSTRING('Ene Feb Mar Abr May Jun Jul Ago Sep Oct Nov Dic', (C.\""Month\""::integer * 4) - 3, 3),
                                        ' ', C.\""DateStart\""::TEXT, ' - ', C.\""DateEnd\""::TEXT) AS \""PeriodName\""
                                        FROM \""" & tablaICM & "\"" C"
        Dim parametros As String = $"INNER JOIN \""Time_\"" T ON C.\""PeriodName\"" = T.\""Name_\"" WHERE T.\""TimeID_\"" = 'T001' AND T.\""Level_\"" = 'Weeks'
                                    AND ((" & Limit & " = 0 AND CAST(C.\""Year\"" AS integer) IN (EXTRACT(YEAR FROM NOW()) - 1, EXTRACT(YEAR FROM NOW())))
      	                            OR (" & Limit & " = 1 AND EXTRACT(MONTH FROM NOW()) > 1 AND CAST(C.\""Year\"" AS integer) = EXTRACT(YEAR FROM NOW()) 
                                    AND C.\""Month\""::integer IN (EXTRACT(MONTH FROM NOW()) - 1, EXTRACT(MONTH FROM NOW()), EXTRACT(MONTH FROM NOW()) + 1))
                                    OR (" & Limit & " = 1 AND EXTRACT(MONTH FROM NOW()) = 1 AND ((CAST(C.\""Year\"" AS integer) = EXTRACT(YEAR FROM NOW()) - 1 AND C.\""Month\""::integer = 12) 
                                    OR (CAST(C.\""Year\"" AS integer) = EXTRACT(YEAR FROM NOW()) AND C.\""Month\""::integer IN (EXTRACT(MONTH FROM NOW()), EXTRACT(MONTH FROM NOW()) + 1))))
                                    OR " & Limit & " = 2 AND C.\""DateStart\"" IN (DATE_TRUNC('week', NOW()) - INTERVAL '7 days', DATE_TRUNC('week', NOW()) - INTERVAL '14 days', DATE_TRUNC('week', NOW()) - INTERVAL '21 days'))"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function

    <WebMethod>
    Public Function GetNominaCritica(ByVal Model As String) As DataTable
        Dim tablaICM As String = "NominaCriticaICMT"
        Dim consultaICM As String = $"SELECT DISTINCT N.\""PeriodId\"" AS \""PeriodId\"",
                                        CONCAT('(', C.\""PeriodName\"", ') #', LEFT(C.\""PeriodNumber\""::TEXT, 1), ' de ',
                                        SUBSTRING('Ene Feb Mar Abr May Jun Jul Ago Sep Oct Nov Dic', (C.\""Month\""::integer * 4) - 3, 3),
                                        ' ', C.\""DateStart\""::TEXT, ' - ', C.\""DateEnd\""::TEXT) AS \""PeriodName\""
                                        FROM \""" & tablaICM & "\"" N"
        Dim parametros As String = $"INNER JOIN \""CfgDateStringPeriod\"" C ON C.\""IDPeriod\"" = N.\""PeriodId\"""
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function
#End Region
#Region "Mail's"
    <WebMethod>
    Public Function WebServiceSendMail(ByVal Mail As String, ByVal Subject As String, ByVal Body As String, ByVal Model As String) As Boolean
        Try
            If Mail = "00000000@oxxo.com" Then Mail = "jarrazola@exsoinf.com"
            Dim response As Boolean = icmService.SendMail_ICM(Model, Mail, Subject, Body).GetAwaiter().GetResult()
            Return response
        Catch ex As Exception
            Return False
        End Try
    End Function

    <WebMethod>
    Public Function WebServiceSendMail(ByVal Mail As String, ByVal Subject As String, ByVal Body As String, ByVal Model As String, ByVal filePath As String) As Boolean
        Try
            If Mail = "00000000@oxxo.com" Then Mail = "jarrazola@exsoinf.com"

            If filePath IsNot Nothing Then
                Dim response As Boolean = icmService.SendMailWFile_ICM(Model, Mail, Subject, Body, filePath).GetAwaiter().GetResult()
                Return response
            Else
                Dim response As Boolean = icmService.SendMail_ICM(Model, Mail, Subject, Body).GetAwaiter().GetResult()
                Return response
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    <WebMethod>
    Public Function WebServiceSendMail(ByVal Mail As List(Of String), ByVal CC As List(Of String), ByVal Subject As String, ByVal Body As String, ByVal Model As String) As Boolean
        Try
            Dim response As Boolean = icmService.SendMail_ICM(Model, CC, Mail, Subject, Body).GetAwaiter().GetResult()
            Return response
        Catch ex As Exception
            Return False
        End Try
    End Function

    <WebMethod>
    Public Function WebServiceSendMailWithFile(ByVal Mail As List(Of String), ByVal CC As List(Of String), ByVal Subject As String, ByVal Body As String, ByVal Model As String, ByVal FilePath As String) As Boolean
        Try
            If FilePath IsNot Nothing And Not String.IsNullOrWhiteSpace(FilePath) Then
                If Not File.Exists(FilePath) Then
                    FilePath = ""
                End If
            End If

            Dim response As Boolean = icmService.SendSomeMailsWFile_ICM(Model, Mail, CC, Subject, Body, FilePath).GetAwaiter().GetResult()
                Return response
        Catch ex As Exception
            Return False
        End Try
    End Function

    <WebMethod>
    Public Function WebServiceSendSomeMails(ByVal MailList As List(Of String), ByVal CC As List(Of String), ByVal Subject As String, ByVal Body As String, ByVal Model As String, ByVal filePath As String) As Boolean
        ValidateMailList(MailList, "Destinatarios")
        ValidateMailList(CC, "CC")

        Try
            If filePath Is Nothing OrElse String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("Error al enviar el correo, el Path de archivo es nulo.")
            End If

            Dim response As Boolean = icmService.SendSomeMailsWFile_ICM(Model, MailList, CC, Subject, Body, filePath).GetAwaiter().GetResult()
            Return response

        Catch ex As Exception
            Return False
        End Try
    End Function
#End Region
#Region "Bonos de Transporte"
    Public Function GetDivisionByUserBT(ByVal PayeeID As String, ByVal Model As String) As DataTable
        Dim tablaICM As String = "CfgWebPermissionLevel"
        Dim consultaICM As String = $"SELECT CPL.\""PayeeID\"", CPD.\""IDSociety\"" AS \""idSociedad\"", CSY.\""Description\"" AS \""sociedad\"",
                                    CPL.\""Level2\"" AS \""idDivision\"", CAST(CPL.\""Level2\"" AS VARCHAR) || ' - ' || CPD.\""Description\"" AS \""division\""
                                    FROM \""" & tablaICM & "\"" AS CPL"
        Dim parametros As String = $"LEFT JOIN \""CatPersonalDivision\"" AS CPD ON CPL.\""Level2\"" = CPD.\""IDPersonalDivision\""
                                    LEFT JOIN \""CatSociety\"" AS CSY ON CPD.\""IDSociety\"" = CSY.\""IDSociety\""
                                    WHERE CPL.\""PayeeID\"" = '" & PayeeID & "';"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function

    Public Function GetLastHistoryPayee(ByVal PayeeID As String, ByVal Model As String) As String
        Dim IDPosition As String = Nothing
        Dim tablaICM As String = "HistoryPayee"
        Dim consultaICM As String = $"SELECT \""IDPosition\"",
                                      CASE
                                      WHEN EXTRACT(YEAR FROM COALESCE(\""DateEnd\"", '9998-01-01'::date)) = 9998 THEN NULL
                                      ELSE \""DateEnd\""
                                      END AS DateEnd,
                                      CASE        
                                      WHEN EXTRACT(YEAR FROM COALESCE(\""DateEnd\"", '9998-01-01'::date)) = 9998 THEN 1
                                      ELSE 0
                                      END AS statusPayee,
                                      ROW_NUMBER() OVER (PARTITION BY \""PayeeID\"" ORDER BY \""DateStart\"" DESC) AS Rn
                                     FROM \""HistoryPayee\"""
        Dim parametros As String = $"WHERE \""PayeeID\"" = '" & PayeeID & "' ORDER BY \""DateStart\"" DESC;"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim firstRow As DataRow = dt.Rows(0)
            If firstRow.ItemArray.Length > 0 Then
                IDPosition = firstRow(0).ToString()
            End If
        End If
        Return IDPosition
    End Function

    Public Function GetAuthorizedPayee(ByVal IDPosition As String, ByVal Model As String) As String
        Dim PayeeID As String = Nothing
        Dim tablaICM As String = "HistoryPayee"
        Dim consultaICM As String = $"SELECT CASE DISTINCT
                                        PAY.\""PayeeID\"",
                                        PAY.\""DateStart\""
                                      FROM \""" & tablaICM & "\"" PAY"
        Dim parametros As String = $"LEFT JOIN \""CatPosition\"" POS ON RIGHT(REPEAT('0', 9) || POS.\""IDPosition\"", 8) = PAY.\""IDPosition\""
                                    WHERE EXTRACT(YEAR FROM COALESCE(PAY.\""DateEnd\"", '9998-01-01'::date)) = 9998
                                    AND RIGHT(REPEAT('0', 9) || POS.\""IDPosition\"", 8) = RIGHT(REPEAT('0', 9) || '" & IDPosition & "', 8)
                                    ORDER BY \""DateStart\"" DESC
                                    LIMIT 1;"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim firstRow As DataRow = dt.Rows(0)
            If firstRow.ItemArray.Length > 0 Then
                PayeeID = firstRow(0).ToString()
            End If
        Else PayeeID = Nothing
        End If
        Return PayeeID
    End Function

    Public Function SemiFinalElseGetLastHistoryPayeeBT(ByVal Payee As String, ByVal Model As String) As DataTable
        Dim TablaICM As String = "HistoryPayee"
        Dim consultaICM As String = $"SELECT \""PayeeID\"", 
				                      \""IDPosition\"", 
				                      \""IDSociety\"", 
				                      \""IDPersonalDivision\"",
				                      \""IDPersonalSubdivision\"", 
				                      \""IDPersonalArea\"",
				                      \""IDPayrollArea\"",
				                      \""IDJobKey\"",
				                      \""IDCostCenter\"", 
				                      \""DateStart\"",
                                      CASE
                                      WHEN EXTRACT(YEAR FROM COALESCE(\""DateEnd\"", '9998-01-01'::date)) = 9998 THEN NULL
                                      ELSE \""DateEnd\""
                                      END AS \""DateEnd\"",
                                      CASE        
                                      WHEN EXTRACT(YEAR FROM COALESCE(\""DateEnd\"", '9998-01-01'::date)) = 9998 THEN 1
                                      ELSE 0
                                      END AS \""statusPayee\"",
                                      ROW_NUMBER() OVER (PARTITION BY \""PayeeID\"" ORDER BY \""DateStart\"" DESC) AS Rn
                                     FROM \""HistoryPayee\"""
        Dim parametros As String = $"WHERE \""PayeeID\"" = '" & Payee & "' ORDER BY \""DateStart\"" DESC;"

        Dim dt As DataTable = icmService.QueryICM(TablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

        Return dt
    End Function

    Public Function GetAuthorizedPayeeList(ByVal rList As List(Of String), ByVal Model As String) As DataTable
        If rList Is Nothing OrElse rList.Count = 0 Then
            Return New DataTable()
        End If

        Dim itemsConComillas = rList.Select(Function(item) $"'{item}'")
        Dim listaComoString = String.Join(",", itemsConComillas)

        If String.IsNullOrEmpty(listaComoString) Then
            Return New DataTable()
        End If

        Dim TablaICM As String = "HistoryPayee"
        Dim consultaICM As String = $"SELECT DISTINCT
                                        PAY.\""PayeeID\"",
                                        PAY.\""DateEnd\"",
                                        PAY.\""IDPosition\""
                                      FROM \""" & TablaICM & "\"" PAY"
        Dim parametros As String = $"WHERE EXTRACT(YEAR FROM COALESCE(PAY.\""DateEnd\"", '9998-01-01'::date)) = 9998
                                     AND RIGHT(REPEAT('0', 9) || PAY.\""IDPosition\"", 8) IN ({listaComoString})"

        Dim dt As DataTable = icmService.QueryICM(TablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()


        Return dt
    End Function

    Public Function GetFinalTable(ByVal pList As DataTable, ByVal Model As String) As DataTable
        Dim PayeeList As New List(Of String)
        If pList IsNot Nothing AndAlso pList.Rows.Count > 0 Then
            PayeeList = pList.AsEnumerable().Select(Function(row) row.Field(Of String)(0)).ToList()
        End If
        Dim itemsConComillas = PayeeList.Select(Function(item) $"'{item}'")
        Dim listaComoString = String.Join(",", itemsConComillas)
        If String.IsNullOrEmpty(listaComoString) Then
            Return New DataTable()
        End If

        Dim TablaICM As String = "HistoryPayee"
        Dim consultaICM As String = $"SELECT
                                        \""PayeeID\"",
                                        \""IDPosition\"",
                                        \""IDSociety\"",
                                        \""IDPersonalDivision\"",
                                        \""IDPersonalSubdivision\"",
                                        \""IDPersonalArea\"",
                                        \""IDPayrollArea\"",
                                        \""IDJobKey\"",
                                        \""IDCostCenter\"",
                                        \""DateStart\"",
                                        \""DateEnd\"",
                                        \""statusPayee\""
                                     FROM"
        Dim parametros As String = $"
                                    (
                                    -- Subconsulta: lastHistory
                                    SELECT
                                        ht.\""PayeeID\"",
                                        ht.\""IDPosition\"",
                                        ht.\""IDSociety\"",
                                        ht.\""IDPersonalDivision\"",
                                        ht.\""IDPersonalSubdivision\"",
                                        ht.\""IDPersonalArea\"",
                                        ht.\""IDPayrollArea\"",
                                        ht.\""IDJobKey\"",
                                        ht.\""IDCostCenter\"",
                                        ht.\""DateStart\"",

                                        -- 1. Cálculo de DateEnd
                                        CASE
                                            WHEN EXTRACT(YEAR FROM COALESCE(ht.\""DateEnd\"", '9998-01-01'::date)) = 9998 THEN NULL
                                            ELSE ht.\""DateEnd\""
                                        END AS \""DateEnd\"",

                                        -- 2. Cálculo de statusPayee
                                        CASE
                                            WHEN EXTRACT(YEAR FROM COALESCE(ht.\""DateEnd\"", '9998-01-01'::date)) = 9998 THEN 1
                                            ELSE 0
                                        END AS \""statusPayee\"",

                                        -- 3. Función de ventana para numerar el historial
                                        ROW_NUMBER() OVER (PARTITION BY ht.\""PayeeID\"" ORDER BY ht.\""DateStart\"" DESC, ht.\""DateEnd\"" DESC) AS \""Rn\""

                                    FROM
                                \""HistoryPayee\"" ht 
                                    INNER JOIN 
                                    -- Simulación de la tabla '@Payees' usando un Array de strings
                                        (
                                            SELECT unnest({listaComoString}]) AS \""PayeeID\"" 
                                        ) p ON ht.\""PayeeID\"" = p.\""PayeeID\""
                                    ) AS \""lastHistory\""
                                WHERE
                                \""Rn\"" = 1;"

        Dim dt As DataTable = icmService.QueryICM(TablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

        Return dt

    End Function
    Public Function GetPayeesBTReemplazos(pList As DataTable, Model As String) As DataTable
        Dim PayeeList As New List(Of String)
        If pList IsNot Nothing AndAlso pList.Rows.Count > 0 Then
            PayeeList = pList.AsEnumerable().Select(Function(row) row.Field(Of String)(0)).ToList()
        End If
        Dim itemsConComillas = PayeeList.Select(Function(item) $"'{item}'")
        Dim listaComoString = String.Join(",", itemsConComillas)
        If String.IsNullOrEmpty(listaComoString) Then
            Return New DataTable()
        End If

        Dim TablaICM As String = "Payee_"
        Dim consultaICM As String = $"SELECT \""PayeeID_"" As \""PayeeID\"" \""Email_\"" As \""Email\"", \""Name\"" As \""Name\"" FROM \""" & TablaICM & "\"""
        Dim parametros As String = $"WHERE \""PayeeID_\"" IN ({listaComoString})"

        Dim dt As DataTable = icmService.QueryICM(TablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()

        Return dt
    End Function
#End Region

#Region "Metodos Cierre Virtual"

    Public Function Get_Time_(Starting_ As String, Ending_ As String, Model As String) As DataTable
        Dim dt As DataTable = Nothing

        Try

            Dim tablaICM As String = "Time_"
            Dim consultaICM As String = "SELECT \""Starting_\"" , \""Ending_\"" , UPPER(\""Name_\"") AS \""MesCierre\"" FROM \""" & tablaICM & "\"" "
            Dim parametros As String = " WHERE \""TimeID_\"" = 'T001' AND \""Level_\"" = 'Meses' " &
                                        "AND \""Starting_\"" = '" & Starting_ & "' AND \""Ending_\"" = '" & Ending_ & "';"

            dt = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function

    Public Function Get_DateStringPeriods(Model As String) As DataTable
        Dim dt As DataTable = Nothing
        Try
            'Valor controlado (no viene del usuario)
            Dim valorOutput As String = "SI"

            'Escapar comillas por seguridad
            valorOutput = valorOutput.Replace("'", "''")

            Dim tablaICM As String = "DateStringPeriods"
            Dim consultaICM As String = "SELECT \""StarDate\"" , \""EndDate\"" FROM \""" & tablaICM & "\"" "
            Dim parametros As String = $" WHERE \""IsOutputInterface\"" = '{valorOutput}' ;"

            dt = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function


    Public Function Get_sptAsignacionCentroTrabajo(FechaInicio As String, FechaFin As String, Model As String) As DataTable
        Dim dt As DataTable = Nothing

        Try

            Dim tablaICM As String = "sptAsignacionCentroTrabajo"
            Dim consultaICM As String = "SELECT \""CentroTrabajoID\"" , \""EmpleadoID\"" , \""RolID\"" , \""FechaInicio\"" , \""FechaFin\"" , \""FuncionID\""   FROM \""" & tablaICM & "\"" "
            Dim parametros As String = " WHERE \""FechaInicio\"" <=  '" & FechaFin & "' AND \""FechaFin\"" >= '" & FechaInicio & "';"

            dt = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function


    Public Function Get_dtCierreVirtualPrevio(Periodo As String, Model As String) As DataTable
        Dim dt As DataTable = Nothing

        Try

            Dim tablaICM As String = "dtCierreVirtualPrevio"
            Dim consultaICM As String = "SELECT \""CentroTrabajoID\"" , \""Periodo\"" , \""FechaBloqueo\"" , \""FechaCancelacion\"" , \""Cerrado\"" , \""BloqueadorID\"" , \""CanceladorID\""   FROM \""" & tablaICM & "\"" "
            Dim parametros As String = " WHERE \""Periodo\"" =  '" & Periodo & "';"

            dt = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function

    Public Function Get_sptOxxoTdaJerarquia(Model As String) As DataTable
        Dim dt As DataTable = Nothing

        Try

            Dim tablaICM As String = "sptOxxoTdaJerarquia"
            Dim consultaICM As String = "SELECT \""TiendaID\"" , \""ZonaID\"" , \""PlazaID\"" , \""MegaPlazaID\"" , \""DistritoID\"" , \""EffStart_\"", \""EffEnd_\""   FROM \""" & tablaICM & "\"" "


            dt = icmService.QueryICM(tablaICM, consultaICM, Model).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function

    Public Function Get_plCatalogos(CatalogosID As String, Model As String) As DataTable
        Dim dt As DataTable = Nothing

        Try

            Dim tablaICM As String = "plCatalogos"
            Dim consultaICM As String = "SELECT \""ID\"" , \""Descripcion\"" , \""IdExterno\"",\""EffStart_\"" ,\""EffEnd_\"" ,\""CatalogosID\""     FROM \""" & tablaICM & "\"" "
            Dim parametros As String = " WHERE \""CatalogosID\"" =  '" & CatalogosID & "';"


            dt = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function

    Private Sub ValidateMailList(mails As List(Of String), fieldName As String)
        If mails Is Nothing OrElse mails.Count = 0 Then
            Throw New ArgumentException($"La lista de {fieldName} no puede ser nula o vacía.")
        End If
        For Each Mail As String In mails
            If String.IsNullOrWhiteSpace(Mail) OrElse Not Mail.Contains("@") OrElse Mail.Contains(vbCrLf) OrElse Mail.Contains(vbLf) Then
                Throw New ArgumentException($"Se detectó un correo inválido en {fieldName}.")
            End If
        Next
    End Sub

#End Region

#Region "Metodos Empleados Lideres"

    <WebMethod>
    Public Function Get_Inicio_Mes_Procesando(ByVal valor As String, ByVal Model As String) As DataTable
        Dim tablaICM As String = "DateStringPeriods"
        Dim consultaICM As String = $"SELECT d.\""StarDate\"" FROM \""" & tablaICM & "\"" d"
        Dim parametros As String = $" INNER JOIN \""sysFechaHoy\"" f ON d.\""PeriodString\"" = f.\""Fecha\"" WHERE f.\""ID\"" = '" & valor & "';"
        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model, parametros).GetAwaiter().GetResult()
        Return dt
    End Function

    Public Function Get_Payee_(ByVal Model As String) As DataTable
        Dim tablaICM As String = "Payee_"
        Dim consultaICM As String = $"SELECT \""PayeeID_\"" , \""RFC\"" FROM \""" & tablaICM & "\"" d"

        Dim dt As DataTable = icmService.QueryICM(tablaICM, consultaICM, Model).GetAwaiter().GetResult()
        Return dt
    End Function

    Public Function Get_sptAsignacionCentroTrabajoCompleto(Model As String) As DataTable
        Dim dt As DataTable = Nothing

        Try

            Dim tablaICM As String = "sptAsignacionCentroTrabajo"
            Dim consultaICM As String = "SELECT \""CentroTrabajoID\"" , \""EmpleadoID\"" , \""RolID\"" , \""FechaInicio\"" , \""FechaFin\"" , \""FuncionID\"" FROM \""" & tablaICM & "\"" "

            dt = icmService.QueryICM(tablaICM, consultaICM, Model).GetAwaiter().GetResult()
        Catch ex As Exception

        End Try

        Return dt

    End Function

#End Region
End Class
