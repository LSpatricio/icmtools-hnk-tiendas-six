Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports Newtonsoft.Json


Public Class DAO_SQL


    Private Shared ReadOnly semaphore As SemaphoreSlim = New SemaphoreSlim(5)
    Private cadenaConexion As String = Environment.GetEnvironmentVariable("SqlConnectionString")

    Public Async Function Insert(Of T As Class)(ByVal query As String, ByVal requestBody As String) As Task(Of Integer)
        Dim r As Integer = 0

        Try
            Dim datosBody As T = JsonConvert.DeserializeObject(Of T)(requestBody)

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Using cmd As SqlCommand = New SqlCommand(query, conn)
                    Dim propierties = GetType(T).GetProperties()

                    For Each [property] In propierties
                        Dim sqlParameter As SqlParameter = New SqlParameter($"@{[property].Name}", [property].GetValue(datosBody))
                        cmd.Parameters.Add(sqlParameter)
                    Next

                    r = Await cmd.ExecuteNonQueryAsync()
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        End Try

        Return r
    End Function

    Public Async Function bulkInsert(ByVal dataTable As DataTable, ByVal nombreTabla As String) As Task(Of String)
        Try

            If dataTable.Rows.Count = 0 Then
                Return "Sin datos por insertar"
            Else

                Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                    Await conn.OpenAsync()

                    Using bulkCopy As SqlBulkCopy = New SqlBulkCopy(conn)
                        bulkCopy.DestinationTableName = nombreTabla
                        Await bulkCopy.WriteToServerAsync(dataTable)
                    End Using
                End Using

                Return "Inserción completada correctamente"
            End If

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function bulkInserWithtDelete(ByVal dataTable As DataTable, ByVal nombreTabla As String) As Task(Of String)
        Try

            If dataTable.Rows.Count = 0 Then
                Return "Sin datos por insertar"
            Else
                Dim r As Integer = Await deleteAll(nombreTabla)

                Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                    Await conn.OpenAsync()

                    Using bulkCopy As SqlBulkCopy = New SqlBulkCopy(conn)
                        bulkCopy.DestinationTableName = nombreTabla
                        Await bulkCopy.WriteToServerAsync(dataTable)
                    End Using
                End Using

                Return "Inserción completada correctamente"
            End If

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function bulkInserWithtDelete(ByVal dataTable As DataTable, ByVal nombreTabla As String, ByVal parametros As String) As Task(Of String)
        Try

            If dataTable.Rows.Count = 0 Then
                Return "Sin datos por insertar"
            Else
                Dim r As Integer = Await deleteAllWithParams(nombreTabla, parametros)

                Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                    Await conn.OpenAsync()

                    Using bulkCopy As SqlBulkCopy = New SqlBulkCopy(conn)
                        bulkCopy.DestinationTableName = nombreTabla
                        Await bulkCopy.WriteToServerAsync(dataTable)
                    End Using
                End Using

                Return "Inserción completada correctamente"
            End If

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function BulkInsertAsync(ByVal dataTable As DataTable, ByVal nombreTabla As String) As Task
        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Using bulkCopy As SqlBulkCopy = New SqlBulkCopy(conn)
                    bulkCopy.DestinationTableName = nombreTabla

                    For Each column As DataColumn In dataTable.Columns

                        If column.ColumnName <> "ID" Then
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName)
                        End If
                    Next

                    Await bulkCopy.WriteToServerAsync(dataTable)
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        End Try
    End Function

    Public Async Function bulkInsert(ByVal dataTableOriginal As DataTable, ByVal nombreTabla As String, ByVal tamaño As Integer) As Task
        Dim dataTables As List(Of DataTable) = FuncionalitySQL.DividirDataTable(dataTableOriginal, tamaño)
        Dim tasks = New List(Of Task)()

        For Each dataTable In dataTables
            tasks.Add(BulkInsertWithSemaphore(dataTable, nombreTabla))
        Next

        Await Task.WhenAll(tasks)
    End Function

    Private Async Function BulkInsertWithSemaphore(ByVal dataTable As DataTable, ByVal nombreTabla As String) As Task
        Await semaphore.WaitAsync()

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Using bulkCopy As SqlBulkCopy = New SqlBulkCopy(conn)
                    bulkCopy.DestinationTableName = nombreTabla
                    Await bulkCopy.WriteToServerAsync(dataTable)
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        Finally
            semaphore.Release()
        End Try
    End Function

    Public Async Function bulkInsert(ByVal dataTableInsertar As DataTable,
                                 ByVal dataTableActual As DataTable,
                                 ByVal nombreTabla As String,
                                 ByVal columnaUnique As String) As Task
        Try
            ' Crear un HashSet con los valores existentes
            Dim valoresExistentes As New HashSet(Of Object)(
            dataTableActual.AsEnumerable().
                Select(Function(row) row(columnaUnique))
        )

            ' Filtrar las filas que no están en el HashSet
            Dim filasFiltradas = dataTableInsertar.AsEnumerable().
            Where(Function(newRow) Not valoresExistentes.Contains(newRow(columnaUnique)))

            If Not filasFiltradas.Any() Then
                Throw New ArgumentException("No hay valores nuevos por insertar")
            End If

            ' Copiar a DataTable solo las filas nuevas
            Dim valoresInsertar = filasFiltradas.CopyToDataTable()

            Using conn As New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Using bulkCopy As New SqlBulkCopy(conn)
                    bulkCopy.DestinationTableName = nombreTabla
                    Await bulkCopy.WriteToServerAsync(valoresInsertar)
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al insertar los datos: {ex.Message}", ex)
        End Try
    End Function

    ' Clase auxiliar para mapear índice y tipo de cada columna
    Public Class ColumnInfo
        Public Property Index As Integer
        Public Property DataType As System.Type
    End Class


    Public Async Function getAllRows(Of T As {Class, New})(nombreTabla As String) As Task(Of List(Of T))
        Dim lista As New List(Of T)()
        Try
            Using conn As New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Dim properties = GetType(T).GetProperties()
                Dim columnas As String = $"{String.Join(",", properties.Select(Function(p) p.Name))} FROM {nombreTabla}"
                Dim querycolumnas As String = $"SELECT TOP 0 {columnas}"
                Dim query As String = $"SELECT {columnas}"

                ' 1. Obtener metadatos de columnas (índices y tipos)
                Dim columnMap As New Dictionary(Of String, ColumnInfo)()
                Using cmd As New SqlCommand(querycolumnas, conn)
                    Using reader = Await cmd.ExecuteReaderAsync(CommandBehavior.SchemaOnly)
                        For i As Integer = 0 To reader.FieldCount - 1
                            columnMap(reader.GetName(i)) = New ColumnInfo With {
                            .Index = i,
                            .DataType = reader.GetFieldType(i)
                        }
                        Next
                    End Using
                End Using

                ' 2. Crear setters optimizados para cada propiedad
                Dim setters(properties.Length - 1) As Action(Of SqlDataReader, T)

                For i As Integer = 0 To properties.Length - 1
                    Dim prop = properties(i)
                    Dim columnInfo As ColumnInfo = Nothing
                    If columnMap.TryGetValue(prop.Name, columnInfo) Then
                        setters(i) = FuncionalitySQL.CreateTypedSetter(Of T)(prop, columnInfo.Index, columnInfo.DataType)
                    End If
                Next

                ' 3. Ejecutar consulta y mapear resultados
                Using cmd As New SqlCommand(query, conn)
                    Using reader = Await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess)
                        While Await reader.ReadAsync()
                            Dim item As New T()
                            For Each setter In setters
                                If setter IsNot Nothing Then
                                    setter(reader, item)
                                End If
                            Next
                            lista.Add(item)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try
        Return lista
    End Function


    Public Async Function getRowsParams(Of T As {Class, New})(nombreTabla As String, parametros As String) As Task(Of List(Of T))
        Dim lista As New List(Of T)()
        Try
            Using conn As New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Dim properties = GetType(T).GetProperties()
                Dim columnas As String = $"{String.Join(",", properties.Select(Function(p) p.Name))} FROM {nombreTabla}"
                Dim querycolumnas As String = $"SELECT TOP 0 {columnas}"
                Dim query As String = $"SELECT {columnas} {parametros}"

                ' 1. Obtener metadatos de columnas (índices y tipos)
                Dim columnMap As New Dictionary(Of String, ColumnInfo)()
                Using cmd As New SqlCommand(querycolumnas, conn)
                    Using reader = Await cmd.ExecuteReaderAsync(CommandBehavior.SchemaOnly)
                        For i As Integer = 0 To reader.FieldCount - 1
                            columnMap(reader.GetName(i)) = New ColumnInfo With {
                            .Index = i,
                            .DataType = reader.GetFieldType(i)
                        }
                        Next
                    End Using
                End Using

                ' 2. Crear setters optimizados para cada propiedad
                Dim setters(properties.Length - 1) As Action(Of SqlDataReader, T)

                For i As Integer = 0 To properties.Length - 1
                    Dim prop = properties(i)
                    Dim columnInfo As ColumnInfo = Nothing
                    If columnMap.TryGetValue(prop.Name, columnInfo) Then
                        setters(i) = FuncionalitySQL.CreateTypedSetter(Of T)(prop, columnInfo.Index, columnInfo.DataType)
                    End If
                Next

                ' 3. Ejecutar consulta y mapear resultados
                Using cmd As New SqlCommand(query, conn)
                    Using reader = Await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess)
                        While Await reader.ReadAsync()
                            Dim item As New T()
                            For Each setter In setters
                                If setter IsNot Nothing Then
                                    setter(reader, item)
                                End If
                            Next
                            lista.Add(item)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try
        Return lista
    End Function


    Public Async Function getAllRowsDataTable(Of T As Class)(ByVal nombreTabla As String) As Task(Of DataTable)
        Dim dt As DataTable = New DataTable()

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()
                Dim properties As PropertyInfo() = GetType(T).GetProperties()

                For Each prop In properties
                    dt.Columns.Add(prop.Name)
                Next

                Dim query As String = $"SELECT {String.Join(",", properties.[Select](Function(p) p.Name))} FROM {nombreTabla}"

                Using cmd As SqlCommand = New SqlCommand(query, conn)

                    Using r As SqlDataReader = Await cmd.ExecuteReaderAsync()

                        While Await r.ReadAsync()
                            Dim row As DataRow = dt.NewRow()

                            For Each prop In properties
                                row(prop.Name) = r(prop.Name)
                            Next

                            dt.Rows.Add(row)
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try

        Return dt
    End Function

    Public Async Function deleteRangeDates(ByVal nombreTabla As String, ByVal fechaInicio As DateTime?, ByVal fechaFin As DateTime?, ByVal fechaInicioNombre As String, ByVal fechaFinNombre As String) As Task(Of Integer)
        Dim r As Integer = 0

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()
                Dim query As String = $"DELETE FROM {nombreTabla} WHERE {fechaInicioNombre} >= @{fechaInicioNombre} AND {fechaFinNombre} <= @{fechaFinNombre}"

                Using cmd As SqlCommand = New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue($"@{fechaInicioNombre}", fechaInicio)
                    cmd.Parameters.AddWithValue($"@{fechaFinNombre}", fechaFin)
                    r = Await cmd.ExecuteNonQueryAsync()
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try

        Return r
    End Function

    Public Async Function deleteAll(ByVal nombreTabla As String) As Task(Of Integer)
        Dim r As Integer = 0

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()
                Dim query As String = $"DELETE FROM {nombreTabla}"

                Using cmd As SqlCommand = New SqlCommand(query, conn)
                    cmd.CommandTimeout = 1200
                    r = Await cmd.ExecuteNonQueryAsync()
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try

        Return r
    End Function

    Public Async Function deleteAllWithParams(ByVal nombreTabla As String, ByVal parametros As String) As Task(Of Integer)
        Dim r As Integer = 0

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()
                Dim query As String = $"DELETE FROM {nombreTabla} WHERE {parametros}"

                Using cmd As SqlCommand = New SqlCommand(query, conn)
                    r = Await cmd.ExecuteNonQueryAsync()
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try

        Return r
    End Function

    Public Async Function deleteDate(Of T)(ByVal nombreTabla As String, ByVal fecha As T) As Task(Of Integer)
        Dim r As Integer = 0

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()
                Dim query As String = $"DELETE FROM {nombreTabla} WHERE FECHA = @FECHA"

                Using cmd As SqlCommand = New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@FECHA", fecha)
                    r = Await cmd.ExecuteNonQueryAsync()
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error: {ex.Message}", ex)
        End Try

        Return r
    End Function

    Public Async Function LogFunctionExecution(ByVal method As MethodBase, ByVal logLevel As String, ByVal message As String, ByVal Optional exception As String = Nothing) As Task
        If method.DeclaringType Is Nothing Then
            Throw New ArgumentNullException(NameOf(method.DeclaringType), "DeclaringType es nulo.")
        End If

        Dim declaringType As Type = method.DeclaringType
        Dim className As String = declaringType.FullName
        Dim assemblyName As String = declaringType.Assembly.GetName().Name
        Dim query = "INSERT INTO FunctionLogs (FunctionName, LogLevel, Message, Exception, Timestamp) VALUES (@FunctionName, @LogLevel, @Message, @Exception, GETDATE())"

        Using connection = New SqlConnection(cadenaConexion)
            Await connection.OpenAsync()

            Using command = New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@FunctionName", className)
                command.Parameters.AddWithValue("@LogLevel", logLevel)
                command.Parameters.AddWithValue("@Message", message)
                command.Parameters.AddWithValue("@Exception", If(CObj(exception), DBNull.Value))
                Await command.ExecuteNonQueryAsync()
            End Using
        End Using
    End Function

    Public Async Function Execute_Stored_Procedure_Datatable(ByVal storedProcedureName As String) As Task(Of DataTable)
        Dim datatable As DataTable = New DataTable()

        Try

            Using conn As SqlConnection = New SqlConnection(cadenaConexion)
                Await conn.OpenAsync()

                Using cmd As SqlCommand = New SqlCommand(storedProcedureName, conn)
                    cmd.CommandType = CommandType.StoredProcedure

                    Using reader As SqlDataReader = Await cmd.ExecuteReaderAsync()
                        datatable.Load(reader)
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException($"Error al ejecutar {storedProcedureName}: {ex.Message}", ex)
        End Try

        Return datatable
    End Function




End Class

