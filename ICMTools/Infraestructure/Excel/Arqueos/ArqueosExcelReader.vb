Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports ExcelDataReader

Public Class ArqueosExcelReader

    Public ReadOnly Property _excelReader As ExcelReader

    Public Sub New()
        _excelReader = New ExcelReader()
    End Sub

    Public Function ValidacionesArqueos(
        rutaArchivo As String,
        filaEncabezado As Integer,
        nombreHoja As String,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim errores As New List(Of ExcelValidationError)

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                errores.AddRange(_excelReader.ValidacionesInformacion(reader, filaEncabezado, nombreHoja, mapeoColumnas))
            End Using
        End Using

        errores.AddRange(ValidacionesReglasArqueos(rutaArchivo, filaEncabezado, nombreHoja, mapeoColumnas))

        Return errores
    End Function

    Private Function ValidacionesReglasArqueos(
        rutaArchivo As String,
        filaEncabezado As Integer,
        nombreHoja As String,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim errores As New List(Of ExcelValidationError)

        Dim idxCodigoListado = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoListado))
        Dim idxNoSap = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NoSap))
        Dim idxAlmacen = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Almacen))
        Dim idxTipoListado = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoListado))
        Dim idxEstatus = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Estatus))
        Dim idxFechaCreacion = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCreacion))
        Dim idxNombre = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Nombre))
        Dim idxUsuarioCreador = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreador))
        Dim idxUsuarioCreadorPerfil = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreadorPerfil))
        Dim idxUsuarioAutorizador = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizador))
        Dim idxUsuarioAutorizadorPerfil = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizadorPerfil))
        Dim idxTipoCierre = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoCierre))
        Dim idxFechaInicioConteo = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaInicioConteo))
        Dim idxFechaFinConteo = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaFinConteo))
        Dim idxFechaCierreConteo = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCierreConteo))
        Dim idxFechaTerminoConteo = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaTerminoConteo))
        Dim idxSubinventario = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Subinventario))
        Dim idxIdProducto = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.IdProducto))
        Dim idxCodigoProducto = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoProducto))
        Dim idxNombreProducto = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NombreProducto))
        Dim idxUnidad = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Unidad))
        Dim idxCantidadSistema = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CantidadSistema))
        Dim idxDiferencia = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Diferencia))
        Dim idxFaltante = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Faltante))
        Dim idxSobrante = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Sobrante))
        Dim idxFaltantePrecioCons = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FaltantePrecioCons))
        Dim idxSobrantePrecioCons = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.SobrantePrecioCons))
        Dim idxComentario = ObtenerIndiceColumna(mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Comentario))

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                If Not MoverAHoja(reader, nombreHoja) Then
                    Return errores
                End If

                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                If Not reader.Read() Then
                    Return errores
                End If

                Dim filaActual As Integer = filaEncabezado + 1

                Do
                    If FilaVacia(reader, mapeoColumnas) Then
                        Exit Do
                    End If

                    ValidarFilaArqueos(
                        errores,
                        reader,
                        nombreHoja,
                        filaActual,
                        idxCodigoListado,
                        idxNoSap,
                        idxAlmacen,
                        idxTipoListado,
                        idxEstatus,
                        idxFechaCreacion,
                        idxNombre,
                        idxUsuarioCreador,
                        idxUsuarioCreadorPerfil,
                        idxUsuarioAutorizador,
                        idxUsuarioAutorizadorPerfil,
                        idxTipoCierre,
                        idxFechaInicioConteo,
                        idxFechaFinConteo,
                        idxFechaCierreConteo,
                        idxFechaTerminoConteo,
                        idxSubinventario,
                        idxIdProducto,
                        idxCodigoProducto,
                        idxNombreProducto,
                        idxUnidad,
                        idxCantidadSistema,
                        idxDiferencia,
                        idxFaltante,
                        idxSobrante,
                        idxFaltantePrecioCons,
                        idxSobrantePrecioCons,
                        idxComentario)

                    filaActual += 1
                Loop While reader.Read()
            End Using
        End Using

        Return errores
    End Function

    Private Sub ValidarFilaArqueos(
        errores As List(Of ExcelValidationError),
        reader As IExcelDataReader,
        nombreHoja As String,
        filaActual As Integer,
        idxCodigoListado As Integer,
        idxNoSap As Integer,
        idxAlmacen As Integer,
        idxTipoListado As Integer,
        idxEstatus As Integer,
        idxFechaCreacion As Integer,
        idxNombre As Integer,
        idxUsuarioCreador As Integer,
        idxUsuarioCreadorPerfil As Integer,
        idxUsuarioAutorizador As Integer,
        idxUsuarioAutorizadorPerfil As Integer,
        idxTipoCierre As Integer,
        idxFechaInicioConteo As Integer,
        idxFechaFinConteo As Integer,
        idxFechaCierreConteo As Integer,
        idxFechaTerminoConteo As Integer,
        idxSubinventario As Integer,
        idxIdProducto As Integer,
        idxCodigoProducto As Integer,
        idxNombreProducto As Integer,
        idxUnidad As Integer,
        idxCantidadSistema As Integer,
        idxDiferencia As Integer,
        idxFaltante As Integer,
        idxSobrante As Integer,
        idxFaltantePrecioCons As Integer,
        idxSobrantePrecioCons As Integer,
        idxComentario As Integer)

        Dim codigoListado = ObtenerTexto(reader.GetValue(idxCodigoListado))
        Dim noSapTexto = ObtenerTexto(reader.GetValue(idxNoSap))
        Dim almacen = ObtenerTexto(reader.GetValue(idxAlmacen))
        Dim tipoListado = ObtenerTexto(reader.GetValue(idxTipoListado))
        Dim estatus = ObtenerTexto(reader.GetValue(idxEstatus))
        Dim fechaCreacion = ObtenerFecha(reader.GetValue(idxFechaCreacion))
        Dim nombre = ObtenerTexto(reader.GetValue(idxNombre))
        Dim usuarioCreador = ObtenerTexto(reader.GetValue(idxUsuarioCreador))
        Dim usuarioCreadorPerfil = ObtenerTexto(reader.GetValue(idxUsuarioCreadorPerfil))
        Dim usuarioAutorizador = ObtenerTexto(reader.GetValue(idxUsuarioAutorizador))
        Dim usuarioAutorizadorPerfil = ObtenerTexto(reader.GetValue(idxUsuarioAutorizadorPerfil))
        Dim tipoCierre = ObtenerTexto(reader.GetValue(idxTipoCierre))
        Dim fechaInicioConteo = ObtenerFecha(reader.GetValue(idxFechaInicioConteo))
        Dim fechaFinConteo = ObtenerFecha(reader.GetValue(idxFechaFinConteo))
        Dim fechaCierreConteo = ObtenerFecha(reader.GetValue(idxFechaCierreConteo))
        Dim fechaTerminoConteo = ObtenerFecha(reader.GetValue(idxFechaTerminoConteo))
        Dim subinventario = ObtenerTexto(reader.GetValue(idxSubinventario))
        Dim idProducto = ObtenerTexto(reader.GetValue(idxIdProducto))
        Dim codigoProductoTexto = ObtenerTexto(reader.GetValue(idxCodigoProducto))
        Dim nombreProducto = ObtenerTexto(reader.GetValue(idxNombreProducto))
        Dim unidad = ObtenerTexto(reader.GetValue(idxUnidad))
        Dim cantidadSistema = ObtenerDecimal(reader.GetValue(idxCantidadSistema))
        Dim diferencia = ObtenerDecimal(reader.GetValue(idxDiferencia))
        Dim faltante = ObtenerDecimal(reader.GetValue(idxFaltante))
        Dim sobrante = ObtenerDecimal(reader.GetValue(idxSobrante))
        Dim faltantePrecioCons = ObtenerDecimal(reader.GetValue(idxFaltantePrecioCons))
        Dim sobrantePrecioCons = ObtenerDecimal(reader.GetValue(idxSobrantePrecioCons))
        Dim comentario = ObtenerTexto(reader.GetValue(idxComentario))

        If fechaCreacion.HasValue AndAlso fechaCreacion.Value > DateTime.Now Then
            AgregarError(errores, "La fecha de creación no puede ser posterior a la fecha de carga.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If fechaInicioConteo.HasValue AndAlso fechaFinConteo.HasValue AndAlso fechaInicioConteo.Value > fechaFinConteo.Value Then
            AgregarError(errores, "Inicio de Conteo debe ser menor o igual a Fin de Conteo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If fechaFinConteo.HasValue AndAlso fechaCierreConteo.HasValue AndAlso fechaFinConteo.Value > fechaCierreConteo.Value Then
            AgregarError(errores, "Fin de Conteo debe ser menor o igual a Cierre de Conteo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If fechaCierreConteo.HasValue AndAlso fechaTerminoConteo.HasValue AndAlso fechaCierreConteo.Value > fechaTerminoConteo.Value Then
            AgregarError(errores, "Cierre de Conteo debe ser menor o igual a Término de Conteo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If faltante.HasValue AndAlso sobrante.HasValue AndAlso faltante.Value > 0D AndAlso sobrante.Value > 0D Then
            AgregarError(errores, "Faltante y Sobrante no pueden ser positivos al mismo tiempo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If diferencia.HasValue Then
            If diferencia.Value > 0D Then
                If Not sobrante.HasValue OrElse sobrante.Value <= 0D Then
                    AgregarError(errores, "Si la diferencia es positiva, Sobrante debe ser mayor que cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
                If faltante.HasValue AndAlso faltante.Value > 0D Then
                    AgregarError(errores, "Si la diferencia es positiva, Faltante debe ser cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
            ElseIf diferencia.Value < 0D Then
                If Not faltante.HasValue OrElse faltante.Value <= 0D Then
                    AgregarError(errores, "Si la diferencia es negativa, Faltante debe ser mayor que cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
                If sobrante.HasValue AndAlso sobrante.Value > 0D Then
                    AgregarError(errores, "Si la diferencia es negativa, Sobrante debe ser cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
            Else
                If (faltante.HasValue AndAlso faltante.Value > 0D) OrElse (sobrante.HasValue AndAlso sobrante.Value > 0D) Then
                    AgregarError(errores, "Si la diferencia es cero, Faltante y Sobrante deben ser cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
            End If
        End If

        If Not String.IsNullOrWhiteSpace(codigoListado) AndAlso Not String.IsNullOrWhiteSpace(almacen) Then
            ' La validación de catálogos se integrará con la fuente autorizada cuando esté disponible.
        End If

        If Not String.IsNullOrWhiteSpace(nombreProducto) AndAlso Not String.IsNullOrWhiteSpace(codigoProductoTexto) Then
            ' La consistencia exacta producto/código depende del catálogo maestro.
        End If

        If Not String.IsNullOrWhiteSpace(comentario) AndAlso comentario.Length > 500 Then
            AgregarError(errores, "Comentarios no puede exceder 500 caracteres.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If
    End Sub

    Private Function MoverAHoja(reader As IExcelDataReader, nombreHoja As String) As Boolean
        Dim indiceHoja As Integer

        If Integer.TryParse(nombreHoja, indiceHoja) Then
            Dim indiceActual As Integer = 0

            Do
                If indiceActual = indiceHoja Then
                    Return True
                End If
                indiceActual += 1
            Loop While reader.NextResult()
        Else
            Do
                If String.Equals(reader.Name, nombreHoja, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Loop While reader.NextResult()
        End If

        Return False
    End Function

    Private Function FilaVacia(reader As IExcelDataReader, mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As Boolean
        For Each mapeo In mapeoColumnas
            Dim indiceColumna As Integer = mapeo.Value.ColumnIndex
            Dim valor = reader.GetValue(indiceColumna)

            If Not EsVacio(valor) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function ObtenerIndiceColumna(mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute), nombrePropiedad As String) As Integer
        Dim propiedad = mapeoColumnas.Keys.FirstOrDefault(Function(p) String.Equals(p.Name, nombrePropiedad, StringComparison.OrdinalIgnoreCase))

        If propiedad Is Nothing Then
            Return -1
        End If

        Return mapeoColumnas(propiedad).ColumnIndex
    End Function

    Private Function ObtenerTexto(valor As Object) As String
        If EsVacio(valor) Then
            Return Nothing
        End If

        Return valor.ToString().Trim()
    End Function

    Private Function ObtenerDecimal(valor As Object) As Nullable(Of Decimal)
        If EsVacio(valor) Then
            Return Nothing
        End If

        If TypeOf valor Is Decimal Then
            Return DirectCast(valor, Decimal)
        End If

        If TypeOf valor Is Integer OrElse TypeOf valor Is Long OrElse TypeOf valor Is Double OrElse TypeOf valor Is Single Then
            Return Convert.ToDecimal(valor, CultureInfo.InvariantCulture)
        End If

        Dim texto = valor.ToString().Trim()
        Dim resultado As Decimal

        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, resultado) Then
            Return resultado
        End If

        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, resultado) Then
            Return resultado
        End If

        Return Nothing
    End Function

    Private Function ObtenerFecha(valor As Object) As Nullable(Of DateTime)
        If EsVacio(valor) Then
            Return Nothing
        End If

        If TypeOf valor Is DateTime Then
            Return DirectCast(valor, DateTime)
        End If

        Dim texto = NormalizarTextoFecha(valor.ToString())
        Dim resultado As DateTime
        Dim formatos As String() = {
            "dd/MM/yyyy",
            "d/MM/yyyy",
            "dd-MM-yyyy",
            "d-MM-yyyy",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HH:mm:ss",
            "dd-MM-yyyy HH:mm",
            "dd-MM-yyyy HH:mm:ss",
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy",
            "M/d/yyyy",
            "MM/dd/yyyy HH:mm",
            "M/d/yyyy HH:mm",
            "MM/dd/yyyy HH:mm:ss",
            "M/d/yyyy HH:mm:ss"
        }

        If DateTime.TryParseExact(texto, formatos, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, resultado) Then
            Return resultado
        End If

        If DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, resultado) Then
            Return resultado
        End If

        If DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, resultado) Then
            Return resultado
        End If

        Return Nothing
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

    Private Function EsVacio(valor As Object) As Boolean
        Return valor Is Nothing OrElse valor Is DBNull.Value OrElse String.IsNullOrWhiteSpace(valor.ToString())
    End Function

    Private Sub AgregarError(errores As List(Of ExcelValidationError), problema As String, detalle As String)
        errores.Add(New ExcelValidationError With {
            .Problema = problema,
            .Detalle = detalle
        })
    End Sub

End Class
