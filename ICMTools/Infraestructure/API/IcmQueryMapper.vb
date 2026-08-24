Imports System.Reflection

Public NotInheritable Class IcmQueryMapper

    Private Sub New()
    End Sub

    Public Shared Function MapResponse(Of T As New)(
        response As IcmQueryResponseDto
    ) As List(Of T)

        Dim result As New List(Of T)

        If response Is Nothing OrElse
           response.Data Is Nothing OrElse
           response.ColumnDefinitions Is Nothing Then

            Return result
        End If

        Dim columnIndexes As New Dictionary(Of String, Integer)(
            StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To response.ColumnDefinitions.Count - 1
            columnIndexes(response.ColumnDefinitions(i).Name) = i
        Next

        For Each row As List(Of Object) In response.Data

            Dim item As New T()

            For Each propertyInfo As PropertyInfo In GetType(T).GetProperties()

                Dim columnIndex As Integer

                If Not columnIndexes.TryGetValue(
                    propertyInfo.Name,
                    columnIndex) Then

                    Continue For
                End If

                If columnIndex >= row.Count Then
                    Continue For
                End If

                Dim value As Object = row(columnIndex)

                If value Is Nothing OrElse value Is DBNull.Value Then
                    Continue For
                End If

                Dim targetType As Type =
                    Nullable.GetUnderlyingType(propertyInfo.PropertyType)

                If targetType Is Nothing Then
                    targetType = propertyInfo.PropertyType
                End If

                Dim convertedValue As Object =
                    Convert.ChangeType(value, targetType)

                propertyInfo.SetValue(item, convertedValue)

            Next

            result.Add(item)

        Next

        Return result

    End Function

End Class