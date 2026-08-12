Imports System.Threading
Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class TiendasGanadorasController
    Inherits ApiController

#Region "[ Variables Privadas ]"

    Private mUser As User
    Private mLog As Log
    Private ReadOnly _Pantalla As String = "Tiendas Ganadoras"

    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ReadOnly fc As New FileController
    ReadOnly sc As New SharedController

    Private Class Registro
        Public plaza As String
        Public tienda As String
        Public concurso As String
    End Class

#End Region

#Region "[ Inicial ]"

    ''' <summary>
    ''' Evento New de la página.
    ''' </summary>
    Public Sub New()
        mUser = CType(HttpContext.Current.Session.Item("User"), User)
        mLog = New Log()
    End Sub

#End Region

    <HttpPost>
    <Route("api/tiendasganadoras/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Try
            Dim rTable As String = Nothing

            Thread.Sleep(1000)
            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})

            Dim jTable As New List(Of Registro)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim filePath = Nothing
            Dim IdStore As String = Nothing
            Dim Subject As String = Nothing

            For row As Integer = 2 To usedRows
                Dim plazaID As String = ExcelArray(row, 1).ToString()
                Dim tiendaID As String = ExcelArray(row, 2).ToString()
                Dim concursoID As String = ExcelArray(row, 3).ToString()

                If String.IsNullOrWhiteSpace(plazaID) AndAlso String.IsNullOrWhiteSpace(tiendaID) Then Continue For

                Dim registroDuplicado As Boolean = jTable.Any(
                    Function(r) r.plaza = plazaID AndAlso
                                      r.tienda = tiendaID AndAlso
                                      r.concurso = concursoID)
                If registroDuplicado Then
                    rTable = sc.GetMessage(_Pantalla, "Duplicados")
                    Return Ok(New With {.d = False, .r = rTable})
                End If

                jTable.Add(New Registro With {
                        .plaza = plazaID,
                        .tienda = tiendaID,
                        .concurso = concursoID
                    })
                IdStore = ExcelArray(row, 2).ToString()
            Next

            If jTable.Count = 0 Then Return Ok(New With {.d = False, .r = sc.GetMessage(_Pantalla, "SinRegistros")})
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)
            Dim Model As String = mUser.Model
            If Model = "DEBUG" Then
                Model = "femcoepdev"
            End If

            Dim ws As New WebServiceICMGeneral()
            Dim epsap As DataTable = ws.TiendasGanadorasAPIQuery(IdStore, Model)
            Dim success As Boolean = False
            Dim partialC As Boolean = False
            Dim hayRegistros As Boolean

            Dim epsapTable As String = JsonConvert.SerializeObject(epsap)
            Dim xlsx As New DataTable()

            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT public.FEMCOEPSAP_Validacion_Archivo_TiendasGanadoresCC(@jTable, @epsapTable)", conn)
                    cmd.Parameters.AddWithValue("jTable", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("epsapTable", NpgsqlDbType.Json, epsapTable)
                    success = cmd.ExecuteScalar()
                End Using

                Dim query As String = $"SELECT ""Tienda"", ""Concurso"", ""ValDescripcion"", ""ValStatus"" FROM tiendasganadoras_precarga WHERE ""ValStatus"" = '1' LIMIT 1"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        Using dataTable As New DataTable()
                            adapter.Fill(dataTable)
                            hayRegistros = dataTable.Rows.Count > 0
                        End Using
                    End Using
                End Using

                query = $"SELECT ""Tienda"", ""Concurso"", ""ValDescripcion"", ""ValStatus"" FROM tiendasganadoras_precarga WHERE ""ValStatus"" = '0'"
                Using cmdQ As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmdQ)
                        adapter.Fill(xlsx)
                    End Using
                End Using
            End Using

            If xlsx.Rows.Count > 0 Then
                filePath = fc.BuildXlsx(xlsx, "TiendasGanadoras")
                partialC = hayRegistros
            End If

            If Not hayRegistros Then
                rTable = sc.GetMessage(_Pantalla, "SinImportacion")
                Return Ok(New With {.d = False, .r = rTable, .f = filePath})
            ElseIf success = True And partialC = False Then
                CargarInformacion()
                SendSFTP()
                rTable = sc.GetMessage(_Pantalla, "CargaCompleta")
                Return Ok(New With {.d = 1, .r = rTable})
            ElseIf success = True And partialC = True Then
                rTable = sc.GetMessage(_Pantalla, "ProcesoIncompleto")
                Return Ok(New With {.d = 5, .r = rTable, .f = filePath})
            Else
                rTable = sc.GetMessage(_Pantalla, "Error",
                       New List(Of String) From {"Plaza", "Tienda", "IDConcurso"},
                       New List(Of String) From {"10CAN DESCRIPCION", "50GLC", "IDConcurso"})
                Return Ok(New With {.d = False, .r = rTable})
            End If
        Catch ex As Exception
            mLog.insertLog("TiendasGanadorasController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpPost>
    <Route("api/tiendasganadoras/uploaddata")>
    Public Function UploadData() As IHttpActionResult
        Try
            Dim mensaje As String = sc.GetMessage(_Pantalla, "CargaParcial")
            CargarInformacion()
            SendSFTP()
            Return Ok(New With {.d = 2, .r = mensaje})
        Catch ex As Exception
            mLog.insertLog("TiendasGanadorasController", "UploadData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    ''' <summary>
    ''' Método que carga la información
    ''' </summary>
    Private Sub CargarInformacion()
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                Const sql As String = "CALL tiendasganadoras_cargar();"
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
            envio.Pantalla = EnvioPGPClass.enuPantalla.TiendasGanadoras
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub

End Class