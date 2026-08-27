Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports ExcelDataReader

Public Class ArqueosReadResult
    Public Property Data As DataTable
    Public Property Errores As List(Of ExcelValidationError)
End Class

Public Class ArqueosExcelReader
    Private Shared ReadOnly AliasEncabezados As New Dictionary(Of String, String()) From {
        {NameOf(ArqueosExcelDto.NumeroSAP), {"No. SAP"}}
    }

    Public Function Leer(rutaArchivo As String, filaEncabezado As Integer) As ArqueosReadResult
        Dim resultado As New ArqueosReadResult With {
            .Data = CrearTablaStaging(),
            .Errores = New List(Of ExcelValidationError)()
        }
        Dim hojasProcesadas As Integer = 0

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                Do
                    Dim encabezados = LeerEncabezados(reader, filaEncabezado)
                    If encabezados Is Nothing Then Continue Do

                    Dim indices = CrearIndices(encabezados)
                    If Not EsHojaArqueos(indices) Then Continue Do

                    hojasProcesadas += 1
                    Dim erroresEncabezado = ValidarEncabezados(indices, encabezados, reader.Name)
                    resultado.Errores.AddRange(erroresEncabezado)

                    If erroresEncabezado.Count = 0 Then
                        LeerFilas(reader, indices, reader.Name, filaEncabezado, resultado)
                    End If
                Loop While reader.NextResult()
            End Using
        End Using

        If hojasProcesadas = 0 Then
            resultado.Errores.Add(New ExcelValidationError With {
                .Problema = "El archivo no contiene hojas con la estructura de Arqueos.",
                .Detalle = "No se encontraron las columnas Codigo de Listado y NumeroSAP."
            })
        End If

        Return resultado
    End Function

    Private Function LeerEncabezados(reader As IExcelDataReader, filaEncabezado As Integer) As List(Of String)
        For fila As Integer = 1 To filaEncabezado
            If Not reader.Read() Then Return Nothing
        Next

        Dim encabezados As New List(Of String)(reader.FieldCount)
        For indice As Integer = 0 To reader.FieldCount - 1
            encabezados.Add(Convert.ToString(reader.GetValue(indice), CultureInfo.InvariantCulture).Trim())
        Next
        Return encabezados
    End Function

    Private Function CrearIndices(encabezados As IList(Of String)) As Dictionary(Of String, Integer)
        Dim resultado As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For Each propiedad In GetType(ArqueosExcelDto).GetProperties()
            Dim atributo = propiedad.GetCustomAttribute(Of ExcelColumnAttribute)()
            Dim nombres As New List(Of String) From {atributo.ColumnName}
            If AliasEncabezados.ContainsKey(propiedad.Name) Then nombres.AddRange(AliasEncabezados(propiedad.Name))

            Dim indice = Enumerable.Range(0, encabezados.Count).FirstOrDefault(
                Function(i) nombres.Any(Function(nombre) Normalizar(encabezados(i)) = Normalizar(nombre)))

            If nombres.Any(Function(nombre) encabezados.Any(Function(actual) Normalizar(actual) = Normalizar(nombre))) Then
                resultado(propiedad.Name) = indice
            Else
                resultado(propiedad.Name) = -1
            End If
        Next

        Return resultado
    End Function

    Private Function EsHojaArqueos(indices As Dictionary(Of String, Integer)) As Boolean
        Return indices(NameOf(ArqueosExcelDto.CodigoListado)) >= 0 AndAlso
            indices(NameOf(ArqueosExcelDto.NumeroSAP)) >= 0
    End Function

    Private Function ValidarEncabezados(
        indices As Dictionary(Of String, Integer),
        encabezados As IList(Of String),
        nombreHoja As String) As List(Of ExcelValidationError)

        Dim errores As New List(Of ExcelValidationError)()
        For Each propiedad In GetType(ArqueosExcelDto).GetProperties()
            Dim atributo = propiedad.GetCustomAttribute(Of ExcelColumnAttribute)()
            If indices(propiedad.Name) < 0 Then
                errores.Add(New ExcelValidationError With {
                    .Problema = $"La columna '{atributo.ColumnName}' no se encuentra en la hoja <strong>{nombreHoja}</strong>.",
                    .Detalle = $"Columnas encontradas: {String.Join(", ", encabezados)}"
                })
            End If
        Next
        Return errores
    End Function

    Private Sub LeerFilas(
        reader As IExcelDataReader,
        indices As Dictionary(Of String, Integer),
        nombreHoja As String,
        filaEncabezado As Integer,
        resultado As ArqueosReadResult)

        Dim numeroFila As Integer = filaEncabezado + 1
        While reader.Read()
            If FilaVacia(reader, indices.Values) Then Exit While

            Dim erroresAntes = resultado.Errores.Count
            Dim fila = resultado.Data.NewRow()

            RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.CodigoListado)), "Codigo de Listado", nombreHoja, numeroFila)
            Dim fechaCreacion = ObtenerFecha(Valor(reader, indices, NameOf(ArqueosExcelDto.FechaCreacion)))
            Dim fechaInicio = ObtenerFecha(Valor(reader, indices, NameOf(ArqueosExcelDto.FechaInicioConteo)))
            Dim fechaFin = ObtenerFecha(Valor(reader, indices, NameOf(ArqueosExcelDto.FechaFinConteo)))
            Dim fechaCierre = ObtenerFecha(Valor(reader, indices, NameOf(ArqueosExcelDto.FechaCierreConteo)))
            Dim fechaTermino = ObtenerFecha(Valor(reader, indices, NameOf(ArqueosExcelDto.FechaTerminoConteo)))
            Dim diferencia = ObtenerDecimal(Valor(reader, indices, NameOf(ArqueosExcelDto.Diferencia)))
            Dim faltante = ObtenerDecimal(Valor(reader, indices, NameOf(ArqueosExcelDto.Faltante)))
            Dim sobrante = ObtenerDecimal(Valor(reader, indices, NameOf(ArqueosExcelDto.Sobrante)))

            fila("NumeroSAP") = RequerirLong(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.NumeroSAP)), "NumeroSAP", nombreHoja, numeroFila)
            fila("Almacen") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.Almacen)), "Almacen", nombreHoja, numeroFila)
            fila("TipoListado") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.TipoListado)), "Tipo de Listado", nombreHoja, numeroFila)
            fila("Estatus") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.Estatus)), "Estatus", nombreHoja, numeroFila)
            fila("FechaCreacion") = RequerirFecha(resultado.Errores, fechaCreacion, "Fecha de Creacion", nombreHoja, numeroFila)
            fila("Nombre") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.Nombre)), "Nombre", nombreHoja, numeroFila)
            fila("UsuarioCreador") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.UsuarioCreador)), "Usuario-Creador", nombreHoja, numeroFila)
            fila("UsuarioCreadorPerfil") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.UsuarioCreadorPerfil)), "Usuario-Creador-Perfil", nombreHoja, numeroFila)
            fila("UsuarioAutorizador") = OpcionalTexto(Valor(reader, indices, NameOf(ArqueosExcelDto.UsuarioAutorizador)))
            fila("UsuarioAutorizadorPerfil") = OpcionalTexto(Valor(reader, indices, NameOf(ArqueosExcelDto.UsuarioAutorizadorPerfil)))
            fila("TipoCierre") = OpcionalTexto(Valor(reader, indices, NameOf(ArqueosExcelDto.TipoCierre)))
            fila("FechaInicioConteo") = RequerirFecha(resultado.Errores, fechaInicio, "Inicio de Conteo", nombreHoja, numeroFila)
            fila("FechaFinConteo") = RequerirFecha(resultado.Errores, fechaFin, "Fin de Conteo", nombreHoja, numeroFila)
            fila("FechaCierreConteo") = RequerirFecha(resultado.Errores, fechaCierre, "Cierre de Conteo", nombreHoja, numeroFila)
            fila("FechaTerminoConteo") = If(fechaTermino.HasValue, CType(fechaTermino.Value, Object), DBNull.Value)
            fila("Subinventario") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.Subinventario)), "Subinventario", nombreHoja, numeroFila)
            fila("IdProducto") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.IdProducto)), "ID Producto", nombreHoja, numeroFila)
            fila("CodigoProducto") = RequerirLong(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.CodigoProducto)), "Codigo de Producto", nombreHoja, numeroFila)
            fila("NombreProducto") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.NombreProducto)), "Nombre del Producto", nombreHoja, numeroFila)
            fila("Unidad") = RequerirTexto(resultado.Errores, Valor(reader, indices, NameOf(ArqueosExcelDto.Unidad)), "Unidad", nombreHoja, numeroFila)
            fila("CantidadSistema") = RequerirDecimal(resultado.Errores, ObtenerDecimal(Valor(reader, indices, NameOf(ArqueosExcelDto.CantidadSistema))), "Cantidad Sistema", nombreHoja, numeroFila)
            fila("Diferencia") = RequerirDecimal(resultado.Errores, diferencia, "Diferencia", nombreHoja, numeroFila)
            fila("Faltante") = RequerirDecimal(resultado.Errores, faltante, "Faltante", nombreHoja, numeroFila)
            fila("Sobrante") = RequerirDecimal(resultado.Errores, sobrante, "Sobrante", nombreHoja, numeroFila)
            fila("FaltantePrecioCons") = RequerirDecimal(resultado.Errores, ObtenerDecimal(Valor(reader, indices, NameOf(ArqueosExcelDto.FaltantePrecioCons))), "$ Faltante Precio/Cons", nombreHoja, numeroFila)
            fila("SobrantePrecioCons") = RequerirDecimal(resultado.Errores, ObtenerDecimal(Valor(reader, indices, NameOf(ArqueosExcelDto.SobrantePrecioCons))), "$ Sobrante Precio/Cons", nombreHoja, numeroFila)
            fila("Comentario") = OpcionalTexto(Valor(reader, indices, NameOf(ArqueosExcelDto.Comentario)))

            ValidarReglas(resultado.Errores, fechaCreacion, fechaInicio, fechaFin, fechaCierre, fechaTermino, diferencia, faltante, sobrante, Convert.ToString(fila("Comentario")), nombreHoja, numeroFila)

            If resultado.Errores.Count = erroresAntes Then resultado.Data.Rows.Add(fila)
            numeroFila += 1
        End While
    End Sub

    Private Sub ValidarReglas(
        errores As List(Of ExcelValidationError),
        fechaCreacion As DateTime?, fechaInicio As DateTime?, fechaFin As DateTime?,
        fechaCierre As DateTime?, fechaTermino As DateTime?, diferencia As Decimal?,
        faltante As Decimal?, sobrante As Decimal?, comentario As String,
        hoja As String, fila As Integer)

        If fechaCreacion.HasValue AndAlso fechaCreacion.Value > DateTime.Now Then AgregarError(errores, "La fecha de creacion no puede ser posterior a la fecha de carga.", hoja, fila)
        If fechaInicio.HasValue AndAlso fechaFin.HasValue AndAlso fechaInicio.Value > fechaFin.Value Then AgregarError(errores, "Inicio de Conteo debe ser menor o igual a Fin de Conteo.", hoja, fila)
        If fechaFin.HasValue AndAlso fechaCierre.HasValue AndAlso fechaFin.Value > fechaCierre.Value Then AgregarError(errores, "Fin de Conteo debe ser menor o igual a Cierre de Conteo.", hoja, fila)
        If fechaCierre.HasValue AndAlso fechaTermino.HasValue AndAlso fechaCierre.Value > fechaTermino.Value Then AgregarError(errores, "Cierre de Conteo debe ser menor o igual a Termino de Conteo.", hoja, fila)

        If faltante.GetValueOrDefault() > 0D AndAlso sobrante.GetValueOrDefault() > 0D Then
            AgregarError(errores, "Faltante y Sobrante no pueden ser positivos al mismo tiempo.", hoja, fila)
        End If

        If diferencia.HasValue Then
            If diferencia.Value > 0D AndAlso sobrante.GetValueOrDefault() <= 0D Then AgregarError(errores, "Si la diferencia es positiva, Sobrante debe ser mayor que cero.", hoja, fila)
            If diferencia.Value < 0D AndAlso faltante.GetValueOrDefault() <= 0D Then AgregarError(errores, "Si la diferencia es negativa, Faltante debe ser mayor que cero.", hoja, fila)
            If diferencia.Value = 0D AndAlso (faltante.GetValueOrDefault() > 0D OrElse sobrante.GetValueOrDefault() > 0D) Then AgregarError(errores, "Si la diferencia es cero, Faltante y Sobrante deben ser cero.", hoja, fila)
        End If

        If comentario IsNot Nothing AndAlso comentario.Length > 500 Then AgregarError(errores, "Comentarios no puede exceder 500 caracteres.", hoja, fila)
    End Sub

    Private Function CrearTablaStaging() As DataTable
        Dim tabla As New DataTable()
        tabla.Columns.Add("NumeroSAP", GetType(Long))
        tabla.Columns.Add("Almacen", GetType(String))
        tabla.Columns.Add("TipoListado", GetType(String))
        tabla.Columns.Add("Estatus", GetType(String))
        tabla.Columns.Add("FechaCreacion", GetType(DateTime))
        tabla.Columns.Add("Nombre", GetType(String))
        tabla.Columns.Add("UsuarioCreador", GetType(String))
        tabla.Columns.Add("UsuarioCreadorPerfil", GetType(String))
        tabla.Columns.Add("UsuarioAutorizador", GetType(String))
        tabla.Columns.Add("UsuarioAutorizadorPerfil", GetType(String))
        tabla.Columns.Add("TipoCierre", GetType(String))
        tabla.Columns.Add("FechaInicioConteo", GetType(DateTime))
        tabla.Columns.Add("FechaFinConteo", GetType(DateTime))
        tabla.Columns.Add("FechaCierreConteo", GetType(DateTime))
        tabla.Columns.Add("FechaTerminoConteo", GetType(DateTime))
        tabla.Columns.Add("Subinventario", GetType(String))
        tabla.Columns.Add("IdProducto", GetType(String))
        tabla.Columns.Add("CodigoProducto", GetType(Long))
        tabla.Columns.Add("NombreProducto", GetType(String))
        tabla.Columns.Add("Unidad", GetType(String))
        tabla.Columns.Add("CantidadSistema", GetType(Decimal))
        tabla.Columns.Add("Diferencia", GetType(Decimal))
        tabla.Columns.Add("Faltante", GetType(Decimal))
        tabla.Columns.Add("Sobrante", GetType(Decimal))
        tabla.Columns.Add("FaltantePrecioCons", GetType(Decimal))
        tabla.Columns.Add("SobrantePrecioCons", GetType(Decimal))
        tabla.Columns.Add("Comentario", GetType(String))
        Return tabla
    End Function

    Private Function Valor(reader As IExcelDataReader, indices As Dictionary(Of String, Integer), propiedad As String) As Object
        Dim indice = indices(propiedad)
        If indice < 0 OrElse indice >= reader.FieldCount Then Return Nothing
        Return reader.GetValue(indice)
    End Function

    Private Function FilaVacia(reader As IExcelDataReader, indices As IEnumerable(Of Integer)) As Boolean
        Return indices.Where(Function(i) i >= 0).All(Function(i) String.IsNullOrWhiteSpace(Convert.ToString(reader.GetValue(i))))
    End Function

    Private Function RequerirTexto(errores As List(Of ExcelValidationError), valor As Object, campo As String, hoja As String, fila As Integer) As Object
        Dim texto = Convert.ToString(valor, CultureInfo.InvariantCulture).Trim()
        If texto = "" Then
            AgregarError(errores, $"{campo} no admite valores vacios.", hoja, fila)
            Return DBNull.Value
        End If
        Return texto
    End Function

    Private Function OpcionalTexto(valor As Object) As Object
        Dim texto = Convert.ToString(valor, CultureInfo.InvariantCulture).Trim()
        Return If(texto = "", CType(DBNull.Value, Object), texto)
    End Function

    Private Function RequerirLong(errores As List(Of ExcelValidationError), valor As Object, campo As String, hoja As String, fila As Integer) As Object
        If TypeOf valor Is Long OrElse TypeOf valor Is Integer OrElse TypeOf valor Is Short OrElse
            TypeOf valor Is Decimal OrElse TypeOf valor Is Double OrElse TypeOf valor Is Single Then
            Try
                Return Convert.ToInt64(valor, CultureInfo.InvariantCulture)
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException
                AgregarError(errores, $"{campo} debe contener un numero entero.", hoja, fila)
                Return DBNull.Value
            End Try
        End If

        Dim numero As Long
        If valor Is Nothing OrElse Not Long.TryParse(Convert.ToString(valor, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, numero) Then
            AgregarError(errores, $"{campo} debe contener un numero entero.", hoja, fila)
            Return DBNull.Value
        End If
        Return numero
    End Function

    Private Function RequerirDecimal(errores As List(Of ExcelValidationError), valor As Decimal?, campo As String, hoja As String, fila As Integer) As Object
        If Not valor.HasValue Then
            AgregarError(errores, $"{campo} debe contener un numero.", hoja, fila)
            Return DBNull.Value
        End If
        Return valor.Value
    End Function

    Private Function RequerirFecha(errores As List(Of ExcelValidationError), valor As DateTime?, campo As String, hoja As String, fila As Integer) As Object
        If Not valor.HasValue Then
            AgregarError(errores, $"{campo} debe contener una fecha valida.", hoja, fila)
            Return DBNull.Value
        End If
        Return valor.Value
    End Function

    Private Function ObtenerDecimal(valor As Object) As Decimal?
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return Nothing
        If TypeOf valor Is Decimal OrElse TypeOf valor Is Double OrElse TypeOf valor Is Single OrElse TypeOf valor Is Integer OrElse TypeOf valor Is Long Then Return Convert.ToDecimal(valor, CultureInfo.InvariantCulture)

        Dim numero As Decimal
        Dim texto = Convert.ToString(valor).Trim()
        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, numero) OrElse Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, numero) Then Return numero
        Return Nothing
    End Function

    Private Function ObtenerFecha(valor As Object) As DateTime?
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return Nothing
        If TypeOf valor Is DateTime Then Return DirectCast(valor, DateTime)
        If TypeOf valor Is Double Then
            Try
                Return DateTime.FromOADate(DirectCast(valor, Double))
            Catch
                Return Nothing
            End Try
        End If

        Dim texto = Convert.ToString(valor).Trim()
        texto = System.Text.RegularExpressions.Regex.Replace(texto, "\ba\.?\s*m\.?", "AM", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        texto = System.Text.RegularExpressions.Regex.Replace(texto, "\bp\.?\s*m\.?", "PM", System.Text.RegularExpressions.RegexOptions.IgnoreCase)

        Dim formatos = {
            "dd/MM/yyyy", "d/MM/yyyy", "dd-MM-yyyy", "d-MM-yyyy",
            "dd/MM/yyyy HH:mm", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm", "dd-MM-yyyy HH:mm:ss",
            "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd hh:mm tt", "yyyy-MM-dd hh:mm:ss tt",
            "MM/dd/yyyy", "M/d/yyyy", "MM/dd/yyyy HH:mm", "M/d/yyyy HH:mm",
            "MM/dd/yyyy HH:mm:ss", "M/d/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt", "M/d/yyyy hh:mm:ss tt"
        }

        Dim fecha As DateTime
        If DateTime.TryParseExact(texto, formatos, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, fecha) OrElse
            DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, fecha) OrElse
            DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, fecha) Then Return fecha
        Return Nothing
    End Function

    Private Sub AgregarError(errores As List(Of ExcelValidationError), problema As String, hoja As String, fila As Integer)
        errores.Add(New ExcelValidationError With {
            .Problema = problema,
            .Detalle = $"Fila {fila}. Hoja <strong>{hoja}</strong>."
        })
    End Sub

    Private Function Normalizar(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then Return ""
        Dim descompuesto = valor.Normalize(NormalizationForm.FormD)
        Dim resultado As New StringBuilder()
        For Each caracter In descompuesto
            If CharUnicodeInfo.GetUnicodeCategory(caracter) <> UnicodeCategory.NonSpacingMark Then resultado.Append(caracter)
        Next
        Return resultado.ToString().Normalize(NormalizationForm.FormC).Trim().ToUpperInvariant()
    End Function
End Class
