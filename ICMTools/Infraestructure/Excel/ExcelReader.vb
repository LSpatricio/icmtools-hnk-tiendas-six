Imports System.Diagnostics.Eventing
Imports System.DirectoryServices.ActiveDirectory
Imports System.IO
Imports System.Reflection
Imports ExcelDataReader
Imports SixLabors.Fonts.Tables.General


Public Class ExcelReader

    Public Function LeerExcel(Of T As New)(rutaArchivo As String, filaEncabezado As Integer) As List(Of T)

        Dim lista As New List(Of T)

        Using stream = File.Open(
            rutaArchivo,
            FileMode.Open,
            FileAccess.Read
        )

            Using reader = ExcelReaderFactory.CreateReader(stream)

                ' ==========================================
                ' LEER ENCABEZADOS
                ' ==========================================

                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim columnas As New Dictionary(Of String, Integer)

                For i As Integer = 0 To reader.FieldCount - 1

                    Dim nombreColumna As String =
                        reader.GetValue(i).ToString()

                    columnas(nombreColumna) = i

                Next


                ' ==========================================
                ' CONSTRUIR MAPPING
                ' ==========================================

                Dim mappings As New List(Of ExcelPropertyMap)

                Dim propiedades =
                    GetType(T).GetProperties()

                For Each propiedad In propiedades

                    Dim atributo = propiedad.GetCustomAttributes(
                        GetType(ExcelColumnAttribute),
                        False
                    ).FirstOrDefault()

                    If atributo Is Nothing Then
                        Continue For
                    End If

                    Dim excelColumn =
                        DirectCast(
                            atributo,
                            ExcelColumnAttribute
                        )

                    Dim indiceColumna =
                        columnas(excelColumn.ColumnName)

                    mappings.Add(
                        New ExcelPropertyMap With {
                            .Property = propiedad,
                            .ColumnIndex = indiceColumna
                        }
                    )

                Next


                ' ==========================================
                ' LEER FILAS
                ' ==========================================

                While reader.Read()

                    Dim classObject As New T()

                    For Each mapping In mappings

                        Dim valor =
                            reader.GetValue(mapping.ColumnIndex)

                        mapping.Property.SetValue(
                            classObject,
                            valor
                        )

                    Next

                    lista.Add(classObject)

                End While

            End Using

        End Using

        Return lista

    End Function

    Public Function ValidarEncabezadosExcel(
    fileType As Type,
    rutaArchivo As String,
    filaEncabezado As Integer,
    hoja As Integer,
    mapeoColumnas As Dictionary(Of String, ExcelColumnAttribute)
) As Boolean

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)

                ' Ir a la hoja indicada
                For i As Integer = 0 To hoja - 1
                    If Not reader.NextResult() Then
                        Return False
                    End If
                Next

                ' Ir a la fila de encabezados
                For i As Integer = 0 To filaEncabezado
                    reader.Read()
                Next

                ' Obtener encabezados del Excel
                Dim encabezados As New List(Of String)
                Dim atributos = fileType.
                GetProperties().
                SelectMany(Function(p) p.GetCustomAttributes(
                    GetType(ExcelColumnAttribute),
                    False
                )).
                Cast(Of ExcelColumnAttribute)().
                ToList()

                For i As Integer = 0 To mapeoColumnas.Count - 1

                    Dim headerName As String =
        If(reader.GetValue(i)?.ToString(), "").Trim()

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    encabezados.Add(headerName)

                Next


                For Each atributo In atributos

                    Dim existe = encabezados.Any(
                    Function(header) String.Equals(
                        header,
                        atributo.ColumnName.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )

                    If Not existe Then
                        Return False
                    End If

                Next




                Return True

            End Using
        End Using

    End Function


    Public Function ValidarEncabezadosExcel(
    fileType As Type,
    rutaArchivo As String,
    filaEncabezado As Integer,
    nombreHoja As String,
    mapeoColumnas As Dictionary(Of String, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim listHojasError As New List(Of ExcelValidationError)
        Dim mensajeError As String = String.Empty

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)

                Dim hojaEncontrada As Boolean = False



                Do
                    If String.Equals(
                    reader.Name,
                    nombreHoja,
                    StringComparison.OrdinalIgnoreCase
                ) Then

                        hojaEncontrada = True
                        Exit Do

                    End If

                Loop While reader.NextResult()


                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim encabezados As New List(Of String)

                For i As Integer = 0 To mapeoColumnas.Count - 1

                    Dim headerName As String =
            If(reader.GetValue(i)?.ToString(), "").Trim().Replace(vbCrLf, vbLf)

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    encabezados.Add(headerName)

                Next




                For Each mapeo In mapeoColumnas

                    Dim existe As Boolean = False

                    If mapeo.Value.ColumnIndex.HasValue Then
                        Dim index As Integer = mapeo.Value.ColumnIndex.Value

                        If index >= 0 AndAlso index < encabezados.Count Then
                            existe = String.Equals(
                                encabezados(index),
                                mapeo.Value.ColumnIndex.ToString().Trim().Replace(vbCrLf, vbLf),
                                StringComparison.OrdinalIgnoreCase
                            )
                            mensajeError = $"La columna #'{mapeo.Value.ColumnIndex.Value + 1}' no existe en la hoja <strong>{nombreHoja}</strong> ."

                        End If



                    Else

                        existe = encabezados.Any(
                    Function(encabezado) String.Equals(
                        encabezado,
                        mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf),
                        StringComparison.OrdinalIgnoreCase))

                        mensajeError = $"La columna '{mapeo.Value.ColumnName}' no existe en la hoja <strong>{nombreHoja}</strong> ."

                    End If


                    If Not existe Then
                        listHojasError.Add(New ExcelValidationError With {
                      .Problema = mensajeError,
                      .Detalle = $"Columnas encontradas en el archivo Excel: {String.Join(", ", encabezados)}"
                  })
                    End If

                Next

                Return listHojasError

            End Using
        End Using

    End Function


    Public Function ValidarTipoDato(
    fileType As Type,
    rutaArchivo As String,
    filaEncabezado As Integer,
    nombreHoja As String,
    mapeoColumnas As Dictionary(Of String, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim listHojasError As New List(Of ExcelValidationError)
        Dim mensajeError As String = String.Empty
        Dim indicesColumnas As New Dictionary(Of String, Integer)

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)

                Dim hojaEncontrada As Boolean = False



                Do
                    If String.Equals(
                    reader.Name,
                    nombreHoja,
                    StringComparison.OrdinalIgnoreCase
                ) Then

                        hojaEncontrada = True
                        Exit Do

                    End If

                Loop While reader.NextResult()


                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim encabezados As New List(Of String)

                For i As Integer = 0 To mapeoColumnas.Count - 1

                    Dim headerName As String =
            If(reader.GetValue(i)?.ToString(), "").Trim().Replace(vbCrLf, vbLf)

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    encabezados.Add(headerName)

                Next


                For Each mapeo In mapeoColumnas

                    Dim indiceColumna As Integer

                    If mapeo.Value.ColumnIndex.HasValue Then

                        indiceColumna = mapeo.Value.ColumnIndex.Value

                    Else

                        indiceColumna = encabezados.FindIndex(
            Function(encabezado) String.Equals(
                encabezado,
                mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf),
                StringComparison.OrdinalIgnoreCase
            )
        )

                    End If

                    indicesColumnas.Add(mapeo.Key, indiceColumna)

                Next

                reader.Read()

                Do

                    For Each mapeo In mapeoColumnas

                        Dim indiceColumna = indicesColumnas(mapeo.Key)

                        Dim valor = reader.GetValue(indiceColumna)

                        Dim tipoDato As Type = fileType.GetProperty(mapeo.Key).PropertyType
                        Dim datoRequerido As Boolean = mapeo.Value.Required


                        'TryConvertValue(valor, tipoDato, datoRequerido)
                        ' Aquí validas el tipo de dato

                    Next

                Loop While reader.Read()

                Return listHojasError

            End Using
        End Using

    End Function

    Public Function ValidarVacios(
    fileType As Type,
    rutaArchivo As String,
    filaEncabezado As Integer,
    nombreHoja As String,
    mapeoColumnas As Dictionary(Of String, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim listVaciosError As New List(Of ExcelValidationError)
        Dim mensajeError As String = String.Empty
        Dim indicesColumnas As New Dictionary(Of String, Integer)
        Dim conteoFilas As Integer = filaEncabezado

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)

                Dim hojaEncontrada As Boolean = False

                Do
                    If String.Equals(
                    reader.Name,
                    nombreHoja,
                    StringComparison.OrdinalIgnoreCase
                ) Then

                        hojaEncontrada = True
                        Exit Do

                    End If

                Loop While reader.NextResult()


                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim encabezados As New List(Of String)

                For i As Integer = 0 To mapeoColumnas.Count - 1

                    Dim headerName As String =
            If(reader.GetValue(i)?.ToString(), "").Trim().Replace(vbCrLf, vbLf)

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    encabezados.Add(headerName)

                Next


                For Each mapeo In mapeoColumnas

                    Dim indiceColumna As Integer

                    If mapeo.Value.ColumnIndex.HasValue Then

                        indiceColumna = mapeo.Value.ColumnIndex.Value

                    Else

                        indiceColumna = encabezados.FindIndex(
            Function(encabezado) String.Equals(
                encabezado,
                mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf),
                StringComparison.OrdinalIgnoreCase
            )
        )

                    End If

                    indicesColumnas.Add(mapeo.Key, indiceColumna)

                Next

                conteoFilas = conteoFilas + 1

                reader.Read()

                Do



                    Dim filaVacia As Boolean = True

                    For Each mapeo In mapeoColumnas

                        Dim indiceColumna As Integer

                        If mapeo.Value.ColumnIndex.HasValue Then

                            indiceColumna = mapeo.Value.ColumnIndex.Value

                        Else

                            indiceColumna = encabezados.FindIndex(
                Function(encabezado) String.Equals(
                    encabezado,
                    mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf),
                    StringComparison.OrdinalIgnoreCase
                )
            )

                        End If

                        Dim valorFila = reader.GetValue(indiceColumna)

                        If valorFila IsNot Nothing AndAlso
           valorFila IsNot DBNull.Value AndAlso
           Not String.IsNullOrWhiteSpace(valorFila.ToString()) Then

                            filaVacia = False
                            Exit For

                        End If

                    Next


                    If Not filaVacia Then

                        conteoFilas += 1


                        For Each mapeo In mapeoColumnas

                            Dim indiceColumna As Integer

                            If mapeo.Value.ColumnIndex.HasValue Then

                                indiceColumna = mapeo.Value.ColumnIndex.Value

                            Else

                                indiceColumna = encabezados.FindIndex(
                    Function(encabezado) String.Equals(
                        encabezado,
                        mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf),
                        StringComparison.OrdinalIgnoreCase
                    )
                )

                            End If


                            Dim valor = reader.GetValue(indiceColumna)

                            If valor Is Nothing OrElse
               valor Is DBNull.Value OrElse
               String.IsNullOrWhiteSpace(valor.ToString()) Then

                                If mapeo.Value.Required Then

                                    If mapeo.Value.ColumnIndex.HasValue Then

                                        mensajeError =
                            $"La columna #'{mapeo.Value.ColumnIndex.Value + 1}' no admite valores vacios."

                                    Else

                                        mensajeError =
                            $"La columna '{mapeo.Value.ColumnName}' no admite valores vacios."

                                    End If


                                    listVaciosError.Add(
                        New ExcelValidationError With {
                            .Problema = mensajeError,
                            .Detalle = $"Columna sin información en la fila {conteoFilas}"
                        }
                    )

                                End If



                            End If

                        Next

                    End If


                Loop While reader.Read()

                Return listVaciosError

            End Using
        End Using

    End Function

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

                ' Obtener las hojas definidas por los atributos
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

    Public Function ObtenerFilas(rutaArchivo As String) As List(Of Object)
        Dim filas As New List(Of Object)
        Using stream = File.Open(
            rutaArchivo,
            FileMode.Open,
            FileAccess.Read
        )
            Using reader = ExcelReaderFactory.CreateReader(stream)
                While reader.Read()
                    Dim fila As New List(Of Object)
                    For i As Integer = 0 To reader.FieldCount - 1
                        fila.Add(reader.GetValue(i))
                    Next
                    filas.Add(fila)
                End While
            End Using
        End Using
        Return filas



    End Function

    Public Function ObtenerFilas(
    fileType As Type,
    rutaArchivo As String,
    filaEncabezado As Integer,
    hoja As Integer
) As Boolean

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)

                ' Ir a la hoja indicada
                For i As Integer = 0 To hoja - 1
                    If Not reader.NextResult() Then
                        Return False
                    End If
                Next

                ' Ir a la fila de encabezados
                For i As Integer = 0 To filaEncabezado
                    reader.Read()
                Next


                ' Obtener encabezados del Excel
                Dim encabezados As New List(Of String)

                For i As Integer = 0 To reader.FieldCount - 1

                    Dim headerName As String =
                    If(reader.GetValue(i)?.ToString(), "").Trim()

                    If Not String.IsNullOrWhiteSpace(headerName) Then
                        encabezados.Add(headerName)
                    End If

                Next

                Dim atributos = fileType.GetProperties().SelectMany(Function(p) p.GetCustomAttributes(GetType(ExcelColumnAttribute), False)).Cast(Of ExcelColumnAttribute).ToList()

                For Each atributo In atributos

                    Dim existe = encabezados.Any(
                    Function(header) String.Equals(
                        header,
                        atributo.ColumnName.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )

                    If Not existe Then
                        Return False
                    End If

                Next




                Return True

            End Using
        End Using

    End Function



    Public Function CrearMepeoAtributos(tipo As Type) As Dictionary(Of String, ExcelColumnAttribute)

        Dim mappings As New Dictionary(Of String, ExcelColumnAttribute)

        Dim propiedades = tipo.GetProperties()

        For Each propiedad In propiedades

            Dim atributo = propiedad.GetCustomAttributes(
                GetType(ExcelColumnAttribute),
                False
            ).FirstOrDefault()

            If atributo Is Nothing Then
                Continue For
            End If

            Dim excelColumn =
                DirectCast(
                    atributo,
                    ExcelColumnAttribute
                )

            mappings(propiedad.Name) = excelColumn


        Next

        Return mappings

    End Function

    '    Private Function TryConvertValue(
    '    valor As Object,
    '    tipoDestino As Type,
    '    datoRequerido As Boolean
    ') As Boolean

    '        valorConvertido = Nothing

    '        Try

    '            ' Ya es del tipo esperado
    '            If tipoDestino.IsInstanceOfType(valor) Then
    '                valorConvertido = valor
    '                Return True
    '            End If

    '            ' String
    '            If tipoDestino Is GetType(String) Then
    '                valorConvertido = valor.ToString()
    '                Return True
    '            End If

    '            ' Integer
    '            If tipoDestino Is GetType(Integer) Then
    '                Dim resultado As Integer

    '                If Integer.TryParse(valor.ToString(), resultado) Then
    '                    valorConvertido = resultado
    '                    Return True
    '                End If

    '                Return False
    '            End If

    '            ' Decimal
    '            If tipoDestino Is GetType(Decimal) Then
    '                Dim resultado As Decimal

    '                If Decimal.TryParse(valor.ToString(), resultado) Then
    '                    valorConvertido = resultado
    '                    Return True
    '                End If

    '                Return False
    '            End If

    '            ' Double
    '            If tipoDestino Is GetType(Double) Then
    '                Dim resultado As Double

    '                If Double.TryParse(valor.ToString(), resultado) Then
    '                    valorConvertido = resultado
    '                    Return True
    '                End If

    '                Return False
    '            End If

    '            ' DateTime
    '            If tipoDestino Is GetType(DateTime) Then
    '                Dim resultado As DateTime

    '                If DateTime.TryParse(valor.ToString(), resultado) Then
    '                    valorConvertido = resultado
    '                    Return True
    '                End If

    '                Return False
    '            End If

    '            ' Boolean
    '            If tipoDestino Is GetType(Boolean) Then
    '                Dim resultado As Boolean

    '                If Boolean.TryParse(valor.ToString(), resultado) Then
    '                    valorConvertido = resultado
    '                    Return True
    '                End If

    '                Return False
    '            End If

    '            ' Otros tipos
    '            valorConvertido = Convert.ChangeType(valor, tipoDestino)
    '            Return True

    '        Catch
    '            valorConvertido = Nothing
    '            Return False
    '        End Try

    '    End Function

End Class
