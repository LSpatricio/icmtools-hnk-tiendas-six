Imports System.Data.SqlClient
Imports System.IO
Imports System.Threading.Tasks
Imports Dapper
Public Class Repository

    Private ReadOnly _connectionString As String

    Public Sub New(connectionString As String)
        _connectionString = connectionString
    End Sub


    Public Async Function EjecutarSPAsync(nombreSP As String, idcarga As Guid) As Task

        Using connection As New SqlConnection(_connectionString)

            Await connection.ExecuteAsync(
            nombreSP,
                New With {.IdCarga = idcarga},
            commandType:=CommandType.StoredProcedure,
            commandTimeout:=600
        )

        End Using

    End Function

    ' Dapper
    Public Async Function LimpiarStaging(
        nombreTabla As String
    ) As Task

        Using connection As New SqlConnection(_connectionString)

            Await connection.OpenAsync()

            Await connection.ExecuteAsync(
                $"TRUNCATE TABLE {nombreTabla}"
            )

        End Using

    End Function

    Public Async Function ValidarDuplicadosAsync(
    tablaStaging As String,
    tablaDestino As String
) As Task(Of List(Of ExcelValidationError))

        Using connection As New SqlConnection(_connectionString)

            Dim parametros = New With {
            .TablaStaging = tablaStaging,
            .TablaDestino = tablaDestino
        }

            Dim errores = Await connection.QueryAsync(Of ExcelValidationError)(
            "dbo.SP_VALIDAR_DUPLICADOS",
            parametros,
            commandType:=CommandType.StoredProcedure
        )

            Return errores.ToList()

        End Using

    End Function

    Public Async Function InsertarBatch(
        nombreTabla As String,
        dataTable As DataTable
    ) As Task

        Using connection As New SqlConnection(_connectionString)

            Await connection.OpenAsync()

            Using bulkCopy As New SqlBulkCopy(connection)

                bulkCopy.DestinationTableName = nombreTabla
                bulkCopy.BatchSize = 50000
                bulkCopy.BulkCopyTimeout = 600

                For Each columna As DataColumn In dataTable.Columns

                    bulkCopy.ColumnMappings.Add(
                        columna.ColumnName,
                        columna.ColumnName
                    )

                Next

                Await bulkCopy.WriteToServerAsync(dataTable)

            End Using

        End Using

    End Function

    Public Async Function GenerarCsvAsync(
    sql As String,
    rutaArchivo As String,
   idcarga As Guid
) As Task

        Using connection As New SqlConnection(_connectionString)

            Using reader = Await connection.ExecuteReaderAsync(
                sql,
                New With {.IdCarga = idcarga},
                commandTimeout:=600)

                Dim encoding As New UTF8Encoding(True)

                Using writer As New StreamWriter(
                    rutaArchivo,
                    append:=False,
                    encoding:=encoding
                )

                    ' Encabezados
                    Dim encabezados As New List(Of String)(reader.FieldCount)

                    For i As Integer = 0 To reader.FieldCount - 1
                        encabezados.Add(
                            EscaparCsv(reader.GetName(i))
                        )
                    Next

                    writer.WriteLine(String.Join(",", encabezados))

                    ' Datos
                    Dim valores As New List(Of String)(reader.FieldCount)

                    While Await reader.ReadAsync()

                        valores.Clear()

                        For i As Integer = 0 To reader.FieldCount - 1

                            If reader.IsDBNull(i) Then
                                valores.Add("")
                            Else
                                valores.Add(
                                    EscaparCsv(reader.GetValue(i).ToString())
                                )
                            End If

                        Next

                        writer.WriteLine(String.Join(",", valores))

                    End While

                End Using

            End Using

        End Using
    End Function



    Private Function EscaparCsv(valor As String) As String

        If String.IsNullOrEmpty(valor) Then
            Return ""
        End If

        If valor.Contains("""") Then
            valor = valor.Replace("""", """""")
        End If

        If valor.Contains(",") OrElse
           valor.Contains(vbCr) OrElse
           valor.Contains(vbLf) OrElse
           valor.Contains("""") Then

            Return $"""{valor}"""
        End If

        Return valor

    End Function

End Class