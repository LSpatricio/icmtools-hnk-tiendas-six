Imports System.Diagnostics.Eventing
Imports System.DirectoryServices.ActiveDirectory
Imports System.IO
Imports System.Reflection
Imports ExcelDataReader
Imports SixLabors.Fonts.Tables.General


Public Class EficienciaEfectividadExcelReader


    Public Function ValiacionesEficienciaEfectividad(
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

                'moverse a la hoja pedida
                Do
                    If String.Equals(
                    reader.Name,
                    nombreHoja,
                    StringComparison.OrdinalIgnoreCase) Then

                        Exit Do

                    End If


                Loop While reader.NextResult()

                'moverse al ennabezado
                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim encabezados As New List(Of String)

                'obtener encebezados 
                For i As Integer = 0 To mapeoColumnas.Count - 1

                    Dim headerName As String =
            If(reader.GetValue(i)?.ToString(), "").Trim().Replace(vbCrLf, vbLf)

                    If String.IsNullOrWhiteSpace(headerName) Then
                        headerName = i.ToString()
                    End If

                    encabezados.Add(headerName)

                Next




                conteoFilas = conteoFilas + 1
                'Moverse a la fila despues del encabezado
                reader.Read()

                Do
                    'validación para ver si la fila esta vacia, si lo esta no se valida y se considera fin de la informacion.
                    Dim tieneInformacion As Boolean = False

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

                        ' VALIDACIÓN DE FILA:
                        ' Existe al menos un valor en esta fila????????????????????????
                        If valor IsNot Nothing AndAlso
           valor IsNot DBNull.Value AndAlso
           Not String.IsNullOrWhiteSpace(valor.ToString()) Then

                            tieneInformacion = True
                            Exit For

                        End If

                    Next


                    ' Si ninguna de las columnas tiene información,
                    ' ya terminó la información del Excel.
                    If Not tieneInformacion Then
                        Exit Do
                    End If

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


                        ' VALIDACIÓN DEL CAMPO:
                        ' ¿Este campo específico puede estar vacío?

                        If valor Is Nothing OrElse
           valor Is DBNull.Value OrElse
           String.IsNullOrWhiteSpace(valor.ToString()) Then

                            If mapeo.Value.Required Then

                                ' Error: campo requerido

                            End If

                        Else

                            ' El campo tiene valor.
                            ' Aquí posteriormente:
                            ' - Validación de tipo
                            ' - Conversión
                            ' - etc.

                        End If

                    Next


                Loop While reader.Read()

                Return listVaciosError

            End Using
        End Using

    End Function

End Class
