Imports Newtonsoft.Json.Linq
Imports System.ServiceModel.Security
Imports System.Text.RegularExpressions

Public Class FunctionalityICM

    Public Shared Function ICMToDataTable(ByVal jsonEncabezado As JObject, ByVal jsonContenido As JArray) As DataTable
        Dim dataTable As DataTable = New DataTable()

        For Each item As JToken In jsonEncabezado("columnDefinitions")
            dataTable.Columns.Add(item("name").ToString(), TipoDe(item("type").ToString()))
        Next

        For Each item As JArray In jsonContenido
            Dim row As DataRow = dataTable.NewRow()

            For i As Integer = 0 To dataTable.Columns.Count - 1

                If i < item.Count Then

                    If item(i).Type = JTokenType.Null OrElse String.IsNullOrWhiteSpace(item(i).ToString()) Then

                        If dataTable.Columns(i).DataType = GetType(DateTime) OrElse dataTable.Columns(i).DataType = GetType(Decimal) Then
                            row(i) = DBNull.Value
                        Else
                            row(i) = item(i)
                        End If
                    Else
                        row(i) = item(i)
                    End If
                Else
                    row(i) = DBNull.Value
                End If
            Next
            dataTable.Rows.Add(row)
        Next
        Return dataTable
    End Function

    Public Shared Function ICMToDataTable(ByVal dt As DataTable, ByVal jsonContenido As JArray) As DataTable
        For Each item As JArray In jsonContenido
            Dim row As DataRow = dt.NewRow()

            For i As Integer = 0 To dt.Columns.Count - 1

                If i < item.Count Then

                    If item(i).Type = JTokenType.Null OrElse String.IsNullOrWhiteSpace(item(i).ToString()) Then

                        If dt.Columns(i).DataType = GetType(DateTime) OrElse dt.Columns(i).DataType = GetType(Decimal) Then
                            row(i) = DBNull.Value
                        Else
                            row(i) = item(i)
                        End If
                    Else
                        row(i) = item(i)
                    End If
                Else
                    row(i) = DBNull.Value
                End If
            Next

            dt.Rows.Add(row)
        Next

        Return dt
    End Function

    Private Shared Function TipoDe(ByVal obj As String) As Type
        Dim type As System.Type

        Select Case obj.ToLower()
            Case "date", "datetime"
                type = GetType(DateTime)
            Case "decimal"
                type = GetType(Single)
            Case Else
                type = GetType(String)
        End Select

        Return type
    End Function

    Public Shared Function AjustarConsulta(ByVal consultaOriginal As String) As String
        If String.IsNullOrWhiteSpace(consultaOriginal) Then
            Throw New ArgumentException("La consulta original no puede ser nula o vacía.", NameOf(consultaOriginal))
        End If

        Dim parts = consultaOriginal.Split({"SELECT", "FROM"}, StringSplitOptions.RemoveEmptyEntries)

        If parts.Length <> 2 Then
            Throw New FormatException("La consulta original no tiene el formato esperado.")
        End If

        Dim columnas As String = parts(0).Trim()
        Dim tabla As String = parts(1).Trim()

        Dim columnasEscapadas As String =
        String.Join(", ", columnas.Split(","c).Select(Function(columna) $"\""{columna.Trim()}\"""))

        Dim tablaEscapada As String = $"\""{tabla}\"""

        Return $"SELECT {columnasEscapadas} FROM {tablaEscapada}"
    End Function








    Public Shared Function getdates() As DataTable
        Dim table As DataTable = New DataTable()
        table.Columns.Add("DateStart", GetType(DateTime))
        table.Columns.Add("DateEnd", GetType(DateTime))
        Dim dateEnd As DateTime = DateTime.Today
        Dim dateStart As DateTime = New DateTime(dateEnd.Year, dateEnd.Month, 1).AddMonths(-2)
        table.Rows.Add(dateStart, dateEnd)
        Return table
    End Function

    Public Shared Function getdates(ByVal meses As Integer) As DataTable
        Dim table As DataTable = New DataTable()
        table.Columns.Add("DateStart", GetType(DateTime))
        table.Columns.Add("DateEnd", GetType(DateTime))
        Dim dateEnd As DateTime = DateTime.Today
        Dim dateStart As DateTime = New DateTime(dateEnd.Year, dateEnd.Month, 1).AddMonths(-meses)
        table.Rows.Add(dateStart, dateEnd)
        Return table
    End Function

End Class
