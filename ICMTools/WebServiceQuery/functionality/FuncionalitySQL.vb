Imports System.Data.SqlClient
Imports System.Reflection

Public Class FuncionalitySQL


    Public Function insertQuery(Of T As Class)() As String
        Dim nombreTabla As String = GetType(T).Name.Remove(0, 3)
        Dim propierties = GetType(T).GetProperties()
        Dim query As String = $"INSERT INTO {nombreTabla} ("

        For Each [property] In propierties
            query = $"{query}
                        {[property].Name},"
        Next

        query = query.Remove(query.Length - 1)
        query = $"{query})
                    VALUES("

        For Each [property] In propierties
            query = $"{query}
                        @{[property].Name},"
        Next

        query = query.Remove(query.Length - 1)
        query = $"{query})"
        Return query
    End Function

    Public Shared Function DividirDataTable(ByVal dataTable As DataTable, ByVal tamaño As Integer) As List(Of DataTable)
        Dim bloques As List(Of DataTable) = New List(Of DataTable)()
        Dim batchTable As DataTable = dataTable.Clone()
        Dim counter As Integer = 0

        For Each row As DataRow In dataTable.Rows
            batchTable.ImportRow(row)
            counter += 1

            If counter = tamaño Then
                bloques.Add(batchTable)
                batchTable = dataTable.Clone()
                counter = 0
            End If
        Next

        If batchTable.Rows.Count > 0 Then
            bloques.Add(batchTable)
        End If

        Return bloques
    End Function

    Public Shared Function getdates() As DataTable
        Dim table As DataTable = New DataTable()
        table.Columns.Add("DateStart", GetType(DateTime))
        table.Columns.Add("DateEnd", GetType(DateTime))
        Dim dateEnd As DateTime = DateTime.Today
        Dim dateStart As DateTime = dateEnd.AddMonths(-3)
        table.Rows.Add(dateStart, dateEnd)
        Return table
    End Function

    Public Shared Function CreateTypedSetter(Of T)(
     ByVal prop As PropertyInfo,
     ByVal columnIndex As Integer,
     ByVal sqlType As System.Type) As Action(Of SqlDataReader, T)

        Dim targetType As Type = If(Nullable.GetUnderlyingType(prop.PropertyType), prop.PropertyType)
        Dim isNullable As Boolean = prop.PropertyType.IsGenericType AndAlso
                                    prop.PropertyType.GetGenericTypeDefinition() = GetType(Nullable(Of))

        If targetType = GetType(Integer) Then
            Return Sub(r, obj)
                       prop.SetValue(obj,
                           If(r.IsDBNull(columnIndex),
                              If(isNullable, CType(Nothing, Integer?), 0),
                              r.GetInt32(columnIndex)))
                   End Sub

        ElseIf targetType = GetType(Decimal) Then
            Return Sub(r, obj)
                       prop.SetValue(obj,
                           If(r.IsDBNull(columnIndex),
                              If(isNullable, CType(Nothing, Decimal?), 0D),
                              r.GetDecimal(columnIndex)))
                   End Sub

        ElseIf targetType = GetType(DateTime) Then
            Return Sub(r, obj)
                       prop.SetValue(obj,
                           If(r.IsDBNull(columnIndex),
                              If(isNullable, CType(Nothing, DateTime?), DateTime.MinValue),
                              r.GetDateTime(columnIndex)))
                   End Sub

        ElseIf targetType = GetType(String) Then
            Return Sub(r, obj)
                       prop.SetValue(obj,
                           If(r.IsDBNull(columnIndex), Nothing, r.GetString(columnIndex)))
                   End Sub
        End If

        Return Sub(r, obj)
                   prop.SetValue(obj,
                       If(r.IsDBNull(columnIndex), Nothing, r.GetValue(columnIndex)))
               End Sub
    End Function

End Class