Imports System.Web.Http
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class MTTVEntradaEnfoqueControllerController
    Inherits ApiController

    Private ReadOnly mUser As User
    Private ReadOnly mLog As Log
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ReadOnly fc As New FileController()
    ReadOnly sc As New SharedController()

    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
    End Sub

    Private success As New DataTable()
    Dim tBody As String = Nothing
    Private scenario As Integer = Nothing

    <HttpPost>
    <Route("api/mttventradaenfoque/insertdata")>
    Public Function InsertData(<FromBody> request As ValidateFileRequest) As IHttpActionResult
        Dim filePath As String = Nothing
        Try
            Dim ExcelArray(,) As Object = fc.GetExcelArray(request.FileType, request.Extension)
            If ExcelArray Is Nothing Then Return BadRequest("No se encontraron datos para insertar.")

            Dim jTable As List(Of Object) = ObtenerJsonExcel(ExcelArray)
            If jTable.Count = 0 Then Return Ok(New With {.d = "No hay filas válidas para insertar."})

            Dim xlsx As New DataTable()
            Dim dsCatalogos As DataSet = ObtenerCatalogos()
            Dim valoresInvalidos = ValidarCatalogos(dsCatalogos, ExcelArray)
            Dim success As DataTable = RegistrarInformacion(jTable, dsCatalogos)

            If success.Rows.Count > 0 Then
                filePath = GetParcials(success)
            End If

            If jTable.Count <> success.Rows.Count Then
                SendSFTP()
            End If

            Dim rTable As String = MostrarMensaje(success, filePath, jTable.Count)

            Return Ok(New With {
                .d = scenario,
                .r = rTable,
                .f = filePath
            })

        Catch ex As Exception
            mLog.insertLog("MTTVEntradaEnfoqueController", "InsertData", ex.Message)
            Return InternalServerError(ex)
        End Try
    End Function

    Function ValidarCatalogos(dsCatalogos As DataSet, ExcelArray As Object) As String
        Dim valoresInexistentes As New StringBuilder()
        Dim valorInexistente As String

        valorInexistente = ValidarCatalogo(dsCatalogos.Tables(0), "IDStore", ExcelArray, 1, "IDStore no válido")
        valoresInexistentes.Append(valorInexistente)

        Return valoresInexistentes.ToString()
    End Function

    Function ValidarCatalogo(dtCatalogo As DataTable, campo As String, ExcelArray As Object, colExcel As Int32, textoError As String) As String
        Dim valoresInexistentes As New StringBuilder()
        Try
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim columnas As New List(Of String) From {campo}

            For row As Integer = 2 To usedRows
                Dim valor As String = ExcelArray(row, colExcel)

                Dim resultados = (From fila In dtCatalogo.AsEnumerable()
                                  Where fila.Field(Of String)(campo) = valor
                                  Select fila).Take(1).SingleOrDefault()

                If (resultados Is Nothing) Then
                    Dim valorInexistente As String = "<tr><td>" + textoError + "</td><td>" + valor + "</td></tr>"
                    valoresInexistentes.Append(valorInexistente)
                End If
            Next
            Return valoresInexistentes.ToString()
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function ObtenerJsonExcel(ExcelArray As Object) As List(Of Object)
        Try
            Dim jTable As New List(Of Object)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)

            For row As Integer = 2 To usedRows
                Dim IDStore As String = ExcelArray(row, 1).ToString()
                Dim Enfoque As String = ExcelArray(row, 2).ToString()

                Dim BEGDA As Date
                If Not (Date.TryParseExact(ExcelArray(row, 3).ToString(), "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, BEGDA)) Then
                    BEGDA = Date.ParseExact(ExcelArray(row, 3).ToString(), "yyyy/MM/dd", Globalization.CultureInfo.InvariantCulture)
                End If

                Dim ENDDA As Date
                If Not (Date.TryParseExact(ExcelArray(row, 4).ToString(), "dd/MM/yyyy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, ENDDA)) Then
                    ENDDA = Date.ParseExact(ExcelArray(row, 4).ToString(), "yyyy/MM/dd", Globalization.CultureInfo.InvariantCulture)
                End If

                If String.IsNullOrWhiteSpace(IDStore) AndAlso String.IsNullOrWhiteSpace(Enfoque) AndAlso String.IsNullOrWhiteSpace(BEGDA) AndAlso String.IsNullOrWhiteSpace(ENDDA) Then Continue For

                jTable.Add(New With {
                        .IDStore = IDStore,
                        .Enfoque = Enfoque,
                        .BEGDA = BEGDA,
                        .ENDDA = ENDDA
                    })
            Next

            Return jTable
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function ObtenerCatalogos() As DataSet
        'Dim modelo As String = ConfigurationManager.AppSettings("ModelFemcoEPDev")
        Dim dsCatalogos As DataSet = New DataSet()
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
        Try
            Using ws As New WebServiceICMGeneral()
                Dim Model As String = mUser.Model
                If Model = "DEBUG" Then
                    Model = "femcoepdev"
                End If
                Dim columnas As New List(Of String) From {"IDStore"}
                Using dataTable As DataTable = ws.ConsultaICMAPIQuery(columnas, "CatStore", Model)
                    dataTable.TableName = "CatStore"
                    dsCatalogos.Tables.Add(dataTable.Copy())
                End Using
            End Using
            Return dsCatalogos
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function RegistrarInformacion(jTable As List(Of Object), dsCatalogos As DataSet) As DataTable
        Try
            Dim jsonTable As String = JsonConvert.SerializeObject(jTable)
            Dim jsonStore As String = JsonConvert.SerializeObject(dsCatalogos.Tables(0))
            Dim filePath As String = Nothing

            Using conn As New NpgsqlConnection(NpgSQL)
                Using cmd As New NpgsqlCommand("SELECT * FROM public.z_mt_inc_variable_entrada_enfoque(@jtable, @catstoretable)", conn)
                    conn.Open()
                    cmd.Parameters.AddWithValue("jtable", NpgsqlDbType.Json, jsonTable)
                    cmd.Parameters.AddWithValue("catstoretable", NpgsqlDbType.Json, jsonStore)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(success)
                    End Using

                    Return success
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Function GetParcials(success As DataTable) As String
        Try
            Dim filePath As String = Nothing

            If success.Rows.Count > 0 Then
                filePath = fc.BuildXlsx(success, "MultiTiendaVariable_EntradaEnfoque")
            End If

            Return filePath
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Function MostrarMensaje(success As DataTable, filePath As String, rowCount As Integer) As String
        If rowCount > 0 AndAlso rowCount = success.Rows.Count Then
            tBody = $"
                <tr>
                    <td>Error al ejecutar el proceso de importación del archivo de MultiTienda Variable Entrada Enfoque</td>
                     <td>No se encontró información válida para importar<br>Por favor verifique la información del archivo</td>
                </tr>"
            scenario = 3
            Return sc.TableBuilder(tBody, 1)
        End If

        If rowCount > 0 And filePath Is Nothing Then
            tBody = $"
                <tr>
                    <td>Ejecución Completada Exitosamente</td>
                    <td>Se ejecutó correctamente el proceso externo
                        <br><strong>Carga de MultiTienda Variable Entrada Enfoque</strong>
                    </td>
                </tr>"
            scenario = 1
        Else
            tBody = $"
                <tr>
                    <td>Ejecución Completada Parcialmente</td>
                    <td>Se ejecutó parcialmente el proceso externo
                        <br><strong>Carga de MultiTienda Variable Entrada Enfoque, por favor revise el archivo descargado para validar errores</strong>
                    </td>
                </tr>"
            scenario = 2
        End If

        Return sc.TableBuilder(tBody, 3)

    End Function

    Private Sub SendSFTP()
        Try
            Dim envio As New EnvioPGPClass
            envio.Pantalla = EnvioPGPClass.enuPantalla.EntradaEnfoque
            envio.Enviar()
        Catch ex As Exception
            Throw
        End Try
    End Sub
End Class