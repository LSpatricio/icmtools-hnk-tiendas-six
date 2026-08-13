Imports System.Diagnostics.Eventing
Imports System.DirectoryServices.ActiveDirectory
Imports System.IO
Imports System.Reflection
Imports ExcelDataReader
Imports Microsoft.Vbe.Interop
Imports SixLabors.Fonts.Tables.General


Public Class ExcelReader

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

                Dim indiceHoja As Integer

                If Integer.TryParse(hoja, indiceHoja) Then

                    Dim indiceActual As Integer = 0

                    Do

                        If indiceActual = indiceHoja Then
                            hoja = (indiceActual + 1).ToString()
                            Exit Do
                        End If

                        indiceActual += 1

                    Loop While reader.NextResult()

                Else

                    Do

                        If String.Equals(
            reader.Name,
            hoja,
            StringComparison.OrdinalIgnoreCase
        ) Then
                            Exit Do

                        End If

                    Loop While reader.NextResult()

                End If


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


                If encabezados.Count <> mapeoColumnas.Count Then
                    listHojasError.Add(New ExcelValidationError With {
                        .Problema = $"El número de columnas en la hoja <strong>{hoja}</strong> no coincide con el número de columnas esperadas.",
                        .Detalle = $"Columnas encontradas en el archivo Excel: {String.Join(", ", encabezados)}"
                    })

                    Return listHojasError

                End If

                For Each mapeo In mapeoColumnas

                    Dim existe As Boolean = False

                    Dim indiceColumna = mapeo.Value.ColumnIndex

                    Dim valor = reader.GetValue(indiceColumna)



                    If String.IsNullOrWhiteSpace(mapeo.Value.ColumnName) Then
                        Dim index As Integer = mapeo.Value.ColumnIndex

                        If index >= 0 AndAlso index < encabezados.Count Then
                            existe = String.Equals(
                                encabezados(index),
                                mapeo.Value.ColumnIndex.ToString().Trim().Replace(vbCrLf, vbLf),
                                StringComparison.OrdinalIgnoreCase
                            )
                            mensajeError = $"La columna #'{mapeo.Value.ColumnIndex + 1}' no existe en la hoja <strong>{hoja}</strong> ."

                        End If



                    Else
                        Dim encabezadoExcel = encabezados.ElementAtOrDefault(mapeo.Value.ColumnIndex)

                        If encabezadoExcel IsNot Nothing Then
                            existe = String.Equals(
                                encabezadoExcel,
                                mapeo.Value.ColumnName.Trim().Replace(vbCrLf, vbLf),
                                StringComparison.OrdinalIgnoreCase
                            )
                            mensajeError = $"La columna '{mapeo.Value.ColumnName}' no se encuentra en la hoja <strong>{hoja}</strong> o no está ubicada en la posición esperada (columna {mapeo.Value.ColumnIndex + 1})."
                        End If



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

    Public Function ValidacionesInformacion(
    reader As IExcelDataReader,
    filaEncabezado As Integer,
    nombreHoja As String,
    mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim listaError As New List(Of ExcelValidationError)
        Dim mensajeError As String = String.Empty
        Dim conteoFilas As Integer = filaEncabezado



        'moverse a la hoja pedida
        'Do
        '    If String.Equals(
        '            reader.Name,
        '            nombreHoja,
        '            StringComparison.OrdinalIgnoreCase) Then

        '        Exit Do

        '    End If


        'Loop While reader.NextResult()

        Dim indiceHoja As Integer

        If Integer.TryParse(nombreHoja, indiceHoja) Then

            Dim indiceActual As Integer = 0

            Do

                If indiceActual = indiceHoja Then
                    nombreHoja = (indiceActual + 1).ToString()
                    Exit Do
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

                    Exit Do

                End If

            Loop While reader.NextResult()

        End If

        'moverse al ennabezado
        For i As Integer = 1 To filaEncabezado
            reader.Read()
        Next

        'Moverse a la fila despues del encabezado
        reader.Read()

        Do
            conteoFilas += 1

            Dim tieneInformacion As Boolean = False

            For Each mapeo In mapeoColumnas

                Dim indiceColumna As Integer = mapeo.Value.ColumnIndex
                Dim valor = reader.GetValue(indiceColumna)

                If valor IsNot Nothing AndAlso
           valor IsNot DBNull.Value AndAlso
           Not String.IsNullOrWhiteSpace(valor.ToString()) Then

                    tieneInformacion = True
                    Exit For
                End If

            Next

            ' Si TODA la fila está vacía, terminamos
            If Not tieneInformacion Then
                Exit Do
            End If

            For Each mapeo In mapeoColumnas

                Dim indiceColumna As Integer = mapeo.Value.ColumnIndex

                Dim valor = reader.GetValue(indiceColumna)

                If valor IsNot Nothing AndAlso valor IsNot DBNull.Value AndAlso Not String.IsNullOrWhiteSpace(valor.ToString()) Then

                    Dim valorIgnorado = mapeo.Value.ValoresIgnorados.Any(Function(x) String.Equals(x, valor.ToString().Trim(), StringComparison.OrdinalIgnoreCase)
    )

                    If valorIgnorado Then
                        Exit For
                    Else
                        Dim tipoEsperado As Type = mapeo.Key.PropertyType

                        Dim tipoReal As Type = If(Nullable.GetUnderlyingType(tipoEsperado), tipoEsperado)
                        Dim excelservice As New ExcelService()
                        If Not excelservice.EsTipoValido(valor, tipoReal) Then
                            Dim descripcion = excelservice.ObtenerDescripcionTipo(tipoEsperado)

                            If Not String.IsNullOrWhiteSpace(mapeo.Value.ColumnName) Then
                                mensajeError = $"La columna '{mapeo.Value.ColumnName}' requiere {descripcion}."
                            Else
                                mensajeError = $"La columna #{mapeo.Value.ColumnIndex + 1} requiere {descripcion}."
                            End If
                            listaError.Add(
                                        New ExcelValidationError With {
                                            .Problema = mensajeError,
                                            .Detalle = $"Valor:'{valor}'. Fila {conteoFilas}. Hoja <strong>{nombreHoja}</strong>."
                                        })

                        End If



                    End If

                ElseIf mapeo.Value.Requerido Then


                    If Not String.IsNullOrWhiteSpace(mapeo.Value.ColumnName) Then
                        mensajeError =
            $"La columna '{mapeo.Value.ColumnName}' no admite valores vacíos."
                    Else
                        mensajeError =
            $"La columna #{mapeo.Value.ColumnIndex + 1} no admite valores vacíos."
                    End If

                    listaError.Add(
        New ExcelValidationError With {
            .Problema = mensajeError,
            .Detalle = $"Columna sin información en la fila {conteoFilas}. Hoja <strong>{nombreHoja}</strong>."
        })

                End If


            Next


        Loop While reader.Read()

        Return listaError


    End Function


End Class
