Imports System.Reflection

Public Class ExcelService
    Public Function ObtenerTipos(tipoClass As Type) As List(Of Type)

        Dim tipos As New List(Of Type)

        For Each propiedad In tipoClass.GetProperties()

            If propiedad.PropertyType.IsGenericType AndAlso
           propiedad.PropertyType.GetGenericTypeDefinition() = GetType(List(Of )) Then

                Dim tipoElemento As Type =
                propiedad.PropertyType.GetGenericArguments()(0)

                tipos.Add(tipoElemento)

            End If

        Next

        Return tipos

    End Function

    Public Function ObtenerPropiedadesListas(tipoClass As Type) As List(Of PropertyInfo)

        Dim propiedadesListas As New List(Of PropertyInfo)

        For Each propiedad In tipoClass.GetProperties()

            If propiedad.PropertyType.IsGenericType AndAlso
               propiedad.PropertyType.GetGenericTypeDefinition() = GetType(List(Of )) Then

                propiedadesListas.Add(propiedad)
            End If

        Next

        Return propiedadesListas

    End Function

    Public Function CrearMepeoAtributos(tipo As Type) As Dictionary(Of PropertyInfo, ExcelColumnAttribute)

        'diccionario regresamos la relación propiedad y atributos..

        Dim mappings As New Dictionary(Of PropertyInfo, ExcelColumnAttribute)

        Dim propiedades = tipo.GetProperties()

        For Each propiedad In propiedades

            Dim atributo = propiedad.GetCustomAttributes(
                GetType(ExcelColumnAttribute),
                False
            ).FirstOrDefault()

            If atributo Is Nothing Then
                Continue For
            End If

            Dim excelColumn =
                DirectCast(
                    atributo,
                    ExcelColumnAttribute
                )

            mappings(propiedad) = excelColumn


        Next

        Return mappings

    End Function

    Public Function EsTipoValido(valor As Object, tipoEsperado As Type) As Boolean

        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return True
        End If

        Select Case tipoEsperado

            Case GetType(String)
                Return True

            Case GetType(Integer)
                Dim resultado As Integer
                Return Integer.TryParse(valor.ToString(), resultado)

            Case GetType(Decimal)
                Dim resultado As Decimal
                Return Decimal.TryParse(valor.ToString(), resultado)

            Case GetType(Double)
                Dim resultado As Double
                Return Double.TryParse(valor.ToString(), resultado)
            Case GetType(DateTime)
                If TypeOf valor Is DateTime Then
                    Return True
                End If

                Dim resultado As DateTime
                Dim texto = NormalizarTextoFecha(valor.ToString())

                Return DateTime.TryParseExact(
                    texto,
                    {
                        "yyyy-MM-dd HH:mm:ss",
                        "yyyy-MM-dd HH:mm",
                        "yyyy-MM-dd",
                        "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yyyy HH:mm",
                        "dd/MM/yyyy",
                        "d/MM/yyyy HH:mm:ss",
                        "d/MM/yyyy HH:mm",
                        "d/MM/yyyy",
                        "dd-MM-yyyy HH:mm:ss",
                        "dd-MM-yyyy HH:mm",
                        "dd-MM-yyyy",
                        "d-MM-yyyy HH:mm:ss",
                        "d-MM-yyyy HH:mm",
                        "d-MM-yyyy",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yyyy HH:mm",
                        "MM/dd/yyyy",
                        "M/d/yyyy HH:mm:ss",
                        "M/d/yyyy HH:mm",
                        "M/d/yyyy"
                    },
                    Globalization.CultureInfo.InvariantCulture,
                    Globalization.DateTimeStyles.AllowWhiteSpaces,
                    resultado
                ) OrElse
                DateTime.TryParse(
                    texto,
                    Globalization.CultureInfo.CurrentCulture,
                    Globalization.DateTimeStyles.AllowWhiteSpaces,
                    resultado
                ) OrElse
                DateTime.TryParse(
                    texto,
                    Globalization.CultureInfo.InvariantCulture,
                    Globalization.DateTimeStyles.AllowWhiteSpaces,
                    resultado
                )

            Case GetType(Boolean)
                Dim resultado As Boolean
                Return Boolean.TryParse(valor.ToString(), resultado)

            Case Else
                Return tipoEsperado.IsAssignableFrom(valor.GetType())

        End Select

    End Function


    Private Function NormalizarTextoFecha(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim texto = valor.Trim()
        texto = System.Text.RegularExpressions.Regex.Replace(
            texto,
            "\s*(a|p)\.?\s*m\.?",
            String.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        )

        Return texto.Trim()
    End Function

    Public Function ObtenerDescripcionTipo(tipo As Type) As String

        Select Case tipo

            Case GetType(String)
                Return "texto"

            Case GetType(Integer)
                Return "un número entero"

            Case GetType(Decimal), GetType(Double)
                Return "un valor numérico"

            Case GetType(DateTime)
                Return "una fecha válida"

            Case GetType(Boolean)
                Return "un valor booleano"

            Case Else
                Return "un valor válido"

        End Select

    End Function

    Public Function CrearDataTable(
    mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)
) As DataTable

        Dim dt As New DataTable()

        For Each mapeo In mapeoColumnas

            Dim propiedad As PropertyInfo = mapeo.Key
            Dim tipo As Type = Nullable.GetUnderlyingType(propiedad.PropertyType)

            If tipo Is Nothing Then
                tipo = propiedad.PropertyType
            End If

            dt.Columns.Add(propiedad.Name, tipo)

        Next

        Return dt

    End Function
End Class
