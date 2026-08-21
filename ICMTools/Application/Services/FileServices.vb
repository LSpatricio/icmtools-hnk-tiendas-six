Imports System.Reflection

Public Class FileServices

    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _excelReader As ExcelReader
    Public Sub New()
        _excelService = New ExcelService()
        _excelReader = New ExcelReader()
    End Sub

    Public Function ValidarExcel(request As ValidateFileRequestt) As List(Of ExcelValidationError)

        Dim erroresValidacion As List(Of ExcelValidationError) = New List(Of ExcelValidationError)()
        Dim tipo As Type = Type.GetType(request.FileClass)

        Dim hojasDefinidas As List(Of Type) = _excelService.ObtenerTipos(tipo)

        If hojasDefinidas.Any() Then
            'Validacion nombre de ojas
            erroresValidacion = _excelReader.ValidarHojasDefinidas(tipo, request.Path)


            If erroresValidacion.Count > 0 Then
                Return erroresValidacion
            End If


            For Each hoja In hojasDefinidas


                Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(hoja)

                Dim propiedad = tipo.GetProperties().
                                            FirstOrDefault(Function(p) p.PropertyType.IsGenericType AndAlso
                                                                       p.PropertyType.GetGenericArguments()(0) = hoja)

                Dim atributo = propiedad?.GetCustomAttributes(GetType(ExcelSheetAttribute), False).Cast(Of ExcelSheetAttribute)().FirstOrDefault()

                erroresValidacion.AddRange(_excelReader.ValidarEncabezadosExcel(hoja, request.Path, atributo.HeaderRow, atributo.SheetName, mapeoColumnas))

            Next

        Else
            Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)
            Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = _excelService.CrearMepeoAtributos(tipo)

            For i As Integer = 0 To cantidadHojas - 1
                Dim erroresHoja As List(Of ExcelValidationError) = _excelReader.ValidarEncabezadosExcel(tipo, request.Path, request.HeaderRow, i.ToString(), mapeoColumnas)

                If Not erroresHoja.Any() Then
                    erroresValidacion.Clear()
                    Return erroresValidacion
                End If


            Next


            erroresValidacion.Add(
                    New ExcelValidationError With {
                        .Problema = "Estructura de archivo inválida",
                        .Detalle = "El archivo no contiene una hoja con los encabezados esperados."
                    }
                )

        End If

        Return erroresValidacion

    End Function


End Class
