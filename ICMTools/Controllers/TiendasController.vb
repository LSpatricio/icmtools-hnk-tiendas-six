Imports System.Globalization
Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class Mails
    Public Property mail As String

    Public Sub New(ByVal mail As String)
        Me.mail = mail
    End Sub
End Class

Public Module MailListModule
    Public ReadOnly MailList As New List(Of Mails) From {
        New Mails("Benjamin.ortiz@xpertal.com"),
        New Mails("rudy.felix@xpertal.com"),
        New Mails("ritoantonio.lara@xpertal.com"),
        New Mails("danielalberto.guzman@xpertal.com"),
        New Mails("julio.salinas@oxxo.com"),
        New Mails("juan.ramosm@oxxo.com")
    }
End Module

Public Class TiendasController
    Inherits ApiController

#Region "Variables Locales"
    Private ReadOnly mUser As User
    Private ReadOnly mLog As Log
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController
    ReadOnly ws As New WebServiceICMGeneral

    Public Class RegistroTiendas
        Public idsociety As String
        Public idpersonaldivision As String
        Public idstore As String
        Public startdate As Date
        Public enddate As Date
        Public amount As Decimal
    End Class

    Public Class RegistroExcepciones
        Public idsociety As String
        Public idpersonaldivision As String
        Public idstore As String
        Public payeeid As String
        Public startdate As Date
        Public enddate As Date
        Public amount As Decimal
    End Class

#End Region
    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
    End Sub

#Region "Clases"
    Public Class CustomValidateRequest
        Property FileType As String
        Property Extension As String
        Property columns As String()
        Property types As String()
        Property Society As String
        Property PersonnelDivision As String
    End Class
    Public Class InsertInfoRequest
        Property FileType As String
        Property Extension As String
    End Class
    Private Class Tienda
        Public IDStore As String
        Public Society As String
        Public PersonnelDivision As String
    End Class
#End Region
#Region "Metodos POST"

    <HttpPost>
    <Route("api/tiendas/validatecustomtiendas")>
    Public Function ValidateCustomTiendas(<FromBody> request As CustomValidateRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim tableString As String = Nothing

            If ExcelArray IsNot Nothing And usedRows > 1 Then
                Dim idSocietyEl As String = Nothing
                Dim idDivisionEl As String = Nothing
                Dim idTiendaEl As String = Nothing
                Dim Date1El As Date = Nothing
                Dim Date2El As Date = Nothing
                Dim MontoDiarioEl As String = Nothing
                Dim dt As New DataTable()

                Dim ws As New WebServiceICMGeneral()
                Dim response = New DataTable
                Dim columnas As New List(Of String) From {"IDStore", "IDSociety", "IDPersonalDivision"}
                response = ws.ConsultaICMAPIQuery(columnas, "CfgStoreHierarchy", GetModel())
                Dim tiendas As List(Of Tienda) = GetListaTiendas(response)

                Dim valoresErroneos As New List(Of String)
                For row As Integer = 2 To usedRows
                    idSocietyEl = ExcelArray(row, 1)
                    idDivisionEl = ExcelArray(row, 2)
                    idTiendaEl = ExcelArray(row, 3)
                    Date1El = ParseDate(ExcelArray(row, 4))
                    Date2El = ParseDate(ExcelArray(row, 5))
                    MontoDiarioEl = ExcelArray(row, 6)

                    Dim tienda As List(Of Tienda)
                    If ((request.Society = "-1" Or request.Society Is Nothing) And (request.PersonnelDivision = "-1" Or request.PersonnelDivision Is Nothing)) Then
                        tienda = (From c In tiendas
                                  Where c.IDStore = idTiendaEl
                                  Select c).ToList
                    Else
                        tienda = (From c In tiendas
                                  Where c.IDStore = idTiendaEl And
                                  c.Society = idSocietyEl And
                                  c.PersonnelDivision = idDivisionEl And
                                  c.Society = request.Society And
                                  c.PersonnelDivision = request.PersonnelDivision
                                  Select c).ToList
                    End If

                    If (tienda.Count <= 0) Then
                        valoresErroneos.Add($"Tienda no válida|{idSocietyEl}|{idDivisionEl}|{idTiendaEl}|{Date1El.ToString()}|{Date2El.ToString()}|{MontoDiarioEl.ToString()}")
                    End If
                Next

                If valoresErroneos.Count Then
                    columnas = New List(Of String) From {"Error", "Sociedad", "División de Personal", "Tienda", "Fecha Inicio", "Fecha Fin", "Monto Diario"}

                    For Each col As String In columnas
                        dt.Columns.Add(col)
                    Next
                    For Each errorRow As String In valoresErroneos
                        Dim values() As String = errorRow.Split("|"c)
                        dt.Rows.Add(values)
                    Next

                    Dim path As String = fc.BuildXlsx(dt, "Validacion-Tiendas")

                    tableString = sc.GetMessage("Tiendas", "RegistrosInvalidos", columnas, valoresErroneos, valoresErroneos.Count)
                    Return Ok(New With {.d = False, .t = tableString, .f = path})
                Else
                    Return Ok(New With {.d = True})
                End If

            End If

            tableString = sc.GetMessage("Tiendas", "SinRegistros")
            Return Ok(New With {.d = False, .t = tableString})

        Catch ex As Exception
            mLog.insertLog("TiendasController", "ValidateCustomTiendas", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/tiendas/validatecustomexcepciones")>
    Public Function ValidateCustomExcepciones(<FromBody> request As CustomValidateRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim tableString As String = Nothing

            If ExcelArray IsNot Nothing And usedRows > 1 Then
                Dim idSocietyEl As String = Nothing
                Dim idDivisionEl As String = Nothing
                Dim idTiendaEl As String = Nothing
                Dim idEmpleado As String = Nothing
                Dim Date1El As Date = Nothing
                Dim Date2El As Date = Nothing
                Dim dt As New DataTable()
                Dim MontoDiarioEl As String = Nothing

                Dim ws As New WebServiceICMGeneral()
                Dim response = New DataTable
                Dim columnas As New List(Of String) From {"IDStore", "IDSociety", "IDPersonalDivision"}
                response = ws.ConsultaICMAPIQuery(columnas, "CfgStoreHierarchy", GetModel())
                Dim tiendas As List(Of Tienda) = GetListaTiendas(response)

                Dim valoresErroneos As New List(Of String)
                For row As Integer = 2 To usedRows
                    idSocietyEl = ExcelArray(row, 1)
                    idDivisionEl = ExcelArray(row, 2)
                    idTiendaEl = ExcelArray(row, 3)
                    idEmpleado = ExcelArray(row, 4)
                    Date1El = ParseDate(ExcelArray(row, 5))
                    Date2El = ParseDate(ExcelArray(row, 6))
                    MontoDiarioEl = ExcelArray(row, 7)

                    Dim tienda As List(Of Tienda)
                    If ((request.Society = "-1" Or request.Society Is Nothing) And (request.PersonnelDivision = "-1" Or request.PersonnelDivision Is Nothing)) Then
                        tienda = (From c In tiendas
                                  Where c.IDStore = idTiendaEl
                                  Select c).ToList
                    Else
                        tienda = (From c In tiendas
                                  Where c.IDStore = idTiendaEl And
                                  c.Society = idSocietyEl And
                                  c.PersonnelDivision = idDivisionEl And
                                  c.Society = request.Society And
                                  c.PersonnelDivision = request.PersonnelDivision
                                  Select c).ToList
                    End If

                    If (tienda.Count <= 0) Then
                        valoresErroneos.Add($"Tienda no válida|{idSocietyEl}|{idDivisionEl}|{idTiendaEl}|{idEmpleado}|{Date1El.ToString()}|{Date2El.ToString()}|{MontoDiarioEl.ToString()}")
                    End If

                Next

                If valoresErroneos.Count Then
                    columnas = New List(Of String) From {"Error", "Sociedad", "División de Personal", "Tienda", "Empleado", "Fecha Inicio", "Fecha Fin", "Monto Diario"}
                    For Each col As String In columnas
                        dt.Columns.Add(col)
                    Next
                    For Each errorRow As String In valoresErroneos
                        Dim values() As String = errorRow.Split("|"c)
                        dt.Rows.Add(values)
                    Next

                    Dim path As String = fc.BuildXlsx(dt, "Validacion-ExcepcionesTiendas")

                    tableString = sc.GetMessage("Excepciones de Tiendas", "RegistrosInvalidos", columnas, valoresErroneos, valoresErroneos.Count)
                    Return Ok(New With {.d = False, .t = tableString, .f = path})
                Else
                    Return Ok(New With {.d = True})
                End If

            End If

            tableString = sc.GetMessage("Excepciones de Tiendas", "SinRegistros")
            Return Ok(New With {.d = False, .t = tableString})

        Catch ex As Exception
            mLog.insertLog("TiendasController", "ValidateCustomExcepciones", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/tiendas/InsertInfoBDTiendas")>
    Public Function InsertInfoBDTiendas(<FromBody> _request As InsertInfoRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Try
            Thread.Sleep(1000)

            Dim headers = Request.Headers
            If headers.Contains("X-XSRF-Token") Then
                Dim formToken As String = headers.GetValues("X-XSRF-Token").FirstOrDefault()
                Dim cookie = HttpContext.Current.Request.Cookies("__RequestVerificationToken")
                Dim cookieToken As String = If(cookie IsNot Nothing, cookie.Value, Nothing)
                System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken)
            Else
                Return BadRequest("Token de Seguridad Inválido")
            End If

            Dim sanitize As New Sanitizacion
            Dim safeFileType As String = sanitize.Texto(_request.FileType)
            Dim safeExtension As String = sanitize.Texto(_request.Extension)

            Dim filedata As List(Of RegistroTiendas) = ProcesarExcelTiendas(safeFileType, safeExtension)
            Dim catalogos As List(Of String) = ObtenerCatalogosTiendas()
            ProcesarTiendas(filedata, catalogos)

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog.insertLog("TiendasController", "InsertInfoBDTiendas", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/tiendas/ConfirmExceptionsT")>
    Public Function ConfirmExceptionsT(<FromBody> request As InsertInfoRequest) As IHttpActionResult
        ' Validación de sesión
        If Me.mUser Is Nothing Then
            Return BadRequest("Session Expired or User Not Authenticated")
        End If

        ' Validación del request
        If request Is Nothing Then
            Return BadRequest("Invalid request: request cannot be null.")
        End If

        Try
            ' 1) Obtener errores y totales
            Dim errores As DataTable = ObtenerErroresTiendas()
            Dim cantidadTotalExcel As Integer = ObtenerRegistrosExcelTiendas(request)
            Dim cantidadTotalPg As Integer = ObtenerRegistrosTotales()
            Dim cantidadErrores As Integer = If(errores IsNot Nothing, errores.Rows.Count, 0)
            Dim cantidadImportados As Integer = Math.Max(cantidadTotalPg - cantidadErrores, 0)
            Dim filePath As String

            ' 2) Construir archivo xlsx 
            If (cantidadErrores > 0) Then
                'si hay errores
                filePath = fc.BuildXlsx(errores, "Tiendas")
            Else
                'si NO hay errores
                filePath = BuildSuccess(1, "Tiendas")
            End If

            ' 3) Determinar código y mensaje
            Dim codigoRespuesta As Integer
            Dim mensaje As String

            If cantidadTotalPg = 0 Then
                codigoRespuesta = 0
                mensaje = sc.GetMessage("Tiendas", "SinImportacion")
            ElseIf cantidadErrores = cantidadTotalPg Then
                codigoRespuesta = 0
                mensaje = sc.GetMessage("Tiendas", "SinImportacion", cantidadTotalPg, cantidadErrores)
            ElseIf cantidadErrores > 0 Then
                codigoRespuesta = 2
                SendSuccessResponse()
                mensaje = sc.GetMessage("Tiendas", "CargaParcial", cantidadTotalPg, cantidadErrores)
                SendSFTP_Tiendas()
            Else
                codigoRespuesta = 6
                SendSuccessResponse()
                mensaje = sc.GetMessage("Tiendas", "CargaCompleta", cantidadTotalPg)
                SendSFTP_Tiendas()
            End If

            ' 4) Respuesta
            Return Ok(New With {
            .d = codigoRespuesta,
            .f = filePath,
            .r = mensaje
        })

        Catch ex As Exception
            mLog.insertLog("TiendasController", "ConfirmExceptionsT", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/tiendas/InsertInfoBDExcepciones")>
    Public Function InsertInfoBDExcepciones(<FromBody> _request As InsertInfoRequest) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")
        Try
            Thread.Sleep(1000)

            Dim headers = Request.Headers
            If headers.Contains("X-XSRF-Token") Then
                Dim formToken As String = headers.GetValues("X-XSRF-Token").FirstOrDefault()
                Dim cookie = HttpContext.Current.Request.Cookies("__RequestVerificationToken")
                Dim cookieToken As String = If(cookie IsNot Nothing, cookie.Value, Nothing)
                System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken)
            Else
                Return BadRequest("Token de Seguridad Inválido")
            End If

            Dim sanitize As New Sanitizacion
            Dim safeFileType As String = sanitize.Texto(_request.FileType)
            Dim safeExtension As String = sanitize.Texto(_request.Extension)

            Dim filedata As List(Of RegistroExcepciones) = ProcesarExcelExcepciones(safeFileType, safeExtension)
            Dim catalogos As List(Of String) = ObtenerCatalogosExcepciones()
            ProcesarExcepciones(filedata, catalogos)

            Return Ok(New With {.d = True})
        Catch ex As Exception
            mLog.insertLog("TiendasController", "InsertInfoBDExcepciones", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/tiendas/ConfirmExceptionsE")>
    Public Function ConfirmExceptionsE(<FromBody> request As InsertInfoRequest) As IHttpActionResult
        ' Validación de sesión
        If Me.mUser Is Nothing Then
            Return BadRequest("Session Expired or User Not Authenticated")
        End If

        ' Validación del request
        If request Is Nothing Then
            Return BadRequest("Invalid request: request cannot be null.")
        End If

        Try
            ' 1) Obtener errores y totales
            Dim errores As DataTable = ObtenerErroresExcepciones()
            Dim cantidadTotalExcel As Integer = ObtenerRegistrosExcelExcepciones(request)
            Dim cantidadTotalPg As Integer = ObtenerRegistrosTotalesExcepciones()
            Dim cantidadErrores As Integer = If(errores IsNot Nothing, errores.Rows.Count, 0)
            Dim cantidadImportados As Integer = Math.Max(cantidadTotalPg - cantidadErrores, 0)
            Dim filePath As String

            ' 2) Construir archivo xlsx 
            If (cantidadErrores > 0) Then
                'si hay errores
                filePath = fc.BuildXlsx(errores, "Tiendas")
            Else
                'si NO hay errores
                filePath = BuildSuccess(0, "TiendasExcepciones")
            End If

            ' 3) Determinar código y mensaje
            Dim codigoRespuesta As Integer
            Dim mensaje As String

            If cantidadTotalPg = 0 Then
                codigoRespuesta = 0
                mensaje = sc.GetMessage("Excepciones de Tiendas", "SinImportacion")
            ElseIf cantidadErrores = cantidadTotalPg Then
                codigoRespuesta = 0
                mensaje = sc.GetMessage("Excepciones de Tiendas", "SinImportacion", cantidadTotalPg, cantidadErrores)
            ElseIf cantidadErrores > 0 Then
                codigoRespuesta = 2
                mensaje = sc.GetMessage("Excepciones de Tiendas", "CargaParcial", cantidadTotalPg, cantidadErrores)
                SendSuccessResponseEX()
                SendSFTP_Excepciones()
            Else
                codigoRespuesta = 6
                mensaje = sc.GetMessage("Excepciones de Tiendas", "CargaCompleta", cantidadTotalPg)
                SendSuccessResponseEX()
                SendSFTP_Excepciones()
            End If

            ' 4) Respuesta
            Return Ok(New With {
            .d = codigoRespuesta,
            .f = filePath,
            .r = mensaje
        })

        Catch ex As Exception
            mLog.insertLog("TiendasController", "ConfirmExceptionsE", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function


#End Region
#Region "Funciones"

    ''' <summary>
    ''' Procesa las excepciones y las inserta en la base de datos.
    ''' </summary>
    ''' <param name="filedata">Lista de registros de excepciones.</param>
    ''' <param name="catalogos">Listado de catálogos.</param>
    Private Sub ProcesarExcepciones(filedata As List(Of RegistroExcepciones), catalogos As List(Of String))

        Dim formToken As String = HttpContext.Current.Request.Headers("X-XSRF-Token")
        Dim cookie = HttpContext.Current.Request.Cookies("__RequestVerificationToken")
        Dim cookieToken As String = If(cookie IsNot Nothing, cookie.Value, Nothing)
        System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken)

        If catalogos Is Nothing OrElse catalogos.Count < 4 Then
            Throw New ArgumentException("Error en la obtencion de Catalogos | ProcesarExcepciones")
        End If
        For Each cat In catalogos
            If String.IsNullOrWhiteSpace(cat) Then
                Throw New ArgumentException("El catálogo esta vacío")
            End If
            Try
                JsonConvert.DeserializeObject(cat)
            Catch ex As Exception
                Throw New ArgumentException("Formato incorrecto de Catalogo")
            End Try
        Next

        Try
            Dim modelo As String = GetModel()
            Dim dateInsertion As DateTime = DateTime.Now

            Dim jsonFileData As String = JsonConvert.SerializeObject(filedata)
            Dim jsonCatSociety As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(0)))
            Dim jsonCatPersonalDivision As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(1)))
            Dim jsonCatStore As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(2)))
            Dim jsonPayee As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(3)))

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Const sql As String = "CALL spicmtoolscfgexceptiontransportationaidinsert(@p_usuario, @p_modelo, @p_fecha_hora, @p_filedata, @p_catsociety, @p_catpersonaldivision, @p_catstore, @p_payee_)"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("p_usuario", NpgsqlDbType.Varchar, mUser.Email)
                    cmd.Parameters.AddWithValue("p_modelo", NpgsqlDbType.Varchar, modelo)
                    cmd.Parameters.AddWithValue("p_fecha_hora", NpgsqlDbType.Date, dateInsertion)
                    cmd.Parameters.AddWithValue("p_filedata", NpgsqlDbType.Json, jsonFileData)
                    cmd.Parameters.AddWithValue("p_catsociety", NpgsqlDbType.Json, jsonCatSociety)
                    cmd.Parameters.AddWithValue("p_catpersonaldivision", NpgsqlDbType.Json, jsonCatPersonalDivision)
                    cmd.Parameters.AddWithValue("p_catstore", NpgsqlDbType.Json, jsonCatStore)
                    cmd.Parameters.AddWithValue("p_payee_", NpgsqlDbType.Json, jsonPayee)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Procesa las tiendas y las inserta en la base de datos.
    ''' </summary>
    ''' <param name="filedata">Lista de registros de tiendas.</param>
    ''' <param name="catalogos">Listado de catálogos.</param>
    Private Sub ProcesarTiendas(filedata As List(Of RegistroTiendas), catalogos As List(Of String))

        Dim formToken As String = HttpContext.Current.Request.Headers("X-XSRF-Token")
        Dim cookie = HttpContext.Current.Request.Cookies("__RequestVerificationToken")
        Dim cookieToken As String = If(cookie IsNot Nothing, cookie.Value, Nothing)
        System.Web.Helpers.AntiForgery.Validate(cookieToken, formToken)

        If catalogos Is Nothing OrElse catalogos.Count < 3 Then
            Throw New ArgumentException("Error en la obtencion de Catalogos | ProcesarExcepciones")
        End If
        For Each cat In catalogos
            If String.IsNullOrWhiteSpace(cat) Then
                Throw New ArgumentException("El catálogo esta vacío")
            End If
            Try
                JsonConvert.DeserializeObject(cat)
            Catch ex As Exception
                Throw New ArgumentException("Formato incorrecto de Catalogo")
            End Try
        Next

        Try
            Dim modelo As String = GetModel()
            Dim dateInsertion As DateTime = DateTime.Now

            Dim jsonFileData As String = JsonConvert.SerializeObject(filedata)
            Dim jsonCatSociety As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(0)))
            Dim jsonCatPersonalDivision As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(1)))
            Dim jsonCatStore As String = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(catalogos(2)))

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Const sql As String = "CALL spicmtoolscfgstoretransportationaidinsert(@p_usuario, @p_modelo, @p_fecha_hora, @p_filedata, @p_catsociety, @p_catpersonaldivision, @p_catstore)"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("p_usuario", NpgsqlDbType.Varchar, mUser.Email)
                    cmd.Parameters.AddWithValue("p_modelo", NpgsqlDbType.Varchar, modelo)
                    cmd.Parameters.AddWithValue("p_fecha_hora", NpgsqlDbType.Date, dateInsertion)
                    cmd.Parameters.AddWithValue("p_filedata", NpgsqlDbType.Json, jsonFileData)
                    cmd.Parameters.AddWithValue("p_catsociety", NpgsqlDbType.Json, jsonCatSociety)
                    cmd.Parameters.AddWithValue("p_catpersonaldivision", NpgsqlDbType.Json, jsonCatPersonalDivision)
                    cmd.Parameters.AddWithValue("p_catstore", NpgsqlDbType.Json, jsonCatStore)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public Function GetListaTiendas(dr As DataTable)

        Dim tiendas = New List(Of Tienda)
        For Each row As DataRow In dr.Rows
            Dim tienda As New Tienda With {
                    .IDStore = row("IDStore").ToString(),
                    .Society = row("IDSociety").ToString(),
                    .PersonnelDivision = row("IDPersonalDivision").ToString()
                }
            tiendas.Add(tienda)
        Next

        Return tiendas
    End Function

    ''' <summary>
    ''' Obtiene el modelo del usuario actual.
    ''' Si el modelo es "DEBUG", retorna el valor por defecto.
    ''' </summary>
    ''' <returns>Modelo del usuario o valor por defecto si está en modo DEBUG.</returns>
    Public Function GetModel() As String
        Dim Model As String = Nothing
        If mUser.Model = "DEBUG" Then
            Model = "femcoepdev"
        Else
            Model = mUser.Model

        End If
        Return Model
    End Function

    Function BuildSuccess(ByVal tOx As Boolean, ByVal name As String) As String
        Dim sql As String
        If tOx Then
            sql = "SELECT idsociety, idpersonaldivision, idstore, startdate, enddate, amount, descstatus AS detalle FROM CfgStoreTransportationAid_TEMP WHERE idstatus = 1;"
        Else
            sql = "SELECT idsociety, idpersonaldivision, idstore, payeeid, startdate, enddate, amount, descstatus AS detalle FROM cfgexceptiontransportationaid_temp WHERE idstatus = 1;"
        End If
        Dim path As String
        Dim dataTable As New DataTable()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        da.Fill(dataTable)
                    End Using
                End Using
            End Using
            path = fc.BuildXlsx(dataTable, name)
            Return path
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Método que obtiene los catálogos.
    ''' </summary>
    ''' <returns>Regresa el listado de catálogos.</returns>
    Function ObtenerCatalogosExcepciones() As List(Of String)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Try
            Dim Model = GetModel()
            Using ws As New WebServiceICMGeneral()

                columnas = New List(Of String) From {"IDSociety"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatSociety", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDPersonalDivision"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatPersonalDivision", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDStore"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatStore", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"PayeeID_"}
                Using dataTable = ws.ConsultaICMAPIQueryLotes(columnas, "Payee_", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

            End Using
            Return jCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    ''' <summary>
    ''' Método que obtiene los catálogos.
    ''' </summary>
    ''' <returns>Regresa el listado de catálogos.</returns>
    Function ObtenerCatalogosTiendas() As List(Of String)
        Dim jCatalogos As New List(Of String)
        Dim columnas As New List(Of String)
        Try
            Dim Model = GetModel()

            Using ws As New WebServiceICMGeneral()

                columnas = New List(Of String) From {"IDSociety"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatSociety", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDPersonalDivision"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatPersonalDivision", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

                columnas = New List(Of String) From {"IDStore"}
                Using dataTable = ws.ConsultaICMAPIQuery(columnas, "CatStore", Model)
                    Dim jsonTable As String = JsonConvert.SerializeObject(dataTable)
                    jCatalogos.Add(jsonTable)
                End Using

            End Using
            Return jCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    ''' <summary>
    ''' Obtiene los registros con errores desde la tabla temporal de excepciones de transporte.
    ''' </summary>
    ''' <returns>
    ''' Un objeto <see cref="DataTable"/> que contiene los registros con errores.
    ''' </returns>
    ''' <exception cref="NpgsqlException">Se lanza si ocurre un error al ejecutar la consulta en PostgreSQL.</exception>
    ''' <exception cref="InvalidOperationException">Se lanza si la conexión no puede abrirse.</exception>
    Function ObtenerErroresExcepciones() As DataTable
        Const sql As String = "SELECT idsociety, idpersonaldivision, idstore, payeeid, startdate, enddate, amount, descstatus AS detalle FROM cfgexceptiontransportationaid_temp WHERE idstatus = 0;"
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        Dim dataTable As New DataTable()
                        da.Fill(dataTable)
                        Return dataTable
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Function ObtenerRegistrosTotales() As Integer
        Const sql As String = "SELECT * FROM CfgStoreTransportationAid_TEMP;"
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        Dim dataTable As New DataTable()
                        da.Fill(dataTable)
                        Dim responseCount As Integer = If(dataTable IsNot Nothing, dataTable.Rows.Count, 0)
                        Return responseCount
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Function ObtenerRegistrosTotalesExcepciones() As Integer
        Const sql As String = "SELECT * FROM cfgexceptiontransportationaid_temp;"
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        Dim dataTable As New DataTable()
                        da.Fill(dataTable)
                        Dim responseCount As Integer = If(dataTable IsNot Nothing, dataTable.Rows.Count, 0)
                        Return responseCount
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Obtiene los registros con errores desde la tabla temporal de tiendas.
    ''' </summary>
    ''' <returns>
    ''' Un objeto <see cref="DataTable"/> que contiene los registros con errores.
    ''' </returns>
    ''' <exception cref="NpgsqlException">Se lanza si ocurre un error al ejecutar la consulta en PostgreSQL.</exception>
    ''' <exception cref="InvalidOperationException">Se lanza si la conexión no puede abrirse.</exception>
    Function ObtenerErroresTiendas() As DataTable
        Const sql As String = "SELECT idsociety, idpersonaldivision, idstore, startdate, enddate, amount, descstatus AS detalle FROM CfgStoreTransportationAid_TEMP WHERE idstatus = 0;"
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        Dim dataTable As New DataTable()
                        da.Fill(dataTable)
                        Return dataTable
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Obtiene los registros Insertados para Tiendas y los envia por Mail
    ''' </summary>
    ''' <exception cref="NpgsqlException">Se lanza si ocurre un error al ejecutar la consulta en PostgreSQL.</exception>
    ''' <exception cref="InvalidOperationException">Se lanza si la conexión no puede abrirse.</exception>
    Sub SendSuccessResponse()
        Dim NowDate As String = Now.ToString("yyyy-MM-dd")
        Dim sql As String = $"SELECT idsociety, idpersonaldivision, idstore, startdate, enddate, amount FROM CfgStoreTransportationAid;" '' WHERE idstatus = 1;"
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

            Dim filePathMail As String = fc.BuildXlsx(TableResponse, "Tiendas_")
            Dim Model As String = GetModel()

            ws.WebServiceSendMail(mUser.Email, "ICMTools | Tienda Transporte  - STATUS VALIDACION C0027 INCENTIVO AYUDA DE TRANSPORTE", mailBody, "femcoepdev", filePathMail)
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene los registros Insertados para Tiendas Excepciones y los envia por Mail
    ''' </summary>
    ''' <exception cref="NpgsqlException">Se lanza si ocurre un error al ejecutar la consulta en PostgreSQL.</exception>
    ''' <exception cref="InvalidOperationException">Se lanza si la conexión no puede abrirse.</exception>
    Sub SendSuccessResponseEX()
        Dim NowDate As String = Now.ToString("yyyy-MM-dd")
        Dim sql As String = $"SELECT idsociety, idpersonaldivision, idstore, payeeid, startdate, enddate, amount FROM CfgExceptionTransportationaid;" ''WHERE IDStatus = 1;"
        Dim TableResponse As New DataTable()
        Dim mailBody As String = "Se Ejecuto el proceso de Validacion <strong>Favor de revisar el archivo anexo al correo </strong>"
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand(sql, conn)
                    Using da As New NpgsqlDataAdapter(cmd)
                        da.Fill(TableResponse)
                    End Using
                End Using
            End Using

            Dim filePath As String = fc.BuildXlsx(TableResponse, "Tiendas-Excepciones")
            Dim Model As String = GetModel()

            ws.WebServiceSendMail(mUser.Email, "ICMTools | Excepciones Transporte - STATUS VALIDACION C0027 INCENTIVO AYUDA DE TRANSPORTE", mailBody, "femcoepdev", filePath)
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene la cantidad de registros encontrados en el excel.
    ''' </summary>
    ''' <param name="request">Request.</param>
    ''' <returns>Número de registros encontrados.</returns>
    ''' <exception cref="NpgsqlException">Se lanza si ocurre un error al ejecutar la consulta en PostgreSQL.</exception>
    ''' <exception cref="InvalidOperationException">Se lanza si la conexión no puede abrirse.</exception>
    Public Function ObtenerRegistrosExcelExcepciones(request As InsertInfoRequest) As Integer
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            Return ExcelArray.GetUpperBound(0) - 1
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Obtiene la cantidad de registros encontrados en el excel.
    ''' </summary>
    ''' <param name="request">Request.</param>
    ''' <returns>Número de registros encontrados.</returns>
    ''' <exception cref="NpgsqlException">Se lanza si ocurre un error al ejecutar la consulta en PostgreSQL.</exception>
    ''' <exception cref="InvalidOperationException">Se lanza si la conexión no puede abrirse.</exception>
    Public Function ObtenerRegistrosExcelTiendas(request As InsertInfoRequest) As Integer
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            Return ExcelArray.GetUpperBound(0) - 1
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Convierte una cadena en una fecha utilizando formatos específicos.
    ''' </summary>
    ''' <param name="fechaTexto">Cadena que contiene la fecha a convertir.</param>
    ''' <returns>Un valor <see cref="DateTime"/> convertido desde la cadena.</returns>
    ''' <exception cref="ArgumentNullException">Se lanza si <paramref name="fechaTexto"/> es nulo o vacío.</exception>
    ''' <exception cref="FormatException">Se lanza si la cadena no coincide con ninguno de los formatos esperados.</exception>
    Private Function ParseDate(fechaTexto As String) As DateTime
        If String.IsNullOrWhiteSpace(fechaTexto) Then
            Throw New ArgumentNullException(NameOf(fechaTexto), "El texto de la fecha no puede ser nulo o vacío.")
        End If

        Dim fechaResultado As DateTime
        Dim formatos() As String = {"dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd"}

        Dim exito As Boolean = DateTime.TryParseExact(
            fechaTexto,
            formatos,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            fechaResultado
        )

        If Not exito Then
            Throw New FormatException($"No se pudo convertir el texto '{fechaTexto}' a un valor de fecha válido.")
        End If

        Return fechaResultado
    End Function

    ''' <summary>
    ''' Método que procesa el archivo de excel de excepciones.
    ''' </summary>
    ''' <param name="fileType">Tipo de archivo.</param>
    ''' <param name="extension">Extensión.</param>
    ''' <returns>Regresa el json del excel.</returns>
    Public Function ProcesarExcelExcepciones(fileType As String, extension As String) As List(Of RegistroExcepciones)
        Dim ExcelArray(,) As Object = fc.GetExcelArray(fileType, extension)
        Dim jTable As New List(Of RegistroExcepciones)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim idsociety As String = Convert.ToString(ExcelArray(row, 1))
            Dim idpersonaldivision As String = Convert.ToString(ExcelArray(row, 2))
            Dim idstore As String = Convert.ToString(ExcelArray(row, 3))
            Dim payeeid As String = Convert.ToString(ExcelArray(row, 4))
            Dim startdate As Date = ParseDate(ExcelArray(row, 5))
            Dim enddate As Date = ParseDate(ExcelArray(row, 6))
            Dim amount As Decimal = Convert.ToDecimal(ExcelArray(row, 7).ToString().Replace("$", ""))

            jTable.Add(New RegistroExcepciones With {
                        .idsociety = idsociety,
                        .idpersonaldivision = idpersonaldivision,
                        .idstore = idstore,
                        .payeeid = payeeid,
                        .startdate = startdate,
                        .enddate = enddate,
                        .amount = amount
                    })
        Next
        Return jTable
    End Function

    ''' <summary>
    ''' Método que procesa el archivo de excel de tiendas.
    ''' </summary>
    ''' <param name="fileType">Tipo de archivo.</param>
    ''' <param name="extension">Extensión.</param>
    ''' <returns>Regresa el json del excel.</returns>
    Public Function ProcesarExcelTiendas(fileType As String, extension As String) As List(Of RegistroTiendas)
        Dim ExcelArray(,) As Object = fc.GetExcelArray(fileType, extension)
        Dim jTable As New List(Of RegistroTiendas)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim idsociety As String = Convert.ToString(ExcelArray(row, 1))
            Dim idpersonaldivision As String = Convert.ToString(ExcelArray(row, 2))
            Dim idstore As String = Convert.ToString(ExcelArray(row, 3))
            Dim startdate As Date = ParseDate(ExcelArray(row, 4))
            Dim enddate As Date = ParseDate(ExcelArray(row, 5))
            Dim amount As Decimal = Convert.ToDecimal(ExcelArray(row, 6).ToString().Replace("$", ""))

            jTable.Add(New RegistroTiendas With {
                        .idsociety = idsociety,
                        .idpersonaldivision = idpersonaldivision,
                        .idstore = idstore,
                        .startdate = startdate,
                        .enddate = enddate,
                        .amount = amount
                    })
        Next
        Return jTable
    End Function

    Private Sub SendSFTP_Excepciones()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.TiendasExcepciones
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub SendSFTP_Tiendas()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.Tiendas
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

#End Region
End Class