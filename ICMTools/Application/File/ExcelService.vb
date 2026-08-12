Public Class ExcelService

    Public Function UsaHojasDefinidas(tipo As Type) As Boolean

        Dim propiedades = tipo.GetProperties()

        For Each propiedad In propiedades

            Dim tipoLista As Type = propiedad.PropertyType


        Next

        Return False

    End Function


    Public Function ObtenerTipos(tipoClass As Type) As List(Of Type)

        Dim tipos As New List(Of Type)

        For Each propiedad In tipoClass.GetProperties()

            If propiedad.PropertyType.IsGenericType Then

                Dim tipoElemento As Type =
                propiedad.PropertyType.GetGenericArguments()(0)

                tipos.Add(tipoElemento)

            End If

        Next

        Return tipos

    End Function

    Public Function DebeIgnorarFila(
    nombreColumna As String,
    valor As Object,
    valoresIgnorados As Dictionary(Of String, List(Of String))
) As Boolean

        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return False
        End If

        If Not valoresIgnorados.ContainsKey(nombreColumna) Then
            Return False
        End If

        Dim texto As String = valor.ToString().Trim()

        For Each valorIgnorado In valoresIgnorados(nombreColumna)

            If texto.IndexOf(
                valorIgnorado,
                StringComparison.OrdinalIgnoreCase
            ) >= 0 Then

                Return True

            End If

        Next

        Return False

    End Function

End Class
