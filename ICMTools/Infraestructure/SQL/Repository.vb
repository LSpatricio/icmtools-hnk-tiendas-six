Imports System.Data.SqlClient
Imports System.IO
Imports System.Threading.Tasks
Imports Dapper
Public Class Repository

    Private ReadOnly _connectionString As String

    Public Sub New(connectionString As String)
        _connectionString = connectionString
    End Sub

    ' Dapper
    Public Async Function Query(Of T)(
        sql As String,
        Optional parametros As Object = Nothing
    ) As Task(Of IEnumerable(Of T))

        Using connection As New SqlConnection(_connectionString)

            Await connection.OpenAsync()

            Return Await connection.QueryAsync(Of T)(
                sql,
                parametros
            )

        End Using

    End Function

    Public Async Function EjecutarSPAsync(nombreSP As String, idcarga As Guid) As Task

        Using connection As New SqlConnection(_connectionString)

            Await connection.ExecuteAsync(
            nombreSP,
                New With {.IdCarga = idcarga},
            commandType:=CommandType.StoredProcedure
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

    Public Async Function InsertarBatch(
        nombreTabla As String,
        dataTable As DataTable
    ) As Task

        Using connection As New SqlConnection(_connectionString)

            Await connection.OpenAsync()

            Using bulkCopy As New SqlBulkCopy(connection)

                bulkCopy.DestinationTableName = nombreTabla
                bulkCopy.BatchSize = 50000

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
    Optional parametros As Object = Nothing
) As Task

        Using connection As New SqlConnection(_connectionString)

            Using reader = Await connection.ExecuteReaderAsync(
                sql,
                parametros
            )

                Dim encoding As New UTF8Encoding(True)

                Using writer As New StreamWriter(
                    rutaArchivo,
                    append:=False,
                    encoding:=encoding
                )

                    ' Encabezados
                    For i As Integer = 0 To reader.FieldCount - 1

                        If i > 0 Then
                            Await writer.WriteAsync(",")
                        End If

                        Await writer.WriteAsync(
                            EscaparCsv(reader.GetName(i))
                        )

                    Next

                    Await writer.WriteLineAsync()

                    ' Datos
                    While Await reader.ReadAsync()

                        For i As Integer = 0 To reader.FieldCount - 1

                            If i > 0 Then
                                Await writer.WriteAsync(",")
                            End If

                            If Not reader.IsDBNull(i) Then

                                Dim valor As String =
                                    reader.GetValue(i).ToString()

                                Await writer.WriteAsync(
                                    EscaparCsv(valor)
                                )

                            End If

                        Next

                        Await writer.WriteLineAsync()

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