Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Npgsql
Imports ICMTools.FunctionalityICM
Imports DocumentFormat.OpenXml.Drawing
Imports DocumentFormat.OpenXml.Math

Public Class ICMService

    Private ReadOnly _httpClient As HttpClient
    Private ICMBaseUrl As String = ConfigurationManager.AppSettings("BASE_URL")
    Private Bearer As String = ConfigurationManager.AppSettings("BEARER_TOKEN")

    Private Shared ReadOnly EstadoTexto As New Dictionary(Of StatusImportacionEnum, String) From {
    {StatusImportacionEnum.Running, "Ejecutándose"},
    {StatusImportacionEnum.Completed, "Completado"},
    {StatusImportacionEnum.Failed, "Falló"},
    {StatusImportacionEnum.Cancelled, "Cancelado"},
    {StatusImportacionEnum.SinRespuesta, "Sin Respuesta"}
}

    Public Sub New(ByVal httpClient As HttpClient)
        If httpClient Is Nothing Then
            Throw New ArgumentNullException(NameOf(httpClient))
        End If

        _httpClient = httpClient

        _httpClient.DefaultRequestHeaders.Clear()

        _httpClient.DefaultRequestHeaders.Accept.Clear()
        _httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        Dim token As String = Bearer  ' aquí tu token sin "Bearer "
        _httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", token)
    End Sub

    Public Async Function GetAudit(ByVal tablaICM As String, ByVal AEvent As String, ByVal modelo As String, LastDate As String) As Task(Of String)
        If String.IsNullOrWhiteSpace(tablaICM) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(tablaICM))
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim encodedEvent As String = Uri.EscapeDataString(AEvent)
            Dim encodedTable As String = Uri.EscapeDataString(tablaICM)
            Dim encodedDate As String = Uri.EscapeDataString(LastDate)

            Dim requestUrl As String = $"{ICMBaseUrl}/uservisiblesystemtables/Audit_/data" &
                               $"?offset=0&limit=10&orderBy=-AuditID_" &
                               $"&filter=Event_⊇{encodedEvent};Message_⊇{encodedTable}" &
                               $";Time_=({encodedDate}T00:00:00.000Z\,]"

            Using request = New HttpRequestMessage(HttpMethod.Get, requestUrl)
                request.Headers.Add("Model", modelo)

                Dim response As HttpResponseMessage = Await _httpClient.SendAsync(request).ConfigureAwait(False)

                If Not response.IsSuccessStatusCode Then
                    response.Dispose()
                    Throw New HttpRequestException($"Error al obtener los encabezados: {response.StatusCode}")
                End If

                Dim responseString As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                Dim responseRow As String = GetFirstDataAsString(responseString)
                response.Dispose()

                Return responseRow
            End Using

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function ConsultarICM(ByVal tablaICM As String, ByVal consultaOriginal As String, ByVal modelo As String, ByVal Optional parametros As String = "") As Task(Of DataTable)
        If String.IsNullOrWhiteSpace(tablaICM) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(tablaICM))
        End If

        Try

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim requestUrlEncabezados As String = $"{ICMBaseUrl}/customtables/{tablaICM}/inputforms/0/data"
            Dim requestEncabezados = New HttpRequestMessage(HttpMethod.[Get], requestUrlEncabezados)
            requestEncabezados.Headers.Add("Model", modelo)

            Dim encabezadosResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestEncabezados)

            If Not encabezadosResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los encabezados: {encabezadosResponse.StatusCode}")
            End If

            Dim jsonEncabezado As JObject = JObject.Parse(Await encabezadosResponse.Content.ReadAsStringAsync())
            Dim consultaAjustada As String = FunctionalityICM.AjustarConsulta(consultaOriginal)
            Dim requestUrlDatos As String = $"{ICMBaseUrl}/imports/getdbpreview"
            Dim body As String = $"
                              {{
                                  ""importParams"": {{
                                      ""query"": ""{consultaAjustada} {parametros}"",
                                      ""model"":""{modelo}"",
                                      ""filename"": null,
                                      ""hasHeader"": null,
                                      ""queryTimeout"": 900,
                                      ""importType"": ""DBImport""
                                  }},
                                  ""numLines"": 999999999
            }}"
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, requestUrlDatos) With {
                .Content = New StringContent(body, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", modelo)
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido)

            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            Dim jsonContenido As JArray = JArray.Parse(Await contenidoResponse.Content.ReadAsStringAsync())
            Return FunctionalityICM.ICMToDataTable(jsonEncabezado, jsonContenido)
        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function ConsultarICM(ByVal tablaICM As String, ByVal consultaOriginal As String, ByVal modelo As String, ByVal dt As DataTable, ByVal Optional parametros As String = "") As Task(Of DataTable)
        If String.IsNullOrWhiteSpace(tablaICM) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(tablaICM))
        End If

        Try
            Dim consultaAjustada As String = FunctionalityICM.AjustarConsulta(consultaOriginal)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12



            Dim requestUrlDatos As String = $"{ICMBaseUrl}/imports/getdbpreview"
            Dim body As String = $"
                              {{
                                  ""importParams"": {{
                                      ""query"": ""{consultaAjustada} {parametros}"",
                                      ""model"":""{modelo}"",
                                      ""filename"": null,
                                      ""hasHeader"": null,
                                      ""queryTimeout"": 900,
                                      ""importType"": ""DBImport""
                                  }},
                                  ""numLines"": 999999999
                              }}"
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, requestUrlDatos) With {
                .Content = New StringContent(body, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", modelo)
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido)

            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            Dim jsonContenido As JArray = JArray.Parse(Await contenidoResponse.Content.ReadAsStringAsync())
            Return FunctionalityICM.ICMToDataTable(dt, jsonContenido)
        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function


    ''' <summary>
    ''' Valida si hay un evento global en ejecución consultando /globalactionstatus.
    ''' Devuelve True si el servicio indica que hay evento en progreso.
    ''' </summary>
    ''' <param name="model">Nombre del modelo a revisar.</param>
    ''' <returns>True si hay un evento global en ejecución; False en caso contrario.</returns>
    Public Async Function GlobalActionStatus(model As String) As Task(Of Boolean)
        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim requestUrl As String = $"{ICMBaseUrl}/globalactionstatus"

            Using request = New HttpRequestMessage(HttpMethod.Get, requestUrl)
                request.Headers.Add("Model", model)

                Dim response As HttpResponseMessage = Await _httpClient.SendAsync(request).ConfigureAwait(False)

                If Not response.IsSuccessStatusCode Then
                    response.Dispose()
                    Throw New HttpRequestException($"Error al obtener los encabezados: {response.StatusCode}")
                End If

                Dim responseString As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                Dim responseBoolean As Boolean = If(Boolean.TryParse(responseString, responseBoolean), responseBoolean, False)

                response.Dispose()
                Return responseBoolean
            End Using

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    '''<summary>
    '''Lanza una peticion POST al QueryTool del API de varicent ICM 
    '''</summary>
    '''<returns>Un Datatable con el resultado del Query.</returns>
    '''<remarks>
    '''Esta función requiere que el HttpClient se haya inicializado correctamente con las cabeceras necesarias.
    '''La petición Serializa un JSON y retorna un DataTable para poder procesar la informacion posteriormente
    '''</remarks>
    Public Async Function QueryICM(ByVal tablaICM As String, ByVal consultaOriginal As String, ByVal modelo As String, ByVal Optional parametros As String = "", ByVal Optional limit As Integer = 0, ByVal Optional offset As Integer = 0) As Task(Of DataTable)

        If String.IsNullOrWhiteSpace(ICMBaseUrl) Then
            Throw New ArgumentException("La ruta de varicent no puede ser nulo o vacío.")
        End If

        If String.IsNullOrWhiteSpace(tablaICM) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(tablaICM))
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim buildURL As String = $"{ICMBaseUrl}/rpc/querytool"

            ''Generamos el Payload
            Dim payload As String = $"{{
                ""queryString"": ""{consultaOriginal} {parametros}"",
                ""offset"": {offset},
                ""limit"": {limit}
            }}"

            ''Genera la peticion POST
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, buildURL) With {
                .Content = New StringContent(payload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", modelo)

            ''Lanza la Peticion POST
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

            ''Valida la Respuesta 
            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            ''Serializamos la respuesta a un DataTable
            Dim jsonString As String = Await contenidoResponse.Content.ReadAsStringAsync().ConfigureAwait(False)
            Dim jsonContenido As JObject = JObject.Parse(jsonString)

            Dim dataTable As New DataTable()
            Dim columnDefinitions As JArray = TryCast(jsonContenido("columnDefinitions"), JArray)

            ''Definición del DataTable
            If columnDefinitions IsNot Nothing Then
                For Each columnDef As JObject In columnDefinitions
                    ''Obtenemos el nombre y el tipo de la columna
                    Dim columnName As String = columnDef.Value(Of String)("name")
                    Dim columnTypeString As String = columnDef.Value(Of String)("type")

                    ''Conversion del tipo de dato de la cadena JSON a un tipo de .NET
                    Dim dataType As Type = GetTypeFromICMType(columnTypeString)

                    ''Agregar la columna al DT
                    dataTable.Columns.Add(columnName, dataType)
                Next
            End If

            ''Extracción de los datos del JSON
            Dim jsonData As JArray = TryCast(jsonContenido("data"), JArray)
            If jsonData IsNot Nothing Then
                For Each rowData As JArray In jsonData
                    Dim newRow As DataRow = dataTable.NewRow()
                    For i As Integer = 0 To rowData.Count - 1
                        Dim value = If(rowData(i).Type = JTokenType.Null, DBNull.Value, CType(rowData(i), JValue).Value)
                        newRow(i) = value
                    Next
                    dataTable.Rows.Add(newRow)
                Next
            End If

            Return dataTable

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function PublishTable(Table As String, destinationPath As String, Modelo As String) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(Table) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(Table))
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim buildURL As String = $"{ICMBaseUrl}/publishdownload"
            Dim payload As String = $"{{
	                                    ""location"": """",
	                                    ""name"": ""{Table}"",
	                                    ""publisherId"": -1,
	                                    ""publisherParams"": [
		                                    {{
			                                    ""columnWithDescription"": [],
			                                    ""customTableName"": ""{Table}"",
			                                    ""filter"": null,
			                                    ""includeExtendedTableColumns"": false,
			                                    ""inputFormID"": 0,
			                                    ""type"": ""CustomTable""
                                            }},
		                                    {{
			                                    ""fileName"": ""{Table}"",
			                                    ""type"": ""File""
		                                    }},
		                                    {{
			                                    ""delimiter"": "","",
			                                    ""type"": ""Delimited""
		                                    }}
	                                    ],
	                                    ""publishType"": ""CustomTableText""
                                     }}"

            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, buildURL) With {
                .Content = New StringContent(payload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Modelo)

            Using contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(False)
                If Not contenidoResponse.IsSuccessStatusCode Then
                    Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
                End If

                Using streamRed As Stream = Await contenidoResponse.Content.ReadAsStreamAsync().ConfigureAwait(False)
                    Using streamDisco As New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, True)
                        Await streamRed.CopyToAsync(streamDisco).ConfigureAwait(False)
                    End Using
                End Using
            End Using

            Return True

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Async Function PublishTable(Table As String, destinationPath As String, Modelo As String, FilterField As String, Filter As String) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(Table) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(Table))
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim buildURL As String = $"{ICMBaseUrl}/publishdownload"
            Dim payload As String = $"{{
	                                    ""location"": """",
	                                    ""name"": ""{Table}"",
	                                    ""publisherId"": -1,
	                                    ""publisherParams"": [
		                                    {{
			                                    ""columnWithDescription"": [],
			                                    ""customTableName"": ""{Table}"",
			                                    ""filter"": ""{FilterField}=[{Filter}\\,]"",
			                                    ""includeExtendedTableColumns"": false,
			                                    ""inputFormID"": 0,
			                                    ""type"": ""CustomTable""
                                            }},
		                                    {{
			                                    ""fileName"": ""{Table}"",
			                                    ""type"": ""File""
		                                    }},
		                                    {{
			                                    ""delimiter"": "","",
			                                    ""type"": ""Delimited""
		                                    }}
	                                    ],
	                                    ""publishType"": ""CustomTableText""
                                     }}"

            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, buildURL) With {
                .Content = New StringContent(payload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Modelo)

            Using contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(False)
                If Not contenidoResponse.IsSuccessStatusCode Then
                    Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
                End If

                Using streamRed As Stream = Await contenidoResponse.Content.ReadAsStreamAsync().ConfigureAwait(False)
                    Using streamDisco As New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, True)
                        Await streamRed.CopyToAsync(streamDisco).ConfigureAwait(False)
                    End Using
                End Using
            End Using

            Return True

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Async Function PublishCalc(Table As String, destinationPath As String, Modelo As String) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(Table) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(Table))
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim calcId As String = "\""_Result374\"""

            SyncCalc("374", Modelo)

            Dim buildURL As String = $"{ICMBaseUrl}/rpc/querytool/export"
            Dim payload As String = $"{{
	                                    ""exportFileFormat"": ""Text"",
	                                    ""limit"": 0,
	                                    ""offset"": 0,
	                                    ""queryString"": ""SELECT * FROM {calcId}""
                                     }}"


            Using clientLong As New HttpClient()
                clientLong.Timeout = TimeSpan.FromMinutes(10)
                clientLong.DefaultRequestHeaders.Authorization = _httpClient.DefaultRequestHeaders.Authorization

                Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, buildURL) With {
                .Content = New StringContent(payload, Encoding.UTF8, "application/json")
            }
                requestContenido.Headers.Add("Model", Modelo)

                Using contenidoResponse As HttpResponseMessage = Await clientLong.SendAsync(requestContenido, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(False)

                    If Not contenidoResponse.IsSuccessStatusCode Then
                        Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
                    End If

                    Using streamRed As Stream = Await contenidoResponse.Content.ReadAsStreamAsync().ConfigureAwait(False)
                        Using streamDisco As New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, True)
                            Await streamRed.CopyToAsync(streamDisco).ConfigureAwait(False)
                        End Using
                    End Using
                End Using
            End Using

            Return True

        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Private Sub SyncCalc(ByVal ID As String, ByVal Modelo As String)
        If String.IsNullOrWhiteSpace(ID) Then
            Throw New ArgumentException("El ID de calculo no puede ser nulo o vacío.")
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            Dim buildURL As String = $"{ICMBaseUrl}/calculations/async/{ID}/data?allowSync=true&partialSyncBack=true"

            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, buildURL)
            requestContenido.Headers.Add("Model", Modelo)

            Using contenidoResponse As HttpResponseMessage = _httpClient.SendAsync(requestContenido, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult()

                If Not contenidoResponse.IsSuccessStatusCode Then
                    Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
                End If
            End Using

            System.Threading.Thread.Sleep(TimeSpan.FromMinutes(5))

        Catch ex As Exception
            Throw ex
        End Try
    End Sub


    '''<summary>
    '''Lanza una peticion POST al QueryTool del API de varicent ICM 
    '''</summary>
    '''<returns>Un Datatable con el resultado del Query.</returns>
    '''<remarks>
    '''Esta función requiere que el HttpClient se haya inicializado correctamente con las cabeceras necesarias.
    '''La petición Serializa un JSON y retorna un DataTable para poder procesar la informacion posteriormente
    '''</remarks>
    Public Async Function OffsetChunk(ByVal tablaICM As String, ByVal offset As Integer, ByVal limit As Integer, ByVal modelo As String, ByVal columnas As List(Of String)) As Task(Of DataTable)
        If String.IsNullOrWhiteSpace(tablaICM) Then
            Throw New ArgumentException("El nombre de la tabla ICM no puede ser nulo o vacío.", NameOf(tablaICM))
        End If

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim buildURL As String = $"{ICMBaseUrl}/customtables/{tablaICM}/inputforms/0/data?offset={offset}&limit={limit}"

            ''Genera la peticion POST
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Get, buildURL)
            requestContenido.Headers.Add("Model", modelo)

            ''Lanza la Peticion POST
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

            ''Valida la Respuesta 
            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            ''Serializamos la respuesta a un DataTable
            Dim jsonString As String = Await contenidoResponse.Content.ReadAsStringAsync().ConfigureAwait(False)
            Dim jsonContenido As JObject = JObject.Parse(jsonString)

            Dim dataTable As New DataTable()
            Dim columnDefinitions As JArray = TryCast(jsonContenido("columnDefinitions"), JArray)
            Dim includedColumnIndices As New List(Of Integer)()

            If columnDefinitions IsNot Nothing Then
                For i As Integer = 0 To columnDefinitions.Count - 1
                    Dim columnDef As JObject = CType(columnDefinitions(i), JObject)
                    Dim columnName As String = columnDef.Value(Of String)("name")

                    If columnas IsNot Nothing AndAlso columnas.Any() AndAlso Not columnas.Contains(columnName) Then
                        Continue For
                    End If

                    Dim columnTypeString As String = columnDef.Value(Of String)("type")
                    Dim dataType As Type = GetTypeFromICMType(columnTypeString)
                    dataTable.Columns.Add(columnName, dataType)
                    includedColumnIndices.Add(i)
                Next
            End If

            ''Extracción de los datos del JSON
            Dim jsonData As JArray = TryCast(jsonContenido("data"), JArray)
            If jsonData IsNot Nothing Then
                For Each rowData As JArray In jsonData
                    Dim newRow As DataRow = dataTable.NewRow()
                    For i As Integer = 0 To includedColumnIndices.Count - 1
                        Dim originalIndex As Integer = includedColumnIndices(i)
                        Dim value = If(rowData(originalIndex).Type = JTokenType.Null, DBNull.Value, CType(rowData(originalIndex), JValue).Value)
                        newRow(i) = value
                    Next
                    dataTable.Rows.Add(newRow)
                Next
            End If

            Return dataTable

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function ImportacionICM(modelo As String, importacion As String) As Task(Of String)

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        Dim buildURL As String = $"{ICMBaseUrl}/rpc/imports/{importacion}/run"

        Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, buildURL)
        requestContenido.Headers.Add("Model", modelo)

        Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)
        'If Not contenidoResponse.IsSuccessStatusCode Then
        '    Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")

        'End If
        If Not contenidoResponse.IsSuccessStatusCode Then
            Select Case contenidoResponse.StatusCode
                Case HttpStatusCode.ServiceUnavailable '503
                    Throw New HttpRequestException("No se puede ejecutar la importación. La tabla XXICMGENDOCUMENTOS está bloqueada por otro proceso en ejecución. Por favor, inténtelo nuevamente en unos minutos.")

                Case HttpStatusCode.Conflict '409
                    Throw New HttpRequestException("No se puede ejecutar la importación. Actualmente hay otros procesos en ejecución. Por favor, inténtelo nuevamente en unos minutos.")

                Case Else
                    Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End Select
        End If

        Dim respuestaresult As String = Await contenidoResponse.Content.ReadAsStringAsync()

        Dim objRespues As RunActivityDto = JsonConvert.DeserializeObject(Of RunActivityDto)(respuestaresult)

        If objRespues Is Nothing Then
            Throw New Exception("La API de ICM devolvió una respuesta vacía o no válida.")
        End If

        If String.IsNullOrWhiteSpace(objRespues.CompletedActivities) Then
            Throw New Exception("El campo 'CompletedActivities' viene vacío en la respuesta de la API.")
        End If

        ' Validación 3: que el formato permita Split
        Dim partes = objRespues.CompletedActivities.Split("/"c)

        If partes.Length = 0 OrElse String.IsNullOrWhiteSpace(partes.Last()) Then
            Throw New Exception("No fue posible extraer el runId del campo 'CompletedActivities'.")
        End If

        Dim runId As String = partes.Last()


        Return runId


    End Function


    Public Async Function StatusLiveActivitiesICM(modelo As String, runId As String) As Task(Of String)
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        Dim statusImportacion As String = ""
        Dim buildURLLiveActivities As String = $"{ICMBaseUrl}/liveactivities?filter=progressId={runId}"

        Dim requestContenido = New HttpRequestMessage(HttpMethod.Get, buildURLLiveActivities)
        requestContenido.Headers.Add("Model", modelo)

        Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

        If Not contenidoResponse.IsSuccessStatusCode Then
            Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
        End If

        Dim respuestaLiveActivities As String = Await contenidoResponse.Content.ReadAsStringAsync()

        If String.IsNullOrWhiteSpace(respuestaLiveActivities) Then
            Throw New Exception("La API devolvió una respuesta vacía en liveactivities.")
        End If

        Dim listaLiveActivities As List(Of LiveActivitiesDto)

        Try
            listaLiveActivities = JsonConvert.DeserializeObject(Of List(Of LiveActivitiesDto))(respuestaLiveActivities)
        Catch ex As Exception
            Throw New Exception("No se pudo deserializar la respuesta de liveactivities. JSON: " & respuestaLiveActivities, ex)
        End Try

        ' Validar que la lista exista
        If listaLiveActivities Is Nothing Then
            Throw New Exception("La API devolvió null en la lista de liveactivities.")
        End If

        ' Validar lista vacía []
        If listaLiveActivities.Count = 0 Then
            Return StatusImportacionEnum.SinRespuesta
        End If

        ' Tomar el primer elemento
        statusImportacion = listaLiveActivities(0).Status

        ' Validar que Status no venga vacío
        If String.IsNullOrWhiteSpace(statusImportacion) Then
            Throw New Exception("El campo 'Status' viene vacío en liveactivities para runId.")
        End If

        Return statusImportacion

    End Function

    Public Async Function StatusCompletedActivitiesICM(modelo As String, runId As String) As Task(Of String)
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        Dim statusImportacion As String = ""
        Dim buildURLCompletedActivities As String = $"{ICMBaseUrl}/completedactivities?filter=progressId={runId}"

        Dim requestContenido = New HttpRequestMessage(HttpMethod.Get, buildURLCompletedActivities)
        requestContenido.Headers.Add("Model", modelo)

        Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

        If Not contenidoResponse.IsSuccessStatusCode Then
            Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
        End If

        Dim respuestaCompletedActivities As String = Await contenidoResponse.Content.ReadAsStringAsync()

        If String.IsNullOrWhiteSpace(respuestaCompletedActivities) Then
            Throw New Exception("La API devolvió una respuesta vacía en completedactivities.")
        End If

        Dim listaCompletedActivities As List(Of CompletedActivitiesDto)

        Try
            listaCompletedActivities = JsonConvert.DeserializeObject(Of List(Of CompletedActivitiesDto))(respuestaCompletedActivities)
        Catch ex As Exception
            Throw New Exception("No se pudo deserializar la respuesta de completedactivities. JSON: " & respuestaCompletedActivities, ex)
        End Try

        If listaCompletedActivities Is Nothing Then
            Throw New Exception("La API devolvió null en completedactivities.")
        End If

        If listaCompletedActivities.Count = 0 Then
            Return StatusImportacionEnum.SinRespuesta
        End If

        statusImportacion = listaCompletedActivities(0).Status

        If String.IsNullOrWhiteSpace(statusImportacion) Then
            Throw New Exception("El campo 'Status' viene vacío en completedactivities.")
        End If

        Return statusImportacion

    End Function

    Public Function ObtenerTextoEstado(estado As StatusImportacionEnum) As String
        Return EstadoTexto(estado)
    End Function

    Public Async Function SendMail_ICM(Model As String, Mail As String, Subject As String, Body As String) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(Mail) Then
            Throw New ArgumentException("El Mail del destinatario no puede ser nulo o vacío.")
        End If

        Dim BuildUrl As String = $"{ICMBaseUrl}/admin/tsapi/sendMail"

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            ''Generamos el Payload
            Dim payloadObject As New JObject()
            payloadObject.Add("to", New JArray(Mail))
            payloadObject.Add("subject", Subject)
            payloadObject.Add("body", Body)
            payloadObject.Add("useHtml", True)

            Dim jsonPayload As String = payloadObject

            ''Genera la peticion POST
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, BuildUrl) With {
                .Content = New StringContent(jsonPayload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Model)

            ''Lanza la Peticion POST
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

            ''Valida la Respuesta 
            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            Return True

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function SendMail_ICM(Model As String, CC As List(Of String), Mail As List(Of String), Subject As String, Body As String) As Task(Of Boolean)
        If Mail.Count <= 0 Then
            Throw New ArgumentException("El Mail del destinatario no puede ser nulo o vacío.")
        End If

        If CC Is Nothing Then CC = New List(Of String)()

        Dim BuildUrl As String = $"{ICMBaseUrl}/admin/tsapi/sendMail"

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            ''Generamos el Payload
            Dim payloadObject As New JObject()
            payloadObject.Add("to", JArray.FromObject(Mail))
            payloadObject.Add("cc", JArray.FromObject(CC))
            payloadObject.Add("subject", Subject)
            payloadObject.Add("body", Body)
            payloadObject.Add("useHtml", True)

            Dim jsonPayload As String = payloadObject.ToString()

            ''Genera la peticion POST
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, BuildUrl) With {
                .Content = New StringContent(jsonPayload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Model)

            ''Lanza la Peticion POST
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

            ''Valida la Respuesta 
            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            Return True

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function SendMailWFile_ICM(Model As String, Mail As String, Subject As String, Body As String, xlsxPath As String) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(Mail) Then
            Throw New ArgumentException("El Mail del destinatario no puede ser nulo o vacío.")
        End If

        Dim BuildUrl As String = $"{ICMBaseUrl}/admin/tsapi/sendMail"

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim tsv_bytes As Byte() = System.IO.File.ReadAllBytes(xlsxPath)
            Dim base64String As String = System.Convert.ToBase64String(tsv_bytes)
            Dim fileName As String = System.IO.Path.GetFileName(xlsxPath)

            Dim attachmentObject As New JObject()
            attachmentObject.Add("fileName", fileName)
            attachmentObject.Add("content", base64String)

            Dim attachmentsArray As New JArray()
            attachmentsArray.Add(attachmentObject)

            ''Generamos el Payload
            Dim payloadObject As New JObject()
            payloadObject.Add("to", New JArray(Mail))
            payloadObject.Add("subject", Subject)
            payloadObject.Add("body", Body)
            payloadObject.Add("useHtml", True)
            payloadObject.Add("attachments", attachmentsArray)

            Dim jsonPayload As String = payloadObject.ToString()

            ''Genera la peticion POST
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, BuildUrl) With {
                .Content = New StringContent(jsonPayload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Model)

            ''Lanza la Peticion POST
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

            ''Valida la Respuesta 
            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            Return True

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function SendSomeMailsWFile_ICM(Model As String, MailList As List(Of String), CC As List(Of String), Subject As String, Body As String, xlsxPath As String) As Task(Of Boolean)
        If MailList Is Nothing AndAlso MailList.Count > 0 Then
            Throw New ArgumentException("El Mail del destinatario no puede ser nulo o vacío.")
        End If

        Dim BuildUrl As String = $"{ICMBaseUrl}/admin/tsapi/sendMail"

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

            Dim attachmentsArray As New JArray()
            If System.IO.File.Exists(xlsxPath) Then
                Dim tsv_bytes As Byte() = System.IO.File.ReadAllBytes(xlsxPath)
                Dim base64String As String = System.Convert.ToBase64String(tsv_bytes)
                Dim fileName As String = System.IO.Path.GetFileName(xlsxPath)

                Dim attachmentObject As New JObject()
                attachmentObject.Add("fileName", fileName)
                attachmentObject.Add("content", base64String)
                attachmentsArray.Add(attachmentObject)
            End If


            ''Generamos el Payload
            Dim payloadObject As New JObject()
            payloadObject.Add("to", JArray.FromObject(MailList))
            payloadObject.Add("cc", JArray.FromObject(CC))
            payloadObject.Add("subject", Subject)
            payloadObject.Add("body", Body)
            payloadObject.Add("useHtml", True)
            payloadObject.Add("attachments", attachmentsArray)

            Dim jsonPayload As String = payloadObject.ToString()

            ''Genera la peticion POST
            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, BuildUrl) With {
                .Content = New StringContent(jsonPayload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Model)

            ''Lanza la Peticion POST
            Dim contenidoResponse As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido).ConfigureAwait(False)

            ''Valida la Respuesta 
            If Not contenidoResponse.IsSuccessStatusCode Then
                Throw New HttpRequestException($"Error al obtener los datos: {contenidoResponse.StatusCode}")
            End If

            Return True

        Catch ex As HttpRequestException
            Console.WriteLine($"Error al realizar la solicitud HTTP: {ex.Message}")
            Throw New InvalidOperationException("Ocurrió un error al comunicarse con el servicio ICM.", ex)
        Catch ex As TaskCanceledException
            Console.WriteLine($"Solicitud cancelada o excedió el tiempo de espera: {ex.Message}")
            Throw New TimeoutException("La solicitud tardó demasiado y fue cancelada.", ex)
        Catch ex As Exception
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}")
            Throw New InvalidOperationException($"Error en ConsultarICM: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function AsyncStream(ByVal Destiny As String, ByVal lcols As List(Of String), ByVal Model As String, ByVal Query As String) As Task
        Try
            Dim NpgConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
            Dim SuperSecretApiUrl As String = $"{ConfigurationManager.AppSettings("BASE_URL")}/rpc/querytool"
            Dim cols As String = String.Join(",", lcols.Select(Function(s) $"{s.ToLower()}"))
            Dim lowerCols As String = cols.ToLower()

            Dim payload As String = $"{{
                ""queryString"": ""{Query}"",
                ""offset"": 0,
                ""limit"": 0
            }}"

            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, SuperSecretApiUrl) With {
                .Content = New StringContent(payload, Encoding.UTF8, "application/json")
            }
            requestContenido.Headers.Add("Model", Model)

            Using conn As New NpgsqlConnection(NpgConn)
                Await conn.OpenAsync().ConfigureAwait(False)
                Using importer As NpgsqlBinaryImporter = conn.BeginBinaryImport($"COPY ""{Destiny}"" ({lowerCols}) FROM STDIN (FORMAT BINARY)")
                    Using response As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(False)
                        response.EnsureSuccessStatusCode()
                        Using stream As Stream = Await response.Content.ReadAsStreamAsync().ConfigureAwait(False)
                            Using streamReader As New StreamReader(stream)
                                Using jsonReader As New JsonTextReader(streamReader)
                                    If Await jsonReader.ReadAsync().ConfigureAwait(False) AndAlso jsonReader.TokenType = JsonToken.StartObject Then
                                        While Await jsonReader.ReadAsync().ConfigureAwait(False)
                                            If jsonReader.TokenType = JsonToken.PropertyName AndAlso CStr(jsonReader.Value) = "data" Then
                                                Await jsonReader.ReadAsync().ConfigureAwait(False)
                                                If jsonReader.TokenType = JsonToken.StartArray Then
                                                    While Await jsonReader.ReadAsync().ConfigureAwait(False) AndAlso jsonReader.TokenType = JsonToken.StartArray
                                                        Dim rowData As JArray = CType(Await JArray.LoadAsync(jsonReader).ConfigureAwait(False), JArray)
                                                        importer.StartRow()
                                                        For i As Integer = 0 To lcols.Count - 1
                                                            If i < rowData.Count Then
                                                                Dim value = If(rowData(i).Type = JTokenType.Null, DBNull.Value, rowData(i).ToObject(Of Object)())
                                                                Await importer.WriteAsync(value, NpgsqlTypes.NpgsqlDbType.Varchar).ConfigureAwait(False)
                                                            Else
                                                                Await importer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Varchar).ConfigureAwait(False)
                                                            End If
                                                        Next
                                                    End While
                                                End If
                                                Exit While
                                            End If
                                        End While
                                    End If
                                End Using
                            End Using
                        End Using
                    End Using
                    Await importer.CompleteAsync().ConfigureAwait(False)
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Async Function AsyncStream(ByVal Destiny As String, ByVal lcols As List(Of String), ByVal Model As String, ByVal Query As String, ByVal loteSize As Integer, ByVal offSet As Integer) As Task
        Try
            Dim NpgConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
            Dim SuperSecretApiUrl As String = $"{ConfigurationManager.AppSettings("BASE_URL")}/rpc/querytool"
            Dim cols As String = String.Join(",", lcols.Select(Function(s) $"{s.ToLower()}"))
            Dim lowerCols As String = cols.ToLower()

            Dim payload As String = $"{{
                ""queryString"": ""{Query}"",
                ""offset"": {offSet},
                ""limit"": {loteSize}
            }}"

            Dim requestContenido = New HttpRequestMessage(HttpMethod.Post, SuperSecretApiUrl) With {
            .Content = New StringContent(payload, Encoding.UTF8, "application/json")
        }
            requestContenido.Headers.Add("Model", Model)

            Using conn As New NpgsqlConnection(NpgConn)
                Await conn.OpenAsync().ConfigureAwait(False)
                Using importer As NpgsqlBinaryImporter = conn.BeginBinaryImport($"COPY {Destiny} ({lowerCols}) FROM STDIN (FORMAT BINARY)")
                    Using response As HttpResponseMessage = Await _httpClient.SendAsync(requestContenido, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(False)
                        response.EnsureSuccessStatusCode()
                        Using stream As Stream = Await response.Content.ReadAsStreamAsync().ConfigureAwait(False)
                            Using streamReader As New StreamReader(stream)
                                Using jsonReader As New JsonTextReader(streamReader)
                                    If Await jsonReader.ReadAsync().ConfigureAwait(False) AndAlso jsonReader.TokenType = JsonToken.StartObject Then
                                        While Await jsonReader.ReadAsync().ConfigureAwait(False)
                                            If jsonReader.TokenType = JsonToken.PropertyName AndAlso CStr(jsonReader.Value) = "data" Then
                                                Await jsonReader.ReadAsync().ConfigureAwait(False)
                                                If jsonReader.TokenType = JsonToken.StartArray Then
                                                    While Await jsonReader.ReadAsync().ConfigureAwait(False) AndAlso jsonReader.TokenType = JsonToken.StartArray
                                                        Dim rowData As JArray = CType(Await JArray.LoadAsync(jsonReader).ConfigureAwait(False), JArray)
                                                        importer.StartRow()
                                                        For i As Integer = 0 To lcols.Count - 1
                                                            If i < rowData.Count Then
                                                                Dim value = If(rowData(i).Type = JTokenType.Null, DBNull.Value, rowData(i).ToObject(Of Object)())
                                                                Await importer.WriteAsync(value.ToString, NpgsqlTypes.NpgsqlDbType.Varchar).ConfigureAwait(False)
                                                            Else
                                                                Await importer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Varchar).ConfigureAwait(False)
                                                            End If
                                                        Next
                                                    End While
                                                End If
                                                Exit While
                                            End If
                                        End While
                                    End If
                                End Using
                            End Using
                        End Using
                    End Using
                    Await importer.CompleteAsync().ConfigureAwait(False)
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Function GetFirstDataAsString(json As String) As String
        Dim root As JObject = JObject.Parse(json)
        Dim dataRows As JArray = CType(root("data"), JArray)

        If dataRows Is Nothing OrElse dataRows.Count = 0 Then
            Return String.Empty
        End If

        Return dataRows(0).ToString()
    End Function

    Public Function BuildDataTableFromJson(json As String) As DataTable
        Dim dt As New DataTable()
        Dim root As JObject = JObject.Parse(json)

        Dim columnDefs As JArray = CType(root("columnDefinitions"), JArray)
        Dim dataRows As JArray = CType(root("data"), JArray)

        ' 1. Crear columnas según columnDefinitions
        For Each colDef As JObject In columnDefs
            Dim colName As String = colDef("name").ToString()
            Dim colType As String = colDef("type").ToString()
            Dim nullable As Boolean = colDef("nullable").ToObject(Of Boolean)()

            Dim dotNetType As Type = GetTypeFromICMType(colType)

            Dim col As New DataColumn(colName, dotNetType)
            col.AllowDBNull = nullable

            dt.Columns.Add(col)
        Next

        ' 2. Agregar filas desde data
        For Each rowArray As JArray In dataRows
            Dim dr As DataRow = dt.NewRow()

            For i As Integer = 0 To dt.Columns.Count - 1
                Dim token As JToken = rowArray(i)
                Dim col As DataColumn = dt.Columns(i)

                If token Is Nothing OrElse token.Type = JTokenType.Null Then
                    dr(i) = DBNull.Value
                Else
                    dr(i) = token.ToObject(col.DataType)
                End If
            Next

            dt.Rows.Add(dr)
        Next

        Return dt
    End Function

    '''<summary>
    '''Convierte un nombre de tipo retornado por Varicent ICM a un tipo de datos de .NET
    '''</summary>
    '''<returns>Un tipo de dato compatible con .NET</returns>
    '''<remarks>
    '''Esta función actúa como diccionario para las peticiones de Varicent ICM.
    '''</remarks>
    Private Function GetTypeFromICMType(ByVal icmType As String) As Type
        Select Case icmType.ToLowerInvariant()
            Case "int", "integer", "bigint", "smallint", "tinyint"
                Return GetType(Integer)
            Case "bit", "boolean"
                Return GetType(Boolean)
            Case "double", "float", "numeric", "money", "smallmoney"
                Return GetType(Double)
            Case "string", "text", "char", "varchar", "nvarchar", "text", "nchar", "ntext"
                Return GetType(String)
            Case "date", "datetime", "datetime2", "smalldatetime"
                Return GetType(DateTime)
            Case Else
                ' Por defecto, se manejará cualquier otro tipo como String
                Return GetType(String)
        End Select
    End Function

End Class
