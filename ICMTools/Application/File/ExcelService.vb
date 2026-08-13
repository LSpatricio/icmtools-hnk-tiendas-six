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
                Dim resultado As DateTime
                Return DateTime.TryParse(valor.ToString(), resultado)

            Case GetType(Boolean)
                Dim resultado As Boolean
                Return Boolean.TryParse(valor.ToString(), resultado)

            Case Else
                Return tipoEsperado.IsAssignableFrom(valor.GetType())

        End Select

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
End Class
