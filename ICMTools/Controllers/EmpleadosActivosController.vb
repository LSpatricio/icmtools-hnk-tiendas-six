Imports System.Threading
Imports System.Web.Http
Imports ICMTools.FileController
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class CEATables
    Public Property TableName As String
    Public Property Cols As List(Of String)
    Public Property NTable As String
    Public Sub New(ByVal nombre As String, ByVal listaCols As List(Of String), ByVal postgreTable As String)
        Me.TableName = nombre
        Me.Cols = listaCols
        Me.NTable = postgreTable
    End Sub
End Class


Public Class ValidateFileResquestEmpleadosActivos
    Inherits ValidateFileRequest
    Public Property FileType2 As String
End Class

Public Module cEmpleadosActivosConfig
    Public ReadOnly EmpleadosActivosCatalogos As New List(Of CEATables) From {
        New CEATables("FEMCOVSPersonalDivisionPlaza", New List(Of String) From {"PersonalDivision", "Plaza", "PlazaCR"}, "FEMCOVS_PersonalDivisionPlaza")
    }
End Module

Public Class EmpleadosActivosController
    Inherits ApiController

    Private ReadOnly _Pantalla As String = "Empleados Activos"
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController
    Private mUser As User
    Private mLog As Log

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log
    End Sub

    <HttpPost>
    <Route("api/empleadosactivos/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileResquestEmpleadosActivos) As IHttpActionResult
        Try
            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim IdStore As String = Nothing
            Dim rTable As String = Nothing

            Dim jTable = ProcesarExcel(request.FileType, request.Extension)
            Dim jTable2 = ProcesarExcel(request.FileType2, request.Extension)

            If jTable.Count = 0 OrElse jTable2.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            If jTable Is Nothing Or jTable2 Is Nothing Then
                rTable = sc.GetMessage(_Pantalla, "Duplicados")
                Return Ok(New With {.d = False, .r = rTable})
            End If

            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)
            Dim jsonTable2 As String = JsonConvert.SerializeObject(jTable2)

            Dim ws As New WebServiceICMGeneral()

            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcovsdev"
            End If
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand($"TRUNCATE TABLE ""FEMCOVS_PersonalDivisionPlaza""", conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            For Each catalogo In cEmpleadosActivosConfig.EmpleadosActivosCatalogos
                ws.InsertaCatalogos(catalogo.Cols, catalogo.TableName, Model, catalogo.NTable)
            Next

            Dim columnascatPlazas As New List(Of String) From {"ID", "plazaId", "Description"}
            Dim catPlazasFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatPlazas, "catPlazas", Model)

            Dim columnascatDistritos As New List(Of String) From {"ID", "plazaId", "Description"}
            Dim catDistritosFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatDistritos, "catDistritos", Model)

            Dim columnasCfgStoreSocietys As New List(Of String) From {"IDSociety", "IDStore"}
            Dim CfgStoreSocietyFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnasCfgStoreSocietys, "CfgStoreSociety", Model)

            Dim columnascatTiendas As New List(Of String) From {"tiendaId"}
            Dim catTiendasFEMCOVS As DataTable = ws.ConsultaICMAPIQuery(columnascatTiendas, "catTiendas", Model)

            Dim columnasPayee_ As New List(Of String) From {"PayeeID_"}
            Dim PayeeFEMCOVS As DataTable = ws.ConsultaICMAPIQueryLotes(columnasPayee_, "Payee_", Model)

            Dim success As Boolean = False

            Dim catPlazasJson As String = JsonConvert.SerializeObject(catPlazasFEMCOVS)
            Dim catDistritosJson As String = JsonConvert.SerializeObject(catDistritosFEMCOVS)
            Dim CfgStoreSocietyJson As String = JsonConvert.SerializeObject(CfgStoreSocietyFEMCOVS)
            Dim catTiendasJson As String = JsonConvert.SerializeObject(catTiendasFEMCOVS)
            Dim PayeeJson As String = JsonConvert.SerializeObject(PayeeFEMCOVS)

            Dim xlsx As New DataTable()
            Dim respuestas As New DataTable()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.ConnectionString &= ";CommandTimeout=0"
                conn.Open()

                Using cmd As New NpgsqlCommand($"TRUNCATE TABLE public.payeetempempleadosactivos;", conn)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand($"TRUNCATE TABLE public.tblpasoempleadosactivos1;", conn)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand($"TRUNCATE TABLE public.tblpasoempleadosactivos2;", conn)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand($"                    
                    INSERT INTO public.payeetempempleadosactivos (PAYEEID_)
                    SELECT 
                        ""PayeeID_""
                    FROM jsonb_to_recordset(@jsonData::jsonb) AS x(
                        ""PayeeID_"" VARCHAR(100)
                    );
                ", conn)
                    cmd.Parameters.AddWithValue("jsonData", NpgsqlTypes.NpgsqlDbType.Jsonb, PayeeJson)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand($"                    
                    INSERT INTO public.tblpasoempleadosactivos1 (PersonalDivision, OU, Society, IDEmployee, Employee, HireDate, Function, Ceco, AuxilaryCeco, PersonalSubdivision, Division)
                    SELECT 
                        personaldivision,
                        ou,
                        society,
                        idemployee,
                        employee,
                        hiredate,
                        functionrow,
                        ceco,
                        auxilaryceco,
                        personalsubdivision,
                        division
                    FROM jsonb_to_recordset(@jsonData::jsonb) AS x(
                        personaldivision text,
                        ou text,
                        society text,
                        idemployee text,
                        employee text,
                        hiredate text,
                        functionrow text,
                        ceco text,
                        auxilaryceco text,
                        personalsubdivision text,
                        division text
                    );
                ", conn)
                    cmd.Parameters.AddWithValue("jsonData", NpgsqlTypes.NpgsqlDbType.Jsonb, jsonTable)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand($"                    
                    INSERT INTO public.tblpasoempleadosactivos2 (PersonalDivision, OU, Society, IDEmployee, Employee, HireDate, Function, Ceco, AuxilaryCeco, PersonalSubdivision, Division)
                    SELECT 
                        personaldivision,
                        ou,
                        society,
                        idemployee,
                        employee,
                        hiredate,
                        functionrow,
                        ceco,
                        auxilaryceco,
                        personalsubdivision,
                        division
                    FROM jsonb_to_recordset(@jsonData2::jsonb) AS x(
                        personaldivision text,
                        ou text,
                        society text,
                        idemployee text,
                        employee text,
                        hiredate text,
                        functionrow text,
                        ceco text,
                        auxilaryceco text,
                        personalsubdivision text,
                        division text
                    );
                ", conn)
                    cmd.Parameters.AddWithValue("jsonData2", NpgsqlTypes.NpgsqlDbType.Jsonb, jsonTable2)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New NpgsqlCommand("SELECT * FROM public.spfemcovsimportempleadosactivos(@catplazastable ,@catdistritostable, @cfgstoresocietytable, @cattiendastable)", conn)
                    cmd.Parameters.AddWithValue("catplazastable", NpgsqlDbType.Json, catPlazasJson)
                    cmd.Parameters.AddWithValue("catdistritostable", NpgsqlDbType.Json, catDistritosJson)
                    cmd.Parameters.AddWithValue("cfgstoresocietytable", NpgsqlDbType.Json, CfgStoreSocietyJson)
                    cmd.Parameters.AddWithValue("cattiendastable", NpgsqlDbType.Json, catTiendasJson)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(xlsx)
                    End Using
                End Using

                Using cmd As New NpgsqlCommand($"
                    SELECT
                        COUNT(*) AS Total,
                        COUNT(CASE WHEN ""IDStatus"" = FALSE THEN 1 END) AS TotalFalse
                    FROM ""empleadosactivos_precarga"";
                    ", conn)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(respuestas)
                    End Using
                End Using
            End Using

            Dim filePath As String = Nothing
            If CInt(respuestas.Rows(0)("TotalFalse")) > 0 Then
                filePath = fc.BuildXlsx(xlsx, "EmpleadosActivos")
            End If

            Dim registrosTotal As Integer = CInt(respuestas.Rows(0)("Total"))
            Dim registrosErrores As Integer = CInt(respuestas.Rows(0)("TotalFalse"))
            Dim registrosCorrectos As Integer = registrosTotal - registrosErrores
            Dim respuesta As Integer

            If registrosCorrectos = 0 Then
                rTable = sc.GetMessage(_Pantalla, "sinimportacion")
                respuesta = 0
            ElseIf registrosTotal = registrosCorrectos Then
                CargarInformacion()
                SendSFTP()
                rTable = sc.GetMessage(_Pantalla, "CargaCompleta", CInt(respuestas.Rows(0)("Total")))
                respuesta = 1
            ElseIf registrosCorrectos > 0 Then
                If xlsx.AsEnumerable.Select(Function(s) s("detalle")).FirstOrDefault().ToString().Contains("No se ha detectado carga de información") Then
                    rTable = sc.GetMessage(_Pantalla, "sinimportacion")
                    respuesta = 0
                Else
                    rTable = sc.GetMessage(_Pantalla, "ProcesoIncompleto", CInt(respuestas.Rows(0)("Total")), CInt(respuestas.Rows(0)("TotalFalse")))
                    respuesta = 5
                End If
            Else
                If mUser IsNot Nothing AndAlso Not String.IsNullOrEmpty(mUser.Model) Then
                    ClassLibrary_PGP_TO_SFTP.Main_PGPtoSFTP.Proceso("VentasServicios", xlsx, mUser.Model)
                End If
                rTable = sc.GetMessage(_Pantalla, "Error",
                    New List(Of String) From {"División de personal", "Unidad organizativa", "Sociedad", "Número de personal", "Nombre editado del empleado o candidato", "Fecha de alta", "Función", "Centro de coste", "CeCo Auxiliar", "Subdivisión de personal", "División"},
                    New List(Of String) From {"OXXO CASRINTA", "OXXO VILLAS DE COPLE RITA", "Especializados Oxxo", "1299", "Jon Doe Muniz", "01.11.2010", "Especialista Analista", "91ABC58IPJ", "0", "ABCJ930713ABC", "Plaza Carsint"},
                    CInt(respuestas.Rows(0)("Total")),
                    CInt(respuestas.Rows(0)("TotalFalse"))
                )
                respuesta = 0
            End If

            Return Ok(New With {
                          .d = respuesta,
                          .f = filePath,
                          .r = rTable
                          })
        Catch ex As Exception
            mLog.insertLog("EmpleadosActivosController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try

    End Function

    <HttpPost>
    <Route("api/empleadosactivos/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("EmpleadosActivosController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL empleadosactivos_cargar();"
                Using cmd As New NpgsqlCommand(sql, conn)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Public Function ProcesarExcel(fileType As String, extension As String) As List(Of Object)
        Dim ExcelArray(,) As Object = fc.GetExcelArray(fileType, extension)
        If ExcelArray Is Nothing Then Return Nothing

        Dim jTable As New List(Of Object)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

        For row As Integer = 2 To usedRows
            Dim personalDivision As String = Convert.ToString(ExcelArray(row, 1))
            Dim OU As String = Convert.ToString(ExcelArray(row, 2))
            Dim society As String = Convert.ToString(ExcelArray(row, 3))
            Dim IDEmployee As String = Convert.ToString(ExcelArray(row, 4))
            Dim employee As String = Convert.ToString(ExcelArray(row, 5))
            Dim hireDate As String = ExcelArray(row, 6).ToString()
            Dim functionRow As String = ExcelArray(row, 7).ToString()
            Dim ceco As String = ExcelArray(row, 8).ToString()
            Dim auxilaryCeco As String = ExcelArray(row, 9).ToString()
            Dim personalSubdivision As String = ExcelArray(row, 10).ToString()
            Dim division As String = ExcelArray(row, 11).ToString()

            'Convert.ToString(ExcelArray(row, 9))

            If String.IsNullOrWhiteSpace(personalDivision) Or String.IsNullOrWhiteSpace(functionRow) Then Continue For

            jTable.Add(New With {
                        .personaldivision = personalDivision,
                        .ou = OU,
                        .society = society,
                        .idemployee = IDEmployee,
                        .employee = employee,
                        .hiredate = hireDate,
                        .functionrow = functionRow,
                        .ceco = ceco,
                        .auxilaryceco = auxilaryCeco,
                        .personalsubdivision = personalSubdivision,
                        .division = division
                    })
        Next
        Return jTable
    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.EmpleadosActivos
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class