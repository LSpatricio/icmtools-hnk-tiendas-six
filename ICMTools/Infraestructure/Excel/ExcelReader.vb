Imports System.Diagnostics.Eventing
Imports System.DirectoryServices.ActiveDirectory
Imports System.IO
Imports System.Reflection
Imports System.Threading.Tasks
Imports ExcelDataReader
Imports Microsoft.Vbe.Interop
Imports SixLabors.Fonts.Tables.General


Public Class ExcelReader

    Public ReadOnly _excelService As ExcelService
    Public ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _catalogoService As CatalogoService
    Private mUser As User


    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _catalogoService = New CatalogoService()

    End Sub

    Public Function ValidarHojasDefinidas(
    fileType As Type,
    rutaArchivo As String
) As List(Of ExcelValidationError)

        Dim listHojasError As New List(Of ExcelValidationError)

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read)

            Using reader = ExcelReaderFactory.CreateReader(stream)

                '  hojas definidas por los atributos
                Dim hojasEsperadas = fileType.GetProperties().
                Select(Function(p) p.GetCustomAttribute(Of ExcelSheetAttribute)()).
                Where(Function(a) a IsNot Nothing).
                ToList()

                ' Obtener todas las hojas existentes en el Excel
                Dim hojasExcel As New HashSet(Of String)(
                StringComparer.OrdinalIgnoreCase)

                Do
                    hojasExcel.Add(reader.Name)
                Loop While reader.NextResult()

                ' Validar que cada hoja esperada exista en el Excel
                For Each hoja In hojasEsperadas

                    If Not hojasExcel.Contains(hoja.SheetName) Then

                        listHojasError.Add(New ExcelValidationError With {
                        .Problema = $"La hoja '{hoja.SheetName}' no existe en el archivo Excel.",
                        .Detalle = $"Hojas encontradas en el archivo Excel: {String.Join(", ", hojasExcel)}"
                    })

                    End If

                Next

            End Using
        End Using

        Return listHojasError

    End Function



    Public Function ValidarEncabezadosExcel(
    fileType As Type,
    rutaArchivo As String,
    filaEncabezado As Integer,
    hoja As String,
    mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim listHojasError As New List(Of ExcelValidationError)
        Dim mensajeError As String = String.Empty

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)
                hoja = MoverAHoja(reader, hoja)

                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                'Dim encabezados As New List(Of String)
                Dim encabezados As New HashSet(Of String)(
                StringComparer.OrdinalIgnoreCase)

                For i As Integer = 0 To reader.FieldCount - 1

                    Dim headerName As String = If(reader.GetValue(i)?.ToString(), "").Trim().Replace(vbCrLf, vbLf)

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    encabezados.Add(NormalizarTextoComparacion(headerName))

                Next

                For Each mapeo In mapeoColumnas
                    If Not String.IsNullOrWhiteSpace(mapeo.Value.ColumnName) Then
                        If Not encabezados.Contains(NormalizarTextoComparacion(mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf))) Then
                            mensajeError = $"La columna '{mapeo.Value.ColumnName}' no se encuentra en la hoja <strong>{hoja}</strong>."
                            listHojasError.Add(New ExcelValidationError With {
                                              .Problema = mensajeError,
                                              .Detalle = $"Columnas encontradas en el archivo Excel: {String.Join(", ", encabezados)}"
                                          })
                        End If
                    End If

                Next

                Return listHojasError

            End Using
        End Using
    End Function

    Public Function ContarHojas(rutaArchivo As String) As Integer
        Dim stream = File.Open(
            rutaArchivo,
            FileMode.Open,
            FileAccess.Read
        )
        Using reader = ExcelReaderFactory.CreateReader(stream)
            Return reader.ResultsCount
        End Using

    End Function

    Public Async Function CargaAsync(
    rutaArchivo As String,
    filaEncabezado As Integer,
    nombreHoja As String,
    mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute),
    tablaStaging As String,
    Optional regionSelector As String = Nothing,
    Optional catalogos As CatalogosDto = Nothing,
    Optional validacionEspecifica As Func(Of DataRow, CatalogosDto, ExcelValidationError) = Nothing) As Task(Of List(Of ExcelValidationError))
        'DataRow, lo que mandamos, string lo que regresamos
        Dim dt As DataTable = _excelService.CrearDataTable(mapeoColumnas)
        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)
                Dim listaError As New List(Of ExcelValidationError)
                Dim mensajeError As String = String.Empty
                Dim conteoFilas As Integer = filaEncabezado
                Dim erroresCantidad As Integer = 0
                Dim claveError As String = String.Empty

                nombreHoja = MoverAHoja(reader, nombreHoja)

                'moverse al ennabezado
                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim encabezados As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

                For i As Integer = 0 To reader.FieldCount - 1

                    Dim headerName As String = If(reader.GetValue(i)?.ToString(), "").Trim().Replace(vbCrLf, vbLf)
                    Dim nombreNormalizado As String = NormalizarTextoComparacion(headerName)

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    If Not encabezados.ContainsKey(nombreNormalizado) Then
                        encabezados.Add(nombreNormalizado, i)
                    End If

                Next

                For Each mapeo In mapeoColumnas
                    Dim nombreMapeo As String = NormalizarTextoComparacion(mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf))
                    If Not String.IsNullOrWhiteSpace(nombreMapeo) Then
                        If Not encabezados.Keys.Contains(nombreMapeo) Then
                            Return New List(Of ExcelValidationError)()
                        End If
                    End If



                    If Not String.IsNullOrWhiteSpace(nombreMapeo) Then

                        Dim indice As Integer

                        If encabezados.TryGetValue(nombreMapeo, indice) Then
                            mapeo.Value.ColumnIndex = indice
                        End If

                    End If

                Next

                'Moverse a la fila despues del encabezado
                reader.Read()

                Dim batchSize As Integer = 50000
                Dim erroresAgrupables As New Dictionary(Of String, Integer)()
                Dim erroresAgrupablesRuta As New Dictionary(Of String, Integer)()

                Do
                    conteoFilas += 1

                    Dim tieneInformacion As Boolean = False

                    For Each mapeo In mapeoColumnas

                        Dim indiceColumna As Integer = mapeo.Value.ColumnIndex
                        Dim valor = reader.GetValue(indiceColumna)

                        If valor IsNot Nothing AndAlso valor IsNot DBNull.Value AndAlso Not String.IsNullOrWhiteSpace(valor.ToString()) Then
                            tieneInformacion = True
                            Exit For
                        End If

                    Next

                    ' Si TODA la fila está vacía, terminamos
                    If Not tieneInformacion Then
                        Exit Do
                    End If

                    Dim fila As DataRow = dt.NewRow()
                    Dim filaValida As Boolean = True


                    For Each mapeo In mapeoColumnas

                        Dim indiceColumna As Integer = mapeo.Value.ColumnIndex

                        Dim valor = reader.GetValue(indiceColumna)

                        If valor IsNot Nothing AndAlso valor IsNot DBNull.Value AndAlso Not String.IsNullOrWhiteSpace(valor.ToString()) Then

                            Dim valorIgnorado = mapeo.Value.ValoresIgnorados.Any(Function(x) String.Equals(x, valor.ToString().Trim(), StringComparison.OrdinalIgnoreCase))

                            If valorIgnorado Then
                                filaValida = False
                                Exit For
                            End If

                            Dim tipoEsperado As Type = mapeo.Key.PropertyType
                            Dim tipoReal As Type = If(Nullable.GetUnderlyingType(tipoEsperado), tipoEsperado)

                            If Not _excelService.EsTipoValido(valor, tipoReal) Then
                                filaValida = False
                                erroresCantidad += 1

                                claveError = "Tipo de dato no válido"
                                If erroresAgrupables.ContainsKey(claveError) Then
                                    erroresAgrupables(claveError) += 1
                                Else
                                    erroresAgrupables.Add(claveError, 1)
                                End If

                                If erroresCantidad <= 200 Then
                                    Dim descripcion = _excelService.ObtenerDescripcionTipo(tipoEsperado)
                                    mensajeError = $"La columna '{mapeo.Value.ColumnName}' requiere {descripcion}."

                                    listaError.Add(New ExcelValidationError With {
                                        .Problema = mensajeError,
                                        .Detalle = $"Valor:'{valor}'. Fila {conteoFilas}. Hoja <strong>{nombreHoja}</strong>."
                                    })
                                End If

                            Else
                                fila(mapeo.Key.Name) = valor

                            End If

                        ElseIf mapeo.Value.Requerido Then

                            filaValida = False
                            erroresCantidad += 1

                            claveError = "No se admiten valores vacío"
                            If erroresAgrupables.ContainsKey(claveError) Then
                                erroresAgrupables(claveError) += 1
                            Else
                                erroresAgrupables.Add(claveError, 1)
                            End If

                            If erroresCantidad <= 200 Then
                                mensajeError = $"La columna '{mapeo.Value.ColumnName}' no admite valores vacíos."

                                listaError.Add(New ExcelValidationError With {
                                    .Problema = mensajeError,
                                .Detalle = $"Columna sin información en la fila {conteoFilas}. Hoja <strong>{nombreHoja}</strong>."
                                })
                            End If
                        Else
                            fila(mapeo.Key.Name) = DBNull.Value
                        End If
                    Next

                    If regionSelector IsNot Nothing Then

                        Dim regionFila As String = fila.Field(Of String)("Region") 'Nombre de la fila en el datatable, mapeado desde la clase...

                        If Not String.Equals(regionSelector, "Todas", StringComparison.OrdinalIgnoreCase) Then

                            If Not String.Equals(regionFila, regionSelector, StringComparison.OrdinalIgnoreCase) Then
                                claveError = $"El registro no corresponde a la región seleccionada: {regionSelector}."
                                filaValida = False
                                If erroresAgrupablesRuta.ContainsKey(claveError) Then
                                    erroresAgrupablesRuta(claveError) += 1
                                Else
                                    erroresAgrupablesRuta.Add(claveError, 1)
                                End If


                            End If

                        Else
                            If Not catalogos.Regiones.Contains(regionFila) Then
                                filaValida = False
                                erroresCantidad += 1
                                claveError = $"Filas con regiones que no pertenecen al catálogo de regiones válido."
                                If erroresAgrupables.ContainsKey(claveError) Then
                                    erroresAgrupables(claveError) += 1
                                Else
                                    erroresAgrupables.Add(claveError, 1)
                                End If

                                If erroresCantidad <= 200 Then
                                    listaError.Add(New ExcelValidationError With {
                                    .Problema = $"La región {regionFila} no pertenece al catálogo de regiones válido.",
                                .Detalle = $"Fila {conteoFilas}. Hoja <strong>{nombreHoja}</strong>."
                                })
                                End If
                            End If

                        End If

                    End If

                    If validacionEspecifica IsNot Nothing AndAlso filaValida Then

                        Dim resultadoValidacion = validacionEspecifica(fila, catalogos)

                        If resultadoValidacion IsNot Nothing Then
                            With resultadoValidacion
                                .Detalle = $"Fila {conteoFilas}. Hoja <strong>{nombreHoja}</strong>."
                                .Advertencia = True
                            End With

                            listaError.Add(resultadoValidacion)

                        End If

                    End If

                    If filaValida Then
                        dt.Rows.Add(fila)
                    End If

                    If dt.Rows.Count >= 50000 Then
                        Await _repository.InsertarBatch(tablaStaging, dt)
                        dt.Clear()
                    End If

                Loop While reader.Read()

                If dt.Rows.Count > 0 Then
                    Await _repository.InsertarBatch(tablaStaging, dt)
                    dt.Clear()
                End If

                If erroresCantidad > 200 Then
                    listaError.Clear()

                    For Each errorAgrupado In erroresAgrupables
                        listaError.Add(New ExcelValidationError With {
                        .Problema = errorAgrupado.Key,
                        .Detalle = $"{errorAgrupado.Value} registros presentan este problema en total."
                    })
                    Next
                End If

                For Each errorAgrupado In erroresAgrupablesRuta
                    listaError.Add(New ExcelValidationError With {
                    .Problema = errorAgrupado.Key,
                    .Detalle = $"{errorAgrupado.Value} registros presentan este problema en total."
                })
                Next


                Return listaError

            End Using
        End Using

    End Function

    Private Function NormalizarTextoComparacion(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim textoNormalizado = valor.Trim().Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        textoNormalizado = textoNormalizado.Normalize(System.Text.NormalizationForm.FormD)

        Dim caracteres = textoNormalizado.
            Where(Function(c) System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) <> System.Globalization.UnicodeCategory.NonSpacingMark).
            ToArray()

        Return New String(caracteres).Normalize(System.Text.NormalizationForm.FormC)
    End Function

    Private Function MoverAHoja(
    reader As IExcelDataReader,
    nombreHoja As String
) As String

        Dim indiceHoja As Integer

        If Integer.TryParse(nombreHoja, indiceHoja) Then

            Dim indiceActual As Integer = 0

            Do

                If indiceActual = indiceHoja Then
                    Return (indiceActual + 1).ToString()
                End If

                indiceActual += 1

            Loop While reader.NextResult()

        Else

            Do

                If String.Equals(
                    reader.Name,
                    nombreHoja,
                    StringComparison.OrdinalIgnoreCase
                ) Then
                    Return nombreHoja
                End If

            Loop While reader.NextResult()

        End If

        Return nombreHoja

    End Function


End Class
