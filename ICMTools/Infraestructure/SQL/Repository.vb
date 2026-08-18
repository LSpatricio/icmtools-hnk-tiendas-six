Imports System.Data.SqlClient
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

    Public Async Function EjecutarSPAsync(nombreSP As String) As Task

        Using connection As New SqlConnection(_connectionString)

            Await connection.ExecuteAsync(
            nombreSP,
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

End Class